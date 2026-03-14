using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Consts
        
        public const string PAYMENT_METHOD_TYPE_CREDIT_CARD = "card";
        public const string PAYMENT_METHOD_TYPE_PAYPAL      = "paypal";
        public const string PAYMENT_METHOD_TYPE_GOOGLE_PAY  = "google_pay";
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public string Type
        {
            get => Data.type;
            set => Data.type = value;       
        }
        public Shared.PaymentMethodDefinitionData Data;
        
        public Sprite IconSprite;
        public Sprite LogoSprite;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties
        
        public string                           DisplayName                     => Type.ToLower().Contains("paypal")     ? "PayPal"
                                                                                 : Type.ToLower().Contains("google_pay") ? "Google Pay"
                                                                                 : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Type.Replace("_", " ").ToLower());
        
        
        public string                           LocalID                         => $"local_pm_id_{this.Type}";
        
        public BonusItem                        BonusItem                       => GetBonusItem();
        
        public bool                             ShouldAddSavedUPMS              => true;
        public bool                             AllowOnlyOneUPM                 => true;
        public bool                             ShouldShowDropdown              => true;
        
        public IEnumerable<UserPaymentMethod>   SavedUPMS                       => CHECKOUT.PaymentMethods.UserPaymentMethods.Where(upm => upm.Type == this.Type);
        
        public bool                             ShouldDisplayemptyUPMIncheckout => Data?.display_empty_upm_in_checkout_page ?? false;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Transaction Steps
        
        public List<string> InitializationSteps  = new();
        public List<string> VaultingSteps        = new();
        public List<string> TransactionSteps     = new();
        public List<string> PostTransactionSteps = new();
     
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public PaymentMethodDefinition()
        {
            this.Data      = new();
            this.Data.type = "";
        }
        
        public Step Initialize() 
        =>
            new Step(name   : $"Initialize_method_definition_{this.DisplayName}"
                    ,action : async (s) =>
                    {
                        // string icon_url  = "https://www.shareicon.net/data/128x128/2015/03/17/8858_512x512_512x512.png";
                        // s.Log($"downloading icon from {icon_url}");
                        // this.IconSprite  = await DownloadImageAsync(icon_url);
                        // 
                        // string logo_url  = "https://epaypolicy.com/wp-content/uploads/2022/08/11.png";
                        // s.Log($"downloading logo from {logo_url}");
                        // this.LogoSprite  = await DownloadImageAsync(logo_url);            
                    });
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Payment Method Methods
        
        public virtual UserPaymentMethod CreateLocalUserPaymentMethod()
        {
            UserPaymentMethod userPaymentMethod = new()
                                                {
                                                    Data               = new UserPaymentMethodData()
                                                                       {
                                                                           type = this.Type,
                                                                           id   = this.LocalID,
                                                                       },
                                                    DisplayName        = this.DisplayName,
                                                    IsNewPaymentMethod = true,
                                                    IsSelected         = false,
                                                };
            
            return userPaymentMethod;
        }
        
        public UserPaymentMethod CreateEmptyPaymentMethod()
        {
            var type = this.Type;
            
            var upm = new UserPaymentMethod()
                    {
                        Data                    = new ()
                                                {
                                                   type = $"empty_{type}"
                                                },
                        DisplayName             = $"{this.DisplayName}",
                        IsNewPaymentMethod      = true,
                        IsSelected              = false,
                        SortOrder               = float.PositiveInfinity, 
                        LastSuccessfulUseTime   = DateTime.MinValue, 
                        Type                    = $"empty_{type}",
                        ButtonText              = $"{DisplayName}"
                    };
            
            upm.Node.Tags.Add("local");
            upm.Data.id = $"local_pm_id_{type}";
            
            return upm;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Methods
        
        public Sprite GetIconSprite()
        {
            string type = this.Type.ToLower();
            type = type.Replace("empty_", "");
            return CHECKOUT.Sprites.GetIconSprite(type);    
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helpers
        
        private async Task<Sprite> DownloadImageAsync(string url)
        {
            var sprite = await CheckoutClient.Instance.Resources.Sprites.LoadSprite(name_or_url: url);
            return sprite;
        }
        
        private BonusItem GetBonusItem()
        {
            var paymentMethodType = this.Type.ToLower();
            
            var first = CHECKOUT.Session?.BonusData?.FirstOrDefault(b => b.PaymentMethodType.ToLower() == this.Type.ToLower());
            if (first != null) return first;
            
            var @default = CHECKOUT.Session?.BonusData?.FirstOrDefault(b => b.PaymentMethodType.ToLower() == "default");
            if (@default != null) return @default;
            
            return null;
        }
    }
}


