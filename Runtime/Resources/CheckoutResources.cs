#define GALLEON_DEV

using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif

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
        
        public Step Initialize()
        =>
            new Step(name   : $"initialize_resources"
                    ,tags   : new []{ "init" }
                    ,action : async (s) =>
                    {   
                    });
        
        /////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        [Header("Screens")]
        public GameObject CheckoutPopupPrefab;
        
        [Header("UI Elements")]
        public GameObject UI_Seporator;
        
        [Header("Tests")]
        public bool IsTest = false;
    }
    
    
    #if UNITY_EDITOR && UNITY_CLOUD_BUILD
    [InitializeOnLoad]
    public static class CheckoutResourcesPreBuild
    {
        static CheckoutResourcesPreBuild()
        {
            CheckoutResources.Instance.IsTest = true;
            EditorUtility.SetDirty(CheckoutResources.Instance);
            AssetDatabase.SaveAssets();
        }
    }
    #endif
}
