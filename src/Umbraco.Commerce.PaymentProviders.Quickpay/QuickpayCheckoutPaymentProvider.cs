using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Umbraco.Commerce.Common.Logging;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Core.PaymentProviders;
using Umbraco.Commerce.Extensions;
using Umbraco.Commerce.PaymentProviders.Quickpay.Api;
using Umbraco.Commerce.PaymentProviders.Quickpay.Api.Models;

namespace Umbraco.Commerce.PaymentProviders.Quickpay
{
    [PaymentProvider("quickpay-v10-checkout")]
    public class QuickpayCheckoutPaymentProvider : QuickpayPaymentProviderBase<QuickpayCheckoutPaymentProvider, QuickpayCheckoutSettings>
    {
        private const string QuickpayStatusCodeApproved = "20000";

        public QuickpayCheckoutPaymentProvider(UmbracoCommerceContext ctx, ILogger<QuickpayCheckoutPaymentProvider> logger)
            : base(ctx, logger)
        { }

        public override bool CanCancelPayments => true;
        public override bool CanCapturePayments => true;
        public override bool CanRefundPayments => true;
        public override bool CanPartiallyRefundPayments => true;
        public override bool CanFetchPaymentStatus => true;

        public override bool FinalizeAtContinueUrl => false;

        public override IEnumerable<TransactionMetaDataDefinition> TransactionMetaDataDefinitions =>
        [
            new TransactionMetaDataDefinition("quickpayOrderId"),
            new TransactionMetaDataDefinition("quickpayPaymentId"),
            new TransactionMetaDataDefinition("quickpayPaymentHash")
        ];

