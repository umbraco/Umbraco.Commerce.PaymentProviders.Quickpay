using Umbraco.Commerce.Core.PaymentProviders;

namespace Umbraco.Commerce.PaymentProviders.Quickpay
{
    public class QuickpayCheckoutSettings : QuickpaySettingsBase
    {
        [PaymentProviderSetting(
            SortOrder = 1100)]
        public bool AutoFee { get; set; }

        [PaymentProviderSetting(
            SortOrder = 1200)]
        public bool AutoCapture { get; set; }

        [PaymentProviderSetting(
            SortOrder = 1300)]
        public bool Framed { get; set; }
    }
}
