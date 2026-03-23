using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Galleon.Checkout.Foundation;
using Newtonsoft.Json;
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
            AddPaypal,
            OtherPaymentMethods,
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members

        public PositionLayoutGroup PositionLayoutGroup;

        [Header("Shop Item")]
        public TextMeshProUGUI          ProductTitleText;
        public TextMeshProUGUI          PriceText;
        public TextMeshProUGUI          TaxText;

        [Header("Payment Methods")]
        public GameObject               PaymentMethodsPanel;
        public GameObject               PaymentMethodItemPrefab;
        public GameObject               AddCreditCardButtonElement;

        public TMP_Dropdown             DropdownMenu;

        [Header("Payment Button")]
        public GameObject               PurchaseButton;
        public TMP_Text                 PurchaseButtonText;

        [Header("Taxes")]
        public List<GameObject>         TaxesPanels;
        public GameObject               TaxesAndFeesRow;
        public GameObject               TaxesContainer;
        public GameObject               TaxPrefab;
        public TextMeshProUGUI          SubtotalPriceText;
        public TextMeshProUGUI          TotalPriceText;

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

        public void OnEnable()
        {
            //CHECKOUT.PaymentMethods.UserPaymentMethodsToDisplay.FirstOrDefault()?.SelectExclusive();
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Refresh

        public override void RefreshState()
        {
            if (CheckoutClient.Instance.CurrentSession == null) return;

            this.ProductTitleText.text = Checkout.CheckoutClient.Instance.CurrentSession.SelectedProduct.DisplayName;
            this.PriceText.text        = Checkout.CheckoutClient.Instance.CurrentSession.SelectedProduct.PriceText;

            ///////////////

            // Remove children (if any)
            foreach (Transform child in PaymentMethodsPanel.transform)
            {
                Destroy(child.gameObject);
            }

            List<string> addedPmTypes = new();
            
            // Add children
            var paymentMethods = CHECKOUT.PaymentMethods.UserPaymentMethodsToDisplay.OrderByDescending(x => x.LastSuccessfulUseTime);
            foreach (var paymentMethod in paymentMethods)
            {
                if (addedPmTypes.Contains(paymentMethod.Data.type))
                    continue;
                
                addedPmTypes.Add(paymentMethod.Data.type);
                
                var go   = Instantiate(original: PaymentMethodItemPrefab, parent: PaymentMethodsPanel.transform);
                var item = go.GetComponent<checkoutPanelPaymentMethodItemView>();
                item.Initialize(paymentMethod, this);

                // Add ui separator
                Instantiate(original: CHECKOUT.Resources.UI_Seporator, parent: PaymentMethodsPanel.transform);
            }

            // Add defult add card button - OLD - NOT USED
            // if (AddCreditCardButtonElement)
            // {
            //     int amoutOfCreditCardUserPaymentMethods = CHECKOUT.PaymentMethods.UserPaymentMethods.Count(x => x?.Data?.type == "credit_card");
            //     int amountOfPaymentMethodsToDisplay     = paymentMethods.Count();
            //     bool shouldShowAddCardButton            = amoutOfCreditCardUserPaymentMethods == 0 && amountOfPaymentMethodsToDisplay < 3;
            //     this.AddCreditCardButtonElement.SetActive(shouldShowAddCardButton);
            // }

            // Set Dropdown Options
            if (DropdownMenu)
            {
                SetDropdown();
            }
            
            // Set Button Display
            var selectedPaymentMethod = CHECKOUT.PaymentMethods.UserPaymentMethods.FirstOrDefault(x => x.IsSelected);
            if (selectedPaymentMethod != null)
                SetButtonDisplay(selectedPaymentMethod);

            if (TaxesContainer != null)
            {
                GenerateTaxes();
            }
            
            GetComponentInChildren<OtherPaymentMethodsElement>()?.Refresh();
        }
        
        public void SoftRefreshState()
        {
            if (CheckoutClient.Instance.CurrentSession == null) return;

            this.ProductTitleText.text = Checkout.CheckoutClient.Instance.CurrentSession.SelectedProduct.DisplayName;
            this.PriceText.text        = Checkout.CheckoutClient.Instance.CurrentSession.SelectedProduct.PriceText;

            // Set Button Display
            var selectedPaymentMethod = CHECKOUT.PaymentMethods.UserPaymentMethods.FirstOrDefault(x => x.IsSelected);
            if (selectedPaymentMethod != null)
                SetButtonDisplay(selectedPaymentMethod);

            foreach (var item in PaymentMethodItemViews)
            {
                item.RefreshState();
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Radio Buttons


        public void OnRadiobuttonSelected(checkoutPanelPaymentMethodItemView SelectedItem)
        {
            foreach (var item in PaymentMethodItemViews)
            {
                if (item.PaymentMethod == SelectedItem.PaymentMethod)
                    continue;

                item.Unselect();
            }
            
            SoftRefreshState();

            // SetButtonDisplay(SelectedItem?.PaymentMethod);
        }
        
        public void SelectUserPaymentMethod(UserPaymentMethod selectedUPM)
        {
            if (selectedUPM == null || selectedUPM.Type.IsNullOrEmpty()) return;
            
            selectedUPM.SelectExclusive();
            SoftRefreshState();
            
            // Analytics: Payment Method Switched
            CheckoutAPI.InvokeAnalyticsEvent("payment_method_switched", new Dictionary<string, object>
            {
                { "checkout_session_id",        CHECKOUT.Session?.SessionID ?? ""     },
                { "payment_method_highlighted", selectedUPM.Type            ?? "none" },
            });
            
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Events

        public void OnConfirmPurchaseClick()
        {
            
            var selectedPaymentMethod = CHECKOUT.PaymentMethods.UserPaymentMethods.FirstOrDefault(x => x.IsSelected);

            if (selectedPaymentMethod == null)
            {
                Debug.LogError("No Payment Method Selected - this should never happen");
                return; // (Should never happen)
            }
            else if (selectedPaymentMethod.Type == "empty_card")
                Result = ViewResult.AddCard;
            else if (selectedPaymentMethod.Type == "empty_paypal")
                Result = ViewResult.AddPaypal;
            else
                this.Result = ViewResult.Confirm;

            // Analytics: Pay Button Clicked
            CheckoutAPI.InvokeAnalyticsEvent("pay_button_clicked", new Dictionary<string, object>
            {
                { "checkout_session_id", CHECKOUT.Session?.SessionID                 ?? ""      },
                { "payment_method",      selectedPaymentMethod?.Type                 ?? "none"  },
                { "purchase_amount",     CHECKOUT.Session?.SelectedProduct?.Amount   ?? 0m      },
                { "currency",            CHECKOUT.Session?.SelectedProduct?.Currency ?? ""      }
            });

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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helper Methods

        private void SetButtonDisplay(UserPaymentMethod upm)
        {
            if (upm == null) return;
            
            if  (upm.HasNonEmptyButtonSprite())
            {   
                var image    = PurchaseButton.gameObject.GetComponent<Image>();
                image.sprite = upm.GetButtonSprite();
                
                PurchaseButtonText.text = "";
                return;
            }
            else
            {
                var image    = PurchaseButton.gameObject.GetComponent<Image>();
                image.sprite = CHECKOUT.Sprites.CheckoutButtonSprite;
                
                if (upm.ButtonText != null)
                    PurchaseButtonText.text = upm.ButtonText;
                else
                    PurchaseButtonText.text = "";    
            }
        }
        
        
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

        public void GenerateTaxes()
        {
            foreach (Transform child in TaxesContainer.transform)
            {
                Destroy(child.gameObject);
            }

            var taxes = CheckoutClient.Instance.CurrentSession.Taxes;

            #if UNITY_EDITOR
            // These are Taxes added only for testing. Should be commented out later on
            // taxes.Clear();
            // taxes.Add("VAT",          new Shared.TaxItem { tax_amount = 9.90m,  inclusive = false });
            // taxes.Add("IRS",          new Shared.TaxItem { tax_amount = 5.50m,  inclusive = false });            
            // taxes.Add("CUSTOMS",      new Shared.TaxItem { tax_amount = 25.15m, inclusive = false });
            // taxes.Add("Delivery Fee", new Shared.TaxItem { tax_amount = 6.00m,  inclusive = false });
            #endif

            if (Checkout.CheckoutClient.Instance != null)
            {
                float SubTotal = 0f;
                var currencySign = Checkout.CheckoutClient.Instance.CurrentSession.SelectedProduct.Currency;

                if (float.TryParse(Checkout.CheckoutClient.Instance.CurrentSession.SelectedProduct.PriceText.Replace("$", "")
                                  ,NumberStyles.Float
                                  ,CultureInfo.InvariantCulture
                                  ,out float result))
                {
                    SubTotal = result;
                }

                // CultureInfo.InvariantCulture is important from parsing perspective from string to float as on mobile devices it can appear ",", instead "." in float values
                SubtotalPriceText.text = $"{currencySign}{SubTotal.ToString(CultureInfo.InvariantCulture)}";

                decimal TaxesAmount = 0;

                // If Location is USA or Canada generate taxes
                if (CHECKOUT.Globals.ShowTaxBreakdown)
                {
                    foreach (var tax in taxes)
                    {
                        CreateTaxPrefab(tax.Key, tax.Value.tax_amount.ToString(CultureInfo.InvariantCulture));
                        TaxesAmount += tax.Value.tax_amount;
                    }
                    
                    ShowTaxesPanels(true);
                    TaxText.gameObject.SetActive(false);
                    TaxText.text = $"{currencySign}{TaxesAmount.ToString(CultureInfo.InvariantCulture)}";
                    
                    TaxesAndFeesRow.gameObject.SetActive(false);
                }
                else
                {
                    foreach (var tax in taxes)
                    {
                        TaxesAmount += tax.Value.tax_amount;
                    }

                    ShowTaxesPanels(false);
                    TaxText.gameObject.SetActive(true);
                    TaxText.text = "Inclusive"; // $"${TaxesAmount.ToString(CultureInfo.InvariantCulture)}";
                    
                    TaxesAndFeesRow.gameObject.SetActive(true);
                }
                
                TotalPriceText.text = $"{currencySign}{(SubTotal + (float)TaxesAmount).ToString(CultureInfo.InvariantCulture)}";
                
                if (taxes.Count == 0)
                    TotalPriceText.text = PriceText.text;
            }
        }

        void CreateTaxPrefab(string taxName, string taxAmount)
        {
            var currencySign = Checkout.CheckoutClient.Instance.CurrentSession.SelectedProduct.Currency;
            var taxPrefab = Instantiate(original: TaxPrefab, parent: TaxesContainer.transform);
            taxPrefab.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = taxName;
            taxPrefab.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{currencySign}{taxAmount}";
        }

        void ShowTaxesPanels(bool status)
        {
            int TaxesPanelsAmount = TaxesPanels.Count;
            for (int i = 0; i < TaxesPanelsAmount; i++)
            {
                TaxesPanels[i].SetActive(status);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Test Scenarios

        public Step test_confirmPurchase()      => new Step(name: "checkout_panel_test_confirm_purchase", action: async (s) => { OnConfirmPurchaseClick();     });
        public Step test_select_other_methods() => new Step(name: "checkout_panel_test_select_other_pm",  action: async (s) => { OnOtherPaymentMethodsClick(); });
        public Step test_settings_page()        => new Step(action: async (s) => { OnSettingsClick(); });
    }
}
