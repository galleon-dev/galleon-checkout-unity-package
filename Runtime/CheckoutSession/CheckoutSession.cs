using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
        [Obsolete("Never read by anything, in any commit, since it was introduced. Use PriceBreakdown instead.")]
        public bool                               ShouldDisplayPriceIncludingTax    = true;
        public Dictionary<string, TaxItem>        Taxes                             = new(); // <name_of_tax, tax_data>
        public PriceData                          SessionPriceData                  = null;

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
        public CheckoutPurchaseConfiguration      PurchaseConfiguration             = null;
        
        // Errors
        public List<string>                       sessionErrors                     = new List<string>();
        
        // Transaction
        public Step                               CurrentTransactionStep            = default;
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties
        
        public CheckoutClient                     Client                    => CheckoutClient.Instance;
        public User                               User                      => Client.CurrentUser;

        /// <summary>
        /// The only sanctioned source of displayable money for this session.
        /// Respects TaxItem.inclusive, so tax-inclusive markets (PL, most of EU) are not
        /// charged tax twice on screen. Do not re-derive totals at the call site.
        /// </summary>
        public CheckoutPriceBreakdown             PriceBreakdown
        => CheckoutPriceBreakdown.Build(priceData        : this.SessionPriceData
                                       ,taxes            : this.Taxes
                                       ,fallbackSubTotal : this.SelectedProduct?.Amount ?? 0m
                                       ,mode             : CHECKOUT.Globals.TaxMode);

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
                        s.AddPreStep(InitializeSession());
                        s.AddPreStep(CheckoutScreenMobile.OpenCheckoutScreenMobile());
                        s.AddPreStep(Client.CheckoutScreenMobile.SetPage(Client.CheckoutScreenMobile.CheckoutLoadingPage));
                        s.AddPreStep(StartSession());
                        s.AddPreStep(CHECKOUT.PaymentMethods.SelectLastUsedUserPaymentMethodToDisplay());
                        s.AddPreStep(ReportCheckoutWindowOpened()); // must run AFTER selection so payment_method_highlighted is populated

                        /////////////////////////////////////// Steps

                        s.AddChildStep(DetermineInitialPage()); // this updates the "navigation-next"
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
                                                      price_metadata            = MapPriceDataToMetadata(this.SessionPriceData),
                                                      IsSuccess                 = false,
                                                      IsCanceled                = true,
                                                      IsError                   = false,
                                                      Errors                    = this.sessionErrors,
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
                                                    price_metadata            = MapPriceDataToMetadata(this.SessionPriceData),
                                                    IsSuccess                 = true,
                                                    DidUserSelectNativeIAP    = true,
                                                    SelectedPaymentMethodType = "native"
                                                };
                            
                            s.RemoveStepsAfterThisInParentFlow();
                        }
                        else
                        {   
                            s.ParentStep.AddChildStep(CHECKOUT.PaymentMethods.SelectLastUsedUserPaymentMethodToDisplay());
                            s.ParentStep.AddChildStep(CheckoutClient.Instance.CheckoutScreenMobile.ViewPage(CheckoutClient.Instance.CheckoutScreenMobile.CheckoutPage));
                            
                        }
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Session Steps

        public Step DetermineInitialPage()
        =>
            new Step(name   : $"determine_initial_page"
                    ,action : async (s) =>
                    {
                        // Check if there are no payment methods to display (network error scenario)
                        if (CHECKOUT.PaymentMethods.UserPaymentMethods.Count == 0)
                        {
                            Client.CheckoutScreenMobile.ErrorPanelView.ErrorMessage     = "Network error";
                            Client.CheckoutScreenMobile.ErrorPanelView.ErrorDescription = "Unable to load payment methods. Please check your connection and try again.";
                            Client.CheckoutScreenMobile.ErrorPanelView.ButtonText       = "Try again";
                            Client.CheckoutScreenMobile.NavigationNext = "Error";
                        }
                        else
                        {
                            bool isPreselectionScreenEnabled = CHECKOUT.Globals.IsPreselectionEnabled;
                            bool isNativeStoreEnabled        = CHECKOUT.Globals.IsNativeStoreEnabled;

                            if (isPreselectionScreenEnabled && isNativeStoreEnabled)
                                Client.CheckoutScreenMobile.NavigationNext = "preselection";
                            else
                                Client.CheckoutScreenMobile.NavigationNext = "checkout";
                        }
                    });

        public Step InitializeSession()
        =>
            new Step(name   : $"initialize_session"
                    ,action : async (s) =>
                    {
                        await CHECKOUT.PaymentMethods.Load();
                    });
        
        public Step StartSession()
        =>
            new Step(name   : $"start_session"
                    ,action : async (s) =>
                    {   
                        string payerIP = null;
                        if (CHECKOUT.Globals.FakeTaxes)
                            payerIP = CHECKOUT.Globals.FakeTaxesIp;
                        
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
                                                                                                        metadata   = CHECKOUT.Session.Metadata,
                                                                                                        payer_ip   = payerIP,
                                                                                                     });
                        this.SessionID        = response.session_id;
                        this.SessionPriceData = response.price_data;

                        // Store tax data in session
                        var taxData = response.price_data.tax;
                        this.Taxes.Clear();

                        s.Log($"tax.should_display_taxes         : {taxData.should_display_taxes}");
                        s.Log($"tax.taxes({taxData.taxes.Count}) : ");

                        CHECKOUT.Config.SetValue("show_tax_breakdown", taxData.should_display_taxes);
                        
                        // Taxes.Add("IRS",       new TaxItem() { inclusive = true, tax_amount = 2.99m} );
                        // Taxes.Add("Levan Tax", new TaxItem() { inclusive = true, tax_amount = 4.99m} );
                        
                        
                        foreach (var t in taxData.taxes)
                        {
                            s.Log($" - {t.Key} : amount:{t.Value.tax_amount.ToString(CultureInfo.InvariantCulture)} inclusive:{t.Value.inclusive}");
                            this.Taxes.Add(t.Key, new TaxItem() { inclusive = t.Value.inclusive, tax_amount = t.Value.tax_amount });
                        }

                        // Resolve the breakdown once here so the numbers the UI will draw are in the flow log,
                        // and so a server/client disagreement about inclusive is loud rather than silent.
                        var breakdown = this.PriceBreakdown;
                        s.Log(breakdown.ToString());

                        if (breakdown.HasMismatch)
                            s.Log($"TAX MISMATCH : server says total({breakdown.Total.ToString(CultureInfo.InvariantCulture)}) "
                                + $"- subtotal({breakdown.SubTotal.ToString(CultureInfo.InvariantCulture)}) "
                                + $"= {breakdown.ServerAddedTax.ToString(CultureInfo.InvariantCulture)} added tax, "
                                + $"but the taxes flagged inclusive=false sum to {breakdown.AddedTax.ToString(CultureInfo.InvariantCulture)}. "
                                + $"Displaying the server total. Check the inclusive flags for tax_country:{taxData.tax_country}.");
                    });

        public Step ReportCheckoutWindowOpened()
        =>
            new Step(name   : $"report_checkout_window_opened"
                    ,action : async (s) =>
                    {
                        // Analytics: Checkout Window Opened
                        // NOTE: must run AFTER the initial payment method is selected/highlighted
                        //       (SelectLastUsedUserPaymentMethodToDisplay), otherwise
                        //       payment_method_highlighted is always reported as "none".
                        var selectedPaymentMethod = CHECKOUT.PaymentMethods.UserPaymentMethods.FirstOrDefault(x => x.IsSelected);
                        CheckoutAPI.InvokeAnalyticsEvent("checkout_window_opened", new Dictionary<string, object>
                        {
                            { "checkout_session_id",          CHECKOUT.Session?.SessionID                 ?? ""     },
                            { "purchase_amount",              CHECKOUT.Session?.SelectedProduct?.Amount   ?? 0m     },
                            { "currency",                     CHECKOUT.Session?.SelectedProduct?.Currency ?? ""     },
                            { "payment_method_highlighted",   selectedPaymentMethod?.DisplayType          ?? "none" },
                        });
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
                        if (CHECKOUT.PaymentMethods.SelectedUserPaymentMethod.Type == "native")
                        {
                            this.PurchaseResult = new PurchaseResult()
                                                {
                                                    price_metadata         = MapPriceDataToMetadata(this.SessionPriceData),
                                                    IsSuccess              = true,
                                                    DidUserSelectNativeIAP = true,
                                                };
                            return;
                        }
                        
                        ////////////////////////////////////////////////////////////// Prep
                        
                        this.CurrentTransactionStep = s;
                        
                        ////////////////////////////////////////////////////////////// Pre Steps
                        
                        // Show Loading Screen
                        s.AddPreStep(Client.CheckoutScreenMobile.SetPage(Client.CheckoutScreenMobile.LoadingPage));
                        
                        // Start Transaction
                        s.AddPreStep(StartTransaction());
                        
                        ////////////////////////////////////////////////////////////// Transaction Steps
                        
                        // Setup Transaction Steps
                        foreach (var transactionStep in CHECKOUT.PaymentMethods.SelectedUserPaymentMethod.GetTransactionSteps())
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
                        
                        if (result == null)
                            return;

                        this.PurchaseResult = new PurchaseResult()
                                              {
                                                  OrderID        = CHECKOUT.Session?.SessionID ?? "NULL",
                                                  price_metadata = MapPriceDataToMetadata(this.SessionPriceData),
                                                  IsSuccess      = result.is_success,
                                                  IsCanceled     = result.is_canceled,
                                                  Errors         = result.errors?.ToList(),
                                                  IsError        = result.errors?.Length > 0,
                                              };

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
                                  CHECKOUT.PaymentMethods.SelectPaymentMethod(paypalPM);

                                  s.AddNextStepInParentFlow(RunTransaction());
                              });

        public Step RefreshPaymentMethodsAndReturnToCheckout()
        =>
            new Step(name   : "refresh_payment_methods_and_return_to_checkout"
                    ,action : async (s) =>
                              {
                                  // this step gets called inside parent transaction step
                                  
                                  if (CurrentTransactionStep != default)
                                  {
                                      // Stop the transaction
                                      s.RemoveStepsAfterThisInParentFlow();
                                      CurrentTransactionStep.ChildSteps.Clear();
                                      CurrentTransactionStep.PostSteps.Clear();
                                      CurrentTransactionStep.ActionState = Step.ACTION_STATE.Error;
                                      CurrentTransactionStep.StepState   = Step.STEP_STATE.Completed;
                                      CurrentTransactionStep             = default;
                                  }
                                  
                                  
                                  // Show Loading Screen
                                  Flow().AddChildStep(Client.CheckoutScreenMobile.SetPage(Client.CheckoutScreenMobile.LoadingPage));

                                  // Refresh payment method definitions and user payment methods
                                  Flow().AddChildStep(CHECKOUT.PaymentMethods.RefreshPaymentMethods());

                                  Flow().AddChildStep("set_navigation_after_error", async s =>
                                  {
                                      // Check if there are no payment methods to display (network error scenario)
                                      if (CHECKOUT.PaymentMethods.UserPaymentMethods.Count == 0)
                                      {
                                          Client.CheckoutScreenMobile.ErrorPanelView.ErrorMessage     = "Network error";
                                          Client.CheckoutScreenMobile.ErrorPanelView.ErrorDescription = "Unable to load payment methods. \nPlease check your connection and try again.";
                                          Client.CheckoutScreenMobile.ErrorPanelView.ButtonText       = "Try again";
                                          Client.CheckoutScreenMobile.NavigationNext = "Error";
                                          Flow().AddChildStep(Client.CheckoutScreenMobile.Navigate());
                                      }
                                      else
                                      {
                                          CHECKOUT.PaymentMethods.SelectLastUsedUserPaymentMethodToDisplay().Execute();
                                          Flow().AddChildStep(Client.CheckoutScreenMobile.ViewPage(Client.CheckoutScreenMobile.CheckoutPage));
                                      }
                                      
                                  });
                              });


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Error Tracking

        public void LogError(string source, Exception ex)
        {
            string errorMessage = $"[{source}] {ex.GetType().Name}: {ex.Message}";
            sessionErrors.Add(errorMessage);
            UnityEngine.Debug.LogError(errorMessage);
        }

        public void LogError(string source, string errorMessage)
        {
            string fullMessage = $"[{source}] {errorMessage}";
            sessionErrors.Add(fullMessage);
            UnityEngine.Debug.LogError(fullMessage);
        }

        public void ClearSessionErrors()
        {
            sessionErrors.Clear();
        }

        /// <summary>
        /// Flattens the session's price data for analytics.
        ///
        /// Every value here must be either true or absent. This method previously ended with a
        /// block of "defaults to ensure all requested fields are present" which invented
        /// exchange_rate = 1.0 and usd_amount = the local amount. Nothing ever populated those
        /// keys beforehand, so the defaults fired on every single event: PLN was reported as USD
        /// at a rate of 1.0. A missing field is a gap; a fabricated field is a wrong answer that
        /// looks like data.
        /// </summary>
        private Dictionary<string, string> MapPriceDataToMetadata(PriceData priceData)
        {
            if (priceData == null) return new Dictionary<string, string>();

            var c      = CultureInfo.InvariantCulture;
            var result = new Dictionary<string, string>();

            // Basic price data
            result["amount"]              = priceData.subtotal_price.ToString(c);
            result["amount_with_tax"]     = priceData.total_price.ToString(c);

            // Currency from session or product
            result["currency"]            = CHECKOUT.Session?.SelectedProduct?.Currency ?? "";

            result["checkout_type"]       = "Galleon";

            // Tax related data
            result["tax_state"]           = priceData.tax?.tax_state   ?? "";
            result["tax_country"]         = priceData.tax?.tax_country ?? "";

            // NOTE : priceData.tax could be null here and this loop used to run unguarded.
            if (priceData.tax?.taxes != null)
            {
                foreach (var taxItem in priceData.tax.taxes)
                {
                    // ToString() without a culture emits "5,61" under a Polish locale, which then
                    // lands in analytics as a decimal-comma string. Always pin the culture.
                    result[taxItem.Key] = taxItem.Value.tax_amount.ToString(c);
                }

                result["total_tax"] = priceData.tax.taxes.Sum(x => x.Value.tax_amount).ToString(c);
            }
            else
            {
                result["total_tax"] = 0m.ToString(c);
            }

            // How the tax relates to the price, so a consumer can tell 29.99-incl-5.61
            // apart from 5.99-plus-1.38 without guessing from the country.
            // "none" is distinct from "inclusive" : no tax at all is not the same claim as
            // tax that happens to sit inside the price.
            // Built from the priceData argument, not from this.SessionPriceData, so this
            // method stays honest about describing what it was handed.
            var breakdown = CheckoutPriceBreakdown.Build(priceData        : priceData
                                                        ,taxes            : this.Taxes
                                                        ,fallbackSubTotal : this.SelectedProduct?.Amount ?? 0m
                                                        ,mode             : CHECKOUT.Globals.TaxMode);
            result["tax_behavior"]         = !breakdown.HasTaxes    ? "none"
                                           :  breakdown.HasAddedTax ? "added"
                                                                    : "inclusive";
            result["tax_inclusive_amount"] = breakdown.InclusiveTax.ToString(c);
            result["tax_added_amount"]     = breakdown.AddedTax    .ToString(c);

            //////////////////////////////////////// USD reference values

            // Forwarded when the server sends them. The client cannot compute these : it is handed
            // a local-currency product (29.99 PLN) and never sees the USD reference price, so it
            // has the denominator of the rate and not the numerator. Only the server knows both.
            if (priceData.exchange_rate       .HasValue) result["exchange_rate"]       = priceData.exchange_rate      .Value.ToString(c);
            if (priceData.usd_amount          .HasValue) result["usd_amount"]          = priceData.usd_amount         .Value.ToString(c);
            if (priceData.usd_amount_with_tax .HasValue) result["usd_amount_with_tax"] = priceData.usd_amount_with_tax.Value.ToString(c);

            //////////////////////////////////////// TEMPORARY : fabricated values. DELETE THIS BLOCK.
            //
            // These numbers are NOT TRUE. exchange_rate = 1.0 asserts that a zloty and a dollar are
            // worth the same, and usd_amount relabels the local amount as USD without converting it.
            // They have been reported to analytics on every purchase for months.
            //
            // They are kept ONLY because price_metadata is part of the public PurchaseResult contract
            // and the host game already reads these keys — removing them outright risks throwing in
            // its purchase-completion handler, which is worse than a wrong number.
            //
            // This block is self-retiring : the guards below only fire while the server is silent.
            // The moment /session returns exchange_rate / usd_amount in price_data, the real values
            // above win and these never run again. Delete this block once that has shipped and the
            // real values are confirmed in analytics.
            //
            // Do NOT copy this pattern. An absent field is a gap someone fixes; a fabricated one is
            // a wrong answer wearing a data costume, which is why this went unnoticed for so long.
            if (!result.ContainsKey("exchange_rate"))       result["exchange_rate"]       = "1.0";
            if (!result.ContainsKey("usd_amount"))          result["usd_amount"]          = result["amount"];
            if (!result.ContainsKey("usd_amount_with_tax")) result["usd_amount_with_tax"] = result["amount_with_tax"];

            if (CHECKOUT.Globals.IsInternal && !priceData.exchange_rate.HasValue)
                UnityEngine.Debug.LogWarning("[Galleon.Checkout] price_metadata is reporting a FABRICATED exchange_rate of 1.0 "
                                           + "because /session did not return one. Analytics for this purchase are not trustworthy. "
                                           + "This warning is internal-only and will stop once the server sends the real value.");

            return result;
        }
        
    }
}
