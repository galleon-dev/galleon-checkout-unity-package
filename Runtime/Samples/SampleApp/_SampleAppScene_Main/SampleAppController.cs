using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Galleon.Checkout;
using Galleon.Checkout.Samples;
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
            // await Task.Yield();
            // await Task.Yield();
            // Root.Instance.Runtime.TestController.Test().Execute();
            
            await CheckoutAPI.Initialize(new CheckoutConfiguration());
            
            if (CHECKOUT.IsTest)
                await TestCheckout();
        
            SampleAppStart().Execute(); 
        }
        
        public async Task TestCheckout()
        {
            var result = await CheckoutAPI.Purchase(new CheckoutProduct
                                           { 
                                               DisplayName = "test product",
                                               PriceText   = "$5.99",
                                           });
            
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle Steps
        
        public Step SampleAppStart() 
        =>
            new Step(name   : $"sample_app_start"
                    ,action : async (s) =>
                    {   
                    });
        
        
        public Step OnBackToStoreScreen() 
        =>
            new Step(name   : $"on_back_to_store_screen"
                    ,action : async (s) =>
                    {   
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Test Steps
        
        public Step TestPurchaseProduct1() 
        =>
            new Step(name   : $"test_purchase_product_1"
                    ,action : async (s) =>
                    {   
                        var result = await CheckoutAPI.Purchase(new CheckoutProduct
                                                               { 
                                                                   DisplayName = "test_product_1",
                                                                   PriceText   = "$5.99",
                                                               });
                        
                        Debug.Log($"==================");
                        Debug.Log($"result : ");
                        Debug.Log($"{result.IsSuccess}");
                        Debug.Log($"==================");
                        
                        OnBackToStoreScreen().Execute();
                    });
        
        
        public Step TestPurchaseProduct2() 
        =>
            new Step(name   : $"test_purchase_product_2"
                    ,action : async (s) =>
                    {   
                        var result = await CheckoutAPI.Purchase(new CheckoutProduct
                                                               { 
                                                                   DisplayName = "test_product_2",
                                                                   PriceText   = "$19.99",
                                                               });
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Misc
        
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
