#define GALLEON_DEV

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
                    instance = Resources.LoadAll<CheckoutResources>("").Single();
                
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
                        s.Log($"Checkout Assets : {CheckoutAssets}");
                        
                        
                        // Load Assets
                        var assets = Resources.LoadAll<CheckoutAssets>("");
                        this._checkoutAssets = assets.First();
                        
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
