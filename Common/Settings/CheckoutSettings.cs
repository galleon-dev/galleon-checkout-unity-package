#define GALLEON_DEV

using UnityEngine;

namespace Galleon.Checkout
{
    #if GALLEON_DEV
    [CreateAssetMenu(fileName = "CheckoutSettings", menuName = "Galleon/Settings/CheckoutSettings")]
    #endif
    public partial class CheckoutSettings : ScriptableObject
    {
        public bool InitializeOnLoad = true;
    }
}