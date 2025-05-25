using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public Logger                   Logger              = new();
        public Network                  Network             = new();
        public Config                   Config              = new();
        public Analytics                Analytics           = new();
        
        // APIs
        public CheckoutAPI              CheckoutAPI         = new();
        public CheckoutIAPStore         IAPStore            = new();
      //public CheckoutIapStoreListener IapStoreListener    = new(); // for testing
        
        // Resources
        [Header("Resources")]
        public CheckoutResources        Resources;
        
        // Entities
        [Header("Entities")]
        public ProductsController       Products            = new();
        public CreditCardsController    CreditCards         = new();
        public TokensController         Tokens              = new();
        public UsersController          Users               = new();
        public User                     CurrentUser         = new();
        public TransactionsController   Transactions        = new();
        
        // Sessions
        public CheckoutSession          CurrentSession;
        public List<CheckoutSession>    CheckoutSessions    = new List<CheckoutSession>();
        
        // UI
        [Header("UI")]
        public CheckoutScreenMobile     CheckoutScreenMobile; 
        
        // TEMP - testing/debug/wip/etc...
        CheckoutTEMP                    Temp                = new();
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Entry Point
        
        [RuntimeInitializeOnLoadMethod]
        public static async Task EntryPoint()
        {
            await Task.Yield();
            Debug.Log("CheckoutClient.EntryPoint()");
            
            Root.Instance.Runtime.Node.Children.Add(Instance);
            
            await Instance.Initialize();
            await Instance.Temp.TEST_FLOW();
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public Step Initialize() 
        => 
            new Step(name   : "initialize_checkout_client"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                              {
                                  // Services
                                  s.AddChildStep(Logger   .Initialize());
                                  s.AddChildStep(Network  .Initialize());
                                  s.AddChildStep(Config   .Initialize());
                                  s.AddChildStep(Analytics.Initialize());
                        
                                  // Resources
                                  this.Resources = CheckoutResources.Instance;
                                  s.AddChildStep(Resources.Initialize());
                        
                                  // Entities
                                  s.AddChildStep(Products    .Initialize());
                                  s.AddChildStep(CreditCards .Initialize());
                                  s.AddChildStep(Tokens      .Initialize());
                                  s.AddChildStep(Users       .Initialize());
                                  s.AddChildStep(Transactions.Initialize());
                              });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Main Flow Steps

        public Step RunCheckoutSession(CheckoutProduct product)
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
                                  
                                  // Start Session
                                  await CurrentSession.Flow();
                              });
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
    
    public static class CHECKOUT
    {
        public static Logger                 Logger         => CheckoutClient.Instance.Logger;
        public static Network                Network        => CheckoutClient.Instance.Network;
        public static Config                 Config         => CheckoutClient.Instance.Config;
        public static Analytics              Analytics      => CheckoutClient.Instance.Analytics;
        
        public static CheckoutResources      Resources      => CheckoutClient.Instance.Resources;
        
        public static ProductsController     Products       => CheckoutClient.Instance.Products;
        public static CreditCardsController  CreditCards    => CheckoutClient.Instance.CreditCards;
        public static TokensController       Tokens         => CheckoutClient.Instance.Tokens;
        public static UsersController        Users          => CheckoutClient.Instance.Users;
        public static User                   User           => CheckoutClient.Instance.CurrentUser;
        public static TransactionsController Transactions   => CheckoutClient.Instance.Transactions;
        public static Transaction            Transaction    => User.CurrentTransaction;
    }   
}

