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
        //// Types
        
        public enum PaymentMethodType
        {
            Card,
            Visa,
            MasterCard,
            Amex,
            Diners,
            Discover,
            GPay,
            PayPal,
            Apple
        }
        
        //// Members
        
        public string Type; 
        public string DisplayName;
        public bool   IsSelected;
        
        public bool   IsNewPaymentMethod = true;
        
        public UserPaymentMethodData Data;

        //// UI Actions
        
        public void Select()
        {
            this.IsSelected = true;
        }
        
        public void Unselect()
        {
            this.IsSelected = false;
        }
        
        //// Vaulting
        
        public virtual Step RunVaultingSteps() 
        =>
            new Step(name   : $"run_vaulting_steps"
                    ,action : async (s) =>
                    {
                        
                    });
        
        //// Transaction Steps
        
        public PaymentMethodDefinition GetPaymentMethodDefinition()
        {
            var myType = this.Data.type == "credit_card" ? "card" : this.Data.type;
            return CHECKOUT.PaymentMethods.PaymentMethodsDefinitions.FirstOrDefault(x => x.Data.type == myType);
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
        
        public List<Func<Step>> InitializationSteps  = new ();
        public List<Func<Step>> VaultingSteps        = new ();
        public List<Func<Step>> TransactionSteps     = new ();
        public List<Func<Step>> PostTransactionSteps = new ();
        
    }
}


