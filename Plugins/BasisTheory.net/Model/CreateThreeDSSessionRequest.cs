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
    /// CreateThreeDSSessionRequest
    /// </summary>
    [DataContract(Name = "CreateThreeDSSessionRequest")]
    public partial class CreateThreeDSSessionRequest : IEquatable<CreateThreeDSSessionRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateThreeDSSessionRequest" /> class.
        /// </summary>
        /// <param name="pan">pan.</param>
        /// <param name="tokenId">tokenId.</param>
        /// <param name="tokenIntentId">tokenIntentId.</param>
        /// <param name="type">type.</param>
        /// <param name="device">device.</param>
        /// <param name="deviceInfo">deviceInfo.</param>
        public CreateThreeDSSessionRequest(string pan = default(string), string tokenId = default(string), string tokenIntentId = default(string), string type = default(string), string device = default(string), ThreeDSDeviceInfo deviceInfo = default(ThreeDSDeviceInfo))
        {
            this.Pan = pan;
            this.TokenId = tokenId;
            this.TokenIntentId = tokenIntentId;
            this.Type = type;
            this.Device = device;
            this.DeviceInfo = deviceInfo;
        }

        /// <summary>
        /// Gets or Sets Pan
        /// </summary>
        [DataMember(Name = "pan", EmitDefaultValue = true)]
        [Obsolete]
        public string Pan { get; set; }

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
        /// Gets or Sets Type
        /// </summary>
        [DataMember(Name = "type", EmitDefaultValue = true)]
        public string Type { get; set; }

        /// <summary>
        /// Gets or Sets Device
        /// </summary>
        [DataMember(Name = "device", EmitDefaultValue = true)]
        public string Device { get; set; }

        /// <summary>
        /// Gets or Sets DeviceInfo
        /// </summary>
        [DataMember(Name = "device_info", EmitDefaultValue = false)]
        public ThreeDSDeviceInfo DeviceInfo { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class CreateThreeDSSessionRequest {\n");
            sb.Append("  Pan: ").Append(Pan).Append("\n");
            sb.Append("  TokenId: ").Append(TokenId).Append("\n");
            sb.Append("  TokenIntentId: ").Append(TokenIntentId).Append("\n");
            sb.Append("  Type: ").Append(Type).Append("\n");
            sb.Append("  Device: ").Append(Device).Append("\n");
            sb.Append("  DeviceInfo: ").Append(DeviceInfo).Append("\n");
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
            return this.Equals(input as CreateThreeDSSessionRequest);
        }

        /// <summary>
        /// Returns true if CreateThreeDSSessionRequest instances are equal
        /// </summary>
        /// <param name="input">Instance of CreateThreeDSSessionRequest to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(CreateThreeDSSessionRequest input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.Pan == input.Pan ||
                    (this.Pan != null &&
                    this.Pan.Equals(input.Pan))
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
                    this.Type == input.Type ||
                    (this.Type != null &&
                    this.Type.Equals(input.Type))
                ) && 
                (
                    this.Device == input.Device ||
                    (this.Device != null &&
                    this.Device.Equals(input.Device))
                ) && 
                (
                    this.DeviceInfo == input.DeviceInfo ||
                    (this.DeviceInfo != null &&
                    this.DeviceInfo.Equals(input.DeviceInfo))
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
                if (this.Pan != null)
                {
                    hashCode = (hashCode * 59) + this.Pan.GetHashCode();
                }
                if (this.TokenId != null)
                {
                    hashCode = (hashCode * 59) + this.TokenId.GetHashCode();
                }
                if (this.TokenIntentId != null)
                {
                    hashCode = (hashCode * 59) + this.TokenIntentId.GetHashCode();
                }
                if (this.Type != null)
                {
                    hashCode = (hashCode * 59) + this.Type.GetHashCode();
                }
                if (this.Device != null)
                {
                    hashCode = (hashCode * 59) + this.Device.GetHashCode();
                }
                if (this.DeviceInfo != null)
                {
                    hashCode = (hashCode * 59) + this.DeviceInfo.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
