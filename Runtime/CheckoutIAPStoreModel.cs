using Galleon.Checkout;
using UnityEngine;
using UnityEngine.Purchasing.Extension;

public class CheckoutIAPStoreModel : AbstractPurchasingModule
{
    public override void Configure()
    {
        RegisterStore("GalleonCheckout", InstantiateStore());
    }

    private IStore InstantiateStore()
    {
        return new CheckoutTestStore();
    }
}
