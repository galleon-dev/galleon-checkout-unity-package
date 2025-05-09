/*
 * Basis Theory API
 *
 * ## Getting Started * Sign-in to [Basis Theory](https://basistheory.com) and go to [Applications](https://portal.basistheory.com/applications) * Create a Basis Theory Private Application * All permissions should be selected * Paste the API Key into the `BT-API-KEY` variable
 *
 * The version of the OpenAPI document: v1
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using OpenAPIDateConverter = BasisTheory.net.Client.OpenAPIDateConverter;

namespace BasisTheory.net.Model
{
    /// <summary>
    /// BankVerificationRequest
    /// </summary>
    [DataContract(Name = "BankVerificationRequest")]
    public partial class BankVerificationRequest : IEquatable<BankVerificationRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BankVerificationRequest" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected BankVerificationRequest() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="BankVerificationRequest" /> class.
        /// </summary>
        /// <param name="tokenId">tokenId (required).</param>
        /// <param name="countryCode">countryCode.</param>
        /// <param name="routingNumber">routingNumber.</param>
        public BankVerificationRequest(string tokenId = default(string), string countryCode = default(string), string routingNumber = default(string))
        {
            this.TokenId = tokenId;
            this.CountryCode = countryCode;
            this.RoutingNumber = routingNumber;
        }

        /// <summary>
        /// Gets or Sets TokenId
        /// </summary>
        [DataMember(Name = "token_id", IsRequired = true, EmitDefaultValue = true)]
        public string TokenId { get; set; }

        /// <summary>
        /// Gets or Sets CountryCode
        /// </summary>
        [DataMember(Name = "country_code", EmitDefaultValue = true)]
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or Sets RoutingNumber
        /// </summary>
        [DataMember(Name = "routing_number", EmitDefaultValue = true)]
        public string RoutingNumber { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class BankVerificationRequest {\n");
            sb.Append("  TokenId: ").Append(TokenId).Append("\n");
            sb.Append("  CountryCode: ").Append(CountryCode).Append("\n");
            sb.Append("  RoutingNumber: ").Append(RoutingNumber).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as BankVerificationRequest);
        }

        /// <summary>
        /// Returns true if BankVerificationRequest instances are equal
        /// </summary>
        /// <param name="input">Instance of BankVerificationRequest to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(BankVerificationRequest input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.TokenId == input.TokenId ||
                    (this.TokenId != null &&
                    this.TokenId.Equals(input.TokenId))
                ) && 
                (
                    this.CountryCode == input.CountryCode ||
                    (this.CountryCode != null &&
                    this.CountryCode.Equals(input.CountryCode))
                ) && 
                (
                    this.RoutingNumber == input.RoutingNumber ||
                    (this.RoutingNumber != null &&
                    this.RoutingNumber.Equals(input.RoutingNumber))
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                if (this.TokenId != null)
                {
                    hashCode = (hashCode * 59) + this.TokenId.GetHashCode();
                }
                if (this.CountryCode != null)
                {
                    hashCode = (hashCode * 59) + this.CountryCode.GetHashCode();
                }
                if (this.RoutingNumber != null)
                {
                    hashCode = (hashCode * 59) + this.RoutingNumber.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
