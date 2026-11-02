using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Umbraco.Commerce.Common.Logging;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Core.PaymentProviders;
using Umbraco.Commerce.Extensions;
using Umbraco.Commerce.PaymentProviders.Quickpay.Api.Models;

namespace Umbraco.Commerce.PaymentProviders.Quickpay.UnitTests
{
    /// <summary>
    /// The webhook callback: checksum, order check and status transitions.
    /// </summary>
    public class CallbackTests
    {
        private static QuickpayCheckoutPaymentProvider CreateProvider()
            => new(null!, DispatchProxy.Create<ILogger<QuickpayCheckoutPaymentProvider>, PaymentStatusTests.TestLogger>());

        private static PaymentProviderContext<QuickpayCheckoutSettings> CreateContext(PaymentStatus currentStatus)
            => PaymentStatusTests.CreateContext(currentStatus);

        private static QuickpayPayment Payment(PaymentProviderContext<QuickpayCheckoutSettings> context, params Operation[] operations) => new()
        {
            Id = 123,
            Fee = 150,
            Variables = new Dictionary<string, string> { { "orderReference", context.Order.GenerateOrderReference() } },
            Operations = [.. operations],
        };

        private static string Sign(string json, string key)
            => Convert.ToHexStringLower(HMACSHA256.HashData(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(json)));

        private static void SetBody(PaymentProviderContext<QuickpayCheckoutSettings> context, string json, string? checksum)
        {
            context.HttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
            if (checksum != null)
            {
                context.HttpContext.Request.Headers["Quickpay-Checksum-Sha256"] = checksum;
            }
        }

        [Fact]
        public async Task Authorize_Callback_On_A_New_Order_Is_Authorized()
        {
            var context = CreateContext(PaymentStatus.Initialized);
            string json = JsonSerializer.Serialize(Payment(context, new Operation { Type = "authorize", Amount = 10000, QuickpayStatusCode = "20000" }));
            SetBody(context, json, Sign(json, context.Settings.PrivateKey));

            CallbackResult result = await CreateProvider().ProcessCallbackAsync(context);

            result.TransactionInfo.Should().NotBeNull();
            result.TransactionInfo!.PaymentStatus.Should().Be(PaymentStatus.Authorized);
            result.TransactionInfo.AmountAuthorized.Should().Be(100m);
            result.TransactionInfo.TransactionFee.Should().Be(1.5m);
            result.TransactionInfo.TransactionId.Should().Be("123");
        }

        [Fact]
        public async Task Repeated_Authorize_Callback_Is_Ignored()
        {
            var context = CreateContext(PaymentStatus.Authorized);
            string json = JsonSerializer.Serialize(Payment(context, new Operation { Type = "authorize", Amount = 10000, QuickpayStatusCode = "20000" }));
            SetBody(context, json, Sign(json, context.Settings.PrivateKey));

            CallbackResult result = await CreateProvider().ProcessCallbackAsync(context);

            result.TransactionInfo.Should().BeNull();
        }

        [Fact]
        public async Task Capture_Callback_Without_An_Authorization_Is_Rejected()
        {
            var context = CreateContext(PaymentStatus.Initialized);
            string json = JsonSerializer.Serialize(Payment(context, new Operation { Type = "capture", Amount = 10000, QuickpayStatusCode = "20000" }));
            SetBody(context, json, Sign(json, context.Settings.PrivateKey));

            CallbackResult result = await CreateProvider().ProcessCallbackAsync(context);

            result.TransactionInfo.Should().BeNull();
            result.ActionResult.Should().NotBeNull();
        }

        [Fact]
        public async Task Wrong_Checksum_Is_Rejected()
        {
            var context = CreateContext(PaymentStatus.Initialized);
            string json = JsonSerializer.Serialize(Payment(context, new Operation { Type = "authorize", Amount = 10000, QuickpayStatusCode = "20000" }));
            SetBody(context, json, Sign(json, "some-other-key"));

            CallbackResult result = await CreateProvider().ProcessCallbackAsync(context);

            result.TransactionInfo.Should().BeNull();
            result.ActionResult.Should().NotBeNull();
        }

        [Fact]
        public async Task Tampered_Body_Is_Rejected()
        {
            var context = CreateContext(PaymentStatus.Initialized);
            string json = JsonSerializer.Serialize(Payment(context, new Operation { Type = "authorize", Amount = 10000, QuickpayStatusCode = "20000" }));
            string checksum = Sign(json, context.Settings.PrivateKey);
            SetBody(context, json.Replace("10000", "1", StringComparison.Ordinal), checksum);

            CallbackResult result = await CreateProvider().ProcessCallbackAsync(context);

            result.TransactionInfo.Should().BeNull();
        }

        [Fact]
        public async Task Missing_Checksum_Is_Rejected()
        {
            var context = CreateContext(PaymentStatus.Initialized);
            string json = JsonSerializer.Serialize(Payment(context, new Operation { Type = "authorize", Amount = 10000, QuickpayStatusCode = "20000" }));
            SetBody(context, json, null);

            CallbackResult result = await CreateProvider().ProcessCallbackAsync(context);

            result.TransactionInfo.Should().BeNull();
            result.ActionResult.Should().NotBeNull();
        }

        [Fact]
        public async Task Payment_For_Another_Order_Is_Ignored()
        {
            var context = CreateContext(PaymentStatus.Initialized);
            QuickpayPayment payment = Payment(context, new Operation { Type = "authorize", Amount = 10000, QuickpayStatusCode = "20000" });
            payment.Variables["orderReference"] = "some-other-order";
            string json = JsonSerializer.Serialize(payment);
            SetBody(context, json, Sign(json, context.Settings.PrivateKey));

            CallbackResult result = await CreateProvider().ProcessCallbackAsync(context);

            result.TransactionInfo.Should().BeNull();
        }

        [Fact]
        public async Task Declined_Operation_Gives_No_Transaction()
        {
            var context = CreateContext(PaymentStatus.Initialized);
            string json = JsonSerializer.Serialize(Payment(context, new Operation { Type = "authorize", Amount = 10000, QuickpayStatusCode = "40000", AcquirerStatusCode = "100" }));
            SetBody(context, json, Sign(json, context.Settings.PrivateKey));

            CallbackResult result = await CreateProvider().ProcessCallbackAsync(context);

            result.TransactionInfo.Should().BeNull();
        }
    }
}
