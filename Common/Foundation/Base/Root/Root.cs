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
        
        public Context  Context  = new Context();
        public Runtime  Runtime  = new Runtime();
        public Project  Project  = new Project();
        
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

