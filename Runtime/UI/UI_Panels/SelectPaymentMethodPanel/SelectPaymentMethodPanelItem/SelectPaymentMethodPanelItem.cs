using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
        
        public void Initialize(PaymentMethodDefinition paymentMethodDefinition, SelectPaymentMethodPanelView SelectPaymentMethodPanelView)
        {
            this.PaymentMethodDefinition      = paymentMethodDefinition;
            this.SelectPaymentMethodPanelView = SelectPaymentMethodPanelView;
            Refresh();
        }
        
        public void Initialize(UserPaymentMethod userPaymentMethod, SelectPaymentMethodPanelView SelectPaymentMethodPanelView)
        {
            this.UserPaymentMethod            = userPaymentMethod;
            this.SelectPaymentMethodPanelView = SelectPaymentMethodPanelView;
            Refresh();
        }
        
        
        //// Refresh
        
        public override void RefreshState()
        {    
            this.Label.text = PaymentMethodDefinition?.DisplayName;

            //////////////////////////////////////////////// Default
            if (this.PaymentMethodDefinition      == null
            &&  this.UserPaymentMethod            == null)
            {
                this.Label.text  = "Add Credit or Debit Card";
                this.Icon.sprite = AddCreditCardSprite;
            }
            //////////////////////////////////////////////// Payment Method Definitions
            else if (this.PaymentMethodDefinition != null
                 &&  this.PaymentMethodDefinition.Type == PaymentMethodDefinition.PAYMENT_METHOD_TYPE_CREDIT_CARD)
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
            //////////////////////////////////////////////// UserPaymentMethods
            else if (this.UserPaymentMethod != null
                 &&  this.UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.MasterCard.ToString())
            {
                this.Label.text  = this.UserPaymentMethod.DisplayName;
                this.Icon.sprite = this.MasterCardSprite;
            }
            else if (this.UserPaymentMethod      != null
                 &&  this.UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.Visa.ToString())
            {
                this.Label.text  = this.UserPaymentMethod.DisplayName;
                this.Icon.sprite = this.VisaSprite;
            }
            else if (this.UserPaymentMethod      != null
                 &&  this.UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.Amex.ToString())
            {
                this.Label.text  = this.UserPaymentMethod.DisplayName;
                this.Icon.sprite = this.AmexSprite;
            }
            else if (this.UserPaymentMethod      != null
                 &&  this.UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.Diners.ToString())
            {
                this.Label.text  = this.UserPaymentMethod.DisplayName;
                this.Icon.sprite = this.DinersSprite;
            }
            else if (this.UserPaymentMethod      != null
                 &&  this.UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.Discover.ToString())
            {
                this.Label.text  = this.UserPaymentMethod.DisplayName;
                this.Icon.sprite = this.DiscoverSprite;
            }
            else if (this.UserPaymentMethod      != null
                 &&  this.UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.GPay.ToString())
            {
                this.Label.text  = this.UserPaymentMethod.DisplayName;
                this.Icon.sprite = this.GPaySprite;
            }
            else if (this.UserPaymentMethod      != null
                 &&  this.UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.PayPal.ToString())
            {
                this.Label.text  = this.UserPaymentMethod.DisplayName;
                this.Icon.sprite = this.PaypalSprite;
            }
            else if (this.UserPaymentMethod      != null
                 &&  this.UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.Apple.ToString())
            {
                this.Label.text  = this.UserPaymentMethod.DisplayName;
                this.Icon.sprite = this.AppleSprite;
            }
        }
        
        //// UI Events
        
        public void On_Click()
        {
          if (this.PaymentMethodDefinition == null)
              this.SelectPaymentMethodPanelView.On_NewCardClicked();
          else
          {
            //this.SelectPaymentMethodPanelView.On_Select();
            //CheckoutClient.Instance.CurrentSession.User.SelectPaymentMethod(this.PaymentMethodDefinition);
          }
        }
    }
}
