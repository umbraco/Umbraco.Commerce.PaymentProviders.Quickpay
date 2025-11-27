using System.Text.Json.Serialization;

namespace Umbraco.Commerce.PaymentProviders.Quickpay.Api.Models
{
    public class Operation
    {
        /// <summary>
        /// Operation ID
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Type of operation
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// Amount
        /// </summary>
        [JsonPropertyName("amount")]
        public int? Amount { get; set; }

        /// <summary>
        /// If the operation is pending
        /// </summary>
        [JsonPropertyName("pending")]
        public bool Pending { get; set; }

        /// <summary>
        /// Quickpay status code
        /// </summary>
        [JsonPropertyName("qp_status_code")]
        public string QuickpayStatusCode { get; set; }

        /// <summary>
        /// Quickpay status message
        /// </summary>
        [JsonPropertyName("qp_status_msg")]
        public string QuickpayStatusMessage { get; set; }

        /// <summary>
        /// Acquirer status code
        /// </summary>
        [JsonPropertyName("aq_status_code")]
        public string AcquirerStatusCode { get; set; }

        /// <summary>
        /// Acquirer status message
        /// </summary>
        [JsonPropertyName("aq_status_msg")]
        public string AcquirerStatusMessage { get; set; }
    }
}