        public override async Task<PaymentFormResult> GenerateFormAsync(PaymentProviderContext<QuickpayCheckoutSettings> ctx, CancellationToken cancellationToken = default)
        {
            var currency = await Context.Services.CurrencyService.GetCurrencyAsync(ctx.Order.CurrencyId);
            var currencyCode = currency.Code.ToUpperInvariant();

            // Ensure currency has valid ISO 4217 code
            if (!Iso4217.CurrencyCodes.ContainsKey(currencyCode))
            {
                throw new Exception("Currency must be a valid ISO 4217 currency code: " + currency.Name);
            }

            string paymentFormLink = string.Empty;
            var orderAmount = AmountToMinorUnits(ctx.Order.TransactionAmount.Value);

            var paymentMethods = ctx.Settings.PaymentMethods?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                   .Where(x => !string.IsNullOrWhiteSpace(x))
                   .Select(s => s.Trim())
                   .ToArray();

            // Parse language - default language is English.
            Enum.TryParse(ctx.Settings.Lang, true, out QuickpayLang lang);

            var quickpayOrderId = ctx.Order.Properties["quickpayOrderId"]?.Value;
            var quickpayPaymentId = ctx.Order.Properties["quickpayPaymentId"]?.Value;
            var quickpayPaymentHash = ctx.Order.Properties["quickpayPaymentHash"]?.Value ?? string.Empty;
            var quickpayPaymentLinkHash = ctx.Order.Properties["quickpayPaymentLinkHash"]?.Value ?? string.Empty;

            if (quickpayPaymentHash != GetPaymentHash(quickpayPaymentId, ctx.Order.OrderNumber, currencyCode, orderAmount))
            {
                try
                {
                    // https://learn.quickpay.net/tech-talk/guides/payments/#introduction-to-payments

                    var clientConfig = GetQuickpayClientConfig(ctx.Settings);
                    var client = new QuickpayClient(clientConfig);

                    var reference = ctx.Order.OrderNumber;

                    // Quickpay has a limit of order id between 4-20 characters.
                    if (reference.Length > 20)
                    {
                        var store = await Context.Services.StoreService.GetStoreAsync(ctx.Order.StoreId);
                        var orderNumberTemplate = store.OrderNumberTemplate;

                        // If the order number template is not equals Vendr generated order number, we need to decide whether to trim prefix, suffix or both.
                        if (orderNumberTemplate.Equals("{0}") == false)
                        {
                            var index = orderNumberTemplate.IndexOf("{0}");
                            var prefix = orderNumberTemplate.Substring(0, index);
                            var suffix = orderNumberTemplate.Substring(index + 3, orderNumberTemplate.Length - (index + 3));

                            if (orderNumberTemplate.StartsWith("{0}"))
                            {
                                // Trim suffix
                                reference = reference.Substring(index, reference.Length - suffix.Length);
                            }
                            else if (orderNumberTemplate.EndsWith("{0}"))
                            {
                                // Trim prefix
                                reference = reference.Substring(prefix.Length);
                            }
                            else if (orderNumberTemplate.Contains("{0}"))
                            {
                                // Trim prefix & suffix
                                reference = reference.Substring(prefix.Length, reference.Length - prefix.Length - suffix.Length);
                            }
                        }
                    }

                    var metaData = new Dictionary<string, string>
                    {
                        { "orderReference", ctx.Order.GenerateOrderReference() },
                        { "orderId", ctx.Order.Id.ToString("D") },
                        { "orderNumber", ctx.Order.OrderNumber }
                    };

                    quickpayOrderId = reference;

                    var payment = await client.CreatePaymentAsync(
                        new QuickpayPaymentRequest
                        {
                            OrderId = quickpayOrderId,
                            Currency = currencyCode,
                            Variables = metaData
                        },
                        cancellationToken);

                    quickpayPaymentId = GetTransactionId(payment);

                    var paymentLink = await client.CreatePaymentLinkAsync(payment.Id.ToString(), new QuickpayPaymentLinkRequest
                    {
                        Amount = orderAmount,
                        Language = lang.ToString(),
                        ContinueUrl = ctx.Urls.ContinueUrl,
                        CancelUrl = ctx.Urls.CancelUrl,
                        CallbackUrl = ctx.Urls.CallbackUrl,
                        PaymentMethods = paymentMethods?.Length > 0 ? string.Join(",", paymentMethods) : null,
                        AutoFee = ctx.Settings.AutoFee,
                        AutoCapture = ctx.Settings.AutoCapture,
                        Framed = ctx.Settings.Framed

                    },
                        cancellationToken);

                    paymentFormLink = paymentLink.Url;

                    quickpayPaymentHash = GetPaymentHash(payment.Id.ToString(), ctx.Order.OrderNumber, currencyCode, orderAmount);
                    quickpayPaymentLinkHash = Base64Encode(paymentFormLink);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Quickpay - error creating payment.");
                }
            }
            else
            {
                // Get payment link from order properties.
                paymentFormLink = Base64Decode(quickpayPaymentLinkHash);
            }

            return new PaymentFormResult()
            {
                MetaData = new Dictionary<string, string>
                {
                    { "quickpayOrderId", quickpayOrderId },
                    { "quickpayPaymentId", quickpayPaymentId },
                    { "quickpayPaymentHash", quickpayPaymentHash },
                    { "quickpayPaymentLinkHash", quickpayPaymentLinkHash }
                },
                Form = new PaymentForm(paymentFormLink, PaymentFormMethod.Get)
            };
        }

        public override async Task<CallbackResult> ProcessCallbackAsync(PaymentProviderContext<QuickpayCheckoutSettings> context, CancellationToken cancellationToken = default)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(context);

                var webhookBody = await GetJsonBodyAsync(context.HttpContext.Request, cancellationToken);

                if (!await ValidateChecksumAsync(
                    context.HttpContext.Request.Headers["Quickpay-Checksum-Sha256"],
                    webhookBody,
                    context.Settings.PrivateKey,
                    cancellationToken))
                {
                    Logger.Warn($"Quickpay [{context.Order.OrderNumber}] - Checksum validation failed");
                    return CallbackResult.BadRequest();
                }

                QuickpayPayment payment = await ParseCallbackAsync(
                    webhookBody,
                    cancellationToken);

                if (!VerifyOrder(context.Order, payment))
                {
                    Logger.Warn($"Quickpay [{context.Order.OrderNumber}] - Couldn't verify the order");
                    return CallbackResult.Empty;
                }

