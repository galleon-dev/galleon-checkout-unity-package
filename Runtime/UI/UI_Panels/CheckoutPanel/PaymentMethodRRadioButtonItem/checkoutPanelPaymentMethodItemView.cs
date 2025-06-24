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
        public List<Image> Separators;
        public List<Image> ExtraSeparators;
        Color SelectedOptionColor = new Color(0.2862745f, 0.7411765f, 0.9529412f, 1);
        Color UnselectedOptionColor = new Color(0.8679245f, 0.8679245f, 0.8679245f, 1);

        [Header("Sprites")]
        public Sprite   VisaSprite;
        public Sprite   MasterCardSprite;
        public Sprite   GPaySprite;
        public Sprite   PaypalSprite;
        public Sprite   AddCardSprite;

        //// Properties

        public PaymentMethod     PaymentMethod     { get; set; }
        public CheckoutPanelView CheckoutPanelView { get; set; }

        //// Lifecycle

        public void Initialize(PaymentMethod paymentMethod, CheckoutPanelView CheckoutPanelView)
        {
            this.PaymentMethod     = paymentMethod;
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
                
            this.Label.text    = PaymentMethod.DisplayName;
            this.CheckedImage  .gameObject.SetActive( this.PaymentMethod.IsSelected);
            this.UncheckedImage.gameObject.SetActive(!this.PaymentMethod.IsSelected);

            if (this.PaymentMethod.Type == PaymentMethod.PaymentMethodType.Visa.ToString())
            {
                this.Icon.sprite = VisaSprite;
                CheckoutPanelView.ShowPurchaseButton();
            }
            else if (this.PaymentMethod.Type == PaymentMethod.PaymentMethodType.MasterCard.ToString())
            {
                this.Icon.sprite = MasterCardSprite;
                CheckoutPanelView.ShowPurchaseButton();
            }
            else if (this.PaymentMethod.Type == PaymentMethod.PaymentMethodType.GPay.ToString())
            {
                this.Icon.sprite = GPaySprite;
                CheckoutPanelView.ShowGooglePlayButton();
            }
            else if (this.PaymentMethod.Type == PaymentMethod.PaymentMethodType.PayPal.ToString())
            {
                this.Icon.sprite = PaypalSprite;
                CheckoutPanelView.ShowPurchaseButton();
            }

            
            if(this.PaymentMethod.IsSelected)
            {
                SetSeperatorColor(SelectedOptionColor, true);
            }
            else
            {
                SetSeperatorColor(UnselectedOptionColor, false);
            }
        }
        
        void SetSeperatorColor(Color _Color, bool _Status)
        {
            for(int i = 0; i < Separators.Count; i++)
            {
                Separators[i].color = _Color;
                Separators[i].gameObject.SetActive(_Status);
            }
        }



        //// UI Events
       
        public void On_Click()
        {
            Select();
        }
        
        //// UI Actions
        
        public void Select()
        {
            this.PaymentMethod?.Select();
            this.CheckoutPanelView.OnRadiobuttonSelected(this);
            Refresh();
        }
        
        public void Unselect()
        {
            this.PaymentMethod?.Unselect();
            Refresh();
        }
    }
}
