using System.Text.Json.Serialization;

namespace Umbraco.Commerce.PaymentProviders.Quickpay.Api.Models
{
    public class PaymentLink : PaymentLinkUrl
    {
        /// <summary>
        /// Id of agreement that will be used in the payment window
        /// </summary>
        [JsonPropertyName("agreement_id")]
        public int AgreementId { get; set; }

        /// <summary>
        /// Two letter language code that determines the language of the payment window
        /// </summary>
        [JsonPropertyName("language")]
        public string Language { get; set; }

        /// <summary>
        /// Amount to authorize
        /// </summary>
        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        /// <summary>
        /// Where cardholder is redirected after success
        /// </summary>
        [JsonPropertyName("continue_url")]
        public string ContinueUrl { get; set; }

        /// <summary>
        /// Where cardholder is redirected after cancel
        /// </summary>
        [JsonPropertyName("cancel_url")]
        public string CancelUrl { get; set; }

        /// <summary>
        /// Endpoint for a POST callback
        /// </summary>
        [JsonPropertyName("callback_url")]
        public string CallbackUrl { get; set; }

        /// <summary>
        /// Lock to these payment methods
        /// </summary>
        [JsonPropertyName("payment_methods")]
        public string PaymentMethods { get; set; }

        /// <summary>
        /// If true, will add acquirer fee to the amount
        /// </summary>
        [JsonPropertyName("auto_fee")]
        public bool? AutoFee { get; set; }

        /// <summary>
        /// If true, will capture the transaction after authorize succeeds
        /// </summary>
        [JsonPropertyName("auto_capture")]
        public bool? AutoCapture { get; set; }
    }
}
