using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Galleon.Checkout.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Galleon.Checkout
{
    public class PaymentMethodDefinition : Entity
    {
        //// Consts
        
        public const string PAYMENT_METHOD_TYPE_CREDIT_CARD = "card";
        public const string PAYMENT_METHOD_TYPE_PAYPAL      = "paypal";
        public const string PAYMENT_METHOD_TYPE_GOOGLE_PAY  = "google_pay";
        
        //// Members
        
        public string                             Type;
        public Shared.PaymentMethodDefinitionData Data;
        
        public Sprite IconSprite;
        public Sprite LogoSprite;
        
        //// Properties
        
        public string DisplayName => Data?.type ?? Type.ToString();
        
        //// Transaction Steps
        
        public List<string> InitializationSteps  = new();
        public List<string> VaultingSteps        = new();
        public List<string> TransactionSteps     = new();
        public List<string> PostTransactionSteps = new();
     
        //// Lifecycle
        
        public Step Initialize() 
        =>
            new Step(name   : $"Initialize_method_definition_{this.DisplayName}"
                    ,action : async (s) =>
                    {
                        string icon_url  = "https://www.shareicon.net/data/128x128/2015/03/17/8858_512x512_512x512.png";
                        s.Log($"downloading icon from {icon_url}");
                        this.IconSprite  = await DownloadImageAsync(icon_url);
                        
                        string logo_url  = "https://epaypolicy.com/wp-content/uploads/2022/08/11.png";
                        s.Log($"downloading logo from {logo_url}");
                        this.LogoSprite  = await DownloadImageAsync(logo_url);            
                    });
        
        //// Helpers
        
        private async Task<Sprite> DownloadImageAsync(string url)
        {
            var sprite = await CheckoutClient.Instance.ResourceManager.LoadSprite(name_or_url: url);
            return sprite;
        }
    }
}


/// {
///     "definitions":
///     [
///         {
///             "type"                      : "web_checkout",
///             "providers"                 :
///                                         [
///                                             {
///                                                 "provider"  : "custom",
///                                                 "config"    :
///                                                             {
///                                                                 "flow" : "checkout_link"
///                                                             }
///                                             }
///                                         ],
///             "icon_url"                  : "",
///             "logo_url"                  : "",
///             "initialization_actions"    : null,
///             "vaulting_actions"          : null,
///             "charge_actions"            :
///                                         [
///                                             {
///                                                 "action"        : "charge",
///                                                 "parameters"    : {}
///                                             }
///                                         ]
///         },
///         {
///             "type"                      : "test",
///             "providers"                 :
///                                         [
///                                             {
///                                                 "provider"    : "stripe",
///                                                 "config"      :
///                                                               {
///                                                                   "method": "card"
///                                                               }
///                                             }
///                                         ],
///             "icon_url"                  : "",
///             "logo_url"                  : "",
///             "supported_card_types"      :
///                                         [
///                                             "visa",
///                                             "master_card",
///                                             "american_express"
///                                         ],
///             "initialization_actions"    : null,
///             "vaulting_actions"          :
///                                         [
///                                             {
///                                               "action"      : "get_tokenizer",
///                                               "parameters"  : {}
///                                             },
///                                             {
///                                               "action"      : "tokenize",
///                                               "parameters"  : {}
///                                             }
///                                         ],
///             "charge_actions"            :
///                                         [
///                                             {
///                                               "action"      : "charge",
///                                               "parameters"  : {}
///                                             }
///                                         ]
///         }
///     ]
/// }

