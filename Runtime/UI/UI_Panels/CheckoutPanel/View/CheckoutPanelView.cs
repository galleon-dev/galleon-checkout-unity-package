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

        public ViewResult Result = ViewResult.None;
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
        public TextMeshProUGUI ProductTitleText;
        public TextMeshProUGUI PriceText;
        public TextMeshProUGUI TaxText;

        [Header("Payment Methods")]
        public GameObject PaymentMethodsPanel;
        public GameObject PaymentMethodItemPrefab;
        public GameObject AddCreditCardButtonElement;

        public TMP_Dropdown DropdownMenu;

        [Header("Payment Buttons")]
        public GameObject PurchaseButton;
        public GameObject GooglePayButton;
        public GameObject PaypalPayButton;
        public GameObject ApplePayButton;

        [Header("Taxes")]
        public List<GameObject> TaxesPanels;
        public GameObject TaxesContainer;
        public GameObject TaxPrefab;
        public TextMeshProUGUI SubtotalPriceText;
        public TextMeshProUGUI TotalPriceText;

        private bool IsUSAorCanadaUser = false;

        public Config Configutation;

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
            CHECKOUT.PaymentMethods.UserPaymentMethodsToDisplay.FirstOrDefault()?.SelectExclusive();
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Refresh

        public override void RefreshState()
        {
            Debug.Log("RefreshState(): " + CheckoutClient.Instance.CurrentSession);

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
            this.PriceText.text = Checkout.CheckoutClient.Instance.CurrentSession.SelectedProduct.PriceText;

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

                var go = Instantiate(original: PaymentMethodItemPrefab, parent: PaymentMethodsPanel.transform);
                var item = go.GetComponent<checkoutPanelPaymentMethodItemView>();
                item.Initialize(paymentMethod, this);

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

            if (TaxesContainer != null)
            {
                GenerateTaxes();
            }
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

            var image = PurchaseButton.gameObject.GetComponent<Image>();
            image.sprite = SelectedItem.PaymentMethod.GetButtonSprite();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Events

        public void OnConfirmPurchaseClick()
        {
            var selectedPaymentMethod = CHECKOUT.PaymentMethods.UserPaymentMethodsToDisplay.FirstOrDefault(x => x.IsSelected);

            if (selectedPaymentMethod == null)
                return; // (Should never happen)
            else if (selectedPaymentMethod.Type == "empty_card")
                Result = ViewResult.AddCard;
            else if (selectedPaymentMethod.Type == "empty_paypal")
                Result = ViewResult.AddPaypal;
            else
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

        public void GenerateTaxes()
        {
            foreach (Transform child in TaxesContainer.transform)
            {
                Destroy(child.gameObject);
            }

            var taxes = CheckoutClient.Instance.TaxController.taxes;

#if UNITY_EDITOR
            // These are Taxes added only for testing. Should be commented out later on
            // taxes.Clear();
            // taxes.Add("VAT", new Shared.TaxItem { tax_amount = 9.90m, inclusive = false });
            // taxes.Add("IRS", new Shared.TaxItem { tax_amount = 5.50m, inclusive = false });            
            // taxes.Add("CUSTOMS",      new Shared.TaxItem { tax_amount = 25.15m, inclusive = false });
            // taxes.Add("Delivery Fee", new Shared.TaxItem { tax_amount = 6.00m,  inclusive = false });
#endif

            if (Checkout.CheckoutClient.Instance != null)
            {
                float SubTotal = 0f;
                if (float.TryParse(Checkout.CheckoutClient.Instance.CurrentSession.SelectedProduct.PriceText.Replace("$", ""), NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                {
                    SubTotal = result;
                }

                //Debug.Log("SubTotal Parsed: " + SubTotal);

                // CultureInfo.InvariantCulture is important from parsing perspective from string to float as on mobile devices it can appear ",", instead "." in float values
                SubtotalPriceText.text = $"${SubTotal.ToString(CultureInfo.InvariantCulture)}";

                decimal TaxesAmount = 0;

                // Debug.Log("Taxes Amount: " + taxes.Count);

                // If Location is USA or Canada generate taxes
                if (IsUSAorCanadaUser)
                {
                   // Debug.Log("USA/CANADA USER");
                    foreach (var tax in taxes)
                    {
                        CreateTaxPrefab(tax.Key, tax.Value.tax_amount.ToString(CultureInfo.InvariantCulture));
                        TaxesAmount += tax.Value.tax_amount;
                    }
                    ShowTaxesPanels(true);
                    TaxText.gameObject.SetActive(false);
                    TaxText.text = $"${TaxesAmount.ToString(CultureInfo.InvariantCulture)}";
                }
                else
                {
                   // Debug.Log("NOT USA/CANADA USER");
                    foreach (var tax in taxes)
                    {
                        //CreateTaxPrefab(tax.Key, tax.Value.tax_amount.ToString(CultureInfo.InvariantCulture));
                        TaxesAmount += tax.Value.tax_amount;
                    }

                    ShowTaxesPanels(false);
                    TaxText.gameObject.SetActive(true);
                    TaxText.text = "Inclusive"; // $"${TaxesAmount.ToString(CultureInfo.InvariantCulture)}";
                }
                TotalPriceText.text = $"${(SubTotal + (float)TaxesAmount).ToString(CultureInfo.InvariantCulture)}";
            }
        }

        void CreateTaxPrefab(string taxName, string taxAmount)
        {
            var taxPrefab = Instantiate(original: TaxPrefab, parent: TaxesContainer.transform);
            taxPrefab.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = taxName;
            taxPrefab.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = $"${taxAmount}";
        }

        void ShowTaxesPanels(bool status)
        {
            int TaxesPanelsAmount = TaxesPanels.Count;
            for (int i = 0; i < TaxesPanelsAmount; i++)
            {
                TaxesPanels[i].SetActive(status);
            }
        }

        public void SetUSAorCanadaUser(bool _IsUSAorCanadaUser)
        {
            IsUSAorCanadaUser = _IsUSAorCanadaUser;
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Test Scenarios

        public Step test_confirmPurchase() => new Step(name: "checkout_panel_test_confirm_purchase", action: async (s) => { OnConfirmPurchaseClick(); });
        public Step test_select_other_methods() => new Step(name: "checkout_panel_test_select_other_pm", action: async (s) => { OnOtherPaymentMethodsClick(); });
        public Step test_settings_page() => new Step(action: async (s) => { OnSettingsClick(); });

        /// Test Rule : On_Next("checkoutPanel").Do("confirm_purchase")
        /// Test Rule : On_ALL ("whatever")     .Do("confirm_purchase")


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Test

        public Step TEST_CHANGE_TAX_VIEW()
        =>
            new Step(name: $"TEST_CHANGE_TAX_VIEW"
                    , action: async (s) =>
                              {
                                  IsUSAorCanadaUser = !IsUSAorCanadaUser;
                                  this.Refresh();
                              });
    }
}

