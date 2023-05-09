using Umbraco.Commerce.Core.PaymentProviders;

namespace Umbraco.Commerce.PaymentProviders.Quickpay
{
    public class QuickpaySettingsBase
    {
        [PaymentProviderSetting(Name = "Continue URL",
            Description = "The URL to continue to after this provider has done processing. eg: /continue/",
            SortOrder = 100)]
        public string ContinueUrl { get; set; }

        [PaymentProviderSetting(Name = "Cancel URL",
            Description = "The URL to return to if the payment attempt is canceled. eg: /cancel/",
            SortOrder = 200)]
        public string CancelUrl { get; set; }

        [PaymentProviderSetting(Name = "Error URL",
            Description = "The URL to return to if the payment attempt errors. eg: /error/",
            SortOrder = 300)]
        public string ErrorUrl { get; set; }

        [PaymentProviderSetting(Name = "API Key",
            Description = "API Key from the Quickpay administration portal.",
            SortOrder = 400)]
        public string ApiKey { get; set; }

        [PaymentProviderSetting(Name = "Private Key",
            Description = "Private Key from the Quickpay administration portal.",
            SortOrder = 500)]
        public string PrivateKey { get; set; }

        [PaymentProviderSetting(Name = "Merchant ID",
            Description = "Merchant ID supplied by Quickpay during registration.",
            SortOrder = 600)]
        public string MerchantId { get; set; }

        [PaymentProviderSetting(Name = "Agreement ID",
            Description = "Agreement ID supplied by Quickpay during registration.",
            SortOrder = 700)]
        public string AgreemendId { get; set; }

        [PaymentProviderSetting(Name = "Language",
            Description = "The language of the payment portal to display.",
            SortOrder = 900)]
        public string Lang { get; set; }

        [PaymentProviderSetting(Name = "Accepted Payment Methods",
            Description = "A comma separated list of Payment Methods to accept. To use negation just put a \"!\" in front the those you do not wish to accept.",
            SortOrder = 1000)]
        public string PaymentMethods { get; set; }
    }
}
