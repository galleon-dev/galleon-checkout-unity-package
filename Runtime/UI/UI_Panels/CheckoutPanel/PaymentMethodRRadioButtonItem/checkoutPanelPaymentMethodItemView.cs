using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{
    public class checkoutPanelPaymentMethodItemView : View
    {
        //// Members
        
        [Header("UI")]
        public Image    Icon;
        public TMP_Text Label;
        public Image    CheckedImage;
        public Image    UncheckedImage;
        
        [Header("Sprites")]
        public Sprite   VisaSprite;
        public Sprite   MasterCardSprite;
        public Sprite   GPaySprite;
        public Sprite   PaypalSprite;
        public Sprite   AddCardSprite;
        
        //// Properties
        
        public UserPaymentMethod     UserPaymentMethod     { get; set; }
        public CheckoutPanelView CheckoutPanelView { get; set; }
        
        //// Lifecycle
        
        public void Initialize(UserPaymentMethod userPaymentMethod, CheckoutPanelView CheckoutPanelView)
        {
            this.UserPaymentMethod     = userPaymentMethod;
            this.CheckoutPanelView = CheckoutPanelView;
            Refresh();
        }
        
        //// Refresh
        
        public override void RefreshState()
        {
            if (CheckoutPanelView == null)
            {
                this.Icon.sprite = AddCardSprite;
                this.Label.text  = "Add Credit Card";
                return;
            }
                
            this.Label.text    = UserPaymentMethod.DisplayName;
            this.CheckedImage  .gameObject.SetActive( this.UserPaymentMethod.IsSelected);
            this.UncheckedImage.gameObject.SetActive(!this.UserPaymentMethod.IsSelected);
            
            if      (this.UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.Visa.ToString())
                this.Icon.sprite = VisaSprite;
            else if (this.UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.MasterCard.ToString())
                this.Icon.sprite = MasterCardSprite;
            else if (this.UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.GPay.ToString())
                this.Icon.sprite = GPaySprite;
            else if (this.UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.PayPal.ToString())
                this.Icon.sprite = PaypalSprite;
        }
        
        //// UI Events
       
        public void On_Click()
        {
            Select();
        }
        
        //// UI Actions
        
        public void Select()
        {
            this.UserPaymentMethod?.Select();
            this.CheckoutPanelView.OnRadiobuttonSelected(this);
            Refresh();
        }
        
        public void Unselect()
        {
            this.UserPaymentMethod?.Unselect();
            Refresh();
        }
    }
}
