using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Umbraco.Commerce.Common.Logging;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Core.PaymentProviders;
using Umbraco.Commerce.Extensions;
using Umbraco.Commerce.PaymentProviders.Quickpay.Api;
using Umbraco.Commerce.PaymentProviders.Quickpay.Api.Models;

namespace Umbraco.Commerce.PaymentProviders.Quickpay
{
    [PaymentProvider("quickpay-v10-checkout", "Quickpay V10", "Quickpay V10 payment provider for one time payments")]
    public class QuickpayCheckoutPaymentProvider : QuickpayPaymentProviderBase<QuickpayCheckoutPaymentProvider, QuickpayCheckoutSettings>
    {
        private const string QuickpayStatusCodeApproved = "20000";

        public QuickpayCheckoutPaymentProvider(UmbracoCommerceContext ctx, ILogger<QuickpayCheckoutPaymentProvider> logger)
            : base(ctx, logger)
        { }

        public override bool CanCancelPayments => true;
        public override bool CanCapturePayments => true;
        public override bool CanRefundPayments => true;
        public override bool CanFetchPaymentStatus => true;

        public override bool FinalizeAtContinueUrl => false;

        public override IEnumerable<TransactionMetaDataDefinition> TransactionMetaDataDefinitions => new[]
        {
            new TransactionMetaDataDefinition("quickPayOrderId", "Quickpay Order ID"),
            new TransactionMetaDataDefinition("quickPayPaymentId", "Quickpay Payment ID"),
            new TransactionMetaDataDefinition("quickPayPaymentHash", "Quickpay Payment Hash")
        };

