using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;

namespace Umbraco.Commerce.PaymentProviders.Quickpay.Api.Models
{
    public class QuickpayPayment
    {
        /// <summary>
        /// Id
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Merchant id
        /// </summary>
        [JsonPropertyName("merchant_id")]
        public int MerchantId { get; set; }

        /// <summary>
        /// Order id/number
        /// </summary>
        [JsonPropertyName("order_id")]
        public string OrderId { get; set; }

        /// <summary>
        /// Accepted by acquirer
        /// </summary>
        [JsonPropertyName("accepted")]
        public bool Accepted { get; set; }

        /// <summary>
        /// Transaction type
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// State of transaction
        /// </summary>
        [JsonPropertyName("state")]
        public string State { get; set; }

        /// <summary>
        /// Operations
        /// </summary>
        [JsonPropertyName("operations")]
        public List<Operation> Operations { get; set; }

        /// <summary>
        /// Variables
        /// </summary>
        [JsonPropertyName("variables")]
        public Dictionary<string, string> Variables { get; set; }

        /// <summary>
        /// Metadata
        /// </summary>
        [JsonPropertyName("metadata")]
        public MetaData MetaData { get; set; }

        /// <summary>
        /// Payment link
        /// </summary>
        [JsonPropertyName("link")]
        public PaymentLink Link { get; set; }

        /// <summary>
        /// Test mode
        /// </summary>
        [JsonPropertyName("test_mode")]
        public bool TestMode { get; set; }

        /// <summary>
        /// Acquirer that processed the transaction
        /// </summary>
        [JsonPropertyName("acquirer")]
        public string Acquirer { get; set; }

        /// <summary>
        /// Balance
        /// </summary>
        [JsonPropertyName("balance")]
        public int Balance { get; set; }

        /// <summary>
        /// Fee added to authorization amount (only relevant on auto-fee)
        /// </summary>
        [JsonPropertyName("fee")]
        public int? Fee { get; set; }

        /// <summary>
        /// Timestamp of creation
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp of last updated
        /// </summary>
        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Timestamp of retention
        /// </summary>
        [JsonPropertyName("retented_at")]
        public DateTime? RetentedAt { get; set; }

        /// <summary>
        /// Authorize deadline
        /// </summary>
        [JsonPropertyName("deadline_at")]
        public DateTime? DeadlineAt { get; set; }

        /// <summary>
        /// Parent subscription id (only recurring)
        /// </summary>
        [JsonPropertyName("subscription_id")]
        public int? SubscriptionId { get; set; }
    }
}
