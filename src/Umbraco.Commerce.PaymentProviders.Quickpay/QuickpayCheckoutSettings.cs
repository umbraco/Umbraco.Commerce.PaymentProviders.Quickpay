using Umbraco.Commerce.Core.PaymentProviders;

namespace Umbraco.Commerce.PaymentProviders.Quickpay
{
    public class QuickpayCheckoutSettings : QuickpaySettingsBase
    {
        [PaymentProviderSetting(Name = "Auto Fee",
            Description = "Flag indicating whether to automatically calculate and apply the fee from the acquirer.",
            SortOrder = 1100)]
        public bool AutoFee { get; set; }

        [PaymentProviderSetting(Name = "Auto Capture",
            Description = "Flag indicating whether to immediately capture the payment, or whether to just authorize the payment for later (manual) capture.",
            SortOrder = 1200)]
        public bool AutoCapture { get; set; }

        [PaymentProviderSetting(Name = "Framed",
            Description = "Flag indicating whether to allow opening payment page in iframe.",
            SortOrder = 1300)]
        public bool Framed { get; set; }
    }
}
