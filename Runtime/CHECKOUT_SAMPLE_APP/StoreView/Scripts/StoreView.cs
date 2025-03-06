using System;
using System.Collections;
using System.Collections.Generic;
using Galleon;
using Galleon.Checkout;
using Galleon.SampleApp;
using UnityEngine;

public class StoreView : MonoBehaviour
{
    //////////////////////////////////////////////////////////////////////// Members
    
    [Header("Temp")]
    public GameObject Panel;
    
    [Header("UI")]
    public GameObject StoreViewItemPrefab; 
    public GameObject StoreViewItemParent; 
    
    public List<Product> Products = new List<Product>();
    
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
    
    public async void OnClick()
    {
        Instantiate(Panel);
    }   
    
    //////////////////////////////////////////////////////////////////////// Helper Methods
    
    public GameObject AddStoreViewItem(Product product)
    {
        // var go              = Instantiate(StoreViewItemPrefab, StoreViewItemParent.transform);
        // var item            = go.GetComponent<StoreViewItem>();
        // item.Product        = product;
        // item.TitleText.text = $"BUY \n<b>{product.DisplayName}</b>";
        // 
        // return go;
        
        return default;
    }
    
}
