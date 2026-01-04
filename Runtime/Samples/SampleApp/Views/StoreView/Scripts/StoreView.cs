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
        public TMP_Dropdown drp_products;
        public TMP_Dropdown drp_preselection;
        public TMP_Dropdown drp_tax;
        public TMP_Dropdown drp_footer;
        
        [Header("Config")]
        public GameObject ConfigPanel;
        public GameObject ConfigPanelItemTemplate;
        
        
        // Products
        public List<CheckoutProduct> Products = new List<CheckoutProduct>();
        
        
        //////////////////////////////////////////////////////////////////////// Properties
        
        private SampleAppController _sampleAppController;
        public SampleAppController SampleAppController => _sampleAppController ??= GameObject.FindObjectOfType<SampleAppController>();
        
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


///////////////////////////////////////////////////////////////////////////////////////////////////
//namespace Galleon.TEMP
//{
//    public class NativeKeyboard
//    {           
//        // Create a method to show the credit card keyboard
//        public void ShowCreditCardKeyboard()
//        {
//            if (Application.platform == RuntimePlatform.Android)
//            {
//                AndroidJavaClass  unityPlayer     = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
//                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
//                AndroidJavaObject editText        = CreateCreditCardEditText(currentActivity);
//                
//                // Show the keyboard with credit card input type
//                editText.Call("requestFocus");
//                AndroidJavaObject inputMethodManager = GetInputMethodManager(currentActivity);
//                inputMethodManager.Call("showSoftInput", editText, 0);
//            }
//        }
//        
//        private AndroidJavaObject CreateCreditCardEditText(AndroidJavaObject activity)
//        {
//            AndroidJavaObject editText = new AndroidJavaObject("android.widget.EditText", activity);
//            
//            // Set the credit card input type (16 is TYPE_CLASS_NUMBER | TYPE_CREDIT_CARD)
//            editText.Call("setInputType", 0x00000010 | 0x00001000);
//            
//            // Enable autofill
//            if (AndroidBuildVersion() >= 26) // Android 8.0 (Oreo) or higher
//            {
//                editText.Call("setImportantForAutofill", 1); // IMPORTANT_FOR_AUTOFILL_YES
//                editText.Call("setAutofillHints",        new string[] { "creditCardNumber" });
//            }
//            
//            return editText;
//        }
//        
//        private AndroidJavaObject GetInputMethodManager(AndroidJavaObject activity)
//        {
//            AndroidJavaObject context                 = activity.Call<AndroidJavaObject>("getApplicationContext");
//            AndroidJavaClass  inputMethodManagerClass = new AndroidJavaClass("android.view.inputmethod.InputMethodManager");
//            return context.Call<AndroidJavaObject>("getSystemService", "input_method");
//        }
//        
//        private int AndroidBuildVersion()
//        {
//            AndroidJavaClass buildVersion = new AndroidJavaClass("android.os.Build$VERSION");
//            return buildVersion.GetStatic<int>("SDK_INT");
//        }
//    }
//}