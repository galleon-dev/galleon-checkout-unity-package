using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{
    public class PreselectionPanelView : View
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// View Result

        public      ViewResult Result = ViewResult.None;
        public enum ViewResult
        {
            None,
            Back,
            Confirm,
            Settings,
            AddCard,
            OtherPaymentMethods,
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members

        public PositionLayoutGroup PositionLayoutGroup;

        [Header("Shop Item")]
        public TextMeshProUGUI      ProductTitleText;
        public TextMeshProUGUI      PriceText;
        public TextMeshProUGUI      TaxText;

        [Header("Payment Methods")]
        public GameObject           PaymentMethodsPanel;
        public GameObject           PaymentMethodItemPrefab;        
        public GameObject           AddCreditCardButtonElement;

        public TMP_Dropdown         DropdownMenu;

        [Header("Payment Buttons")]
        public GameObject           PurchaseButton;
        public GameObject           GooglePayButton;
        public GameObject           PaypalPayButton;
        public GameObject           ApplePayButton;

        public  Config              Configutation;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helper Types
        
        [Serializable]
        public class Config
        {
            public bool ShowMinimalOptions = false;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Links

        public IEnumerable<checkoutPanelPaymentMethodItemView> PaymentMethodItemViews => GetComponentsInChildren<checkoutPanelPaymentMethodItemView>();

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public override void Initialize()
        {
          //RefreshState();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Refresh

        public override void RefreshState()
        {
            if (CheckoutClient.Instance.CurrentSession == null) return;
            
            // // Panel config
            // var configText = CheckoutClient.Instance.CheckoutScreenMobile.CurrentPage.panelConfiguration;
            // if (configText != null)
            //     this.Configutation = JsonConvert.DeserializeObject<Config>(configText);
            // if (this.Configutation == null)
            //     this.Configutation = new Config() { ShowMinimalOptions = false };
            // 
            // if (this.Configutation.ShowMinimalOptions)
            // {
            //     this.TaxesContainer.SetActive(false);
            // }
            
            // Debug.Log("<color=green>RefreshState</color>");
            
            this.ProductTitleText.text = Checkout.CheckoutClient.Instance.CurrentSession.SelectedProduct.DisplayName;
            this.PriceText.text        = Checkout.CheckoutClient.Instance.CurrentSession.SelectedProduct.PriceText;

            ///////////////

            // Remove children (if any)
            // Debug.Log("<color=orange>- Removing Payment Methods</color>");
            foreach (Transform child in PaymentMethodsPanel.transform)
            {
                // Debug.Log($"-Removing Item {child.gameObject.name}");
                Destroy(child.gameObject);
            }

            // Add children
            var paymentMethods = CHECKOUT.PaymentMethods.UserPaymentMethodsToDisplay;
            foreach (var paymentMethod in paymentMethods)
            {
                // if (this.Configutation != null && this.Configutation.ShowMinimalOptions)
                //     if (paymentMethod.Type != "native" && paymentMethod.Type != "card") continue;
                
                var go   = Instantiate(original: PaymentMethodItemPrefab, parent: PaymentMethodsPanel.transform);
                var item = go.GetComponent<checkoutPanelPaymentMethodItemView>();
              //item.Initialize(paymentMethod, this);

                // Add ui separator
                Instantiate(original: CHECKOUT.Resources.UI_Seporator, parent: PaymentMethodsPanel.transform);
            }

            // Add defult add card button
            if (AddCreditCardButtonElement)
            {
                this.AddCreditCardButtonElement.SetActive(paymentMethods.Count() == 0);
            }

            // Set Dropdown Options
            if (DropdownMenu)
            {
                SetDropdown();
            }

            ///////////////
            // checkoutPanelPaymentMethodItemView[] methods = this.gameObject.GetComponentsInChildren<checkoutPanelPaymentMethodItemView>();
            // foreach (var method in methods)
            //     method.Refresh();

            //CheckoutClient.Instance.CheckoutScreenMobile.ShowInitialCheckoutPanelLoader();

        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Radio Buttons


        public void OnRadiobuttonSelected(checkoutPanelPaymentMethodItemView SelectedItem)
        {
            foreach (var item in PaymentMethodItemViews)
            {
                if (item == SelectedItem)
                    continue;

                item.Unselect();
            }
            
            //ShowPurchaseButton();
            var image = PurchaseButton.gameObject.GetComponent<Image>();
            image.sprite = CHECKOUT.PaymentMethods.PaymentMethodsDefinitions.First().LogoSprite;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Events

        public void OnConfirmPurchaseClick()
        {
            this.Result = ViewResult.Confirm;
            CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(Result.ToString());
        }

        public void OnSettingsClick()
        {
            this.Result = ViewResult.Settings;
            CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(Result.ToString());
        }

        public void OnOtherPaymentMethodsClick()
        {
            this.Result = ViewResult.OtherPaymentMethods;
            CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(Result.ToString());
        }

        public void On_AddCardClicked()
        {
            this.Result = ViewResult.AddCard;
            CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(Result.ToString());
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Methods

        public void SetPurchaseButtonSprite(Sprite sprite)
        {
            var image = this.PurchaseButton.GetComponentInChildren<Image>();
            image.sprite = sprite;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helper Methods
        
        public void SetDropdown()
        {
            DropdownMenu.ClearOptions();

                ManagePaymentSprites ManagePaymentSprites = DropdownMenu.gameObject.GetComponent<ManagePaymentSprites>();

                var paymentMethods = CHECKOUT.PaymentMethods.UserPaymentMethods;

                // Add Dropdown Options
                int i = 0;
                foreach (var paymentMethod in paymentMethods)
                {
                   
                    Sprite Icon = null;

                    if (ManagePaymentSprites)
                    {
                        Icon = ManagePaymentSprites.GetPaymentIcon(paymentMethod);
                    }

                    var newOption = new TMP_Dropdown.OptionData(paymentMethod.DisplayName, Icon);

                    DropdownMenu.options.Add(newOption);

                    if (paymentMethod.IsSelected)
                    {
                        DropdownMenu.value = i;
                    }

                    i++;
                }
               
                Debug.Log("Set Dropdown 1st Option");
               
                // ForceReselect
                DropdownMenu.onValueChanged.Invoke(DropdownMenu.value); // Forces the event
                DropdownMenu.RefreshShownValue();

                // Hide Dropdown if no Payments are available
                DropdownMenu.gameObject.SetActive(paymentMethods.Count() > 0);
        }
    }
}

