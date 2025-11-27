using Umbraco.Commerce.Core.PaymentProviders;

namespace Umbraco.Commerce.PaymentProviders.Quickpay
{
    public class QuickpaySettingsBase
    {
        [PaymentProviderSetting(
            SortOrder = 100)]
        public string ContinueUrl { get; set; }

        [PaymentProviderSetting(
            SortOrder = 200)]
        public string CancelUrl { get; set; }

        [PaymentProviderSetting(
            SortOrder = 300)]
        public string ErrorUrl { get; set; }

        [PaymentProviderSetting(
            SortOrder = 400)]
        public string ApiKey { get; set; }

        [PaymentProviderSetting(
            SortOrder = 500)]
        public string PrivateKey { get; set; }

        [PaymentProviderSetting(
            SortOrder = 600)]
        public string MerchantId { get; set; }

        [PaymentProviderSetting(
            SortOrder = 700)]
        public string AgreemendId { get; set; }

        [PaymentProviderSetting(
            SortOrder = 900)]
        public string Lang { get; set; }

        [PaymentProviderSetting(
            SortOrder = 1000)]
        public string PaymentMethods { get; set; }
    }
}