                Operation latestOperation = payment.Operations.LastOrDefault();
                if (latestOperation == null)
                {
                    return CallbackResult.BadRequest();
                }

                if (latestOperation.QuickpayStatusCode == QuickpayStatusCodeApproved || latestOperation.AcquirerStatusCode == "000")
                {
                    PaymentStatus? currentPaymentStatus = context.Order?.TransactionInfo?.PaymentStatus;
                    PaymentStatus newPaymentStatus = GetPaymentStatus(latestOperation);

                    Logger.Debug($"ProcessCallbackAsync - current payment status: {currentPaymentStatus}, new payment status: {newPaymentStatus}, operations: {System.Text.Json.JsonSerializer.Serialize(payment.Operations)}.");
                    if (newPaymentStatus == PaymentStatus.Authorized && currentPaymentStatus != PaymentStatus.Initialized)
                    {
                        return CallbackResult.Empty;
                    }

                    if (newPaymentStatus == PaymentStatus.Captured && currentPaymentStatus != PaymentStatus.Authorized)
                    {
                        return CallbackResult.BadRequest();
                    }

                    int totalAmount = latestOperation.Amount ?? 0;
                    return new CallbackResult
                    {
                        TransactionInfo = new TransactionInfo
                        {
                            AmountAuthorized = AmountFromMinorUnits(totalAmount),
                            TransactionId = GetTransactionId(payment),
                            PaymentStatus = newPaymentStatus,
                        },
                    };
                }

