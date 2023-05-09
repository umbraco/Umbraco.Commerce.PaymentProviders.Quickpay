using Newtonsoft.Json;
using System.Collections.Generic;

namespace Umbraco.Commerce.PaymentProviders.Quickpay.Api.Models
{
    public class MetaData
    {
        /// <summary>
        /// Type (card, mobile, nin)
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Origin of this transaction or card. If set, describes where it came from.
        /// </summary>
        [JsonProperty("origin")]
        public string Origin { get; set; }

        /// <summary>
        /// Card type only: The card brand
        /// </summary>
        [JsonProperty("brand")]
        public string Brand { get; set; }

        /// <summary>
        /// Card type only: Card BIN
        /// </summary>
        [JsonProperty("bin")]
        public string Bin { get; set; }

        /// <summary>
        /// Card type only: Corporate status
        /// </summary>
        [JsonProperty("corporate")]
        public string Corporate { get; set; }

        /// <summary>
        /// Card type only: The last 4 digits of the card number
        /// </summary>
        [JsonProperty("last4")]
        public string Last4 { get; set; }

        /// <summary>
        /// Card type only: The expiration month
        /// </summary>
        [JsonProperty("exp_month")]
        public int? ExpMonth { get; set; }

        /// <summary>
        /// Card type only: The expiration year
        /// </summary>
        [JsonProperty("exp_year")]
        public int? ExpYear { get; set; }

        /// <summary>
        /// Card type only: The card country in ISO 3166-1 alpha-3
        /// </summary>
        [JsonProperty("country")]
        public string Country { get; set; }

        /// <summary>
        /// Card type only: Verified via 3D-Secure
        /// </summary>
        [JsonProperty("is_3d_secure")]
        public bool? Is3dSecure { get; set; }

        /// <summary>
        /// Name of cardholder
        /// </summary>
        [JsonProperty("issued_to")]
        public string IssuedTo { get; set; }

        /// <summary>
        /// Card type only: PCI safe hash of card number
        /// </summary>
        [JsonProperty("hash")]
        public string Hash { get; set; }

        /// <summary>
        /// Mobile type only: The mobile number
        /// </summary>
        [JsonProperty("number")]
        public object Number { get; set; }

        /// <summary>
        /// Customer IP
        /// </summary>
        [JsonProperty("customer_ip")]
        public string CustomerIp { get; set; }

        /// <summary>
        /// Customer country based on IP geo-data, ISO 3166-1 alpha-2
        /// </summary>
        [JsonProperty("customer_country")]
        public string CustomerCountry { get; set; }

        /// <summary>
        /// Suspected fraud
        /// </summary>
        [JsonProperty("fraud_suspected")]
        public bool FraudSuspected { get; set; }

        /// <summary>
        /// Fraud remarks
        /// </summary>
        [JsonProperty("fraud_remarks")]
        public List<object> FraudRemarks { get; set; }

        /// <summary>
        /// Reported as fraudulent
        /// </summary>
        [JsonProperty("fraud_reported")]
        public bool FraudReported { get; set; }

        /// <summary>
        /// Fraud report date
        /// </summary>
        [JsonProperty("fraud_reported_at")]
        public string FraudReportedAt { get; set; }

        /// <summary>
        /// NIN type only. NIN number
        /// </summary>
        [JsonProperty("nin_number")]
        public string NinNumber { get; set; }

        /// <summary>
        /// NIN type only. NIN country code, ISO 3166-1 alpha-3
        /// </summary>
        [JsonProperty("nin_country_code")]
        public string NinCountryCode { get; set; }

        /// <summary>
        /// NIN type only. NIN gender
        /// </summary>
        [JsonProperty("nin_gender")]
        public string NinGender { get; set; }
    }
}
