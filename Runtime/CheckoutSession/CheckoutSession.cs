using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Galleon.Checkout.Shared;
using Galleon.Checkout.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace Galleon.Checkout
{
    public class CheckoutSession : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        // Purchase data
        public CheckoutProduct                    SelectedProduct;
        public PurchaseResult                     PurchaseResult        = new PurchaseResult();
        
        // Simple Dialog Panel data
        public string                             LastDialogRequest     = null;
        public SimpleDialogPanelView.DialogResult LastDialogResult      = SimpleDialogPanelView.DialogResult.None;
        public PaymentMethod                      PaymentMethodToDelete = null;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties
        
        public CheckoutClient                     Client                => CheckoutClient.Instance;
        public User                               User                  => Client.CurrentUser;
        public Transaction                        CurrentTransaction    => User.CurrentTransaction;
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Last transaction result
        
        public TransactionResultData lastTransactionResult;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Temp
        
        public static event Action Report;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Steps
        
        private Step _flow; 
        public  Step Flow()
        => _flow ??=
            new Step(name   : $"checkout_session_flow"
                    ,tags   : new [] { "report"}
                    ,action : async (s) =>
                    {
                        
                        /////////////////////////////////////// Pre Steps
                        
                        // Open Screen
                        s.AddPreStep(CheckoutScreenMobile.OpenCheckoutScreenMobile());
                        
                        /////////////////////////////////////// Steps
                        
                        // View CheckoutPage
                        s.AddChildStep(Client.CheckoutScreenMobile.ViewPage(Client.CheckoutScreenMobile.CheckoutPage));
                        
                        /////////////////////////////////////// Post Steps
                        
                        // Report
                        s.AddPostStep("report", async x =>
                                               {
                                                   Report?.Invoke();
                                               });
                        
                        // Close
                        s.AddPostStep(CheckoutScreenMobile.EndCheckoutScreenMobile());

                    });
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// transaction Steps
        
        public Step RunTransaction()
        =>
            new Step(name   : $"run_transaction"
                    ,action : async (s) =>
                    {
                        ////////////////////////////////////////////////////////////// Pre Steps
                        
                        // Show Loading Screen
                        s.AddPreStep(Client.CheckoutScreenMobile.SetPage(Client.CheckoutScreenMobile.LoadingPage));
                        
                        // Start Transaction
                        s.AddPreStep("start_transaction"
                                      ,async x => CHECKOUT.Network.Get($"{CHECKOUT.Network.SERVER_BASE_URL}/start_transaction"));
                        
                        
                        // Setup Transaction Steps
                        User.CurrentTransaction = new Transaction();
                        foreach (var stepFunc in User.SelectedPaymentMethod.TransactionSteps)
                        {
                            var step = stepFunc?.Invoke();
                            s.Log($"adding transaction step : {step.Name}");
                            User.CurrentTransaction.TransactionSteps.Add(step);
                        }
                        
                        ////////////////////////////////////////////////////////////// Steps
                        
                        // Run Transaction Steps
                        foreach (var transactionStep in User.CurrentTransaction.TransactionSteps)
                        {
                            s.Log($"scheduling transaction step : {transactionStep.Name}");
                            s.AddChildStep(transactionStep);
                        }
                        
                        
                        s.AddChildStep("get_transaction_result"
                                      ,async x => CHECKOUT.Network.Get($"{CHECKOUT.Network.SERVER_BASE_URL}/transaction_result"));
                        
                        
                        // Navigate
                        s.AddChildStep("wait",        async x => await Task.Delay(1000));
                        s.AddChildStep("set_success", async x => Client.CheckoutScreenMobile.NavigationNext = "Success");
                        s.AddChildStep(Client.CheckoutScreenMobile.Navigate());
                       
                        ////////////////////////////////////////////////////////////// Post Steps
                        
          //            CheckoutClient.Instance.IAPStore.FinishTransaction(product        : new ProductDefinition(id   : CheckoutClient.Instance.CurrentSession.SelectedProduct.DisplayName
          //                                                                                         ,type : ProductType.Consumable)
          //                                                   ,transactionId : "transactionID");
          //
                        
                        // Finally, handle transaction result
                        s.AddPostStep(HandleTransactionResult());
                    });
        
        public Step HandleTransactionResult()
        =>
            new Step(name   : $"handle_transaction_result"
                    ,action : async (s) =>
                    { 
                        ///////// TEMP
                        this.lastTransactionResult = new TransactionResultData()
                                                     {
                                                        isSuccess         = true,
                                                        errors            = null,
                                                        isCanceled        = false,
                                                        transaction_id    = "test_transaction",
                                                     };
                        
                        var result = this.lastTransactionResult;
                        
                        this.PurchaseResult = new PurchaseResult()
                                              {
                                                  IsSuccess   = result.isSuccess,
                                                  IsCanceled  = result.isSuccess,
                                                  Errors      = result.errors?.ToList(),
                                                  IsError     = result.errors?.Length > 0,
                                              };
                    });

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Credit Card Vaulting Flow

        public Step AddCard()
        =>
            new Step(name   : $"add_card"
                    ,action : async (s) =>
                    {
                        foreach (var vaultingStepFunc in User.SelectedPaymentMethod.VaultingSteps)
                        {
                            var step = vaultingStepFunc?.Invoke();
                            s.AddChildStep(step);
                        }
                    });
        
        public Step ViewPage()
        =>
            new Step(name   : $"view_page"
                    ,action : async (s) =>
                    {
                        
                    });
        
                                  

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Nav
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Nav
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Nav
        
        public class NavigationController
        {
            
        }
        
        public class NavigationLayer
        {
            public string          ID;
            public List<INavEntry> History;
        }
        
        public interface INavEntry
        {
            
        }
        
            public class fakePage : INavEntry
            {
                
            }
            
            public class fakeScreen : INavEntry
            {
                
            }
            
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}


