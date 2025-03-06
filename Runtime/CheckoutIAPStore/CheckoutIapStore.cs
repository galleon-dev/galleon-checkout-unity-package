using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace CheckoutIAPStore
{
    public class CheckoutIAPStore : IStore
    {
        //////////////////////////////////////////////////////////////////////////////////////// Members

        private IStoreCallback callback;
        
        //////////////////////////////////////////////////////////////////////////////////////// IStore Methods
        
        public void Initialize(IStoreCallback callback)
        {
            this.callback = callback;
            
            Debug.Log("===============================");
            Debug.Log($"IAP - Store Initialized");
            Debug.Log("===============================");
        }

        public void RetrieveProducts(ReadOnlyCollection<ProductDefinition> products)
        {
            ////////////////////////////////////////////////////////////////////////////
            Debug.Log("===============================");
            Debug.Log("IAP - Retrieving Products :");
            foreach (var product in products)
                Debug.Log($"- {product.id} ({product.type})");
            Debug.Log("===============================");
            ////////////////////////////////////////////////////////////////////////////
            
            var productDescriptions = new List<ProductDescription>();
            
            foreach (var product in products)
            {   
                productDescriptions.Add(new ProductDescription(id       : product.storeSpecificId
                                                              ,metadata : new ProductMetadata(priceString:    "$9.99"
                                                                                             ,title:          "My Product"
                                                                                             ,description:    "My Product Description"
                                                                                             ,currencyCode:   "USD"
                                                                                             ,localizedPrice: 9.99m)));
            }
            
            this.callback.OnProductsRetrieved(productDescriptions);
        }

        public void Purchase(ProductDefinition product, string developerPayload)
        {
            Debug.Log("===============================");
            Debug.Log($"IAP - Purchasing {product.id} ({product.type})");
            Debug.Log("===============================");
            
            // Simulate a successful purchase
            this.callback.OnPurchaseSucceeded(storeSpecificId       : product.storeSpecificId
                                             ,receipt               : "receipt"
                                             ,transactionIdentifier : "transactionId"
            );
        }

        public void FinishTransaction(ProductDefinition product, string transactionId)
        {
            Debug.Log("===============================");
            Debug.Log($"IAP - Finished transaction for : {product.id} ({product.type})");
            Debug.Log("===============================");
        }
    }
}