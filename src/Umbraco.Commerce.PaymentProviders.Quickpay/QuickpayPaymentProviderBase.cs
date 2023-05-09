using Umbraco.Commerce.Common.Logging;
using Umbraco.Commerce.PaymentProviders.Quickpay.Api.Models;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Core.PaymentProviders;
using Umbraco.Commerce.Extensions;

namespace Umbraco.Commerce.PaymentProviders.Quickpay
{
    public abstract class QuickpayPaymentProviderBase<TSelf, TSettings> : PaymentProviderBase<TSettings>
        where TSelf : QuickpayPaymentProviderBase<TSelf, TSettings>
        where TSettings : QuickpaySettingsBase, new()
    {
        protected ILogger<TSelf> Logger { get; }

        public QuickpayPaymentProviderBase(UmbracoCommerceContext ctx,
            ILogger<TSelf> logger)
            : base(ctx)
        {
            Logger = logger;
        }

        public override string GetCancelUrl(PaymentProviderContext<TSettings> ctx)
        {
            ctx.Settings.MustNotBeNull("settings");
            ctx.Settings.CancelUrl.MustNotBeNull("settings.CancelUrl");

            return ctx.Settings.CancelUrl;
        }

        public override string GetContinueUrl(PaymentProviderContext<TSettings> ctx)
        {
            ctx.Settings.MustNotBeNull("settings");
            ctx.Settings.ContinueUrl.MustNotBeNull("settings.ContinueUrl");

            return ctx.Settings.ContinueUrl;
        }

        public override string GetErrorUrl(PaymentProviderContext<TSettings> ctx)
        {
            ctx.Settings.MustNotBeNull("settings");
            ctx.Settings.ErrorUrl.MustNotBeNull("settings.ErrorUrl");

            return ctx.Settings.ErrorUrl;
        }

        protected PaymentStatus GetPaymentStatus(Operation operation)
        {
            if (operation.Type == "authorize")
                return PaymentStatus.Authorized;

            if (operation.Type == "capture")
                return PaymentStatus.Captured;

            if (operation.Type == "refund")
                return PaymentStatus.Refunded;

            if (operation.Type == "cancel")
                return PaymentStatus.Cancelled;

            return PaymentStatus.Initialized;
        }

        protected string GetTransactionId(QuickpayPayment payment)
        {
            return payment?.Id.ToString();
        }

        protected string GetPaymentHash(string paymentId, string orderNumber, string currency, long amount)
        {
            return Base64Encode(paymentId + orderNumber + currency + amount);
        }

        protected QuickpayClientConfig GetQuickpayClientConfig(QuickpaySettingsBase settings)
        {
            var basicAuth = Base64Encode(":" + settings.ApiKey);

            return new QuickpayClientConfig
            {
                BaseUrl = "https://api.quickpay.net",
                Authorization = "Basic " + basicAuth
            };
        }
    }
}
