
using UnityEngine;
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
        #if UNITY_EDITOR
        static Bootstrap()
        {
            Root.CreateRoot();
        }
        #else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InitializeOnLoad() => Root.CreateRoot();
        #endif
        
    }
}