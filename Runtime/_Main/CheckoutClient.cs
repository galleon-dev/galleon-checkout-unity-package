using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Galleon.Checkout
{
    public class CheckoutClient
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Instance
        
        private static CheckoutClient instance;
        public  static CheckoutClient Instance => instance ?? (instance = new CheckoutClient());
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Children
        
        // Services
        public Logger                       Logger           = new();
        public Network                      Network          = new();
        public Config                       Config           = new();
        public Analytics                    Analytics        = new();
        
        // Resources
        public CheckoutResources            Resources;
        
        // Entities
        public ProductsController           Products         = new();
        public CreditCardsController        CreditCards      = new();
        public TokensController             Tokens           = new();
        public UsersController              Users            = new();
        public TransactionsController       Transactions     = new();
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Entry Point
        
        [RuntimeInitializeOnLoadMethod]
        public static async Task EntryPoint()
        {
            Debug.Log("CheckoutClient.EntryPoint()");
            
            await Instance.Initialize.Execute();
            await Instance.TEST_FLOW.Execute();
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public Step Initialize 
        => 
            new Step(name   : "initialize_checkout_client"
                    ,tags   : new[] { "init"}
                    ,action : async s =>
                    {
                        // Services
                        s.AddChildStep(Logger   .Initialize);
                        s.AddChildStep(Network  .Initialize);
                        s.AddChildStep(Config   .Initialize);
                        s.AddChildStep(Analytics.Initialize);
                        
                        // Resources
                        this.Resources = CheckoutResources.Instance;
                        s.AddChildStep(Resources.Initialize);
                        
                        // Entities
                        s.AddChildStep(Products    .Initialize);
                        s.AddChildStep(CreditCards .Initialize);
                        s.AddChildStep(Tokens      .Initialize);
                        s.AddChildStep(Users       .Initialize);
                        s.AddChildStep(Transactions.Initialize);
                    });
        
        
        public Step TEST_FLOW
        => 
            new Step(name   : "TEST_FLOW"
                    ,tags   : new[] { "init"}
                    ,action : async s =>
                    {
                        // Test
                        s.AddChildStep(this.SetupTest);
                        s.AddChildStep(this.TestConfig);
                        s.AddChildStep(this.TestToken);
                        s.AddChildStep(this.TestTransaction);
                        s.AddChildStep(this.TestUI);
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Main Flow

        public Step SetupTest
        =>
            new Step(name   : $"setup_test"
                    ,action : async (s) =>
                    {
                        User user = new User();
                        this.Users.Users.Add(user);
                    });
        
        
        public Step TestConfig
        =>
            new Step(name   : $"test_config"
                    ,action : async (s) =>
                    {
                        foreach (var pair in CHECKOUT.Config.ConfigData)
                        {
                            s.Log($"{pair.Key} : {pair.Value}");
                        }
                    });


        public Step TestToken
        =>
            new Step(name   : $"test_token"
                    ,action : async (s) =>
                    {
                        var token = await Tokens.CreateCreditCardToken();
                        var user  = Users.Users.FirstOrDefault();
                        user.Tokens.Add(token);
                    });

        public Step TestTransaction
        =>
            new Step(name   : $"test_transaction"
                    ,action : async (s) =>
                    {
                        var transaction = await Transactions.CreateTestTransaction();
                        s.AddChildStep(transaction.Purchase);
                    });

        public Step TestUI
        =>
            new Step(name   : $"test_ui"
                    ,action : async (s) =>
                    {
                        GameObject.Instantiate(Resources.CheckoutPopupPrefab);
                    });
        
    }
    
    public static class CHECKOUT
    {
        public static Logger                  Logger      => CheckoutClient.Instance.Logger;
        public static Network                 Network     => CheckoutClient.Instance.Network;
        public static Config                  Config      => CheckoutClient.Instance.Config;
        public static Analytics               Analytics   => CheckoutClient.Instance.Analytics;
        
        public static CheckoutResources       Resources   => CheckoutClient.Instance.Resources;
        
        public static ProductsController      Products    => CheckoutClient.Instance.Products;
        public static CreditCardsController   CreditCards => CheckoutClient.Instance.CreditCards;
        public static TokensController        Tokens      => CheckoutClient.Instance.Tokens;
        public static UsersController         Users       => CheckoutClient.Instance.Users;
        public static TransactionsController  Transaction => CheckoutClient.Instance.Transactions;
    }   
}
