//#define GALLEON_PROD

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
using Environment = Galleon.Checkout.Environment;

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
            
            Root.Instance.Runtime.Node.AddChild(this);
        }
        
        async void Start()
        {
            ReportText.text = "> Initializing ... ";
            
            var user = $"test_user_{DateTime.Now.ToString()}";
            
            CHECKOUT.Globals.IsInternal = true;
            await CheckoutAPI.Initialize(new CheckoutConfiguration()
                                         {
                                            #if GALLEON_PROD
                                            JWT                     = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhcHBJZCI6ImRpY2UucHJvZC5hcHAiLCJpYXQiOjE3NTczMjUxNzZ9.vx5KdC6JVTsxtw4YmSFwSgy4UUw1RtRe5r3bHUqYJhk",  // PROD
                                            #elif GALLEON_PROD2
                                            JWT                     = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhcHBJZCI6ImRpY2UucHJvZC5hcHAiLCJpYXQiOjE3NTczMjUxNzZ9.vx5KdC6JVTsxtw4YmSFwSgy4UUw1RtRe5r3bHUqYJhk",  // PROD
                                            #elif DEBUG
                                            JWT                     = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhcHBJZCI6ImRpY2Uuc2IuYXBwIiwiaWF0IjoxNzU2Nzk5OTA4fQ.JzzQK4LWemC_VVITMUd-N1B8Ej6ORLdd5rv46LWFK44",    // TEST
                                            #else 
                                            JWT                     = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhcHBJZCI6ImRpY2UucHJvZC5hcHAiLCJpYXQiOjE3NTczMjUxNzZ9.vx5KdC6JVTsxtw4YmSFwSgy4UUw1RtRe5r3bHUqYJhk",  // PROD
                                            #endif
                                         
                                            #if DEBUG
                                            AppUserID               = user,
                                            #else
                                            AppUserID               = $"dice.prod.app",
                                            #endif
                                            ApplicationDisplayName  = "Dice Dreams",
                                            DeepLinkName            = "test.app",
                                            Config                  = new()
                                                                    {
                                                                       { "is_preselection_screen_enabled", false },
                                                                    },
                                            Country                 = "US",
                                            Currency                = "USD",
                                            #if GALLEON_PROD || GALLEON_PROD2
                                            Environment             = Environment.Prod,
                                            #else
                                            Environment             = Environment.Test,
                                            #endif
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
            
            ReportText.text = "> Reinitializing ... ";
            
            // cleanup
            CheckoutClient.Instance.Cleanup();
         
            // define user Id
            var appUserID = CHECKOUT.Globals.TestUser == "new" 
                                                       ? $"test_user_{DateTime.Now.ToString()}" 
                                                       : CHECKOUT.Globals.TestUser;
            
            var country  = "US";
            var currency = "USD";
            
            if (CHECKOUT.Globals.TestCountry.ToLower() != "dont override")
                country  = CHECKOUT.Globals.TestCountry;
            if (CHECKOUT.Globals.TestCurrency.ToLower() != "dont override")
                currency = CHECKOUT.Globals.TestCurrency;
            
            // Reinitialize if needed
            CHECKOUT.Globals.IsInternal = true;
            await CheckoutAPI.Initialize(new CheckoutConfiguration()
                                         {
                                            #if PROD
                                            JWT                     = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhcHBJZCI6ImRpY2UucHJvZC5hcHAiLCJpYXQiOjE3NTczMjUxNzZ9.vx5KdC6JVTsxtw4YmSFwSgy4UUw1RtRe5r3bHUqYJhk",  // PROD
                                            #elif PROD2
                                            JWT                     = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhcHBJZCI6ImRpY2UucHJvZC5hcHAiLCJpYXQiOjE3NTczMjUxNzZ9.vx5KdC6JVTsxtw4YmSFwSgy4UUw1RtRe5r3bHUqYJhk",  // PROD
                                            #elif DEBUG
                                            JWT                     = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhcHBJZCI6ImRpY2Uuc2IuYXBwIiwiaWF0IjoxNzU2Nzk5OTA4fQ.JzzQK4LWemC_VVITMUd-N1B8Ej6ORLdd5rv46LWFK44",    // TEST
                                            #else 
                                            JWT                     = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhcHBJZCI6ImRpY2UucHJvZC5hcHAiLCJpYXQiOjE3NTczMjUxNzZ9.vx5KdC6JVTsxtw4YmSFwSgy4UUw1RtRe5r3bHUqYJhk",  // PROD
                                            #endif
                                         
                                            #if DEBUG
                                            AppUserID               = appUserID,
                                            #else
                                            AppUserID               = $"dice.prod.app",
                                            #endif
                                         
                                            ApplicationDisplayName  = "Dice Dreams",
                                            DeepLinkName            = "test.app",
                                            Config                  = new()
                                                                    {
                                                                       { "is_preselection_screen_enabled", false }
                                                                    },
                                            Country                 = country,
                                            Currency                = currency,
                                            #if GALLEON_PROD || GALLEON_PROD2
                                            Environment             = Environment.Prod,
                                            #else
                                            Environment             = Environment.Test,
                                            #endif
                                         });
            
            ReportText.text = $"> ready. \n> user is <color=yellow>{appUserID}</color>.";
            
        }
        
        private decimal GetCurrencyEquivalentFor1USD(string currency)
        {
            return currency.ToUpper() switch
            {
                "USD" => 1.00m,
                "EUR" => 0.92m,
                "GBP" => 0.79m,
                "JPY" => 149.50m,
                "CNY" => 7.24m,
                "INR" => 83.12m,
                "CAD" => 1.36m,
                "AUD" => 1.53m,
                "CHF" => 0.88m,
                "KRW" => 1340.00m,
                "BRL" => 4.97m,
                "MXN" => 16.80m,
                "RUB" => 92.00m,
                "TRY" => 32.15m,
                "ILS" => 3.64m,
                "SEK" => 10.87m,
                "NOK" => 10.96m,
                "DKK" => 6.88m,
                "PLN" => 3.98m,
                "ZAR" => 18.23m,
                _     => 1.00m
            };
        }

        public async Task Purchase()
        {
            if (isInitializing) return;

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            var currency = "USD";
            if (CHECKOUT.Globals.TestCurrency.ToLower() != "dont override")
                currency = CHECKOUT.Globals.TestCurrency;

            var conversionRate = GetCurrencyEquivalentFor1USD(currency);

            CheckoutProduct product = default;
            switch (CHECKOUT.Globals.TestProduct.ToLower())
            {
                case "coins" :     var amount1 = 1.00m * conversionRate;
                                   product = new CheckoutProduct()
                                           {
                                               DisplayName = "Bunch Of Coins",
                                             //Sku         = "sku-1-3DS",
                                               Sku         = "sku-1",
                                               Amount      = amount1,
                                               Currency    = currency,
                                           }; break;
                case "spins" :     var amount2 = 1.99m * conversionRate;
                                   product = new CheckoutProduct()
                                           {
                                               DisplayName = "Bunch Of Spins",
                                               Sku         = "sku-2",
                                               Amount      = amount2,
                                               Currency    = currency,
                                           }; break;
                case "spins 3ds" : var amount3 = 1.99m * conversionRate;
                                   product = new CheckoutProduct()
                                           {
                                               DisplayName = "Bunch Of Spins (3DS)",
                                               Sku         = "sku-3-3DS",
                                               Amount      = amount3,
                                               Currency    = currency,
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
                                                                        #if !PROD2
                                                                        AllowedPaymentMethodTypes = new ()
                                                                                                     {
                                                                                                         "native",
                                                                                                         "card",
                                                                                                         "link",
                                                                                                         //"galleon-wrapped-paypal-vaulted",
                                                                                                         "paypal",
                                                                                                         "amazon_pay",
                                                                                                         "google_pay_browser",
                                                                                                        // "web_checkout",
                                                                                                     }
                                                                        #endif
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
        string lastCountry    = "dont override";
        string lastCurrency   = "dont override";
        bool   isInitializing = false;
        
        public void Update()
        {
            if (Time.realtimeSinceStartup < 1)
                return;
            
            if (isInitializing) return;
            
            if (lastUser     != CHECKOUT.Globals.TestUser
            ||  lastCountry  != CHECKOUT.Globals.TestCountry
            ||  lastCurrency != CHECKOUT.Globals.TestCurrency)
            {
                lastUser     = CHECKOUT.Globals.TestUser;
                lastCountry  = CHECKOUT.Globals.TestCountry;
                lastCurrency = CHECKOUT.Globals.TestCurrency;
                
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
