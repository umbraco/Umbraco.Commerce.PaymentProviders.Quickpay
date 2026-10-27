using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Flurl.Http.Testing;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Umbraco.Commerce.Common.Logging;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Core.PaymentProviders;
using Umbraco.Commerce.Extensions;
using Umbraco.Commerce.PaymentProviders.Quickpay.Api.Models;

namespace Umbraco.Commerce.PaymentProviders.Quickpay.UnitTests
{
    public class PaymentStatusTests
    {
        [Theory]
        [InlineData(7500, PaymentStatus.PartiallyRefunded)]
        [InlineData(1, PaymentStatus.PartiallyRefunded)]
        [InlineData(0, PaymentStatus.Refunded)]
        public async Task FetchPaymentStatus_Refund_UsesRemainingBalance(int balance, PaymentStatus expectedStatus)
        {
            using var http = new HttpTest();
            var payment = CreateRefundedPayment(balance);
            http.RespondWith(JsonSerializer.Serialize(payment));
            var provider = new QuickpayCheckoutPaymentProvider(null!, null!);

            var result = await provider.FetchPaymentStatusAsync(CreateContext()).ConfigureAwait(true);

            result.TransactionInfo.PaymentStatus.Should().Be(expectedStatus);
            result.TransactionInfo.TransactionId.Should().Be("123");
            http.ShouldHaveCalled("https://api.quickpay.net/payments/123").WithVerb(HttpMethod.Get).Times(1);
            http.CallLog.Should().HaveCount(1);
        }

        [Theory]
        [InlineData(true, "20000")]
        [InlineData(false, "40000")]
        public async Task FetchPaymentStatus_UnsuccessfulRefund_PreservesCapturedStatus(bool pending, string statusCode)
        {
            using var http = new HttpTest();
            var payment = CreateRefundedPayment(10000);
            payment.Operations[^1].Pending = pending;
            payment.Operations[^1].QuickpayStatusCode = statusCode;
            http.RespondWith(JsonSerializer.Serialize(payment));
            var provider = new QuickpayCheckoutPaymentProvider(null!, null!);

            var result = await provider.FetchPaymentStatusAsync(CreateContext()).ConfigureAwait(true);

            result.TransactionInfo.PaymentStatus.Should().Be(PaymentStatus.Captured);
        }

        [Theory]
        [InlineData(7500, 25, PaymentStatus.PartiallyRefunded)]
        [InlineData(0, 100, PaymentStatus.Refunded)]
        public async Task RefundPayment_Refund_UsesRemainingBalance(int balance, int refundAmount, PaymentStatus expectedStatus)
        {
            using var http = new HttpTest();
            http.RespondWith(JsonSerializer.Serialize(CreateRefundedPayment(balance)));
            var provider = new QuickpayCheckoutPaymentProvider(null!, null!);

            var result = await provider.RefundPaymentAsync(CreateContext(), new PaymentProviderOrderRefundRequest { RefundAmount = refundAmount }).ConfigureAwait(true);

            result.Should().NotBeNull();
            result!.TransactionInfo.PaymentStatus.Should().Be(expectedStatus);
            http.ShouldHaveCalled("https://api.quickpay.net/payments/123/refund*")
                .WithVerb(HttpMethod.Post)
                .WithRequestJson(new { amount = refundAmount * 100 })
                .Times(1);
            http.CallLog.Should().HaveCount(1);
        }

        [Theory]
        [InlineData(4000, PaymentStatus.PartiallyRefunded)]
        [InlineData(0, PaymentStatus.Refunded)]
        public async Task FetchPaymentStatus_MultipleRefunds_UsesRemainingBalance(int balance, PaymentStatus expectedStatus)
        {
            using var http = new HttpTest();
            var payment = CreateRefundedPayment(balance);
            payment.Operations.Insert(2, new Operation { Type = "refund", Amount = 1250, QuickpayStatusCode = "20000" });
            payment.Operations[^1].Amount -= 1250;
            http.RespondWith(JsonSerializer.Serialize(payment));
            var provider = new QuickpayCheckoutPaymentProvider(null!, null!);

            var result = await provider.FetchPaymentStatusAsync(CreateContext()).ConfigureAwait(true);

            result.TransactionInfo.PaymentStatus.Should().Be(expectedStatus);
        }

        [Theory]
        [InlineData(7500, PaymentStatus.PartiallyRefunded)]
        [InlineData(0, PaymentStatus.Refunded)]
        public async Task ProcessCallback_Refund_UsesRemainingBalance(int balance, PaymentStatus expectedStatus)
        {
            var context = CreateContext();
            var payment = CreateRefundedPayment(balance);
            payment.Variables = new Dictionary<string, string> { { "orderReference", context.Order.GenerateOrderReference() } };
            var json = JsonSerializer.Serialize(payment);
            using var body = new MemoryStream(Encoding.UTF8.GetBytes(json));
            context.HttpContext.Request.Body = body;
            context.HttpContext.Request.Headers["Quickpay-Checksum-Sha256"] = Convert.ToHexStringLower(
                HMACSHA256.HashData(Encoding.UTF8.GetBytes(context.Settings.PrivateKey), Encoding.UTF8.GetBytes(json)));
            var logger = DispatchProxy.Create<ILogger<QuickpayCheckoutPaymentProvider>, TestLogger>();
            var provider = new QuickpayCheckoutPaymentProvider(null!, logger);

            var result = await provider.ProcessCallbackAsync(context).ConfigureAwait(true);

            result.TransactionInfo.PaymentStatus.Should().Be(expectedStatus);
            result.TransactionInfo.TransactionId.Should().Be("123");
        }

        private static QuickpayPayment CreateRefundedPayment(int balance) => new()
        {
            Id = 123,
            Balance = balance,
            Operations =
            [
                new Operation { Type = "authorize", Amount = 10000, QuickpayStatusCode = "20000" },
                new Operation { Type = "capture", Amount = 10000, QuickpayStatusCode = "20000" },
                new Operation { Type = "refund", Amount = 10000 - balance, QuickpayStatusCode = "20000" },
            ],
        };

        private static PaymentProviderContext<QuickpayCheckoutSettings> CreateContext()
        {
            // Commerce's order and context constructors are internal.
            var state = Create<OrderState>();
            typeof(OrderState).GetProperty(nameof(OrderState.TransactionId))!.SetValue(state, "123");
            typeof(OrderState).GetProperty(nameof(OrderState.OrderNumber))!.SetValue(state, "ORDER-123");
            typeof(OrderState).GetProperty(nameof(OrderState.Id))!.SetValue(state, Guid.Parse("80b44f7c-27fa-4e10-a829-853df45966b1"));
            var order = Create<OrderReadOnly>(state);
            var context = Create<PaymentProviderContext>(order, new Dictionary<string, string>(), null!);
            typeof(PaymentProviderContext).GetProperty(nameof(PaymentProviderContext.HttpContext))!.SetValue(context, new DefaultHttpContext());
            return Create<PaymentProviderContext<QuickpayCheckoutSettings>>(context, new QuickpayCheckoutSettings { ApiKey = "test", PrivateKey = "test-private-key" });
        }

        private static T Create<T>(params object[] arguments) => (T)Activator.CreateInstance(
            typeof(T), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, arguments, null)!;

        public class TestLogger : DispatchProxy
        {
            protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => targetMethod?.ReturnType == typeof(bool) ? false : null;
        }
    }
}
