using System.Linq;
using Galleon.Checkout.Foundation;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout
{
    #if UNITY_EDITOR
    [InitializeOnLoad]
    #endif
    public class Root : Entity
    {
        ////////////////////////////////////////////////////// Singleton
        
        private static Root _instance;
        public  static Root Instance => _instance ??= new Root();

        ////////////////////////////////////////////////////// Members
        
        public Runtime  Runtime  = new Runtime();
        public Project  Project  = new Project();
        public Assets   Assets   = new Assets();
        public Core     Core     = new Core();
        
        ////////////////////////////////////////////////////// Lifecycle
        
        static Root()
        {
            var root = Instance;    
        }
        
        public Root()
        {
        }
    }
}