using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galleon;
using Galleon.Checkout;
using Galleon.SampleApp;
using TMPro;
using UnityEngine;

namespace Galleon.Checkout.Samples
{
    public class StoreView : MonoBehaviour
    {
        //////////////////////////////////////////////////////////////////////// Members
        
        [Header("UI")]
        public GameObject StoreViewItemPrefab; 
        public GameObject StoreViewItemParent; 
        
        [Header("test")]
        public GameObject ScenarioPrefab;
        public GameObject ScenarioParent;
        public TMP_Text   ScenarioText;
        
        [Header("Config")]
        public GameObject ConfigPanel;
        public GameObject ConfigPanelItemTemplate;
        
        // Products
        public List<CheckoutProduct> Products = new List<CheckoutProduct>();
        
        
        //////////////////////////////////////////////////////////////////////// Properties
        
        private SampleAppController _sampleAppController;
        public  SampleAppController SampleAppController => _sampleAppController ??= GameObject.FindObjectOfType<SampleAppController>();
        
        //////////////////////////////////////////////////////////////////////// Lifecycle

        public async void Start()
        {
            // // Initialize checkout API
            // await CheckoutAPI.Initialize();
            // 
            // // Populate Products
            // var products = await CheckoutAPI.GetProducts();
            // this.Products.AddRange(products);
            // 
            // // Populate UI
            // foreach (var product in products)
            // {
            //     AddStoreViewItem(product);
            // }
            
            #if GALLEON_PROD || GALLEON_PROD2
            var button = GameObject.Find("buy_galleon");
            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                
                #if GALLEON_PROD
                buttonText.text += $"\n<color=red>PROD</color>";
                #elif GALLEON_PROD2
                buttonText.text += $"\n<color=red>PROD 2</color>";
                #endif
            #endif
        }

        //////////////////////////////////////////////////////////////////////// UI Events
        
        public async void PurchaseGooglePay()
        {
            // IAP button component in the gameobject
        }   
        
        public async void PurchaseGalleon()
        {   
            await SampleAppController.Purchase();
        }   
        
        //////////////////////////////////////////////////////////////////////// Config Dropdown Methods
        
        public async Task RefreshConfigPanel()
        {
            // Destroy all existing children
            foreach (Transform child in ConfigPanel.transform)
                if (child.gameObject.activeSelf)
                    Destroy(child.gameObject);
            
            
            foreach (var configValue in CHECKOUT.Globals.GlobalValues)
            {
                // Skip configs that have nothing to pick from (e.g. the currency_sign_* overrides).
                // A dropdown with zero options would throw on the first selection.
                if (configValue.possibleValues == null || configValue.possibleValues.Count == 0)
                    continue;

                // Instantiate item
                var item     = Instantiate(ConfigPanelItemTemplate, ConfigPanel.transform);
                item.SetActive(true);

                // Get ui components
                var label    = item.GetComponentInChildren<TMP_Text>();
                var dropdown = item.GetComponentInChildren<TMP_Dropdown>();
                dropdown.gameObject.name = $"drp_{configValue.displayName}";

                // set label
                label.text = configValue.displayName;

                // set options
                dropdown.ClearOptions();
                foreach (var possibleValue in configValue.possibleValues)
                    dropdown.options.Add(new TMP_Dropdown.OptionData(possibleValue.DisplayName));

                dropdown.SetValueWithoutNotify(0);
                dropdown.RefreshShownValue();

                // set event
                dropdown.onValueChanged.AddListener(delegate { ON_DropdownValueChanged(dropdown.value); });
            }
        }
        
        public async void ON_DropdownValueChanged(int value)
        {
            var dropdowns = GetComponentsInChildren<TMP_Dropdown>();
            foreach (var dropdown in dropdowns)
            {
                var configValue = CHECKOUT.Globals.GlobalValues.Find(x => x.displayName == dropdown.gameObject.name.Replace("drp_", ""));

                // Not a config dropdown, or it has no options to read - leave it alone.
                if (configValue == null)
                    continue;

                if (dropdown.value < 0 || dropdown.value >= dropdown.options.Count)
                    continue;

                var valueDisplayName = dropdown.options[dropdown.value].text;
                var actualValue      = configValue.possibleValues.Find(x => x.DisplayName == valueDisplayName);

                if (actualValue == null)
                    continue;

                // A possible value of null means "dont override" - fall back to the config's default.
                if (actualValue.Value == null)
                    configValue.ClearOverrideValue();
                else
                    configValue.OverrideValue(actualValue.Value);

                #region test_country and test_currency

                // If test_country dropdown changed, update test_currency to match
                if (configValue.displayName == "test_country")
                {
                    var currencyDropdown = dropdowns.FirstOrDefault(x => x.gameObject.name == "drp_test_currency");
                    var countryValue     = configValue.Value as string;

                    if (currencyDropdown != null && !string.IsNullOrEmpty(countryValue))
                    {
                        // Find currency option by splitting display name and matching currency code
                        var currencyIndex = currencyDropdown.options.FindIndex(opt =>
                        {
                            var displayNameParts = opt.text.Split(' ');
                            var countryCode = displayNameParts.Length > 0 ? displayNameParts[0] : "";
                            return countryCode == countryValue;
                        });

                        if (currencyIndex >= 0)
                        {
                            // Update dropdown value without triggering another event
                            currencyDropdown.value = currencyIndex;
                        }
                    }
                }

                #endregion
            }

            foreach (var v in CHECKOUT.Globals.GlobalValues)
            {
                Debug.Log($"- {v.displayName, -15} = ({v.Value?.GetType().Name ?? "null"}) {v.Value}");
            }

            
        }

        //////////////////////////////////////////////////////////////////////// Helper Methods
        
        public GameObject AddStoreViewItem(CheckoutProduct checkoutProduct)
        {
            var go              = Instantiate(StoreViewItemPrefab, StoreViewItemParent.transform);
            var item            = go.GetComponent<StoreViewItem>();
            item.Product        = checkoutProduct;
            item.TitleText.text = $"BUY \n<b>{checkoutProduct.DisplayName}</b>";
            
            return go;
        }
    }
}
