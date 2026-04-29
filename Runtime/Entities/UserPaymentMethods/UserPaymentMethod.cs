using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Galleon.Checkout.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Galleon.Checkout
{
    public class UserPaymentMethod : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public UserPaymentMethodData Data = new();
        
        public string Type
        {
            get => Data.type;
            set => Data.type = value;
        }
        
        public string DisplayType => Type.Replace("empty_","");
        
        public string                ID => this.Data?.id ?? "";
        
        public string                DisplayName;
        public bool                  IsSelected;
        
        public float                 SortOrder = 1f;
        
        public bool                  IsNewPaymentMethod      = false;
        public bool                  ShouldSavePaymentMethod = false;
        
        public string                ButtonText              = null;

        public DateTime              LastSuccessfulUseTime   = DateTime.MinValue;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Actions
        
        public void Select()
        {
            this.IsSelected = true;
        }
        public void SelectExclusive()
        {
            CHECKOUT.PaymentMethods.UserPaymentMethods.ForEach(x => x.Unselect());
            this.IsSelected = true;
        }
        
        public void Unselect()
        {
            this.IsSelected = false;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Vaulting
        
        public List<Step> GetVaultingSteps()
        {
            List<string> definitions = GetPaymentMethodDefinition().Data.vaulting_actions.Select(x => x.action).ToList();
            List<Step>   steps       = CheckoutClient.Instance.CheckoutActions.Node.Descendants().SelectMany(x => x.Node.Reflection.Steps().Where(s => definitions.Contains(s.Name))).ToList();
            return steps;
        }
        
        public virtual Step RunVaultingSteps() 
        =>
            new Step(name   : $"run_vaulting_steps"
                    ,action : async (s) =>
                    {
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Transaction Steps
        
        public PaymentMethodDefinition GetPaymentMethodDefinition()
        {
            try
            {
                string myType = this.Type;
                myType        = myType.Replace("empty_", "");
                return CHECKOUT.PaymentMethods.PaymentMethodsDefinitions.FirstOrDefault(x => x.Type == myType);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }
        
        public List<Step> GetTransactionSteps()
        {
            List<string> definitions = GetPaymentMethodDefinition().Data.charge_actions.Select(x => x.action).ToList();
            List<Step>   steps       = CheckoutClient.Instance.CheckoutActions.Node.Descendants().SelectMany(x => x.Node.Reflection.Steps().Where(s => definitions.Contains(s.Name))).ToList();
            return steps;
        }
        
        public List<PaymentAction> GetTransactionPaymentActions()
        {
            return default;
        }
        
        public Dictionary<string, object> GetDataForCharge()
        {
            Dictionary<string, object> data       = new();
            var                        definition = this.GetPaymentMethodDefinition();
            var                        providers  = definition.Data.providers;
            
            foreach (var provider in providers)
                data.Add("provider", provider);
            
            if (this is CreditCardUserUserPaymentMethod cc)
            {
                data.Add("type", "card");
                data.Add("token", cc.TokenID);
            }
            
            return data;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Storage
        
        public struct StorageData
        {
            public string   id;
            public string   type;
            public DateTime lastSuccessfulUseTime;
        }
        
        public void SaveData()
        {
            StorageData data = new()
            {
                id                    = this.ID,
                type                  = this.Type,
                lastSuccessfulUseTime = this.LastSuccessfulUseTime
            };
            
            string json = JsonConvert.SerializeObject(data);
            CHECKOUT.Storage.Write(this.ID, json);
        }
        
        public void LoadData()
        {
            string json = CHECKOUT.Storage.Read<string>(this.ID);
            if (string.IsNullOrEmpty(json)) return;
            
            StorageData data = JsonConvert.DeserializeObject<StorageData>(json);
            this.LastSuccessfulUseTime = data.lastSuccessfulUseTime;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Helper Methods
        
        public string GetPaymentMethodTypeeActual()
        {
            string result;
            if (this is CreditCardUserUserPaymentMethod cc)
                result = cc.CreditCardType.ToLower();
            else 
                result = this.Type.ToLower();
            
            result = result.Replace("empty_", "");
            
            return result;
        }
        
        public Sprite GetIconSprite()
        {
            if (this is CreditCardUserUserPaymentMethod)
                return CHECKOUT.Sprites.GetIconSprite(GetPaymentMethodTypeeActual());
            
            var definition = GetPaymentMethodDefinition();
            if (definition == null) 
                return CHECKOUT.Sprites.GetIconSprite(GetPaymentMethodTypeeActual());
            return definition?.GetIconSprite();
        }
        
        public bool HasNonEmptyButtonSprite()
        {
            
            if (this is CreditCardUserUserPaymentMethod)
                return true;
            
            var definition = GetPaymentMethodDefinition();
            if (definition == null) definition = CHECKOUT.PaymentMethods.PaymentMethodsDefinitions.FirstOrDefault(x => x.Type == "card");
            return definition.GetButtonprite() != CHECKOUT.Sprites.CheckoutButtonSprite;
        }
        
        public Sprite GetButtonSprite()
        {
            if (this is CreditCardUserUserPaymentMethod)
                return CHECKOUT.Sprites.GetButtonSprite(GetPaymentMethodTypeeActual());
            
            var definition = GetPaymentMethodDefinition();
            if (definition == null) 
                return CHECKOUT.Sprites.GetButtonSprite(GetPaymentMethodTypeeActual());
            
            return definition.GetButtonprite();
        }
    }
}

