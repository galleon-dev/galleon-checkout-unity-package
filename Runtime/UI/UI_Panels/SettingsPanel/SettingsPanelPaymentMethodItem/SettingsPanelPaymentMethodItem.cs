using System.Collections;
using System.Collections.Generic;
using Galleon.Checkout.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{
    public class SettingsPanelPaymentMethodItem : View
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
        
        //// Properties
        
        public UserPaymentMethod     UserPaymentMethod     { get; set; }
        public SettingsPanelView SettingsPanelView { get; set; }
        
        //// Lifecycle
        
        public void Initialize(UserPaymentMethod userPaymentMethod, SettingsPanelView settingsPanelView)
        {
            this.UserPaymentMethod     = userPaymentMethod;
            this.SettingsPanelView = settingsPanelView;
            Refresh();
        }
        
        //// Refresh
        
        public override void RefreshState()
        {    
            this.Label.text = UserPaymentMethod?.DisplayName;

            if (this.UserPaymentMethod == null)
            {
                this.Label.text  = "Add Credit or Debit Card";
                this.Icon.sprite = AddCreditCardSprite;
            }
            else if (this.UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.Visa.ToString())
                this.Icon.sprite = VisaSprite;
            else if (this.UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.MasterCard.ToString())
                this.Icon.sprite = MasterCardSprite;
            else if (this.UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.Amex.ToString())
                this.Icon.sprite = AmexSprite;
            else if (this.UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.Diners.ToString())
                this.Icon.sprite = DinersSprite;
            else if (this.UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.Discover.ToString())
                this.Icon.sprite = DiscoverSprite;
            else if (this.UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.GPay.ToString())
                this.Icon.sprite = GPaySprite;
            else if (this.UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.PayPal.ToString())
                this.Icon.sprite = PaypalSprite;
        }
        
        //// UI Events
        
        public void On_Delete_Clicked()
        {
            Debug.Log((this.UserPaymentMethod?.DisplayName??"NULL") + "_delete clicked");
            this.SettingsPanelView.DeletePaymentMethod(this.UserPaymentMethod);
        }
    }
}