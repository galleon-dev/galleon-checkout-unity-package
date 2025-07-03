using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Galleon.Checkout
{
    public class CheckoutIAPStore : Entity, IStore
    {
        //////////////////////////////////////////////////////////////////////////////////////// Members

        private IStoreCallback callback;
        
        public List<ProductDescription> ProductDescriptions = new List<ProductDescription>(); 
        
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
            
            foreach (var product in products)
            {   
                this.ProductDescriptions.Add(new ProductDescription(id       : product.storeSpecificId
                                                                   ,metadata : new ProductMetadata(priceString:    "$9.99"
                                                                                                  ,title:          "My Product"
                                                                                                  ,description:    "My Product Description"
                                                                                                  ,currencyCode:   "USD"
                                                                                                  ,localizedPrice: 9.99m)));
            }
            
            this.callback.OnProductsRetrieved(this.ProductDescriptions);
        }

        public async void Purchase(ProductDefinition product, string developerPayload)
        {
            Debug.Log("===============================");
            Debug.Log($"IAP - Purchasing {product.id} ({product.type})");
            Debug.Log("===============================");
            
            await CheckoutClient.Instance.RunCheckoutSession(product: new CheckoutProduct()
                                                                      {
                                                                          DisplayName = this.ProductDescriptions.First(p => p.storeSpecificId == product.storeSpecificId).metadata.localizedTitle,
                                                                          PriceText   = this.ProductDescriptions.First(p => p.storeSpecificId == product.storeSpecificId).metadata.localizedPriceString
                                                                      })
                                                                      .Execute();
            
            // Simulate a successful purchase
            this.callback.OnPurchaseSucceeded(storeSpecificId       : product.storeSpecificId
                                             ,receipt               : "receipt"
                                             ,transactionIdentifier : "transactionID"
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
