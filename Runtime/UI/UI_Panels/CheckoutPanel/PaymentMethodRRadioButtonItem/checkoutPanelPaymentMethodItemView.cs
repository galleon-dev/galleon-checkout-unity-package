using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout.Foundation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{   
    public class checkoutPanelPaymentMethodItemView : View
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members

        [Header("UI")]
        public Image           Icon;
        public TMP_Text        Label;
        public Image           CheckedImage;
        public Image           UncheckedImage;
        public List<Image>     Separators;
        public List<Image>     ExtraSeparators;
        public Color           SelectedOptionColor   = new Color(0.2862745f, 0.7411765f, 0.9529412f, 1);
        public Color           UnselectedOptionColor = new Color(0.8679245f, 0.8679245f, 0.8679245f, 1);

        [Header("Bonus")]
        public GameObject      BonusContainer;
        public BonusItemView   bonusItemView;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties

        public UserPaymentMethod PaymentMethod     { get; set; }
        public CheckoutPanelView CheckoutPanelView { get; set; }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public void Initialize(UserPaymentMethod paymentMethod, CheckoutPanelView CheckoutPanelView)
        {
            // "log"
            this.gameObject.name += $"_{paymentMethod.Type}";
            
            // Definitions
            this.PaymentMethod     = paymentMethod;
            this.CheckoutPanelView = CheckoutPanelView;
            
            // Bonus
            if (this.bonusItemView != null)
                Destroy(this.bonusItemView.gameObject); // destroy placeholder
            if (CheckoutClient.Instance.Resources.CheckoutAssets.BonusItemPrefab != null)
            {
                this.bonusItemView = Instantiate(CheckoutClient.Instance.Resources.CheckoutAssets.BonusItemPrefab, BonusContainer.transform).GetComponent<BonusItemView>();
                var bonus = this.bonusItemView.gameObject.GetComponentInChildren<IBonusItemView>();
                bonus.Initialize("Extra", "1000");
            }
            
            // Refresh
            Refresh();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Refresh

        public override void RefreshState()
        {
            if (CheckoutPanelView == null)
            {
                this.Icon.sprite = CHECKOUT.Sprites.AddCreditCardIconSprite;
                this.Label.text  = "Add Credit Card";
                return;
            }

            this.Label.text    = PaymentMethod.DisplayName;
            this.CheckedImage  .gameObject.SetActive( this.PaymentMethod.IsSelected);
            this.UncheckedImage.gameObject.SetActive(!this.PaymentMethod.IsSelected);
            
            // Debug.Log("this.PaymentMethod.Type: " + this.PaymentMethod.Type + "  this.PaymentMethod.IsSelected" + this.PaymentMethod.IsSelected);
            
            // Set icon Sprite
            this.Icon.sprite = this.PaymentMethod.GetIconSprite();
            
            // Set button Sprite
            if (this.PaymentMethod.IsSelected)
                CheckoutPanelView.SetPurchaseButtonSprite(this.PaymentMethod.GetButtonSprite());
            
            // Set Seperator Color 
            if (this.PaymentMethod.IsSelected)
                SetSeperatorColor(SelectedOptionColor, true);
            else
                SetSeperatorColor(UnselectedOptionColor, false);
            
            // Bonus
            if (this.bonusItemView != null)
            {    
                if (this.PaymentMethod.IsSelected) bonusItemView.Open();
                else                               bonusItemView.Close();
                
                if (this.PaymentMethod.Type == "native")
                    bonusItemView.gameObject.SetActive(false);
            }
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Events

        public void On_Click()
        {
            Select();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Actions

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
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// helper Methods
        
        private void SetSeperatorColor(Color _Color, bool _Status)
        {
            for (int i = 0; i < Separators.Count; i++)
            {
                Separators[i].color = _Color;
                Separators[i].gameObject.SetActive(_Status);
            }
        }

        // For Dropdown Menu
        public void SelectDropdownPaymentMethod(UserPaymentMethod paymentMethod, CheckoutPanelView CheckoutPanelView)
        {
            Debug.Log("SetPaymentMethod: " + paymentMethod.Type + "  CheckoutPanelView: " + CheckoutPanelView);
         
            this.PaymentMethod = paymentMethod;
            
            this.CheckoutPanelView = CheckoutPanelView;

            this.PaymentMethod?.Select();

            // Refresh();

            // if (this.PaymentMethod.Type == UserPaymentMethod.PaymentMethodType.Visa.ToString())
            // {
            //     if (this.PaymentMethod.IsSelected)
            //         CheckoutPanelView.ShowPurchaseButton();
            // }
            // else if (this.PaymentMethod.Type == UserPaymentMethod.PaymentMethodType.MasterCard.ToString())
            // {
            //     if (this.PaymentMethod.IsSelected)
            //         CheckoutPanelView.ShowPurchaseButton();
            // }
            // else if (this.PaymentMethod.Type == UserPaymentMethod.PaymentMethodType.GPay.ToString())
            // {
            //     if (this.PaymentMethod.IsSelected)
            //         CheckoutPanelView.ShowGooglePayButton();
            // }
            // else if (this.PaymentMethod.Type == UserPaymentMethod.PaymentMethodType.PayPal.ToString())
            // {
            //     if (this.PaymentMethod.IsSelected)
            //         CheckoutPanelView.ShowPaypalPayButton();
            // }
            // else if (this.PaymentMethod.Type == UserPaymentMethod.PaymentMethodType.Apple.ToString())
            // {
            //     if (this.PaymentMethod.IsSelected)
            //         CheckoutPanelView.ShowApplePayButton();
            // }
           
        }
    }
}
