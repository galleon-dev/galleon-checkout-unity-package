using System.Collections;
using System.Collections.Generic;
using Galleon.Checkout;
using UnityEngine;
using UnityEngine.Purchasing;

public class CheckoutIapStoreListener : IStoreListener
{
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
    
    public Step Initialize()
    =>
        new Step(name   : $"initialize_store_listener"
                ,action : async (s) =>
                {    
                    var module                = StandardPurchasingModule.Instance();
                    module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
                    
                    var builder = ConfigurationBuilder.Instance(CheckoutIAPStoreModel.Instance, module);
                });
    
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// IStoreListener Interface
    
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError(error.ToString());
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError(error.ToString());
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"Product {product.definition.id} failed to Purchase because : {failureReason}");
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log($"CheckoutIapStoreListener is initialized, controller: {controller}, extensions: {extensions}");
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
}
