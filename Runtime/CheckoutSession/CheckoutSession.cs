using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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
                        // Open Screen
                        s.AddPreStep(CheckoutScreenMobile.OpenCheckoutScreenMobile());
                        
                        // View CheckoutPage
                        s.AddChildStep(Client.CheckoutScreenMobile.ViewPage(Client.CheckoutScreenMobile.CheckoutPage));
                        
                        // Report
                        s.AddPostStep("report", async x =>
                                               {
                                                   Report?.Invoke();
                                               });
                        
                        // Close
                        s.AddPostStep(CheckoutScreenMobile.CloseCheckoutScreenMobile());

                    });
        
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Steps
        
        public Step RunTransaction()
        =>
            new Step(name   : $"run_transaction"
                    ,action : async (s) =>
                    {
                        // Show Loading Screen
                        s.AddChildStep(Client.CheckoutScreenMobile.SetPage(Client.CheckoutScreenMobile.LoadingPage));
                        
                        // Setup Transaction Steps
                        User.CurrentTransaction = new Transaction();
                        User.CurrentTransaction.TransactionSteps.AddRange(User.SelectedPaymentMethod.TransactionSteps);
                        
                        // Run Transaction Steps
                        foreach (var transactionStep in User.CurrentTransaction.TransactionSteps)
                        {
                            s.AddChildStep(transactionStep);
                        }
                        
                        // Navigate
                        s.AddChildStep("wait",        async x => await Task.Delay(1000));
                        s.AddChildStep("set_success", async x => Client.CheckoutScreenMobile.NavigationNext = "Success");
                        s.AddChildStep(Client.CheckoutScreenMobile.Navigate());
                    });

        
        public Step AddCard()
        =>
            new Step(name   : $"add_card"
                    ,action : async (s) =>
                    {
                        foreach (var vaultingStep in User.SelectedPaymentMethod.VaultingSteps)
                        {
                            s.AddChildStep(vaultingStep);
                        }
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Credit Card Vaulting Flow
        
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
            
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Json
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Json
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Json

        public class AuthenticateRequest
        {
            public string AppID;
            public string ID;
            public string Device;
        }
        /// /authenticate
        /// {
        ///     AppID  : "test.app"
        ///     ID     : "test_ID"
        ///     Device : "test_device"
        /// }
        
            public class AuthenticateResponse
            {
                public string accessToken;
                public string appID;
                public string id;
                public string externalId;
            }
            /// /authenticate Response :
            /// {
            ///     "accessToken" : "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiMSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6InBheWVyIiwiaXNzIjoiR2FsbGVvbiIsImF1ZCI6InRlc3QuYXBwIn0.xdg3d7xMIVAhDm_9JpSl5MENnmWAaZDHRjApWUMj-t8",
            ///     "appId"       : "test.app",
            ///     "id"          : 1,
            ///     "externalId"  : "user id 1"
            /// }

        ////////////////////////////////
        
        public class InitializeRequest
        {
            public string AccessToken;
        }
        /// /initialize
        /// {
        ///     accessToken : "..."
        /// }
            
            public class InitializeResponse
            {
                public ConfigData ConfigData;
            }   
                public class ConfigData
                {
                    
                }
                
                public class PaymentMethodData
                {
                    public string              type;
                    public List<PaymentAction> vaulting_actions    = new();
                    public List<PaymentAction> transaction_actions = new();
                }
                
                    public class CardPaymentMethodData : PaymentMethodData
                    {
                        public string[] card_types;
                    }
                    public class GPayPaymentMethodData : PaymentMethodData
                    {
                    }
                    public class PaypalPaymentMethodData : PaymentMethodData
                    {
                    }
                    
                public class PaymentAction
                {
                    public string action;
                }
                
                    public class PaypalPaymentAction : PaymentAction
                    {
                        
                    }
                
            /// /initialize response
            /// {
            ///     config :
            ///            {
            ///                 ...
            ///            }
            ///
            ///     payment_methods:
            ///     [
            ///         {
            ///             type                : "credit_card",
            ///             card_types          :
            ///                                 [
            ///                                     "mastercard",
            ///                                     "visa",
            ///                                 ],
            ///             vaulting_actions    : 
            ///                                 [
            ///                                    {
            ///                                        action : "get_tokenizer",
            ///                                        url    : "https://tokenizer_url",
            ///                                    },
            ///                                    {
            ///                                        action : "tokenize"
            ///                                    },
            ///                                 ],
            ///             transaction_actions :
            ///                                 [
            ///                                     {
            ///                                         action : "charge"
            ///                                     }
            ///                                 ]
            ///         },
            ///         {
            ///             type                : "gpay",
            ///             vaulting_actions    : 
            ///                                 [
            ///                                    {
            ///                                        action : "check_gpay_availability"
            ///                                    }
            ///                                 ],
            ///             transaction_actions :
            ///                                 [
            ///                                     {
            ///                                         action : "create_gpay_order"
            ///                                     },
            ///                                     {
            ///                                         action         : "open_url",
            ///                                         url            : "https://galleon/gpay_url",
            ///                                         deep_link_path : "gpay_redirect",
            ///                                         socket_info    : "bla_bal"
            ///                                     },
            ///                                     {
            ///                                         action : "get_result"
            ///                                     },
            ///                                 ]
            ///         },
            ///         {
            ///             type                : "paypal",
            ///             vaulting_actions    : 
            ///                                 [
            ///                                    {
            ///                                        action : "check__paypal_availability"
            ///                                    }
            ///                                 ],
            ///             transaction_actions :
            ///                                 [
            ///                                     {
            ///                                         action : "create_paypal_order"
            ///                                     },
            ///                                     {
            ///                                         action         : "open_url",
            ///                                         url            : "https://galleon/paypal_url",
            ///                                         deep_link_path : "paypal_redirect",
            ///                                         socket_info    : "bla_bal",
            ///                                     },
            ///                                     {
            ///                                         action : "get_result"
            ///                                     },
            ///                                 ]
            ///         },
            ///     ]
            ///     
            /// }
            
        ////////////////////////
        
        public class TaxDataRequest
        {
            public string location;
        }
        /// /Tax
        /// {
        ///     location : "isr"
        /// }
        
            public class TaxDataResponse
            {
                public bool                      should_display_price_including_tax;
                public Dictionary<string, float> taxes;
            }
            /// /Tax
            /// {
            ///     location : "isr"
            /// }
            
        
        ////////////////////////
        
        public class AddCardRequest
        {
            public string cardToken;
        }
        /// /add_card
        /// {
        ///     card_token : "..."
        /// }
            
            public class AddCardResponse
            {
                public string result;
            }
            /// /add_card response
            /// {
            ///     result : "ok"
            /// }
         
        public class RemoveCardRequest
        {
            public string  cardToken;
        }
        /// /remove_card
        /// {
        ///     card_token : "..."
        /// }
            
            public class RemoveCardResponse
            {
                public string result;
            }
            /// /remove_card response
            /// {
            ///     result : "ok"
            /// }
            
            
        ////////////////////////
        
        public class StartTransactionRequest
        {
            public string selected_payment_method_type;
            public string payment_method_id;
        }
        /// /start_transaction
        /// {
        ///     selected_payment_method_type : "card_1234"
        ///     payment_method_id            : "...token..."
        /// }
        
            public class StartTransactionResponse
            {
                public string transaction_id;
            }
            /// /start_transaction response
            /// {
            ///     transaction_id : "12345"
            /// }
        
        public class ValidateRequest
        {
            public string transaction_id;
            public string recipt_data;
            public string date_time_utc;
        }
        /// /validate_recipt
        /// {
        ///     transaction_id : "12345"
        ///     recipt_data    : "12345"
        ///     date_time_utc  : "2025-01-01T12:34"
        /// }
            
            public class ValidateResponse
            {
                public string result;
            }
            /// /validate_recipt
            /// {
            ///     result : "valid"
            /// }
            
        public class TransactionResultRequest
        {
            public string transaction_id;
        }
        /// /transaction_result
        /// {
        ///     transaction_id : "12345"
        /// }
        
            public class TransactionResultResponse
            {
                public bool is_success;
            }
            /// /transaction_result
            /// {
            ///     is_success : "true"
            /// }
        
        ////////////////////////
        
        public class UpdateEmailRequest
        {
            public string user_id;
            public string email;
        }
        /// /update_email
        /// {
        ///     user_id : "12345"
        ///     email   : "email.email@email.com"
        /// }
        
            public class UpdateEmailResponse
            {
                public string result;
            }
            /// /update_email
            /// {
            ///     result : "ok"
            /// }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Data
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Data
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Data
        
        
        public class CheckoutInputData
        {
            public string AppID;
            public string ID;
            public String Device;
        }
        
        // Actual class in CheckoutAPI
        public class PurchaseResultData
        {
            public bool         IsSuccess  { get; set; }
            public bool         IsCanceled { get; set; }
            public bool         IsError    { get; set; }
            public List<string> Errors     { get; set; } 
        }
        
        ////////////////////
        
      //public class PaymentMethodData
      //{
      //    public string ID;
      //    public string Type;
      //    public string DisplayName;
      //    
      //    public string[] actions = new[]
      //                              {
      //                                  // Generic
      //                                  "start    https://www.galleon.com/start_url",
      //                                  "open_url https://www.galleon.com/web_url",
      //                                  "result   https://www.galleon.com/result_url",
      //                              };
      //}
        
      //public class CreditCardFlowData
      //{
      //    public string TokenizerURL;
      //    public string ChargeURL;
      //}
        
        ////////////////////
        
      //public class ConfigData
      //{
      //    public bool IsCalifornia;
      //    
      //    public Dictionary<string, string> GenericConfigValues;
      //}
        
        ////////////////////
        
      //public class TaxData
      //{
      //    public Dictionary<string, float> taxes = new();
      //}
        
        
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}


