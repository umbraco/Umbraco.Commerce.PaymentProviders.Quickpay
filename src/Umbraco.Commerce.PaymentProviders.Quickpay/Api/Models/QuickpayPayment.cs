using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Umbraco.Commerce.PaymentProviders.Quickpay.Api.Models
{
    public class QuickpayPayment
    {
        /// <summary>
        /// Id
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Merchant id
        /// </summary>
        [JsonProperty("merchant_id")]
        public int MerchantId { get; set; }

        /// <summary>
        /// Order id/number
        /// </summary>
        [JsonProperty("order_id")]
        public string OrderId { get; set; }

        /// <summary>
        /// Accepted by acquirer
        /// </summary>
        [JsonProperty("accepted")]
        public bool Accepted { get; set; }

        /// <summary>
        /// Transaction type
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// State of transaction
        /// </summary>
        [JsonProperty("state")]
        public string State { get; set; }

        /// <summary>
        /// Operations
        /// </summary>
        [JsonProperty("operations")]
        public List<Operation> Operations { get; set; }

        /// <summary>
        /// Variables
        /// </summary>
        [JsonProperty("variables")]
        public Dictionary<string, string> Variables { get; set; }

        /// <summary>
        /// Metadata
        /// </summary>
        [JsonProperty("metadata")]
        public MetaData MetaData { get; set; }

        /// <summary>
        /// Payment link
        /// </summary>
        [JsonProperty("link")]
        public PaymentLink Link { get; set; }

        /// <summary>
        /// Test mode
        /// </summary>
        [JsonProperty("test_mode")]
        public bool TestMode { get; set; }

        /// <summary>
        /// Acquirer that processed the transaction
        /// </summary>
        [JsonProperty("acquirer")]
        public string Acquirer { get; set; }

        /// <summary>
        /// Balance
        /// </summary>
        [JsonProperty("balance")]
        public int Balance { get; set; }

        /// <summary>
        /// Fee added to authorization amount (only relevant on auto-fee)
        /// </summary>
        [JsonProperty("fee")]
        public int? Fee { get; set; }

        /// <summary>
        /// Timestamp of creation
        /// </summary>
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp of last updated
        /// </summary>
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Timestamp of retention
        /// </summary>
        [JsonProperty("retented_at")]
        public DateTime? RetentedAt { get; set; }

        /// <summary>
        /// Authorize deadline
        /// </summary>
        [JsonProperty("deadline_at")]
        public DateTime? DeadlineAt { get; set; }

        /// <summary>
        /// Parent subscription id (only recurring)
        /// </summary>
        [JsonProperty("subscription_id")]
        public int? SubscriptionId { get; set; }
    }
}