                Logger.Warn($"Quickpay [{context.Order.OrderNumber}] - Payment not approved. Quickpay status code: {latestOperation.QuickpayStatusCode} ({latestOperation.QuickpayStatusMessage}). Acquirer status code: {latestOperation.AcquirerStatusCode} ({latestOperation.AcquirerStatusMessage}).");
                return CallbackResult.Empty;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Quickpay - ProcessCallback");
                return CallbackResult.BadRequest();
            }
        }

        private static bool VerifyOrder(OrderReadOnly order, QuickpayPayment payment)
        {
            if (payment.Variables.Count > 0 &&
                payment.Variables.TryGetValue("orderReference", out string orderReference))
            {
                if (order.GenerateOrderReference() == orderReference)
                {
                    return true;
                }
            }
            else
            {
                if (order.Properties["quickpayOrderId"]?.Value == payment.OrderId)
                {
                    return true;
                }
            }

            return false;
        }

        public override async Task<ApiResult> FetchPaymentStatusAsync(PaymentProviderContext<QuickpayCheckoutSettings> ctx, CancellationToken cancellationToken = default)
        {
            // GET: /payments/{id}

            try
            {
                var id = ctx.Order.TransactionInfo.TransactionId;

                var clientConfig = GetQuickpayClientConfig(ctx.Settings);
                var client = new QuickpayClient(clientConfig);

                var payment = await client.GetPaymentAsync(id, cancellationToken);

                Operation lastCompletedOperation = payment.Operations.LastOrDefault(o => !o.Pending && o.QuickpayStatusCode == "20000");

                if (lastCompletedOperation != null)
                {
                    var paymentStatus = GetPaymentStatus(lastCompletedOperation);

                    return new ApiResult()
                    {
                        TransactionInfo = new TransactionInfoUpdate()
                        {
                            TransactionId = GetTransactionId(payment),
                            PaymentStatus = paymentStatus
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Quickpay - FetchPaymentStatus");
            }

            return ApiResult.Empty;
        }

        public override async Task<ApiResult> CancelPaymentAsync(PaymentProviderContext<QuickpayCheckoutSettings> ctx, CancellationToken cancellationToken = default)
        {
            // POST: /payments/{id}/cancel

            try
            {
                var id = ctx.Order.TransactionInfo.TransactionId;

                var clientConfig = GetQuickpayClientConfig(ctx.Settings);
                var client = new QuickpayClient(clientConfig);

                var payment = await client.CancelPaymentAsync(id, cancellationToken);

                Operation lastCompletedOperation = payment.Operations.LastOrDefault(o => !o.Pending && o.QuickpayStatusCode == "20000");

                if (lastCompletedOperation != null)
                {
                    var paymentStatus = GetPaymentStatus(lastCompletedOperation);

                    return new ApiResult()
                    {
                        TransactionInfo = new TransactionInfoUpdate()
                        {
                            TransactionId = GetTransactionId(payment),
                            PaymentStatus = paymentStatus
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Quickpay - CancelPayment");
            }

            return ApiResult.Empty;
        }

        public override async Task<ApiResult> CapturePaymentAsync(PaymentProviderContext<QuickpayCheckoutSettings> ctx, CancellationToken cancellationToken = default)
        {
            // POST: /payments/{id}/capture

            try
            {
                var id = ctx.Order.TransactionInfo.TransactionId;

                var clientConfig = GetQuickpayClientConfig(ctx.Settings);
                var client = new QuickpayClient(clientConfig);

                var payment = await client.CapturePaymentAsync(id, new
                {
                    amount = AmountToMinorUnits(ctx.Order.TransactionInfo.AmountAuthorized.Value)
                },
                cancellationToken);

                Operation lastCompletedOperation = payment.Operations.LastOrDefault(o => !o.Pending && o.QuickpayStatusCode == "20000");

                if (lastCompletedOperation != null)
                {
                    var paymentStatus = GetPaymentStatus(lastCompletedOperation);

                    return new ApiResult()
                    {
                        TransactionInfo = new TransactionInfoUpdate()
                        {
                            TransactionId = GetTransactionId(payment),
                            PaymentStatus = paymentStatus
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Quickpay - CapturePayment");
            }

            return ApiResult.Empty;
        }

        public override async Task<ApiResult?> RefundPaymentAsync(PaymentProviderContext<QuickpayCheckoutSettings> context, PaymentProviderOrderRefundRequest refundRequest, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(refundRequest);
            try
            {
                string id = context.Order.TransactionInfo.TransactionId;

                QuickpayClientConfig clientConfig = GetQuickpayClientConfig(context.Settings);
                QuickpayClient client = new(clientConfig);

                QuickpayPayment payment = await client.RefundPaymentAsync(
                    id,
                    new { amount = AmountToMinorUnits(refundRequest.RefundAmount) },
                    cancellationToken);

                Operation? lastCompletedOperation = payment.Operations.LastOrDefault(o => !o.Pending && o.QuickpayStatusCode == "20000");

                if (lastCompletedOperation != null)
                {
                    PaymentStatus paymentStatus = GetPaymentStatus(lastCompletedOperation);

                    return new ApiResult()
                    {
                        TransactionInfo = new TransactionInfoUpdate()
                        {
                            TransactionId = GetTransactionId(payment),
                            PaymentStatus = paymentStatus,
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Quickpay - RefundPayment");
            }

            return ApiResult.Empty;
        }

        private async Task<string> GetJsonBodyAsync(HttpRequest request, CancellationToken cancellationToken = default)
        {
            if (request.Body.CanSeek)
            {
                request.Body.Seek(0, SeekOrigin.Begin);
            }

            using var reader = new StreamReader(request.Body);
            return await reader.ReadToEndAsync(cancellationToken);
        }

        private Task<QuickpayPayment> ParseCallbackAsync(string webhookBody, CancellationToken cancellationToken = default)
            => Task.FromResult(JsonSerializer.Deserialize<QuickpayPayment>(webhookBody));

        private async Task<bool> ValidateChecksumAsync(string checksum, string webhookBody, string privateAccountKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(checksum))
                return false;

            var calculatedChecksum = Checksum(webhookBody, privateAccountKey);

            return checksum.Equals(calculatedChecksum);
        }

        private string Checksum(string content, string privateKey)
        {
            var s = new StringBuilder();
            var e = Encoding.UTF8;
            var bytes = e.GetBytes(privateKey);

            using (var hmac = new HMACSHA256(bytes))
            {
                var b = hmac.ComputeHash(e.GetBytes(content));

                foreach (var t in b)
                {
                    s.Append(t.ToString("x2"));
                }
            }

            return s.ToString();
        }
    }
}
