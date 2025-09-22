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
        public Image       Icon;
        public TMP_Text    Label;
        public Image       CheckedImage;
        public Image       UncheckedImage;
        public List<Image> Separators;
        public List<Image> ExtraSeparators;
        
        Color              SelectedOptionColor   = new Color(0.2862745f, 0.7411765f, 0.9529412f, 1);
        Color              UnselectedOptionColor = new Color(0.8679245f, 0.8679245f, 0.8679245f, 1);

        [Header("Sprites")]
        public Sprite VisaSprite;
        public Sprite MasterCardSprite;
        public Sprite GPaySprite;
        public Sprite PaypalSprite;
        public Sprite AppleSprite;
        public Sprite AddCardSprite;

        //// Properties

        public UserPaymentMethod PaymentMethod { get; set; }
        public CheckoutPanelView CheckoutPanelView { get; set; }

        //// Lifecycle

        public void Initialize(UserPaymentMethod paymentMethod, CheckoutPanelView CheckoutPanelView)
        {
            Debug.Log("paymentMethod: " + paymentMethod.DisplayName);
            this.PaymentMethod     = paymentMethod;
            this.CheckoutPanelView = CheckoutPanelView;
            Refresh();
        }

        //// Refresh

        public override void RefreshState()
        {
            if (CheckoutPanelView == null)
            {
                Debug.Log("RefreshState(): " + this.name);
                this.Icon.sprite = AddCardSprite;
                this.Label.text = "Add Credit Card";
                return;
            }

            this.Label.text = PaymentMethod.DisplayName;
            
            if(this.CheckedImage)
                this.CheckedImage.gameObject.SetActive(this.PaymentMethod.IsSelected);

            if (this.UncheckedImage)
                this.UncheckedImage.gameObject.SetActive(!this.PaymentMethod.IsSelected);

            Debug.Log("this.PaymentMethod.Type: " + this.PaymentMethod.Type + "  this.PaymentMethod.IsSelected: " + this.PaymentMethod.IsSelected);
            if (this.PaymentMethod.Type == UserPaymentMethod.PaymentMethodType.Visa.ToString())
            {
                this.Icon.sprite = VisaSprite;

                if (this.PaymentMethod.IsSelected)
                    CheckoutPanelView.ShowPurchaseButton();
            }
            else if (this.PaymentMethod.Type == UserPaymentMethod.PaymentMethodType.MasterCard.ToString())
            {
                this.Icon.sprite = MasterCardSprite;

                if (this.PaymentMethod.IsSelected)
                    CheckoutPanelView.ShowPurchaseButton();
            }
            else if (this.PaymentMethod.Type == UserPaymentMethod.PaymentMethodType.GPay.ToString())
            {
                this.Icon.sprite = GPaySprite;

                if (this.PaymentMethod.IsSelected)
                    CheckoutPanelView.ShowGooglePayButton();
            }
            else if (this.PaymentMethod.Type == UserPaymentMethod.PaymentMethodType.PayPal.ToString())
            {
                this.Icon.sprite = PaypalSprite;

                if (this.PaymentMethod.IsSelected)
                    CheckoutPanelView.ShowPaypalPayButton();
            }
            else if (this.PaymentMethod.Type == UserPaymentMethod.PaymentMethodType.Apple.ToString())
            {
                this.Icon.sprite = AppleSprite;

                if (this.PaymentMethod.IsSelected)
                    CheckoutPanelView.ShowApplePayButton();
            }


            if (this.PaymentMethod.IsSelected)
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
            for (int i = 0; i < Separators.Count; i++)
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

        // For Dropdown Menu
        public void SelectDropdownPaymentMethod(UserPaymentMethod paymentMethod, CheckoutPanelView CheckoutPanelView)
        {
            Debug.Log("SetPaymentMethod: " + paymentMethod.Type + "  CheckoutPanelView: " + CheckoutPanelView);
         
            this.PaymentMethod = paymentMethod;
            
            this.CheckoutPanelView = CheckoutPanelView;

            this.PaymentMethod?.Select();

            // Refresh();

            if (this.PaymentMethod.Type == UserPaymentMethod.PaymentMethodType.Visa.ToString())
            {
                if (this.PaymentMethod.IsSelected)
                    CheckoutPanelView.ShowPurchaseButton();
            }
            else if (this.PaymentMethod.Type == UserPaymentMethod.PaymentMethodType.MasterCard.ToString())
            {
                if (this.PaymentMethod.IsSelected)
                    CheckoutPanelView.ShowPurchaseButton();
            }
            else if (this.PaymentMethod.Type == UserPaymentMethod.PaymentMethodType.GPay.ToString())
            {
                if (this.PaymentMethod.IsSelected)
                    CheckoutPanelView.ShowGooglePayButton();
            }
            else if (this.PaymentMethod.Type == UserPaymentMethod.PaymentMethodType.PayPal.ToString())
            {
                if (this.PaymentMethod.IsSelected)
                    CheckoutPanelView.ShowPaypalPayButton();
            }
            else if (this.PaymentMethod.Type == UserPaymentMethod.PaymentMethodType.Apple.ToString())
            {
                if (this.PaymentMethod.IsSelected)
                    CheckoutPanelView.ShowApplePayButton();
            }
           
        }
    }
}
