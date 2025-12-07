using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Galleon.Checkout;
using Galleon.Checkout.Samples;
using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.SampleApp
{
    public class SampleAppController : MonoBehaviour, IEntity
    {
        ////////////////////////////////////////////////////////////////////// Members
        
        public StoreView StoreView;
        public TMP_Text  ReportText;
        
        ////////////////////////////////////////////////////////////////////// Lifecycle

        public SampleAppController()
        {
            this.Node = new EntityNode(this);
        }
        
        void Awake()
        {
            Debug.Log($"SampleAppController.Awake");
            
            this.Node.Initialize();
            Root.Instance.Runtime.Node.AddChild(this);
        }
        
        async void Start()
        {
            ReportText.text = "> Initializing ... ";
            
          //await CheckoutAPI.Initialize(new CheckoutConfiguration() { AppUserID = $"levan", ApplicationDisplayName = "Dice Dreams"} );
            await CheckoutAPI.Initialize(new CheckoutConfiguration()
                                         {
                                            AppUserID              = $"test_user_{DateTime.Now.ToString()}",
                                            ApplicationDisplayName = "Dice Dreams"
                                         
                                         } );
            
            Debug.Log($"Is Test Mode : {CHECKOUT.IsTest}");
            if (CHECKOUT.IsTest)
            {
                await Task.Delay(1000);
                CheckoutClient.Instance.Storage.ClearAll();
                Root.Instance.Runtime.TestController.Test().Execute();    
            }
            
            CHECKOUT.PaymentMethods.ClearSavedData();
            RefreshConfig();
            
            SampleAppStart().Execute();
            
            ReportText.text = "> ready";
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle Steps
        
        public Step SampleAppStart() 
        =>
            new Step(name   : $"sample_app_start"
                    ,action : async (s) =>
                    {   
                        Debug.Log("Sample App Start");
                    });
        
        
        public Step OnBackToStoreScreen() 
        =>
            new Step(name   : $"on_back_to_store_screen"
                    ,action : async (s) =>
                    {   
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Events
        
        public void On_DropdownValueChanged(int value)
        {
            RefreshConfig();
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Methods
        
        public void RefreshConfig()
        {
            switch (this.StoreView.drp_preselection.captionText.text.ToLower())
            {
                case "enabled"  : CHECKOUT.Globals.IsPreselectionEnabled = true;           break;
                case "disabled" : CHECKOUT.Globals.IsPreselectionEnabled = false;          break;
                default         : CHECKOUT.Globals.clear_override_IsPreselectionEnabled(); break;
            }

            switch (this.StoreView.drp_footer.captionText.text.ToLower())
            {
                case "regular"    : CHECKOUT.Globals.ShowLongFooter = false;          break;
                case "california" : CHECKOUT.Globals.ShowLongFooter = true;           break;
                default           : CHECKOUT.Globals.clear_override_ShowLongFooter(); break;
            }

            switch (this.StoreView.drp_tax.captionText.text.ToLower())
            {
                case "inclusive" : CHECKOUT.Globals.ShowTaxBreakdown = false;          break;
                case "show full" : CHECKOUT.Globals.ShowTaxBreakdown = true;           break;
                default          : CHECKOUT.Globals.clear_override_ShowTaxBreakdown(); break;
            }
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Test Steps
        
        public Step TestPurchaseProduct1() 
        =>
            new Step(name   : $"test_purchase_product_1"
                    ,action : async (s) =>
                    {   
                        //////////////////////////////////////////////////////////////////////////////////////////////// 

                        CHECKOUT.Config.SetOverrideValue("is_preselection_screen_enabled", true);
                        CHECKOUT.Config.SetOverrideValue("show_tax_breakdown",             true);
                        CHECKOUT.Config.SetOverrideValue("show_log_footer",                true);
                        
                        ////////////////////////////////////////////////////////////////////////////////////////////////
                        
                        ReportText.text = $@"
> Test Scenario 1
----------------------------------------------------
> Product : 
    > name  : ''bunch of spins''
    > price : $5.99
    > sku   : 'sku-1'
----------------------------------------------------
> Payment Method : new credit card
----------------------------------------------------
> Preselection : true
----------------------------------------------------
> show tax : true
----------------------------------------------------
> Is California : true
----------------------------------------------------
                                          ";
                        
                        new Step(name: $"test_scenario_1", tags: new [] {"report"} ).Execute();
                        
                        ////////////////////////////////////////////////////////////////////////////////////////////////
                        
                        await Task.Delay(1000);
                        
                        //////////////////////////////////////////////////////////////////////////////////////////////// 
                        
                        var result = await CheckoutAPI.Purchase(new CheckoutProduct
                                                               { 
                                                                   DisplayName     = "bunch of coins",
                                                                   PriceText       = "$5.99",
                                                                   //Sku           = "sku-1-3DS", 
                                                                   Sku             = "sku-1",
                                                                   Amount          = 100,
                                                                   Currency        = "USD",
                                                               });
                        
                        ////////////////////////////////////////////////////////////////////////////////////////////////
                         
                        Debug.Log($"==================");
                        Debug.Log($"result : ");
                        Debug.Log($"{result.IsSuccess}");
                        Debug.Log($"==================");
                        
                        ////////////////////////////////////////////////////////////////////////////////////////////////
                        
                        OnBackToStoreScreen().Execute();
                    });
        
        
        public Step TestPurchaseProduct2() 
        =>
            new Step(name   : $"test_purchase_product_2"
                    ,action : async (s) =>
                    {   
                        //////////////////////////////////////////////////////////////////////////////////////////////// 
                        
                        
                        CHECKOUT.Config.SetOverrideValue("is_preselection_screen_enabled", false);
                        CHECKOUT.Config.SetOverrideValue("show_tax_breakdown",             false);
                        CHECKOUT.Config.SetOverrideValue("show_log_footer",                false);
                        
                        //////////////////////////////////////////////////////////////////////////////////////////////// 
                         
                        ReportText.text = @"
> Test Scenario 2
----------------------------------------------------
> Product : 
    > name  : ''bunch of spins''
    > price : $19.99
    > sku   : 'sku-2'
----------------------------------------------------
> Payment Method : first existing credit card
----------------------------------------------------
> Preselection : false
----------------------------------------------------
> show tax : false
----------------------------------------------------
> Is California : false
----------------------------------------------------
                                          ";
                        
                        new Step(name: $"test_scenario_1", tags: new [] {"report"} ).Execute();
                        
                        ////////////////////////////////////////////////////////////////////////////////////////////////
                        
                        await Task.Delay(1000);
                        
                        //////////////////////////////////////////////////////////////////////////////////////////////// 
                        
                        var result = await CheckoutAPI.Purchase(new CheckoutProduct
                                                               { 
                                                                   DisplayName     = "bunch of spins",
                                                                   PriceText       = "$19.99",
                                                                   //Sku           = "sku-1-3DS", 
                                                                   Sku             = "sku-2",
                                                                   Amount          = 100,
                                                                   Currency        = "USD",
                                                               });
                        
                        ////////////////////////////////////////////////////////////////////////////////////////////////
                         
                        Debug.Log($"==================");
                        Debug.Log($"result : ");
                        Debug.Log($"{result.IsSuccess}");
                        Debug.Log($"==================");
                        
                        ////////////////////////////////////////////////////////////////////////////////////////////////
                        
                        OnBackToStoreScreen().Execute();
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Misc
        
        
        #if UNITY_EDITOR
        [ContextMenu("Dump Steps")]
        #endif
        public void DumpSteps()
        {
            Debug.Log(Root.Instance.Context.StepController.DumpSteps());
        }
        
        #if UNITY_EDITOR
        [MenuItem("Tools/Galleon/Open Test Scene")]
        #endif
        public static void OpenTestScene()
        {
            string scenePath = "Packages/com.galleon.checkout/Runtime/Samples/SampleApp/_SampleAppScene.unity";
         
            if (!string.IsNullOrEmpty(scenePath))
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
                }
                else
                    UnityEngine.SceneManagement.SceneManager.LoadScene(scenePath);
                #endif

            }
            else
            {
                Debug.LogError("Scene path is not provided or invalid.");
            }
        }

        public EntityNode Node { get; set; }
    }
}
