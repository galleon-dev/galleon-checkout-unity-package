#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout.Foundation
{
    #if UNITY_EDITOR
    [InitializeOnLoad]
    #endif
    public class Bootstrap
    {
        static Bootstrap()
        {
            Root.CreateRoot();
        }
    }
}