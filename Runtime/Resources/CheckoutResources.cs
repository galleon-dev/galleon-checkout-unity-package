using System.Linq;
using UnityEngine;

namespace Galleon.Checkout
{
    [CreateAssetMenu(fileName = "CheckoutResources", menuName = "Galleon/CheckoutResources")]
    public class CheckoutResources : ScriptableObject
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////// Singleton
        
        private static CheckoutResources instance;
        public  static CheckoutResources Instance
        {
            get
            {
                if (instance == null)
                    instance = Resources.LoadAll<CheckoutResources>("CheckoutResources").Single();
                
                return instance;
            }
        }
        
        /////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public Step Initialize
        =>
            new Step(name   : $"initialize_resources"
                    ,action : async (s) =>
                    {   
                    });
        
        /////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public GameObject CheckoutPopupPrefab;       
    }
}
