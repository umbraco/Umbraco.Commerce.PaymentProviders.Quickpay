using Newtonsoft.Json;

namespace Umbraco.Commerce.PaymentProviders.Quickpay.Api.Models
{
    public class Operation
    {
        /// <summary>
        /// Operation ID
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Type of operation
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Amount
        /// </summary>
        [JsonProperty("amount")]
        public int Amount { get; set; }

        /// <summary>
        /// If the operation is pending
        /// </summary>
        [JsonProperty("pending")]
        public bool Pending { get; set; }

        /// <summary>
        /// Quickpay status code
        /// </summary>
        [JsonProperty("qp_status_code")]
        public string QuickpayStatusCode { get; set; }

        /// <summary>
        /// Quickpay status message
        /// </summary>
        [JsonProperty("qp_status_msg")]
        public string QuickpayStatusMessage { get; set; }

        /// <summary>
        /// Acquirer status code
        /// </summary>
        [JsonProperty("aq_status_code")]
        public string AcquirerStatusCode { get; set; }

        /// <summary>
        /// Acquirer status message
        /// </summary>
        [JsonProperty("aq_status_msg")]
        public string AcquirerStatusMessage { get; set; }
    }
}
