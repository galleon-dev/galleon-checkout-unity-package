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
        
        // Simple Dialog Panel data
        public string                             LastDialogRequest         = null;
        public SimpleDialogPanelView.DialogResult LastDialogResult          = SimpleDialogPanelView.DialogResult.None;
        public UserPaymentMethod                  userPaymentMethodToDelete = null;
        
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
                    ,tags   : new [] { "report" }
                    ,action : async (s) =>
                    {
                        /////////////////////////////////////// Pre Steps
                        
                        // Open Screen
                        s.AddPreStep(StartSession());
                        s.AddPreStep(CheckoutScreenMobile.OpenCheckoutScreenMobile());
                        
                        /////////////////////////////////////// Steps
                        
                        // View CheckoutPage
                        s.AddChildStep(Client.CheckoutScreenMobile.SetPage(Client.CheckoutScreenMobile.CheckoutLoadingPage));
                        s.AddChildStep(CheckoutClient.Instance.TaxController.GetTaxInfo());
                        s.AddChildStep("wait",        async x => await Task.Delay(1000));
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
                        var response = await CHECKOUT.Network.Post<CreateCheckoutSessionResponse>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/checkout-sessions/init"
                                                                                                 ,headers  : new ()
                                                                                                           {
                                                                                                               { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                                           }
                                                                                                 ,body     : new Shared.CreateCheckoutSessionRequest()
                                                                                                           {
                                                                                                              Order     = new OrderDetails()
                                                                                                                        {
                                                                                                                            sku      = "sku-1", // SelectedProduct.DisplayName,
                                                                                                                            amount   = 100,
                                                                                                                            currency = "USD",
                                                                                                                            quantity = 1,
                                                                                                                        },
                                                                                                              ExpiresAt = DateTime.UtcNow.AddDays(1),
                                                                                                           });
                        
                        if (!response.Success)
                            throw new Exception("Create checkout session fail");
                        
                        this.SessionID = response.SessionId;           
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
                        s.AddChildStep("wait",        async x => await Task.Delay(1000));
                        s.AddChildStep("set_success", async x => Client.CheckoutScreenMobile.NavigationNext = "Success");
                        s.AddChildStep(Client.CheckoutScreenMobile.Navigate());
                       
                        ////////////////////////////////////////////////////////////// Post Steps
                        
                        ///////// TEMP
                        this.lastTransactionResult = new TransactionResultData()
                                                   {
                                                      isSuccess         = true,
                                                      errors            = null,
                                                      isCanceled        = false,
                                                      transaction_id    = "test_transaction",
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
                        card.CardNumber = "4242424242424242";
                        card.CardMonth  = "12";
                        card.CardYear   = "2026";
                        card.CardCCV    = "123";
                        await card.GetTokenizer().Execute();
                        await card.Tokenize()    .Execute();
                        
                        var cardToken = card.TokenID;
                        
                        var response = await CHECKOUT.Network.Post<PaymentInitiationResponse>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/payments/initiate"
                                                                                             ,headers  : new ()
                                                                                                       {
                                                                                                           { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                                       }
                                                                                             ,body     : new Shared.PaymentInitiationRequest()
                                                                                                       {
                                                                                                           method       = new PaymentMethodDetails()
                                                                                                                        {
                                                                                                                            id   = "stripe",
                                                                                                                            data = new()
                                                                                                                                 {
                                                                                                                                      { "Number",   cardToken },
                                                                                                                                      { "ExpMonth", cardToken },
                                                                                                                                      { "ExpYear",  cardToken },
                                                                                                                                      { "Cvc",      cardToken }
                                                                                                                                 },
                                                                                                                        },
                                                                                                           order        = new OrderDetails()
                                                                                                                        {
                                                                                                                            sku      = "sku-1",
                                                                                                                            quantity = 1,
                                                                                                                            amount   = 100,
                                                                                                                            currency = "USD",
                                                                                                                        },
                                                                                                           expiresAt    = DateTime.UtcNow.AddDays(1),
                                                                                                           metadata     = new()
                                                                                                                        {
                                                                                                                            { "TransactionId", Guid.NewGuid().ToString() }
                                                                                                                        }
                                                                                                       });
                        
                        var orderID = response.orderId;
                    });
        
        public Step HandleTransactionResult()
        =>
            new Step(name   : $"handle_transaction_result"
                    ,action : async (s) =>
                    {
                        ///////// IAP
                        // CheckoutClient.Instance.IAPStore.FinishTransaction(product        : new ProductDefinition(id : CheckoutClient.Instance.CurrentSession.SelectedProduct.DisplayName, type : ProductType.Consumable)
                        //                                                   ,transactionId  : "transactionID");

                        var result = this.lastTransactionResult;
                        
                        this.PurchaseResult = new PurchaseResult()
                                              {
                                                  IsSuccess   = result.isSuccess,
                                                  IsCanceled  = result.isCanceled,
                                                  Errors      = result.errors?.ToList(),
                                                  IsError     = result.errors?.Length > 0,
                                              };
                    });
        
    }
}


