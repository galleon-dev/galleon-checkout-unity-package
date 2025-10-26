using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Galleon.Checkout.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{
    public class SelectPaymentMethodPanelItem : View
    {
        //// Members
        
        [Header("UI")]
        public Image    Icon;
        public TMP_Text Label;
        
        //// Properties
        
        public PaymentMethodDefinition      PaymentMethodDefinition      { get; set; }
        public UserPaymentMethod            UserPaymentMethod            { get; set; }
        public SelectPaymentMethodPanelView SelectPaymentMethodPanelView { get; set; }
        
        
        //// Lifecycle
        
        public void Initialize(PaymentMethodDefinition      paymentMethodDefinition, 
                               SelectPaymentMethodPanelView SelectPaymentMethodPanelView)
        {
            this.PaymentMethodDefinition      = paymentMethodDefinition;
            this.SelectPaymentMethodPanelView = SelectPaymentMethodPanelView;
            Refresh();
        }
        
        public void Initialize(UserPaymentMethod            userPaymentMethod, 
                               SelectPaymentMethodPanelView SelectPaymentMethodPanelView)
        {
            this.UserPaymentMethod            = userPaymentMethod;
            this.SelectPaymentMethodPanelView = SelectPaymentMethodPanelView;
            Refresh();
        }
        
        
        //// Refresh
        
        public override async void RefreshState()
        {    
            if (PaymentMethodDefinition != null)
                this.Label.text = PaymentMethodDefinition.DisplayName;
            else if (UserPaymentMethod != null)
                this.Label.text = UserPaymentMethod.DisplayName;

            //////////////////////////////////////////////// Default
            if (this.PaymentMethodDefinition      == null
            &&  this.UserPaymentMethod            == null)
            {
                this.Label.text  = "Add Credit or Debit Card";
                this.Icon.sprite = CHECKOUT.Sprites.AddCreditCardIconSprite;
            }
            //////////////////////////////////////////////// Payment Method Definitions
            else if (this.PaymentMethodDefinition != null)
            {
                this.Icon.sprite = this.PaymentMethodDefinition.GetIconSprite();

                if (this.PaymentMethodDefinition.Type == PaymentMethodDefinition.PAYMENT_METHOD_TYPE_CREDIT_CARD)
                    this.Label.text  = "Add Credit or Debit Card";
            }
            //////////////////////////////////////////////// UserPaymentMethods
            else if (this.UserPaymentMethod != null)
            {
                this.Label.text  = this.UserPaymentMethod.DisplayName;
                this.Icon.sprite = this.UserPaymentMethod.GetIconSprite();
            }
        }

        //// UI Events
        
        public void On_Click()
        {
            if (this.PaymentMethodDefinition == null
            &&  this.UserPaymentMethod       == null)
            {
                this.SelectPaymentMethodPanelView.On_NewCardClicked();
            }
            else
            {
                this.SelectPaymentMethodPanelView.On_Select(this);
            }
        }
    }
}



