using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout.Foundation;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout
{
    public class Root : Entity
    {
        ////////////////////////////////////////////////////// Singleton
        
        private static Root _instance;
        public  static Root Instance => _instance;
        
        ////////////////////////////////////////////////////// Members
        
        public Context  Context  = new Context();
        public Global   Global   = new Global();
        public Runtime  Runtime  = new Runtime();
        
        ////////////////////////////////////////////////////// Lifecycle
        
        public static void CreateRoot()
        {
            _instance = new Root();
            Instance.Node.Initialize();
        }
    }
}
