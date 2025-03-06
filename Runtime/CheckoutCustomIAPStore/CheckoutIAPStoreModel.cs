using Galleon.Checkout;
using UnityEngine;
using UnityEngine.Purchasing.Extension;

public class CheckoutIAPStoreModel : AbstractPurchasingModule
{
    //////////////////////////////////////////////////////////// Singleton
    
    private static CheckoutIAPStoreModel instance;
    public  static CheckoutIAPStoreModel Instance
    {
        get
        {
            if (instance == null)
                instance = new CheckoutIAPStoreModel();
            
            return instance;
        }
    }
    
    //////////////////////////////////////////////////////////// AbstractPurchasingModule Methods 
    
    public override void Configure()
    {
        RegisterStore("GalleonCheckout", InstantiateStore());
    }

    private IStore InstantiateStore()
    {
        return new CheckoutIAPStore();
    }
}
