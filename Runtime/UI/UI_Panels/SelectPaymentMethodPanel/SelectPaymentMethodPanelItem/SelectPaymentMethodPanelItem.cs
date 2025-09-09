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
        
        [Header("Sprites")]
        public Sprite   AddCreditCardSprite;
        public Sprite   VisaSprite;
        public Sprite   MasterCardSprite;
        public Sprite   AmexSprite;
        public Sprite   DinersSprite;
        public Sprite   DiscoverSprite;
        public Sprite   GPaySprite;
        public Sprite   PaypalSprite;
        public Sprite   AppleSprite;
        
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
                this.Icon.sprite = AddCreditCardSprite;
            }
            //////////////////////////////////////////////// Payment Method Definitions
            else if (this.PaymentMethodDefinition != null)
            {
                if (this.PaymentMethodDefinition.Type == PaymentMethodDefinition.PAYMENT_METHOD_TYPE_CREDIT_CARD)
                {
                    this.Label.text  = "Add Credit or Debit Card";
                    this.Icon.sprite = AddCreditCardSprite;
                }
                else if (this.PaymentMethodDefinition != null
                     &&  this.PaymentMethodDefinition.Type == PaymentMethodDefinition.PAYMENT_METHOD_TYPE_GOOGLE_PAY)
                {
                    this.Icon.sprite = GPaySprite;
                }
                else if (this.PaymentMethodDefinition != null
                     &&  this.PaymentMethodDefinition.Type == PaymentMethodDefinition.PAYMENT_METHOD_TYPE_PAYPAL)
                {
                    this.Icon.sprite = PaypalSprite;
                }
            }
            //////////////////////////////////////////////// UserPaymentMethods
            else if (this.UserPaymentMethod != null
                 &&  this.UserPaymentMethod.Data is CreditCardUserPaymentMethodData cd)
            {
                 if (cd.credit_card_type == nameof(UserPaymentMethod.PaymentMethodType.MasterCard))
                 {
                     this.Label.text  = this.UserPaymentMethod.DisplayName;
                     this.Icon.sprite = this.MasterCardSprite;
                 }
                 else if (cd.credit_card_type == nameof(UserPaymentMethod.PaymentMethodType.Visa))
                 {
                     this.Label.text  = this.UserPaymentMethod.DisplayName;
                     this.Icon.sprite = this.VisaSprite;
                 }
                 else if (cd.credit_card_type == nameof(UserPaymentMethod.PaymentMethodType.Amex))
                 {
                     this.Label.text  = this.UserPaymentMethod.DisplayName;
                     this.Icon.sprite = this.AmexSprite;
                 }
                 else if (cd.credit_card_type == nameof(UserPaymentMethod.PaymentMethodType.Diners))
                 {
                     this.Label.text  = this.UserPaymentMethod.DisplayName;
                     this.Icon.sprite = this.DinersSprite;
                 }
                 else if (cd.credit_card_type == nameof(UserPaymentMethod.PaymentMethodType.Discover))
                 {
                     this.Label.text  = this.UserPaymentMethod.DisplayName;
                     this.Icon.sprite = this.DiscoverSprite;
                 }
                 else if (cd.credit_card_type == nameof(UserPaymentMethod.PaymentMethodType.GPay))
                 {
                     this.Label.text  = this.UserPaymentMethod.DisplayName;
                     this.Icon.sprite = this.GPaySprite;
                 }
                 else if (cd.credit_card_type == nameof(UserPaymentMethod.PaymentMethodType.PayPal))
                 {
                     this.Label.text  = this.UserPaymentMethod.DisplayName;
                     this.Icon.sprite = this.PaypalSprite;
                 }
                 else if (cd.credit_card_type == nameof(UserPaymentMethod.PaymentMethodType.Apple))
                 {
                     this.Label.text  = this.UserPaymentMethod.DisplayName;
                     this.Icon.sprite = this.AppleSprite;
                 }
            }
        }

        //// UI Events
        
        public void On_Click()
        {
            if (this.PaymentMethodDefinition == null
            &&  this.UserPaymentMethod       == null)
                this.SelectPaymentMethodPanelView.On_NewCardClicked();
            else
            {
                this.SelectPaymentMethodPanelView.On_Select(this);
            }
        }
    }
}



