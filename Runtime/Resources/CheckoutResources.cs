
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif

namespace Galleon.Checkout
{
    [CreateAssetMenu(fileName = "CheckoutResources", menuName = "Galleon/Checkout/CheckoutResources")]
    public class CheckoutResources : ScriptableObject
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////// Singleton
        
        private static CheckoutResources instance;
        public  static CheckoutResources Instance
        {
            get
            {
                if (instance == null)
                    instance = Resources.Load<CheckoutResources>("Checkout_Resources");
                
                return instance;
            }
        }
        
        /////////////////////////////////////////////////////////////////////////////////////////////////// Entity
        
        public EntityNode Node { get; set; }
        
        /////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public CheckoutResources()
        {
            
        }
        
        public Step Initialize()
        =>
            new Step(name   : $"initialize_resources"
                    ,tags   : new []{ "init" }
                    ,action : async (s) =>
                    {
                        s.Log($"Checkout Assets : {CheckoutAssets}");
                        
                        
                        // Load Assets
                        var asset = Resources.Load<CheckoutAssets>("CheckoutAssets");
                        this._checkoutAssets = asset;
                        
                        // Validations
                        // if (assets.Length < 1) throw new Exception("No CheckoutAssets found");
                        // if (assets.Length > 1) throw new Exception("More than one CheckoutAssets found");
                        
                    });
        
        /////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        [Header("Screens")]
        public GameObject CheckoutPopupPrefab;
		public GameObject CheckoutPopupLandscapePrefab;
        
        [Header("UI Elements")]
        public GameObject UI_Seporator;
        
        [Header("Tests")]
        public bool IsTest = false;
        
        [Header("Sprites")]
        public CheckoutSprites Sprites;
        
        /////////////////////////////////////////////////////////////////////////////////////////////////// Public Assets
        
        private CheckoutAssets _checkoutAssets;
        public  CheckoutAssets CheckoutAssets => _checkoutAssets;
    }
    
    
    //#if UNITY_EDITOR && UNITY_CLOUD_BUILD
    //[InitializeOnLoad]
    //public static class CheckoutResourcesPreBuild
    //{
    //    static CheckoutResourcesPreBuild()
    //    {
    //        CheckoutResources.Instance.IsTest = true;
    //        EditorUtility.SetDirty(CheckoutResources.Instance);
    //        AssetDatabase.SaveAssets();
    //    }
    //}
    //#endif
}
