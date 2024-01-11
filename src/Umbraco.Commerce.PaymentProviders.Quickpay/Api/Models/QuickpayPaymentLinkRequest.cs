using System.Text.Json.Serialization;

namespace Umbraco.Commerce.PaymentProviders.Quickpay.Api.Models
{
    public class QuickpayPaymentLinkRequest
    {
        /// <summary>
        /// Amount to authorize
        /// </summary>
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Language
        /// </summary>
        [JsonPropertyName("language")]
        public string Language { get; set; }

        /// <summary>
        /// URL that cardholder is redirected to after authorize.
        /// </summary>
        [JsonPropertyName("continue_url")]
        public string ContinueUrl { get; set; }

        /// <summary>
        /// URL that cardholder is redirected to after cancelation.
        /// </summary>
        [JsonPropertyName("cancel_url")]
        public string CancelUrl { get; set; }

        /// <summary>
        /// Endpoint for async callback.
        /// </summary>
        [JsonPropertyName("callback_url")]
        public string CallbackUrl { get; set; }

        /// <summary>
        /// Limit payment methods.
        /// </summary>
        [JsonPropertyName("payment_methods")]
        public string PaymentMethods { get; set; }

        /// <summary>
        /// Add acquirer fee to amount. Default is merchant autofee.
        /// </summary>
        [JsonPropertyName("auto_fee")]
        public bool? AutoFee { get; set; }

        /// <summary>
        /// When true, payment is captured after authorization. Default is false.
        /// </summary>
        [JsonPropertyName("auto_capture")]
        public bool? AutoCapture { get; set; }

        /// <summary>
        /// Allow opening in iframe. Default is false.
        /// </summary>
        [JsonPropertyName("framed")]
        public bool? Framed { get; set; }
    }
}