        public override async Task<PaymentFormResult> GenerateFormAsync(PaymentProviderContext<QuickpayCheckoutSettings> ctx, CancellationToken cancellationToken = default)
        {
            var currency = Context.Services.CurrencyService.GetCurrency(ctx.Order.CurrencyId);
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

            var quickPayOrderId = ctx.Order.Properties["quickPayOrderId"]?.Value;
            var quickPayPaymentId = ctx.Order.Properties["quickPayPaymentId"]?.Value;
            var quickPayPaymentHash = ctx.Order.Properties["quickPayPaymentHash"]?.Value ?? string.Empty;
            var quickPayPaymentLinkHash = ctx.Order.Properties["quickPayPaymentLinkHash"]?.Value ?? string.Empty;

            if (quickPayPaymentHash != GetPaymentHash(quickPayPaymentId, ctx.Order.OrderNumber, currencyCode, orderAmount))
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
                        var store = Context.Services.StoreService.GetStore(ctx.Order.StoreId);
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
                                reference = reference.Substring(prefix.Length - 1);
                            }
                            else if (orderNumberTemplate.Contains("{0}"))
                            {
                                // Trim prefix & suffix
                                reference = reference.Substring(prefix.Length - 1, reference.Length - suffix.Length);
                            }
                        }
                    }

                    var metaData = new Dictionary<string, string>
                    {
                        { "orderReference", ctx.Order.GenerateOrderReference() },
                        { "orderId", ctx.Order.Id.ToString("D") },
                        { "orderNumber", ctx.Order.OrderNumber }
                    };

                    quickPayOrderId = reference;

                    var payment = await client.CreatePaymentAsync(
                        new QuickpayPaymentRequest
                        {
                            OrderId = quickPayOrderId,
                            Currency = currencyCode,
                            Variables = metaData
                        },
                        cancellationToken).ConfigureAwait(false);

                    quickPayPaymentId = GetTransactionId(payment);

                    var paymentLink = await client.CreatePaymentLinkAsync(payment.Id.ToString(), new QuickpayPaymentLinkRequest
                    {
                        Amount = orderAmount,
                        Language = lang.ToString(),
                        ContinueUrl = ctx.Urls.ContinueUrl,
                        CancelUrl = ctx.Urls.CancelUrl,
                        CallbackUrl = ctx.Urls.CallbackUrl,
                        PaymentMethods = paymentMethods?.Length > 0 ? string.Join(",", paymentMethods) : null,
                        AutoFee = ctx.Settings.AutoFee,
                        AutoCapture = ctx.Settings.AutoCapture
                    },
                        cancellationToken).ConfigureAwait(false);

                    paymentFormLink = paymentLink.Url;

                    quickPayPaymentHash = GetPaymentHash(payment.Id.ToString(), ctx.Order.OrderNumber, currencyCode, orderAmount);
                    quickPayPaymentLinkHash = Base64Encode(paymentFormLink);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Quickpay - error creating payment.");
                }
            }
            else
            {
                // Get payment link from order properties.
                paymentFormLink = Base64Decode(quickPayPaymentLinkHash);
            }

            return new PaymentFormResult()
            {
                MetaData = new Dictionary<string, string>
                {
                    { "quickPayOrderId", quickPayOrderId },
                    { "quickPayPaymentId", quickPayPaymentId },
                    { "quickPayPaymentHash", quickPayPaymentHash },
                    { "quickPayPaymentLinkHash", quickPayPaymentLinkHash }
                },
                Form = new PaymentForm(paymentFormLink, PaymentFormMethod.Get)
            };
        }

        public override async Task<CallbackResult> ProcessCallbackAsync(PaymentProviderContext<QuickpayCheckoutSettings> context, CancellationToken cancellationToken = default)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(context);
                if (!await ValidateChecksumAsync(context.Request, context.Settings.PrivateKey, cancellationToken).ConfigureAwait(false))
                {
                    Logger.Warn($"Quickpay [{context.Order.OrderNumber}] - Checksum validation failed");
                    return CallbackResult.BadRequest();
                }

                QuickpayPayment payment = await ParseCallbackAsync(
                        context.Request,
                        cancellationToken).ConfigureAwait(false);

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

                    int totalAmount = latestOperation.Amount;
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
                if (order.Properties["quickPayOrderId"]?.Value == payment.OrderId)
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

                var payment = await client.GetPaymentAsync(id, cancellationToken).ConfigureAwait(false);

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

                var payment = await client.CancelPaymentAsync(id, cancellationToken).ConfigureAwait(false);

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
                cancellationToken).ConfigureAwait(false);

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

        public override async Task<ApiResult> RefundPaymentAsync(PaymentProviderContext<QuickpayCheckoutSettings> ctx, CancellationToken cancellationToken = default)
        {
            // POST: /payments/{id}/refund

            try
            {
                var id = ctx.Order.TransactionInfo.TransactionId;

                var clientConfig = GetQuickpayClientConfig(ctx.Settings);
                var client = new QuickpayClient(clientConfig);

                var payment = await client.RefundPaymentAsync(id, new
                {
                    amount = AmountToMinorUnits(ctx.Order.TransactionInfo.AmountAuthorized.Value)
                },
                cancellationToken).ConfigureAwait(false);

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
                Logger.Error(ex, "Quickpay - RefundPayment");
            }

            return ApiResult.Empty;
        }

        public async Task<QuickpayPayment> ParseCallbackAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            using (var stream = await request.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
            {
                if (stream.CanSeek)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }

                // Get quickpay callback body text - See parameters: https://learn.quickpay.net/tech-talk/api/callback/

                using (var reader = new StreamReader(stream))
                {
                    var json = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

                    // Deserialize json body text 
                    return JsonConvert.DeserializeObject<QuickpayPayment>(json);
                }
            }
        }

        private async Task<bool> ValidateChecksumAsync(HttpRequestMessage request, string privateAccountKey, CancellationToken cancellationToken = default)
        {
            var json = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var checkSum = request.Headers.GetValues("Quickpay-Checksum-Sha256").FirstOrDefault();

            if (string.IsNullOrEmpty(checkSum))
                return false;

            var calculatedChecksum = Checksum(json, privateAccountKey);

            return checkSum.Equals(calculatedChecksum);
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
