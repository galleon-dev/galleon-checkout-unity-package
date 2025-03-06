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
    /// ThreeDSAuthentication
    /// </summary>
    [DataContract(Name = "ThreeDSAuthentication")]
    public partial class ThreeDSAuthentication : IEquatable<ThreeDSAuthentication>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThreeDSAuthentication" /> class.
        /// </summary>
        /// <param name="panTokenId">panTokenId.</param>
        /// <param name="tokenId">tokenId.</param>
        /// <param name="tokenIntentId">tokenIntentId.</param>
        /// <param name="threedsVersion">threedsVersion.</param>
        /// <param name="acsTransactionId">acsTransactionId.</param>
        /// <param name="dsTransactionId">dsTransactionId.</param>
        /// <param name="sdkTransactionId">sdkTransactionId.</param>
        /// <param name="acsReferenceNumber">acsReferenceNumber.</param>
        /// <param name="dsReferenceNumber">dsReferenceNumber.</param>
        /// <param name="authenticationValue">authenticationValue.</param>
        /// <param name="authenticationStatus">authenticationStatus.</param>
        /// <param name="authenticationStatusCode">authenticationStatusCode.</param>
        /// <param name="authenticationStatusReason">authenticationStatusReason.</param>
        /// <param name="eci">eci.</param>
        /// <param name="acsChallengeMandated">acsChallengeMandated.</param>
        /// <param name="acsDecoupledAuthentication">acsDecoupledAuthentication.</param>
        /// <param name="authenticationChallengeType">authenticationChallengeType.</param>
        /// <param name="acsRenderingType">acsRenderingType.</param>
        /// <param name="acsSignedContent">acsSignedContent.</param>
        /// <param name="acsChallengeUrl">acsChallengeUrl.</param>
        /// <param name="challengeAttempts">challengeAttempts.</param>
        /// <param name="challengeCancelReason">challengeCancelReason.</param>
        /// <param name="cardholderInfo">cardholderInfo.</param>
        /// <param name="whitelistStatus">whitelistStatus.</param>
        /// <param name="whitelistStatusSource">whitelistStatusSource.</param>
        /// <param name="messageExtensions">messageExtensions.</param>
        public ThreeDSAuthentication(string panTokenId = default(string), string tokenId = default(string), string tokenIntentId = default(string), string threedsVersion = default(string), Guid? acsTransactionId = default(Guid?), Guid? dsTransactionId = default(Guid?), Guid? sdkTransactionId = default(Guid?), string acsReferenceNumber = default(string), string dsReferenceNumber = default(string), string authenticationValue = default(string), string authenticationStatus = default(string), string authenticationStatusCode = default(string), string authenticationStatusReason = default(string), string eci = default(string), string acsChallengeMandated = default(string), string acsDecoupledAuthentication = default(string), string authenticationChallengeType = default(string), ThreeDSAcsRenderingType acsRenderingType = default(ThreeDSAcsRenderingType), string acsSignedContent = default(string), string acsChallengeUrl = default(string), string challengeAttempts = default(string), string challengeCancelReason = default(string), string cardholderInfo = default(string), string whitelistStatus = default(string), string whitelistStatusSource = default(string), List<ThreeDSMessageExtension> messageExtensions = default(List<ThreeDSMessageExtension>))
        {
            this.PanTokenId = panTokenId;
            this.TokenId = tokenId;
            this.TokenIntentId = tokenIntentId;
            this.ThreedsVersion = threedsVersion;
            this.AcsTransactionId = acsTransactionId;
            this.DsTransactionId = dsTransactionId;
            this.SdkTransactionId = sdkTransactionId;
            this.AcsReferenceNumber = acsReferenceNumber;
            this.DsReferenceNumber = dsReferenceNumber;
            this.AuthenticationValue = authenticationValue;
            this.AuthenticationStatus = authenticationStatus;
            this.AuthenticationStatusCode = authenticationStatusCode;
            this.AuthenticationStatusReason = authenticationStatusReason;
            this.Eci = eci;
            this.AcsChallengeMandated = acsChallengeMandated;
            this.AcsDecoupledAuthentication = acsDecoupledAuthentication;
            this.AuthenticationChallengeType = authenticationChallengeType;
            this.AcsRenderingType = acsRenderingType;
            this.AcsSignedContent = acsSignedContent;
            this.AcsChallengeUrl = acsChallengeUrl;
            this.ChallengeAttempts = challengeAttempts;
            this.ChallengeCancelReason = challengeCancelReason;
            this.CardholderInfo = cardholderInfo;
            this.WhitelistStatus = whitelistStatus;
            this.WhitelistStatusSource = whitelistStatusSource;
            this.MessageExtensions = messageExtensions;
        }

        /// <summary>
        /// Gets or Sets PanTokenId
        /// </summary>
        [DataMember(Name = "pan_token_id", EmitDefaultValue = true)]
        [Obsolete]
        public string PanTokenId { get; set; }

        /// <summary>
        /// Gets or Sets TokenId
        /// </summary>
        [DataMember(Name = "token_id", EmitDefaultValue = true)]
        public string TokenId { get; set; }

        /// <summary>
        /// Gets or Sets TokenIntentId
        /// </summary>
        [DataMember(Name = "token_intent_id", EmitDefaultValue = true)]
        public string TokenIntentId { get; set; }

        /// <summary>
        /// Gets or Sets ThreedsVersion
        /// </summary>
        [DataMember(Name = "threeds_version", EmitDefaultValue = true)]
        public string ThreedsVersion { get; set; }

        /// <summary>
        /// Gets or Sets AcsTransactionId
        /// </summary>
        [DataMember(Name = "acs_transaction_id", EmitDefaultValue = true)]
        public Guid? AcsTransactionId { get; set; }

        /// <summary>
        /// Gets or Sets DsTransactionId
        /// </summary>
        [DataMember(Name = "ds_transaction_id", EmitDefaultValue = true)]
        public Guid? DsTransactionId { get; set; }

        /// <summary>
        /// Gets or Sets SdkTransactionId
        /// </summary>
        [DataMember(Name = "sdk_transaction_id", EmitDefaultValue = true)]
        public Guid? SdkTransactionId { get; set; }

        /// <summary>
        /// Gets or Sets AcsReferenceNumber
        /// </summary>
        [DataMember(Name = "acs_reference_number", EmitDefaultValue = true)]
        public string AcsReferenceNumber { get; set; }

        /// <summary>
        /// Gets or Sets DsReferenceNumber
        /// </summary>
        [DataMember(Name = "ds_reference_number", EmitDefaultValue = true)]
        public string DsReferenceNumber { get; set; }

        /// <summary>
        /// Gets or Sets AuthenticationValue
        /// </summary>
        [DataMember(Name = "authentication_value", EmitDefaultValue = true)]
        public string AuthenticationValue { get; set; }

        /// <summary>
        /// Gets or Sets AuthenticationStatus
        /// </summary>
        [DataMember(Name = "authentication_status", EmitDefaultValue = true)]
        public string AuthenticationStatus { get; set; }

        /// <summary>
        /// Gets or Sets AuthenticationStatusCode
        /// </summary>
        [DataMember(Name = "authentication_status_code", EmitDefaultValue = true)]
        public string AuthenticationStatusCode { get; set; }

        /// <summary>
        /// Gets or Sets AuthenticationStatusReason
        /// </summary>
        [DataMember(Name = "authentication_status_reason", EmitDefaultValue = true)]
        public string AuthenticationStatusReason { get; set; }

        /// <summary>
        /// Gets or Sets Eci
        /// </summary>
        [DataMember(Name = "eci", EmitDefaultValue = true)]
        public string Eci { get; set; }

        /// <summary>
        /// Gets or Sets AcsChallengeMandated
        /// </summary>
        [DataMember(Name = "acs_challenge_mandated", EmitDefaultValue = true)]
        public string AcsChallengeMandated { get; set; }

        /// <summary>
        /// Gets or Sets AcsDecoupledAuthentication
        /// </summary>
        [DataMember(Name = "acs_decoupled_authentication", EmitDefaultValue = true)]
        public string AcsDecoupledAuthentication { get; set; }

        /// <summary>
        /// Gets or Sets AuthenticationChallengeType
        /// </summary>
        [DataMember(Name = "authentication_challenge_type", EmitDefaultValue = true)]
        public string AuthenticationChallengeType { get; set; }

        /// <summary>
        /// Gets or Sets AcsRenderingType
        /// </summary>
        [DataMember(Name = "acs_rendering_type", EmitDefaultValue = false)]
        public ThreeDSAcsRenderingType AcsRenderingType { get; set; }

        /// <summary>
        /// Gets or Sets AcsSignedContent
        /// </summary>
        [DataMember(Name = "acs_signed_content", EmitDefaultValue = true)]
        public string AcsSignedContent { get; set; }

        /// <summary>
        /// Gets or Sets AcsChallengeUrl
        /// </summary>
        [DataMember(Name = "acs_challenge_url", EmitDefaultValue = true)]
        public string AcsChallengeUrl { get; set; }

        /// <summary>
        /// Gets or Sets ChallengeAttempts
        /// </summary>
        [DataMember(Name = "challenge_attempts", EmitDefaultValue = true)]
        public string ChallengeAttempts { get; set; }

        /// <summary>
        /// Gets or Sets ChallengeCancelReason
        /// </summary>
        [DataMember(Name = "challenge_cancel_reason", EmitDefaultValue = true)]
        public string ChallengeCancelReason { get; set; }

        /// <summary>
        /// Gets or Sets CardholderInfo
        /// </summary>
        [DataMember(Name = "cardholder_info", EmitDefaultValue = true)]
        public string CardholderInfo { get; set; }

        /// <summary>
        /// Gets or Sets WhitelistStatus
        /// </summary>
        [DataMember(Name = "whitelist_status", EmitDefaultValue = true)]
        public string WhitelistStatus { get; set; }

        /// <summary>
        /// Gets or Sets WhitelistStatusSource
        /// </summary>
        [DataMember(Name = "whitelist_status_source", EmitDefaultValue = true)]
        public string WhitelistStatusSource { get; set; }

        /// <summary>
        /// Gets or Sets MessageExtensions
        /// </summary>
        [DataMember(Name = "message_extensions", EmitDefaultValue = true)]
        public List<ThreeDSMessageExtension> MessageExtensions { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ThreeDSAuthentication {\n");
            sb.Append("  PanTokenId: ").Append(PanTokenId).Append("\n");
            sb.Append("  TokenId: ").Append(TokenId).Append("\n");
            sb.Append("  TokenIntentId: ").Append(TokenIntentId).Append("\n");
            sb.Append("  ThreedsVersion: ").Append(ThreedsVersion).Append("\n");
            sb.Append("  AcsTransactionId: ").Append(AcsTransactionId).Append("\n");
            sb.Append("  DsTransactionId: ").Append(DsTransactionId).Append("\n");
            sb.Append("  SdkTransactionId: ").Append(SdkTransactionId).Append("\n");
            sb.Append("  AcsReferenceNumber: ").Append(AcsReferenceNumber).Append("\n");
            sb.Append("  DsReferenceNumber: ").Append(DsReferenceNumber).Append("\n");
            sb.Append("  AuthenticationValue: ").Append(AuthenticationValue).Append("\n");
            sb.Append("  AuthenticationStatus: ").Append(AuthenticationStatus).Append("\n");
            sb.Append("  AuthenticationStatusCode: ").Append(AuthenticationStatusCode).Append("\n");
            sb.Append("  AuthenticationStatusReason: ").Append(AuthenticationStatusReason).Append("\n");
            sb.Append("  Eci: ").Append(Eci).Append("\n");
            sb.Append("  AcsChallengeMandated: ").Append(AcsChallengeMandated).Append("\n");
            sb.Append("  AcsDecoupledAuthentication: ").Append(AcsDecoupledAuthentication).Append("\n");
            sb.Append("  AuthenticationChallengeType: ").Append(AuthenticationChallengeType).Append("\n");
            sb.Append("  AcsRenderingType: ").Append(AcsRenderingType).Append("\n");
            sb.Append("  AcsSignedContent: ").Append(AcsSignedContent).Append("\n");
            sb.Append("  AcsChallengeUrl: ").Append(AcsChallengeUrl).Append("\n");
            sb.Append("  ChallengeAttempts: ").Append(ChallengeAttempts).Append("\n");
            sb.Append("  ChallengeCancelReason: ").Append(ChallengeCancelReason).Append("\n");
            sb.Append("  CardholderInfo: ").Append(CardholderInfo).Append("\n");
            sb.Append("  WhitelistStatus: ").Append(WhitelistStatus).Append("\n");
            sb.Append("  WhitelistStatusSource: ").Append(WhitelistStatusSource).Append("\n");
            sb.Append("  MessageExtensions: ").Append(MessageExtensions).Append("\n");
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
            return this.Equals(input as ThreeDSAuthentication);
        }

        /// <summary>
        /// Returns true if ThreeDSAuthentication instances are equal
        /// </summary>
        /// <param name="input">Instance of ThreeDSAuthentication to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ThreeDSAuthentication input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.PanTokenId == input.PanTokenId ||
                    (this.PanTokenId != null &&
                    this.PanTokenId.Equals(input.PanTokenId))
                ) && 
                (
                    this.TokenId == input.TokenId ||
                    (this.TokenId != null &&
                    this.TokenId.Equals(input.TokenId))
                ) && 
                (
                    this.TokenIntentId == input.TokenIntentId ||
                    (this.TokenIntentId != null &&
                    this.TokenIntentId.Equals(input.TokenIntentId))
                ) && 
                (
                    this.ThreedsVersion == input.ThreedsVersion ||
                    (this.ThreedsVersion != null &&
                    this.ThreedsVersion.Equals(input.ThreedsVersion))
                ) && 
                (
                    this.AcsTransactionId == input.AcsTransactionId ||
                    (this.AcsTransactionId != null &&
                    this.AcsTransactionId.Equals(input.AcsTransactionId))
                ) && 
                (
                    this.DsTransactionId == input.DsTransactionId ||
                    (this.DsTransactionId != null &&
                    this.DsTransactionId.Equals(input.DsTransactionId))
                ) && 
                (
                    this.SdkTransactionId == input.SdkTransactionId ||
                    (this.SdkTransactionId != null &&
                    this.SdkTransactionId.Equals(input.SdkTransactionId))
                ) && 
                (
                    this.AcsReferenceNumber == input.AcsReferenceNumber ||
                    (this.AcsReferenceNumber != null &&
                    this.AcsReferenceNumber.Equals(input.AcsReferenceNumber))
                ) && 
                (
                    this.DsReferenceNumber == input.DsReferenceNumber ||
                    (this.DsReferenceNumber != null &&
                    this.DsReferenceNumber.Equals(input.DsReferenceNumber))
                ) && 
                (
                    this.AuthenticationValue == input.AuthenticationValue ||
                    (this.AuthenticationValue != null &&
                    this.AuthenticationValue.Equals(input.AuthenticationValue))
                ) && 
                (
                    this.AuthenticationStatus == input.AuthenticationStatus ||
                    (this.AuthenticationStatus != null &&
                    this.AuthenticationStatus.Equals(input.AuthenticationStatus))
                ) && 
                (
                    this.AuthenticationStatusCode == input.AuthenticationStatusCode ||
                    (this.AuthenticationStatusCode != null &&
                    this.AuthenticationStatusCode.Equals(input.AuthenticationStatusCode))
                ) && 
                (
                    this.AuthenticationStatusReason == input.AuthenticationStatusReason ||
                    (this.AuthenticationStatusReason != null &&
                    this.AuthenticationStatusReason.Equals(input.AuthenticationStatusReason))
                ) && 
                (
                    this.Eci == input.Eci ||
                    (this.Eci != null &&
                    this.Eci.Equals(input.Eci))
                ) && 
                (
                    this.AcsChallengeMandated == input.AcsChallengeMandated ||
                    (this.AcsChallengeMandated != null &&
                    this.AcsChallengeMandated.Equals(input.AcsChallengeMandated))
                ) && 
                (
                    this.AcsDecoupledAuthentication == input.AcsDecoupledAuthentication ||
                    (this.AcsDecoupledAuthentication != null &&
                    this.AcsDecoupledAuthentication.Equals(input.AcsDecoupledAuthentication))
                ) && 
                (
                    this.AuthenticationChallengeType == input.AuthenticationChallengeType ||
                    (this.AuthenticationChallengeType != null &&
                    this.AuthenticationChallengeType.Equals(input.AuthenticationChallengeType))
                ) && 
                (
                    this.AcsRenderingType == input.AcsRenderingType ||
                    (this.AcsRenderingType != null &&
                    this.AcsRenderingType.Equals(input.AcsRenderingType))
                ) && 
                (
                    this.AcsSignedContent == input.AcsSignedContent ||
                    (this.AcsSignedContent != null &&
                    this.AcsSignedContent.Equals(input.AcsSignedContent))
                ) && 
                (
                    this.AcsChallengeUrl == input.AcsChallengeUrl ||
                    (this.AcsChallengeUrl != null &&
                    this.AcsChallengeUrl.Equals(input.AcsChallengeUrl))
                ) && 
                (
                    this.ChallengeAttempts == input.ChallengeAttempts ||
                    (this.ChallengeAttempts != null &&
                    this.ChallengeAttempts.Equals(input.ChallengeAttempts))
                ) && 
                (
                    this.ChallengeCancelReason == input.ChallengeCancelReason ||
                    (this.ChallengeCancelReason != null &&
                    this.ChallengeCancelReason.Equals(input.ChallengeCancelReason))
                ) && 
                (
                    this.CardholderInfo == input.CardholderInfo ||
                    (this.CardholderInfo != null &&
                    this.CardholderInfo.Equals(input.CardholderInfo))
                ) && 
                (
                    this.WhitelistStatus == input.WhitelistStatus ||
                    (this.WhitelistStatus != null &&
                    this.WhitelistStatus.Equals(input.WhitelistStatus))
                ) && 
                (
                    this.WhitelistStatusSource == input.WhitelistStatusSource ||
                    (this.WhitelistStatusSource != null &&
                    this.WhitelistStatusSource.Equals(input.WhitelistStatusSource))
                ) && 
                (
                    this.MessageExtensions == input.MessageExtensions ||
                    this.MessageExtensions != null &&
                    input.MessageExtensions != null &&
                    this.MessageExtensions.SequenceEqual(input.MessageExtensions)
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
                if (this.PanTokenId != null)
                {
                    hashCode = (hashCode * 59) + this.PanTokenId.GetHashCode();
                }
                if (this.TokenId != null)
                {
                    hashCode = (hashCode * 59) + this.TokenId.GetHashCode();
                }
                if (this.TokenIntentId != null)
                {
                    hashCode = (hashCode * 59) + this.TokenIntentId.GetHashCode();
                }
                if (this.ThreedsVersion != null)
                {
                    hashCode = (hashCode * 59) + this.ThreedsVersion.GetHashCode();
                }
                if (this.AcsTransactionId != null)
                {
                    hashCode = (hashCode * 59) + this.AcsTransactionId.GetHashCode();
                }
                if (this.DsTransactionId != null)
                {
                    hashCode = (hashCode * 59) + this.DsTransactionId.GetHashCode();
                }
                if (this.SdkTransactionId != null)
                {
                    hashCode = (hashCode * 59) + this.SdkTransactionId.GetHashCode();
                }
                if (this.AcsReferenceNumber != null)
                {
                    hashCode = (hashCode * 59) + this.AcsReferenceNumber.GetHashCode();
                }
                if (this.DsReferenceNumber != null)
                {
                    hashCode = (hashCode * 59) + this.DsReferenceNumber.GetHashCode();
                }
                if (this.AuthenticationValue != null)
                {
                    hashCode = (hashCode * 59) + this.AuthenticationValue.GetHashCode();
                }
                if (this.AuthenticationStatus != null)
                {
                    hashCode = (hashCode * 59) + this.AuthenticationStatus.GetHashCode();
                }
                if (this.AuthenticationStatusCode != null)
                {
                    hashCode = (hashCode * 59) + this.AuthenticationStatusCode.GetHashCode();
                }
                if (this.AuthenticationStatusReason != null)
                {
                    hashCode = (hashCode * 59) + this.AuthenticationStatusReason.GetHashCode();
                }
                if (this.Eci != null)
                {
                    hashCode = (hashCode * 59) + this.Eci.GetHashCode();
                }
                if (this.AcsChallengeMandated != null)
                {
                    hashCode = (hashCode * 59) + this.AcsChallengeMandated.GetHashCode();
                }
                if (this.AcsDecoupledAuthentication != null)
                {
                    hashCode = (hashCode * 59) + this.AcsDecoupledAuthentication.GetHashCode();
                }
                if (this.AuthenticationChallengeType != null)
                {
                    hashCode = (hashCode * 59) + this.AuthenticationChallengeType.GetHashCode();
                }
                if (this.AcsRenderingType != null)
                {
                    hashCode = (hashCode * 59) + this.AcsRenderingType.GetHashCode();
                }
                if (this.AcsSignedContent != null)
                {
                    hashCode = (hashCode * 59) + this.AcsSignedContent.GetHashCode();
                }
                if (this.AcsChallengeUrl != null)
                {
                    hashCode = (hashCode * 59) + this.AcsChallengeUrl.GetHashCode();
                }
                if (this.ChallengeAttempts != null)
                {
                    hashCode = (hashCode * 59) + this.ChallengeAttempts.GetHashCode();
                }
                if (this.ChallengeCancelReason != null)
                {
                    hashCode = (hashCode * 59) + this.ChallengeCancelReason.GetHashCode();
                }
                if (this.CardholderInfo != null)
                {
                    hashCode = (hashCode * 59) + this.CardholderInfo.GetHashCode();
                }
                if (this.WhitelistStatus != null)
                {
                    hashCode = (hashCode * 59) + this.WhitelistStatus.GetHashCode();
                }
                if (this.WhitelistStatusSource != null)
                {
                    hashCode = (hashCode * 59) + this.WhitelistStatusSource.GetHashCode();
                }
                if (this.MessageExtensions != null)
                {
                    hashCode = (hashCode * 59) + this.MessageExtensions.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
