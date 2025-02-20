using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Galleon.Checkout
{       
    public class CheckoutTestStore : IStore
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
            Debug.Log("===============================");
            Debug.Log("IAP - Retrieving Products :");
            foreach (var product in products)
                Debug.Log($"- {product.id} ({product.type})");
            Debug.Log("===============================");
        }

        public void Purchase(ProductDefinition product, string developerPayload)
        {
            Debug.Log("===============================");
            Debug.Log($"IAP - Purchasing {product.id} ({product.type})");
            Debug.Log("===============================");
        }

        public void FinishTransaction(ProductDefinition product, string transactionId)
        {
            Debug.Log("===============================");
            Debug.Log($"IAP - Finished transaction for : {product.id} ({product.type})");
            Debug.Log("===============================");
        }
    }
}