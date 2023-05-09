using Newtonsoft.Json;

namespace Umbraco.Commerce.PaymentProviders.Quickpay.Api.Models
{
    public class PaymentLinkUrl
    {
        /// <summary>
        /// Url to payment window for this payment link
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
