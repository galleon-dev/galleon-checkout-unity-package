using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout;
using Galleon.Checkout.Samples;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
            
            this.Node.LateInitialize();
            Root.Instance.Runtime.Node.AddChild(this);
        }
        
        async void Start()
        {
            ReportText.text = "> Initializing ... ";
            
            var user = $"test_user_{DateTime.Now.ToString()}";
            await CheckoutAPI.Initialize(new CheckoutConfiguration()
                                         {
                                            JWT                     = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhcHBJZCI6ImRpY2Uuc2IuYXBwIiwiaWF0IjoxNzU2Nzk5OTA4fQ.JzzQK4LWemC_VVITMUd-N1B8Ej6ORLdd5rv46LWFK44",
                                            AppUserID               = $"test_user_{DateTime.Now.ToString()}",
                                            ApplicationDisplayName  = "Dice Dreams",
                                            DeepLinkName            = "test.app",
                                            Config                  = new()
                                                                    {
                                                                       { "is_preselection_screen_enabled", false },
                                                                    },
                                            Country                 = "US",
                                            Currency                = "USD"
                                         });
            
            StoreView.RefreshConfigPanel();
            
            Debug.Log($"Is Test Mode : {CHECKOUT.IsTest}");
            if (CHECKOUT.IsTest)
            {
                StoreView.ConfigPanel.gameObject.GetComponentInParent<ScrollRect>().gameObject.SetActive(false);
                
                await Task.Delay(1000);
                CheckoutClient.Instance.Storage.ClearAll();
                Root.Instance.Runtime.TestController.Test().Execute();    
            }
            
            CHECKOUT.PaymentMethods.ClearSavedData();
            
            SampleAppStart().Execute();
            
            ReportText.text = $"> ready. \n> user is <color=yellow>{user}</color>.";
        }
        
        
        public async Task Reinitialize()
        {
            ///////////////////////////////////////////////////////////////////////////////////// Reinitialize
            
            ReportText.text = "> User swapped - Reinitializing ... ";
            
            // cleanup
            CheckoutClient.Instance.Cleanup();
         
            // define user Id
            var appUserID = CHECKOUT.Globals.TestUser == "new" 
                                                       ? $"test_user_{DateTime.Now.ToString()}" 
                                                       : CHECKOUT.Globals.TestUser;
            
            // Reinitialize if needed
            await CheckoutAPI.Initialize(new CheckoutConfiguration()
                                         {
                                            JWT                     = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhcHBJZCI6ImRpY2Uuc2IuYXBwIiwiaWF0IjoxNzU2Nzk5OTA4fQ.JzzQK4LWemC_VVITMUd-N1B8Ej6ORLdd5rv46LWFK44",
                                            AppUserID               = $"test_user_{DateTime.Now.ToString()}",
                                            ApplicationDisplayName  = "Dice Dreams",
                                            DeepLinkName            = "test.app",
                                            Config                  = new()
                                                                    {
                                                                       { "is_preselection_screen_enabled", false }
                                                                    },
                                            Country                 = "US",
                                            Currency                = "USD"
                                         });
            
            ReportText.text = $"> ready. \n> user is <color=yellow>{appUserID}</color>.";
            
        }
        
        public async Task Purchase()
        { 
            if (isInitializing) return;
           
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            
            CheckoutProduct product = default;
            switch (CHECKOUT.Globals.TestProduct.ToLower())
            {
                case "coins" :     product = new CheckoutProduct()
                                           { 
                                               DisplayName = "Bunch Of Coins",
                                               PriceText   = "$24.99",
                                             //Sku         = "sku-1-3DS", 
                                               Sku         = "sku-1",
                                               Amount      = 24.99m,
                                               Currency    = "USD",
                                           }; break;
                case "spins" :     product = new CheckoutProduct()
                                           { 
                                               DisplayName = "Bunch Of Spins",
                                               PriceText   = "$1000.99",
                                               Sku         = "sku-2", 
                                               Amount      = 1000.99m,
                                               Currency    = "USD",
                                           }; break;
                case "spins 3ds" : product = new CheckoutProduct()
                                           { 
                                               DisplayName = "Bunch Of Spins (3DS)",
                                               PriceText   = "$1000.99",
                                               Sku         = "sku-3-3DS", 
                                               Amount      = 1000.99m,
                                               Currency    = "USD",
                                           }; break;
                
            }
            
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            
            var bonusData = new List<BonusItem>()
                          {
                              new BonusItem() { PaymentMethodType = "card",    BonusMainText = "200",  BonusRewardText = "Get Extra Rolls" },
                              new BonusItem() { PaymentMethodType = "default", BonusMainText = "100k", BonusRewardText = "Get Extra Rolls" }
                          };
            
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            
            var result = await CheckoutAPI.Purchase(product       : product
                                                   ,configuration : new CheckoutPurchaseConfiguration()
                                                                   {
                                                                       BonusData = bonusData,
                                                                       Config = new()
                                                                                {
                                                                                    { "is_preselection_screen_enabled", false },
                                                                                }
                                                                        ,
                                                                        AllowedPaymentMethodTypes = new ()
                                                                                                     {
                                                                                                         "native",
                                                                                                         "card",
                                                                                                         "credit_card",
                                                                                                         "link",
                                                                                                         "amazon_pay"
                                                                                                     }
                                                                   });
            
            
            Debug.Log("==========================================");
            Debug.Log("Purchase Result: " + result?.ToString());
            Debug.Log("==========================================");
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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Events

        string lastUser       = "new";
        bool   isInitializing = false;
        
        public void Update()
        {
            if (isInitializing) return;
            
            if (lastUser != CHECKOUT.Globals.TestUser)
            {
                lastUser = CHECKOUT.Globals.TestUser;
                StartReinitializing();    
            }
        }
        
        public async void StartReinitializing()
        {
            isInitializing = true;
            StoreView.GetComponentsInChildren<TMP_Dropdown>().ToList().ForEach(x=> x.interactable = false);
            await Reinitialize();
            StoreView.GetComponentsInChildren<TMP_Dropdown>().ToList().ForEach(x=> x.interactable = true);
            isInitializing = false;
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
                                                                   Amount          = 5.99m,
                                                                   Currency        = "USD",
                                                               },
                                                               new CheckoutPurchaseConfiguration()
                                                               {
                                                                
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
                                                                   Amount          = 19.99m,
                                                                   Currency        = "USD",
                                                               },
                                                                new CheckoutPurchaseConfiguration()
                                                                {
                                                                    
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
            Debug.Log(Root.Instance.Context.SystemServices.StepController.DumpSteps());
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
