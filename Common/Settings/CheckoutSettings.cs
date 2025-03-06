using UnityEngine;

namespace Galleon.Checkout
{
    [CreateAssetMenu(fileName = "CheckoutSettings", menuName = "Galleon/CheckoutSettings")]
    public class CheckoutSettings : ScriptableObject
    {
        public bool InitializeOnLoad = true;
    }
}