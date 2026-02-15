using System;
using System.Collections;
using System.Collections.Generic;
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
                dropdown.options.Clear();
                foreach (var possibleValue in configValue.possibleValues)
                    dropdown.options.Add(new TMP_Dropdown.OptionData(possibleValue.DisplayName));
                
                // set event
                dropdown.onValueChanged.AddListener(delegate { ON_DropdownValueChanged(dropdown.value); });
            }
        }
        
        public void ON_DropdownValueChanged(int value)
        {
            var dropdowns = GetComponentsInChildren<TMP_Dropdown>();
            foreach (var dropdown in dropdowns)
            {
                var configValue = CHECKOUT.Globals.GlobalValues.Find(x => x.displayName == dropdown.gameObject.name.Replace("drp_", ""));
                
                if (dropdown.value.ToString().ToLower() == "dont override")
                    configValue.ClearOverrideValue();
                else
                {
                    var valueDisplayName = dropdown.options[dropdown.value].text;
                    var actualValue      = configValue.possibleValues.Find(x => x.DisplayName == valueDisplayName);
                    configValue.OverrideValue(actualValue.Value);
                }
            }

            foreach (var v in CHECKOUT.Globals.GlobalValues)
            {
                Debug.Log($"- {v.displayName, -15} = ({v.Value.GetType().Name}) {v.Value}");
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
