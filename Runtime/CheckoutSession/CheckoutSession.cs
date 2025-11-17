using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout.Shared;
using Galleon.Checkout.UI;

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
        
        // Preselection 
        public UserPaymentMethod                  PreselectedPaymentMethod = null;
        
        // Bonus Data
        public List<BonusItem>                    BonusData                 = new();
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties
        
        public CheckoutClient                     Client                    => CheckoutClient.Instance;
        public User                               User                      => Client.CurrentUser;
        public Transaction                        CurrentTransaction        => User.CurrentTransaction;
        
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
                        
                        // Get Tax Info
                        
                      //s.AddChildStep(CheckoutClient.Instance.TaxController.GetTaxInfo());
                      //s.AddChildStep("wait",        async x => await Task.Delay(1000));
                        
                        // View CheckoutPage
                        s.AddChildStep("tax_success", async x => Client.CheckoutScreenMobile.NavigationNext = "preselection");
                      //s.AddChildStep("tax_success", async x => Client.CheckoutScreenMobile.NavigationNext = "checkout");
                        s.AddChildStep(Client.CheckoutScreenMobile.Navigate());
                        
                        /////////////////////////////////////// Post Steps
                        
                        // Close
                        s.AddPostStep(CheckoutScreenMobile.EndCheckoutScreenMobile());
                        s.AddPostStep(EndCheckoutSession());
                        
                        // Test Report
                        s.AddPostStep("report", async x => { Report?.Invoke(); });
                        
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
                                                      OrderID    = CHECKOUT.Session?.SessionID ?? "NULL",
                                                      IsSuccess  = false,
                                                      IsCanceled = true,
                                                      IsError    = false,
                                                      Errors     = new(),
                                                  };
                        }
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Misc
        
        public Step CheckPreselection() 
        =>
            new Step(name   : $"check_preselection"
                    ,action : async (s) =>
                    {
                        if (CHECKOUT.Session?.PreselectedPaymentMethod?.Type == "native")
                        {
                            this.PurchaseResult = new PurchaseResult()
                                                {
                                                    IsSuccess              = true,
                                                    DidUserSelectNativeIAP = true,
                                                };
                            
                            s.RemoveStepsAfterThisInParentFlow();
                        }
                        else
                        {
                            
                            s.ParentStep.AddChildStep("select_first_upm", async step => {CHECKOUT.PaymentMethods.UserPaymentMethodsToDisplay.First().SelectExclusive();} );
                            s.ParentStep.AddChildStep(CheckoutClient.Instance.CheckoutScreenMobile.ViewPage(CheckoutClient.Instance.CheckoutScreenMobile.CheckoutPage));
                            
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
                                                                                                                       sku      = CHECKOUT.Session.SelectedProduct.Sku,
                                                                                                                     //sku      = "sku-1-3DS",
                                                                                                                       amount   = CHECKOUT.Session.SelectedProduct.Amount,
                                                                                                                       currency = CHECKOUT.Session.SelectedProduct.Currency,
                                                                                                                   },
                                                                                                        expires_at = DateTime.UtcNow.AddDays(1),
                                                                                                        metadata   = new Dictionary<string, string>() { }
                                                                                                     });
                        this.SessionID = response.session_id; 
                        
                    });
        
        public Step CancelSession() 
        =>
            new Step(name   : $"cancel_session"
                    ,action : async (s) =>
                    {
                        var body = new Shared.CancelCheckoutSessionRequest()
                                   {
                                      session_id = CHECKOUT.Session.SessionID,
                                   };
                        
                        var response = await CHECKOUT.Network.Post<CancelCheckoutSessionResponse>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/checkout-session/cancel"
                                                                                                 ,headers  : new ()
                                                                                                           {
                                                                                                               { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                                           }
                                                                                                 ,body     : body);
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// transaction Steps
        
        public Step RunTransaction()
        =>
            new Step(name   : $"run_transaction"
                    ,action : async (s) =>
                    {
                        if (User.SelectedUserPaymentMethod.Type == "native")
                        {
                            this.PurchaseResult = new PurchaseResult()
                                                {
                                                    IsSuccess              = true,
                                                    DidUserSelectNativeIAP = true,
                                                };
                            return;
                        }
                        
                        ////////////////////////////////////////////////////////////// Pre Steps
                        
                        // Show Loading Screen
                        s.AddPreStep(Client.CheckoutScreenMobile.SetPage(Client.CheckoutScreenMobile.LoadingPage));
                        
                        // Start Transaction
                        s.AddPreStep(StartTransaction());
                        
                        ////////////////////////////////////////////////////////////// Transaction Steps
                        
                        // Setup Transaction Steps
                        User.CurrentTransaction = new Transaction();
                        foreach (var transactionStep in User.SelectedUserPaymentMethod.GetTransactionSteps())
                        {
                            s.Log($"scheduling transaction step : {transactionStep.Name}");
                            s.AddChildStep(transactionStep);
                        }
                        
                        ////////////////////////////////////////////////////////////// Final Navigation Step
                        
                        // Navigate
                        s.AddChildStep("set_success", async x => Client.CheckoutScreenMobile.NavigationNext = "Success");
                        s.AddChildStep(Client.CheckoutScreenMobile.Navigate());
                       
                        ////////////////////////////////////////////////////////////// Post Steps
                        
                        // ///////// TEMP
                        // this.lastChargeResult = new ChargeResultData()
                        //                       {
                        //                          is_success  = true,
                        //                          errors      = null,
                        //                          is_canceled = false,
                        //                          charge_id   = "test_transaction",
                        //                       };
                        // /////////
                        
                        s.AddPostStep(name   : "save_used_payment_method_if_success"
                                     ,action : async x =>
                                               {
                                                   if (this.lastChargeResult.is_success)
                                                       x.AddChildStep(CHECKOUT.PaymentMethods.SaveUsedUserPaymentMethod());
                                               });
                        
                        // Finally, handle transaction result
                        s.AddPostStep(HandleTransactionResult());
                        
                    });
        
        
        public Step StartTransaction()
        =>
            new Step(name   : $"start_transaction"
                    ,action : async (s) =>
                    {
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
                                                  OrderID     = CHECKOUT.Session?.SessionID ?? "NULL",
                                                  IsSuccess   = result.is_success,
                                                  IsCanceled  = result.is_canceled,
                                                  Errors      = result.errors?.ToList(),
                                                  IsError     = result.errors?.Length > 0,
                                              };
                    });
    }
}
