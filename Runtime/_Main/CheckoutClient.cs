using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout.Foundation;
using Galleon.Checkout.UI;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Galleon.Checkout
{
    public class CheckoutClient : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Instance
        
        private static CheckoutClient instance;
        public  static CheckoutClient Instance => instance ?? (instance = new CheckoutClient());
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Children
        
        // Services
        [Header("Services")]
        public Logger                       Logger                      = new();
        public Network                      Network                     = new();
        public Config                       Config                      = new();
        public Analytics                    Analytics                   = new();
        public DeepLinkController           DeepLinks                   = new();
        public ExternalURLsController       URLs                        = new();
        
        // APIs
        public CheckoutAPI                  CheckoutAPI                 = new();
        public CheckoutIAPStore             IAPStore                    = new();
      //public CheckoutIapStoreListener     IapStoreListener            = new(); // for testing
      
        // Resources
        public CheckoutResources            Resources                   => CheckoutResources.Instance;
        
        // System
        public NavigationController         Navigation                  = new ();
        
        // Controllers
        [Header("Controllers")]
        public TokenizerController          TokenizerController         = new(); 
        public PaymentMethodsController     PaymentMethodsController    = new();
        public TaxController                TaxController               = new();  
        
        // Payment Actions
        public CheckoutActionsController    CheckoutActions              = new();
        
        // Entities
        [Header("Entities")]
        public ProductsController           Products                    = new();
        public UsersController              Users                       = new();
        public User                         CurrentUser                 = new();
        
        // Sessions
        public CheckoutSession              CurrentSession;
        public List<CheckoutSession>        CheckoutSessions            = new List<CheckoutSession>();
        
        // UI
        [Header("UI")]
        public CheckoutScreenMobile         CheckoutScreenMobile; 
        
        // TEMP - testing/debug/wip/etc...
        [Header("Temp")]
        public CheckoutTEMP                 Temp                        = new();
        
        public string                       CurrentTest                 = "test_1";
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Entry Point
        
        [RuntimeInitializeOnLoadMethod]
        public static async Task EntryPoint()
        {
            await Task.Yield();
            Debug.Log("CheckoutClient.EntryPoint()");
            
            Root.Instance.Runtime.Node.Children.Add(Instance);
            Instance.Node.Initialize();
            
          //await Instance.SystemInitFlow();
          //
          //if (CHECKOUT.IsTest)
          //    await Instance.Temp.TEST_FLOW();
                
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public Step SystemInitFlow() 
        => 
            new Step(name   : "checkout_init_flow"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                              {   
                                  // Services
                                  s.AddChildStep(Logger                         .Initialize());
                                  s.AddChildStep(Network                        .Initialize());
                                  s.AddChildStep(Config                         .Initialize());
                                  s.AddChildStep(Analytics                      .Initialize());
                        
                                  // Controllers
                                  s.AddChildStep(TokenizerController            .Initialize());
                                  s.AddChildStep(PaymentMethodsController       .Initialize());
                                
                              //s.AddChildStep(TaxController                  .Initialize());
                                  
                                  // Resources
                                  s.AddChildStep(Resources                      .Initialize());
                        
                                  // Entities
                                  s.AddChildStep(Products                       .Initialize());
                                  s.AddChildStep(Users                          .Initialize());
                                  
                                  // UI
                                  s.AddChildStep(CheckoutScreenMobile.InitializeCheckoutScreenMobile());
                              });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Main Flow Steps

        public Step CreateCheckoutSession(CheckoutProduct product)
        =>
            new Step(name   : $"run_checkout_session"
                    ,action : async (s) =>
                              {
                                  // Archive last session
                                  if (CurrentSession != null)
                                      CheckoutSessions.Add(CurrentSession);
                                  
                                  // Create new session
                                  CurrentSession = new CheckoutSession();
                                  
                                  // Assign product
                                  CurrentSession.SelectedProduct = product;
                              });
        
        public Step RunCheckoutSession()
        =>
            new Step(name   : $"run_checkout_session"
                    ,action : async (s) =>
                              {
                                  // Start Session
                                  await CurrentSession.Flow().Execute();
                              });
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
    
    public static class CHECKOUT
    {
        public static Logger                    Logger          => CheckoutClient.Instance.Logger;
        public static Network                   Network         => CheckoutClient.Instance.Network;
        public static Config                    Config          => CheckoutClient.Instance.Config;
        public static Analytics                 Analytics       => CheckoutClient.Instance.Analytics;
         
        public static CheckoutResources         Resources       => CheckoutClient.Instance.Resources;
        
        public static CheckoutSession           Session         => CheckoutClient.Instance.CurrentSession;
        
        public static NavigationController      Navigation       => CheckoutClient.Instance.Navigation;
        public static CheckoutActionsController Actions          => CheckoutClient.Instance.CheckoutActions;
        
        public static PaymentMethodsController  PaymentMethods  => CheckoutClient.Instance.PaymentMethodsController;
        public static ProductsController        Products        => CheckoutClient.Instance.Products;
        public static UsersController           Users           => CheckoutClient.Instance.Users;
        public static User                      User            => CheckoutClient.Instance.CurrentUser;
        public static Transaction               Transaction     => User.CurrentTransaction;
         
        public static bool                      IsTest          => Resources.IsTest;
        public static string                    CurrentTest     { get => CheckoutClient.Instance.CurrentTest; set => CheckoutClient.Instance.CurrentTest = value; } 
    }   
}

