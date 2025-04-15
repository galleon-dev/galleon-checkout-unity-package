using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Galleon;
using Galleon.Checkout;
using Galleon.SampleApp;
using UnityEngine;

namespace Galleon.Checkout.Samples
{
    
    public class StoreView : MonoBehaviour
    {
        //////////////////////////////////////////////////////////////////////// Members
        
        [Header("UI")]
        public GameObject StoreViewItemPrefab; 
        public GameObject StoreViewItemParent; 
        
        public List<CheckoutProduct> Products = new List<CheckoutProduct>();
        
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
            await CheckoutAPI.PurchaseGooglePay();
        }   
        public async void PurchaseGalleon()
        {
            await CheckoutAPI.PurchaseGalleon();
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