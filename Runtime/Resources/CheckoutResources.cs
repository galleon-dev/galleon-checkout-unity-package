#define GALLEON_DEV

using System.Linq;
using UnityEngine;

namespace Galleon.Checkout
{
    #if GALLEON_DEV
    #endif
    [CreateAssetMenu(fileName = "CheckoutResources", menuName = "Galleon/Checkout/CheckoutResources")]
    public partial class CheckoutResources : ScriptableObject, IEntity
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
        
        /////////////////////////////////////////////////////////////////////////////////////////////////// Entity
        
        public EntityNode Node { get; set; }
        
        /////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public CheckoutResources()
        {
            Node = new EntityNode(this);
        }
        
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
