using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
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
        
        // SessionData
        public string                             SessionID             = "1";
        
        // Purchase data
        public CheckoutProduct                    SelectedProduct;
        public PurchaseResult                     PurchaseResult        = default;
        
        public Dictionary<string, string>         Metadata              = new();
        
        // Simple Dialog Panel data
        public string                             LastDialogRequest         = null;
        public SimpleDialogPanelView.DialogResult LastDialogResult          = SimpleDialogPanelView.DialogResult.None;
        public UserPaymentMethod                  userPaymentMethodToDelete = null;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties
        
        public CheckoutClient                     Client                => CheckoutClient.Instance;
        public User                               User                  => Client.CurrentUser;
        public Transaction                        CurrentTransaction    => User.CurrentTransaction;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Last transaction result
        
        public ChargeResultData                   lastChargeResult;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Temp
        
        public static event Action Report;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Steps
        
        private Step _flow; 
        public  Step Flow()
        => _flow ??=
            new Step(name   : $"checkout_session_flow"
                    ,tags   : new [] { "report" }
                    ,action : async (s) =>
                    {
                        /////////////////////////////////////// Pre Steps
                        
                        // Open Screen
                        s.AddPreStep(CheckoutScreenMobile.OpenCheckoutScreenMobile());
                        s.AddPreStep(Client.CheckoutScreenMobile.SetPage(Client.CheckoutScreenMobile.CheckoutLoadingPage));
                        s.AddPreStep(StartSession());
                        
                        /////////////////////////////////////// Steps
                        
                        // View CheckoutPage
                      //s.AddChildStep(CheckoutClient.Instance.TaxController.GetTaxInfo());
                      //s.AddChildStep("wait",        async x => await Task.Delay(1000));
                        s.AddChildStep("tax_success", async x => Client.CheckoutScreenMobile.NavigationNext = "checkout");
                        s.AddChildStep(Client.CheckoutScreenMobile.Navigate());
                        
                        /////////////////////////////////////// Post Steps
                        
                        // Report
                        s.AddPostStep("report", async x =>
                                                {
                                                    Report?.Invoke();
                                                });
                        
                        // Close
                        s.AddPostStep(CheckoutScreenMobile.EndCheckoutScreenMobile());
                        s.AddPostStep(EndCheckoutSession());

                    });
        
        
        public Step EndCheckoutSession()
        =>
            new Step(name   : $"end_checkout_session"
                    ,action : async (s) =>
                    {
                        if (PurchaseResult == null)
                        {
                            this.PurchaseResult = new PurchaseResult()
                                                  {
                                                      IsSuccess  = false,
                                                      IsCanceled = true,
                                                      IsError    = false,
                                                      Errors     = new(),
                                                  };
                        }
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Session Steps
        
        public Step StartSession()
        =>
            new Step(name   : $"start_session"
                    ,action : async (s) =>
                    {   
                        var response = await CHECKOUT.Network.Post<CheckoutSessionResponse>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/checkout-session/create"
                                                                                           ,headers  : new ()
                                                                                                     {
                                                                                                         { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                                     }
                                                                                           ,body     : new Shared.CheckoutSessionRequest()
                                                                                                     {
                                                                                                        order      = new OrderDetails()
                                                                                                                   {
                                                                                                                       sku      = "sku-1-3DS", // SelectedProduct.DisplayName,
                                                                                                                       amount   = 100,
                                                                                                                       currency = "USD",
                                                                                                                   },
                                                                                                        expires_at = DateTime.UtcNow.AddDays(1),
                                                                                                        metadata   = CHECKOUT.Session.Metadata,
                                                                                                     });
                        
                        
                        this.SessionID = response.session_id;           
                    });
        
        public Step CancelSession() 
        =>
            new Step(name   : $"cancel_session"
                    ,action : async (s) =>
                    {
                        var response = await CHECKOUT.Network.Post<CancelCheckoutSessionResponse>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/checkout-session/cancel"
                                                                                                 ,headers  : new ()
                                                                                                           {
                                                                                                               { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                                           }
                                                                                                 ,body     : new Shared.CancelCheckoutSessionRequest()
                                                                                                           {
                                                                                                              session_id = CHECKOUT.Session.SessionID,
                                                                                                           });
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
                        s.AddPreStep(StartTransaction());
                        
                        ////////////////////////////////////////////////////////////// Transaction Steps
                        
                        // Setup Transaction Steps
                        User.CurrentTransaction = new Transaction();
                        foreach (var stepFunc in User.SelectedUserPaymentMethod.TransactionSteps)
                        {
                            var step = stepFunc?.Invoke();
                            s.Log($"adding transaction step : {step.Name}");
                            User.CurrentTransaction.TransactionSteps.Add(step);
                        }
                        
                        // Add Child Transaction Steps
                        foreach (var transactionStep in User.CurrentTransaction.TransactionSteps)
                        {
                            s.Log($"scheduling transaction step : {transactionStep.Name}");
                            s.AddChildStep(transactionStep);
                        }
                        
                        ////////////////////////////////////////////////////////////// Final Navigation Step
                        
                        // Navigate
                        //s.AddChildStep("wait",        async x => await Task.Delay(1000));
                      //s.AddChildStep("set_success", async x => Client.CheckoutScreenMobile.SetPage(Client.CheckoutScreenMobile.SuccessPage));
                        s.AddChildStep("set_success", async x => Client.CheckoutScreenMobile.NavigationNext = "Success");
                        s.AddChildStep(Client.CheckoutScreenMobile.Navigate());
                       
                        ////////////////////////////////////////////////////////////// Post Steps
                        
                        ///////// TEMP
                        this.lastChargeResult = new ChargeResultData()
                                              {
                                                 is_success  = true,
                                                 errors      = null,
                                                 is_canceled = false,
                                                 charge_id   = "test_transaction",
                                              };
                        
                        // Finally, handle transaction result
                        s.AddPostStep(HandleTransactionResult());
                    });
        
        
        public Step StartTransaction()
        =>
            new Step(name   : $"start_transaction"
                    ,action : async (s) =>
                    {
                        var card        = User.SelectedUserPaymentMethod as CreditCardUserUserPaymentMethod;
                      //card.CardNumber = "4242424242424242";
                      //card.CardMonth  = "12";
                      //card.CardYear   = "2026";
                      //card.CardCCV    = "123";
                      //await card.GetTokenizer().Execute();
                      //await card.Tokenize()    .Execute();
                        
                        var cardToken = card.TokenID;
                    });
        
        public Step HandleTransactionResult()
        =>
            new Step(name   : $"handle_transaction_result"
                    ,action : async (s) =>
                    {
                        ///////// IAP
                        // CheckoutClient.Instance.IAPStore.FinishTransaction(product        : new ProductDefinition(id : CheckoutClient.Instance.CurrentSession.SelectedProduct.DisplayName, type : ProductType.Consumable)
                        //                                                   ,transactionId  : "transactionID");

                        var result = this.lastChargeResult;
                        
                        this.PurchaseResult = new PurchaseResult()
                                              {
                                                  IsSuccess   = result.is_success,
                                                  IsCanceled  = result.is_canceled,
                                                  Errors      = result.errors?.ToList(),
                                                  IsError     = result.errors?.Length > 0,
                                              };
                    });
        
    }
}
