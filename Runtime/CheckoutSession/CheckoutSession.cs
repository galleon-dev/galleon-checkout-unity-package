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
        
        // Purchase data
        public CheckoutProduct                    SelectedProduct;
        public PurchaseResult                     PurchaseResult        = default;
        
        // Simple Dialog Panel data
        public  string                             LastDialogRequest         = null;
        public  SimpleDialogPanelView.DialogResult LastDialogResult          = SimpleDialogPanelView.DialogResult.None;
        [FormerlySerializedAs("PaymentMethodToDelete")] public UserPaymentMethod                  userPaymentMethodToDelete = null;
        
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
                        foreach (var stepFunc in User.SelectedUserPaymentMethod.TransactionSteps)
                        {
                            var step = stepFunc?.Invoke();
                            s.Log($"adding transaction step : {step.Name}");
                            User.CurrentTransaction.TransactionSteps.Add(step);
                        }
                        
                        ////////////////////////////////////////////////////////////// Steps
                        
                        // Add Child Transaction Steps
                        foreach (var transactionStep in User.CurrentTransaction.TransactionSteps)
                        {
                            s.Log($"scheduling transaction step : {transactionStep.Name}");
                            s.AddChildStep(transactionStep);
                        }
                        
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
        
        public Step HandleTransactionResult()
        =>
            new Step(name   : $"handle_transaction_result"
                    ,action : async (s) =>
                    {
                        ///////// IAP
                        // CheckoutClient.Instance.IAPStore.FinishTransaction(product        : new ProductDefinition(id   : CheckoutClient.Instance.CurrentSession.SelectedProduct.DisplayName
                        //                                                                                          ,type : ProductType.Consumable)
                        //                                                   ,transactionId : "transactionID");

                        var result = this.lastTransactionResult;
                        
                        this.PurchaseResult = new PurchaseResult()
                                              {
                                                  IsSuccess   = result.isSuccess,
                                                  IsCanceled  = result.isCanceled,
                                                  Errors      = result.errors?.ToList(),
                                                  IsError     = result.errors?.Length > 0,
                                              };
                    });

        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// TEMP FLOW
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// TEMP FLOW
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// TEMP FLOW
        
        
        public Step Temp_Flow()
        =>
            new Step(name   : $"Temp_Flow"
                    ,action : async (flow) =>
                    {
                        PaymentMethodDefinitionsResponse PamentMethodDefinitions = default;
                        UserPaymentMethodsResponse       userPaymentMethods          = default;
                        
                        CreditCardUserUserPaymentMethod   firstCardUserUser;
                        
                        PaymentInitiationResponse paymentInitiation = default;
                        
                        
                        string sku       = "sku-1";
                        int    quantity  = 1;
                        int    amount    = 100;
                        string currency  = "USD";
                        
                        string cardToken = "asdf";
                         
                        
                        flow.AddChildStep("get_payment_method_definitions"
                                         ,async x =>
                                         {
                                             PamentMethodDefinitions = await CHECKOUT.Network.Get<PaymentMethodDefinitionsResponse>($"{CHECKOUT.Network.SERVER_BASE_URL}/available_payment_methods");
     
                                             foreach (var definition in PamentMethodDefinitions.paymentMethods)
                                                 x.Log($"- {definition.name}, {definition.type}");
                                         });
                        
                        
                        
                        flow.AddChildStep("remove_card"
                                         ,async x =>
                                         {
                                             var response = await CHECKOUT.Network.Post<RemoveCardResponse>(url  : $"{CHECKOUT.Network.SERVER_BASE_URL}/remove_card"
                                                                                                           ,body : new RemoveCardRequest()
                                                                                                           {
                                                                                                              credit_card_token = cardToken,
                                                                                                           });

                                             x.Log($"- {response.ToString()}");
                                         });
                        
                        flow.AddChildStep("add_new_card"
                                         ,async x =>
                                         {
                                             var response = await CHECKOUT.Network.Post<AddCardResponse>(url  : $"{CHECKOUT.Network.SERVER_BASE_URL}/add_card"
                                                                                                        ,body : new AddCardRequest()
                                                                                                        {
                                                                                                           
                                                                                                        });

                                             x.Log($"- {response.ToString()}");
                                         });
                        
                        
                        flow.AddChildStep("get_user_payment_methods"
                                         ,async x =>
                                         {
                                             userPaymentMethods = await CHECKOUT.Network.Get<UserPaymentMethodsResponse>($"{CHECKOUT.Network.SERVER_BASE_URL}/payment_methods");
   
                                             foreach (var paymentMethod in userPaymentMethods.payment_methods)
                                                 x.Log($"- {paymentMethod.type}");
                                         });
                        
                        
                        flow.AddChildStep("get_first_card"
                                         ,async x =>
                                         {
                                             var card = userPaymentMethods.payment_methods.FirstOrDefault(x => x.type == "card") as CreditCardUserPaymentMethodData;
                                             
                                             firstCardUserUser = new CreditCardUserUserPaymentMethod()
                                                         {
                                                           Type = card.type,
                                                         };
                                             
                                             x.Log($"first card : {firstCardUserUser.Type}");
                                         });
                                             
                        
                        flow.AddChildStep("transaction_1_credit_card"
                                         ,async transaction_1 =>
                                         {
                                             
                                             transaction_1.AddChildStep("start_transaction_1"
                                                                       ,async x =>
                                                                       {
                                                                           paymentInitiation = await CHECKOUT.Network.Post<PaymentInitiationResponse>(url  : $"{CHECKOUT.Network.SERVER_BASE_URL}/payment_initiation"
                                                                                                                                                     ,body : new PaymentInitiationRequest
                                                                                                                                                     {
                                                                                                                                                         method    = new PaymentMethodDetails
                                                                                                                                                                   {
                                                                                                                                                                       id   = "stripe",
                                                                                                                                                                       data = new Dictionary<string, object>
                                                                                                                                                                            {
                                                                                                                                                                                { "Number",   cardToken },
                                                                                                                                                                                { "ExpMonth", cardToken },
                                                                                                                                                                                { "ExpYear",  cardToken },
                                                                                                                                                                                { "Cvc",      cardToken }
                                                                                                                                                                            }
                                                                                                                                                                   },
                                                                                                                                                         order     = new OrderDetails
                                                                                                                                                                   {
                                                                                                                                                                       sku      = sku,
                                                                                                                                                                       quantity = quantity,
                                                                                                                                                                       amount   = amount,
                                                                                                                                                                       currency = currency
                                                                                                                                                                   },
                                                                                                                                                         expiresAt = DateTime.UtcNow.AddDays(1),
                                                                                                                                                         metadata  = new Dictionary<string, string>
                                                                                                                                                                   {
                                                                                                                                                                     { "TransactionId", Guid.NewGuid().ToString() }
                                                                                                                                                                   }
                                                                                                                                                     });
                                                                           
                                                                           x.Log($"- is success     : {paymentInitiation.success}");
                                                                           x.Log($"- order ID       : {paymentInitiation.orderId}");
                                                                           x.Log($"- transaction ID : {paymentInitiation.transactionId}");
                                                                       });
                                             
                                             transaction_1.AddChildStep("charge"
                                                                       ,async x =>
                                                                       {   
                                                                           var chargeResponse = await CHECKOUT.Network.Post<ChargeResponse>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/charge"
                                                                                                                                           ,headers  : new ()
                                                                                                                                                     {
                                                                                                                                                         { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                                                                                     }
                                                                                                                                           ,body     : new ChargeRequest()
                                                                                                                                                     {
                                                                                                                                                         sku      = "sku-1",
                                                                                                                                                         quantity = 1,
                                                                                                                                                         amount   = 100,
                                                                                                                                                         currency = "USD",
                                                                                                                                                         card     = new()
                                                                                                                                                                  {
                                                                                                                                                                      number   = cardToken,
                                                                                                                                                                      expMonth = cardToken,
                                                                                                                                                                      expYear  = cardToken,
                                                                                                                                                                      cvc      = cardToken,
                                                                                                                                                                  }
                                                                                                                                                     });
                                                                           
                                                                           x.Log($"- is success : {chargeResponse.transaction_result.isSuccess}");
                                                                           x.Log($"- payment ID : {chargeResponse.PaymentId}");
                                                                       });
                                             
                                             transaction_1.AddChildStep("validate"
                                                                       ,async x =>
                                                                       {
                                                                           var isValid = CHECKOUT.Network.Post<bool>(url  : $"{CHECKOUT.Network.SERVER_BASE_URL}/validate"
                                                                                                                    ,body : new // Validate Request
                                                                                                                    {
                                                                                                                        PaymentId = 2.ToString(),
                                                                                                                        Sku       = sku,
                                                                                                                        Quantity  = quantity,
                                                                                                                        Amount    = amount,
                                                                                                                        Currency  = currency
                                                                                                                    });
                                                                           
                                                                           x.Log($"- is valid : {isValid}");
                                                                       });
                                         });
                        
                        
                        flow.AddChildStep("transaction_2_google_pay"
                                         ,async transaction_2 =>
                                         {
                                             transaction_2.AddChildStep("start_transaction_2"
                                                                       ,async x =>
                                                                       {
                                                                           paymentInitiation = await CHECKOUT.Network.Post<PaymentInitiationResponse>(url  : $"{CHECKOUT.Network.SERVER_BASE_URL}/payment_initiation"
                                                                                                                                                     ,body : new PaymentInitiationRequest
                                                                                                                                                     {
                                                                                                                                                         method    = new PaymentMethodDetails
                                                                                                                                                                   {
                                                                                                                                                                       id   = "google_pay",
                                                                                                                                                                     //data = new Dictionary<string, object>
                                                                                                                                                                     //     {
                                                                                                                                                                     //         { "Number",   cardToken },
                                                                                                                                                                     //         { "ExpMonth", cardToken },
                                                                                                                                                                     //         { "ExpYear",  cardToken },
                                                                                                                                                                     //         { "Cvc",      cardToken }
                                                                                                                                                                     //     }
                                                                                                                                                                   },
                                                                                                                                                         order     = new OrderDetails
                                                                                                                                                                   {
                                                                                                                                                                       sku      = sku,
                                                                                                                                                                       quantity = quantity,
                                                                                                                                                                       amount   = amount,
                                                                                                                                                                       currency = currency
                                                                                                                                                                   },
                                                                                                                                                         expiresAt = DateTime.UtcNow.AddDays(1),
                                                                                                                                                         metadata  = new Dictionary<string, string>
                                                                                                                                                                   {
                                                                                                                                                                       { "TransactionId", Guid.NewGuid().ToString() }
                                                                                                                                                                   }
                                                                                                                                                     });
                                                                           
                                                                           x.Log($"- is success     : {paymentInitiation.success}");
                                                                           x.Log($"- order ID       : {paymentInitiation.orderId}");
                                                                           x.Log($"- transaction ID : {paymentInitiation.transactionId}");
                                                                       });
                                             
                                             transaction_2.AddChildStep("open_url_await_socket"
                                                                       ,async x =>
                                                                       {
                                                                           //var url = "http://www.google.com";
                                                                           //Application.OpenURL(url);
                                                                           
                                                                           NetworkSocket socket = new NetworkSocket();
                                                                           socket.socketIP      = "127.0.0.1";
                                                                           socket.socketPort    = 12345;
                                                                           socket.Timeout       = TimeSpan.FromSeconds(5);
                                                                           
                                                                           await socket.Listen().Execute();

                                                                           foreach (var message in socket.IncomingMessages)
                                                                               x.Log($"> {message}");
                                                                       });
                                         });
                        
                        
                        flow.AddChildStep("transaction_3_pay_pal"
                                         ,async transaction_2 =>
                                         {
                                             transaction_2.AddChildStep("start_transaction_3"
                                                                       ,async x =>
                                                                       {
                                                                           paymentInitiation = await CHECKOUT.Network.Post<PaymentInitiationResponse>(url  : $"{CHECKOUT.Network.SERVER_BASE_URL}/payment_initiation"
                                                                                                                                                     ,body : new PaymentInitiationRequest
                                                                                                                                                     {
                                                                                                                                                         method    = new PaymentMethodDetails
                                                                                                                                                                   {
                                                                                                                                                                       id   = "paypal",
                                                                                                                                                                     //data = new Dictionary<string, object>
                                                                                                                                                                     //     {
                                                                                                                                                                     //         { "Number",   cardToken },
                                                                                                                                                                     //         { "ExpMonth", cardToken },
                                                                                                                                                                     //         { "ExpYear",  cardToken },
                                                                                                                                                                     //         { "Cvc",      cardToken }
                                                                                                                                                                     //     }
                                                                                                                                                                   },
                                                                                                                                                         order     = new OrderDetails
                                                                                                                                                                   {
                                                                                                                                                                       sku      = sku,
                                                                                                                                                                       quantity = quantity,
                                                                                                                                                                       amount   = amount,
                                                                                                                                                                       currency = currency
                                                                                                                                                                   },
                                                                                                                                                         expiresAt = DateTime.UtcNow.AddDays(1),
                                                                                                                                                         metadata  = new Dictionary<string, string>
                                                                                                                                                                   {
                                                                                                                                                                       { "TransactionId", Guid.NewGuid().ToString() }
                                                                                                                                                                   }
                                                                                                                                                     });
                                                                           
                                                                           x.Log($"- is success     : {paymentInitiation.success}");
                                                                           x.Log($"- order ID       : {paymentInitiation.orderId}");
                                                                           x.Log($"- transaction ID : {paymentInitiation.transactionId}");
                                                                       });
                                         });
                        
                        
                    });
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// PM
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// PM
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// PM
                                  
        
        public class Payment_Method : Entity
        {
            public                                                 PaymentMethodDefinition PaymentMethodDefinition;
            [FormerlySerializedAs("userPaymentMethod")] [FormerlySerializedAs("PaymentMethodData")] public UserPaymentMethodData   userPaymentMethodData;
            public                                                 List<Step>              Actions;
        }
        
        public class CreditCard_Payment_Method : Payment_Method
        {
            public new CreditCardPaymentMethodDefinition PaymentMethodDefinition => (CreditCardPaymentMethodDefinition) base.PaymentMethodDefinition;
            public new CreditCardUserPaymentMethodData       UserPaymentMethodData       => (CreditCardUserPaymentMethodData)       base.userPaymentMethodData;
        }
        
        public class GPay_Payment_Method : Payment_Method
        {
            public new ooglePayPaymentMethodDefinition PaymentMethodDefinition => (ooglePayPaymentMethodDefinition) base.PaymentMethodDefinition;
            public new GooglePayUserPaymentMethodData  UserPaymentMethodData       => (GooglePayUserPaymentMethodData)  base.userPaymentMethodData;
        }
        
        public class Paypal_Payment_Method : Payment_Method
        {
            public new PayPalPaymentMethodDefinition PaymentMethodDefinition => (PayPalPaymentMethodDefinition) base.PaymentMethodDefinition;
            public new PaypalUserPaymentMethodData       UserPaymentMethodData       => (PaypalUserPaymentMethodData)       base.userPaymentMethodData;
        }
        
        ///
        ///
        /// 
        public class TransactionActions : Entity
        {
            public Step Charge()        => new Step(name   : $"Charge"
                                                   ,action : async (s) =>
                                                   {
                                                       
                                                   });
            
            public Step Do_3DS()        => new Step(name   : $"Do_3DS"
                                                   ,action : async (s) =>
                                                   {
                                                       
                                                   });
            
            public Step OpenURL()       => new Step(name   : $"OpenURL"
                                                   ,action : async (s) =>
                                                   {
                                                       
                                                   });
        }
        
        ///
        ///
        /// 
        public class VaultingActions
        {
            public Step GetTokenizer() => new Step(name   : $"get_tokenizer"
                                                  ,action : async (s) =>
                                                  {
                                                      
                                                  });
            
            public Step Tokenize()     => new Step(name   : $"tokenize"
                                                  ,action : async (s) =>
                                                  {
                                                      
                                                  });
            
        }

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


