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
        public string                             SessionID                         = "1";

        // Purchase data
        public CheckoutProduct                    SelectedProduct;
        public PurchaseResult                     PurchaseResult                    = default;

        public Dictionary<string, string>         Metadata                          = new();

        // Tax data
        public bool                               ShouldDisplayPriceIncludingTax    = true;
        public Dictionary<string, TaxItem>        Taxes                             = new(); // <name_of_tax, tax_data>
        
        // Simple Dialog Panel data
        public string                             LastDialogRequest                 = null;
        public SimpleDialogPanelView.DialogResult LastDialogResult                  = SimpleDialogPanelView.DialogResult.None;
        public UserPaymentMethod                  userPaymentMethodToDelete         = null;
        
        // Preselection 
        public UserPaymentMethod                  PreselectedPaymentMethod          = null;
        
        // Bonus Data
        public List<BonusItem>                    BonusData                         = new();
        
        // steps
        public Step                               OnSessionFinishedStep;
        
        // Config
        public Dictionary<string, object>         PurchaseConfiguration             = new();
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties
        
        public CheckoutClient                     Client                    => CheckoutClient.Instance;
        public User                               User                      => Client.CurrentUser;
        public Transaction                        CurrentTransaction        => User.CurrentTransaction;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Last transaction result
        
        public ChargeResultData                   lastChargeResult          = null;
        
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
                        /////////////////////////////////////// Definitions
                        
                        this.OnSessionFinishedStep = new Step(name : "on_session_finished");
                        
                        /////////////////////////////////////// Pre Steps
                         
                        // Open Screen
                        s.AddPreStep(CheckoutScreenMobile.OpenCheckoutScreenMobile());
                        s.AddPreStep(Client.CheckoutScreenMobile.SetPage(Client.CheckoutScreenMobile.CheckoutLoadingPage));
                        s.AddPreStep(StartSession());
                        
                        /////////////////////////////////////// Steps
                        
                        // Get Tax Info
                        
                      //s.AddChildStep(CheckoutClient.Instance.TaxController.GetTaxInfo());
                      //s.AddChildStep("wait",        async x => await Task.Delay(1000));
                        
                        bool isPreselectionScreenEnabled = CHECKOUT.Globals.IsPreselectionEnabled;
                        bool isNativeStoreEnabled        = CHECKOUT.Globals.IsNativeStoreEnabled;
                        
                        if (isPreselectionScreenEnabled && isNativeStoreEnabled)
                            s.AddChildStep("view_preselection", async x => Client.CheckoutScreenMobile.NavigationNext = "preselection");
                        else
                            s.AddChildStep("view_checkout", async x => Client.CheckoutScreenMobile.NavigationNext = "checkout");
                            
                        s.AddChildStep(Client.CheckoutScreenMobile.Navigate());
                        
                        /////////////////////////////////////// Post Steps
                        
                        // Close
                        s.AddPostStep(CheckoutScreenMobile.EndCheckoutScreenMobile());
                        s.AddPostStep(EndCheckoutSession());
                        s.AddPostStep(OnSessionFinishedStep);
                        
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
                                                      OrderID                   = CHECKOUT.Session?.SessionID ?? "NULL",
                                                      IsSuccess                 = false,
                                                      IsCanceled                = true,
                                                      IsError                   = false,
                                                      Errors                    = new(),
                                                      SelectedPaymentMethodType = CHECKOUT.Session?.PreselectedPaymentMethod?.Type ?? "none",
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
                                                    IsSuccess                 = true,
                                                    DidUserSelectNativeIAP    = true,
                                                    SelectedPaymentMethodType = "native"
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

                        // Store tax data in session
                        var taxData = response.price_data.tax;
                        this.Taxes.Clear();

                        s.Log($"tax.should_display_taxes         : {taxData.should_display_taxes}");
                        s.Log($"tax.taxes({taxData.taxes.Count}) : ");

                        
                        Taxes.Add("IRS",       new TaxItem() { inclusive = true, tax_amount = 2.99m} );
                        Taxes.Add("Levan Tax", new TaxItem() { inclusive = true, tax_amount = 4.99m} );
                        
                        
                        foreach (var t in taxData.taxes)
                        {
                            s.Log($" - {t.Key} : {t.Value}");
                            this.Taxes.Add(t.Key, new TaxItem() { inclusive = t.Value.inclusive, tax_amount = t.Value.tax_amount });
                        }

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
                        
                        OnSessionFinishedStep.AddChildStep(name   : "save_used_payment_method_if_success"
                                                          ,action : async x =>
                                                                  {
                                                                      if (this.lastChargeResult != null
                                                                      &&  this.lastChargeResult.is_success
                                                                      && !this.lastChargeResult.is_canceled)
                                                                      {
                                                                          x.AddChildStep(CHECKOUT.PaymentMethods.SaveUsedUserPaymentMethod());
                                                                      }
                                                                  });
                        
                        
                        // Refresh user payment methods
                        OnSessionFinishedStep.AddChildStep(CHECKOUT.PaymentMethods.RefreshPaymentMethods());
                        
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

                        // Analytics: Payment Succeeded or Failed
                        var selectedPaymentMethod = CHECKOUT.User.SelectedUserPaymentMethod;
                        if (result.is_success && !result.is_canceled)
                        {
                            CheckoutAPI.InvokeAnalyticsEvent("payment_succeeded", new Dictionary<string, object>
                            {
                                { "checkout_session_id", CHECKOUT.Session?.SessionID                 ?? ""     },
                                { "payment_method",      selectedPaymentMethod?.Type                 ?? "none" },
                                { "purchase_amount",     CHECKOUT.Session?.SelectedProduct?.Amount   ?? 0m     },
                                { "currency",            CHECKOUT.Session?.SelectedProduct?.Currency ?? ""     },
                            });
                        }
                        else if (result.errors?.Length > 0)
                        {
                            CheckoutAPI.InvokeAnalyticsEvent("payment_failed", new Dictionary<string, object>
                            {
                                { "checkout_session_id", CHECKOUT.Session?.SessionID                 ?? "" },
                                { "payment_method",      selectedPaymentMethod?.Type                 ?? "none" },
                                { "fail_reason",         string.Join(", ", result.errors             ?? new string[0]) },
                                { "purchase_amount",     CHECKOUT.Session?.SelectedProduct?.Amount   ?? 0m },
                                { "currency",            CHECKOUT.Session?.SelectedProduct?.Currency ?? "" }
                            });
                        }
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Misc Steps
        
        
        public Step On_ChargeError()
        => 
            new Step(name    : $"on_charge_error_or_cancel"
                     ,action : async (s) =>
                             {
                                 var localPMs = CHECKOUT.PaymentMethods.UserPaymentMethods.Where(x => x.ID.StartsWith("local_pm_id"));
                                 CHECKOUT.PaymentMethods.UserPaymentMethods.RemoveAll(x => x.ID.StartsWith("local_pm_id"));
                             });
        
        
        public Step On_CheckoutScreenClosed()
        => 
            new Step(name    : $"on_checkout_screen_closed"
                     ,action : async (s) =>
                               {
                                   if (lastChargeResult is null)
                                       s.ParentStep.AddPostStep(CancelSession());
                               });
        
        public Step On_EmptyCardSelected() 
        =>
            new Step(name   : $"on_empty_card_selected"
                    ,action : async (s) =>
                              {
                                  s.AddNextStepInParentFlow(CHECKOUT.Screen.ViewPage(CHECKOUT.Screen.CreditCardPage));
                              });
        
        public Step On_EmptyPaypalSelected() 
        =>
            new Step(name   : $"on_empty_paypal_selected"
                    ,action : async (s) =>
                              {
                                  var paypalPM = new UserPaymentMethod()
                                               {
                                                  Data                    = new ()
                                                                          {
                                                                              type           = "paypal",
                                                                              display_name   = "paypal",
                                                                              id             = "local_pm_id_paypal"
                                                                          },
                                                  DisplayName             = "PayPal",
                                                  IsNewPaymentMethod      = true,
                                                  ShouldSavePaymentMethod = true,
                                                  Type                    = "paypal",
                                               };
                                  CHECKOUT.PaymentMethods.UserPaymentMethods.Add(paypalPM);
                                  CHECKOUT.User.SelectPaymentMethod(paypalPM);
                                  
                                  s.AddNextStepInParentFlow(RunTransaction());
                              });
        
    }
}
