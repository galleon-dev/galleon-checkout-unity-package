using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Galleon.Checkout.Foundation;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{
    public class CheckoutPanelView : View
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

        //////////////////////////////////////////////////////////////////////////// Members

        public PositionLayoutGroup PositionLayoutGroup;
        
        [Header("Shop Item")]
        public TextMeshProUGUI  ProductTitleText;
        public TextMeshProUGUI  PriceText;
        public TextMeshProUGUI  TaxText;
        
        [Header("Payment Methods")]
        public GameObject       PaymentMethodsPanel;
        public GameObject       PaymentMethodItemPrefab;        
        public GameObject       AddCreditCardButtonElement;

        [Header("Payment Buttons")]
        public GameObject       PurchaseButton;
        public GameObject       GooglePayButton;
        public GameObject       PaypalPayButton;
        public GameObject       ApplePayButton;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Links

        public IEnumerable<checkoutPanelPaymentMethodItemView> PaymentMethodItemViews => GetComponentsInChildren<checkoutPanelPaymentMethodItemView>();

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public override void Initialize()
        {
            RefreshState();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Refresh

        public override void RefreshState()
        {
            if (CheckoutClient.Instance.CurrentSession == null) return;
            
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
            var paymentMethods = CHECKOUT.PaymentMethods.UserPaymentMethods.Take(3);
            foreach (var paymentMethod in paymentMethods)
            {
                var go   = Instantiate(original : PaymentMethodItemPrefab, parent : PaymentMethodsPanel.transform);
                var item = go.GetComponent<checkoutPanelPaymentMethodItemView>();   
                item.Initialize(paymentMethod, this);
                
                // Add ui separator
                Instantiate(original : CHECKOUT.Resources.UI_Seporator, parent : PaymentMethodsPanel.transform);
            }
            
            // Add defult add card button
            this.AddCreditCardButtonElement.SetActive(paymentMethods.Count() == 0);
            
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
            //var image = PurchaseButton.gameObject.GetComponent<Image>();
            //image.sprite = CHECKOUT.PaymentMethods.PaymentMethodsDefinitions.First().LogoSprite;
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
        
        public void ShowPurchaseButton()
        {
            //Debug.Log("ShowPurchaseButton()");
            GooglePayButton.SetActive(false);
            PurchaseButton .SetActive(true);
            PaypalPayButton.SetActive(false);
            ApplePayButton .SetActive(false);
        }

        public void ShowGooglePayButton()
        {
            //Debug.Log("ShowGooglePayButton()");
            GooglePayButton .SetActive(true);
            PurchaseButton  .SetActive(false);
            PaypalPayButton .SetActive(false);
            ApplePayButton  .SetActive(false);
        }

        public void ShowPaypalPayButton()
        {
            //Debug.Log("ShowPaypalPayButton()");
            GooglePayButton .SetActive(false);
            PurchaseButton  .SetActive(false);
            PaypalPayButton .SetActive(true);
            ApplePayButton  .SetActive(false);
        }

        public void ShowApplePayButton()
        {
            //Debug.Log("ShowApplePayButton()");
            GooglePayButton .SetActive(false);
            PurchaseButton  .SetActive(false);
            PaypalPayButton .SetActive(false);
            ApplePayButton  .SetActive(true);
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Test Scenarios
        
        public TestScenario scenario_1        => new TestScenario(expressions : new[] { $"{nameof(test_confirmPurchase     )}()" });
        public TestScenario scenario_2_part_1 => new TestScenario(expressions : new[] { $"{nameof(test_select_other_methods)}()" });
        public TestScenario scenario_2_part_2 => new TestScenario(expressions : new[] { $"{nameof(test_settings_page       )}()" });
        public TestScenario scenario_2_part_3 => new TestScenario(expressions : new[] { $"{nameof(test_confirmPurchase     )}()" });
        
        public Step test_confirmPurchase()      => new Step(name : "checkout_panel_test_confirm_purchase", action : async (s) => { OnConfirmPurchaseClick();     });
        public Step test_select_other_methods() => new Step(action : async (s) => { OnOtherPaymentMethodsClick(); });
        public Step test_settings_page()        => new Step(action : async (s) => { OnSettingsClick();            });
        
        /// Test Rule : On_Next("checkoutPanel").Do("confirm_purchase")
        /// Test Rule : On_ALL ("whatever")     .Do("confirm_purchase")
    }
}

