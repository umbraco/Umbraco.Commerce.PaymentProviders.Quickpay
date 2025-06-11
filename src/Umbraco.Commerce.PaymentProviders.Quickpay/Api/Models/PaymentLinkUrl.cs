using System.Text.Json.Serialization;

namespace Umbraco.Commerce.PaymentProviders.Quickpay.Api.Models
{
    public class PaymentLinkUrl
    {
        /// <summary>
        /// Url to payment window for this payment link
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
