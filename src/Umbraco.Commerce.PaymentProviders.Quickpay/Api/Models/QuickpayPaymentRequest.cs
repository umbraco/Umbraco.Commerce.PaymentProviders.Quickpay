using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Umbraco.Commerce.PaymentProviders.Quickpay.Api.Models
{
    public class QuickpayPaymentRequest
    {
        /// <summary>
        /// Unique order id (must be between 4-20 characters).
        /// </summary>
        [JsonPropertyName("order_id")]
        public string OrderId { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Custom variables
        /// </summary>
        [JsonPropertyName("variables")]
        public Dictionary<string, string> Variables { get; set; }
    }
}
