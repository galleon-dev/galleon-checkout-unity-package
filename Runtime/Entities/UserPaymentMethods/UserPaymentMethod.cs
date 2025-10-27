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
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Types
                
        public class BonusData
        {
            public string displayText;
            public string reward_type;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public UserPaymentMethodData Data = new();
        
        public string                Type;
        
        public string                ID => this.Data?.id ?? "";
        
        public string                DisplayName;
        public bool                  IsSelected;
        
        public float                 SortOrder = 1f;
        
        public bool                  IsNewPaymentMethod      = false;
        public bool                  ShouldSavePaymentMethod = false;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Actions
        
        public void Select()
        {
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
                var myType = this.Data.type == "credit_card" ? "card" : this.Data.type;
                return CHECKOUT.PaymentMethods.PaymentMethodsDefinitions.FirstOrDefault(x => x.Data.type == myType);
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
            
            return data;
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Helper Methods
        
        public string GetPaymentMethodTypeeActual()
        {
            if (this is CreditCardUserUserPaymentMethod cc)
                return cc.CreditCardType.ToLower();
            else return this.Type.ToLower();
        }
        
        public Sprite GetIconSprite()
        {
            return CHECKOUT.Sprites.GetIconSprite(GetPaymentMethodTypeeActual());
        }
        
        public Sprite GetButtonSprite()
        {
            return CHECKOUT.Sprites.GetButtonSprite(GetPaymentMethodTypeeActual());
        }
    }
}

