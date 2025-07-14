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
        public Sprite   AppleSprite;

        //// Properties

        public PaymentMethod     PaymentMethod     { get; set; }
        public SettingsPanelView SettingsPanelView { get; set; }
        
        //// Lifecycle
        
        public void Initialize(PaymentMethod paymentMethod, SettingsPanelView settingsPanelView)
        {
            this.PaymentMethod     = paymentMethod;
            this.SettingsPanelView = settingsPanelView;
            Refresh();
        }
        
        //// Refresh
        
        public override void RefreshState()
        {    
            this.Label.text = PaymentMethod?.DisplayName;

            if (this.PaymentMethod == null)
            {
                this.Label.text  = "Add Credit or Debit Card";
                this.Icon.sprite = AddCreditCardSprite;
            }
            else if (this.PaymentMethod.Type == PaymentMethod.PaymentMethodType.Visa.ToString())
                this.Icon.sprite = VisaSprite;
            else if (this.PaymentMethod.Type == PaymentMethod.PaymentMethodType.MasterCard.ToString())
                this.Icon.sprite = MasterCardSprite;
            else if (this.PaymentMethod.Type == PaymentMethod.PaymentMethodType.Amex.ToString())
                this.Icon.sprite = AmexSprite;
            else if (this.PaymentMethod.Type == PaymentMethod.PaymentMethodType.Diners.ToString())
                this.Icon.sprite = DinersSprite;
            else if (this.PaymentMethod.Type == PaymentMethod.PaymentMethodType.Discover.ToString())
                this.Icon.sprite = DiscoverSprite;
            else if (this.PaymentMethod.Type == PaymentMethod.PaymentMethodType.GPay.ToString())
                this.Icon.sprite = GPaySprite;
            else if (this.PaymentMethod.Type == PaymentMethod.PaymentMethodType.PayPal.ToString())
                this.Icon.sprite = PaypalSprite;
            else if (this.PaymentMethod.Type == PaymentMethod.PaymentMethodType.Apple.ToString())
                this.Icon.sprite = AppleSprite;
        }
        
        //// UI Events
        
        public void On_Delete_Clicked()
        {
            Debug.Log((this.PaymentMethod?.DisplayName??"NULL") + "_delete clicked");
            this.SettingsPanelView.DeletePaymentMethod(this.PaymentMethod);
        }
    }
}