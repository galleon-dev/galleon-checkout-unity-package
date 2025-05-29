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
                        await CheckoutScreenMobile.OpenCheckoutScreenMobile(); //s.AddChildStep(Client.OpenCheckoutScreenMobile());
                        
                        foreach (var paymentMethod in User.PaymentMethods)
                            paymentMethod.Unselect();
                        if (User.PaymentMethods.Count > 0)
                            User.PaymentMethods.First().Select();
                        
                        s.AddChildStep(Client.CheckoutScreenMobile.ViewPage(Client.CheckoutScreenMobile.CheckoutPage));
                        
                                                
                        _flow.AddPostStep("report", async x =>
                                                    {
                                                        Report?.Invoke();
                                                    });
                        
                        s.AddPostStep("close", async x => CheckoutScreenMobile.CloseCheckoutScreenMobile());
                        
                        return;
                        
                        s.AddChildStep(Client.CheckoutScreenMobile.ViewTestPanel());
                        s.AddChildStep(new Step("wait a second", action: async (s) => {await Task.Delay(1000);}));
                        s.AddChildStep(Client.CheckoutScreenMobile.ViewCheckoutPanel());
                        s.AddChildStep(new Step("wait a second", action: async (s) => {await Task.Delay(1000);}));
                        s.AddChildStep(Client.CheckoutScreenMobile.ViewCreditCardPanel());
                        s.AddChildStep(new Step("wait a second", action: async (s) => {await Task.Delay(1000);}));
                        s.AddChildStep(RunTransaction());
                        s.AddChildStep(Client.CheckoutScreenMobile.ViewSuccessPanel());
                        s.AddChildStep(new Step("wait a second", action: async (s) => {await Task.Delay(1000);}));
                        s.AddChildStep(CheckoutScreenMobile.CloseCheckoutScreenMobile());
                        
                      //s.AddChildStep(new Step("Report", action: async (s) => { Report?.Invoke(); await Task.Delay(5000); }));
                        
                    });
        
        
        public Step RunTransaction()
        =>
            new Step(name   : $"run_test_transaction"
                    ,action : async (s) =>
                    {
                        Task.Run(async () => 
                        {
                            await Task.Delay(2000);
                            Client.CheckoutScreenMobile.LoadingPanelView.FinishLoading();
                        });
                        
                        _flow.AddChildStep(Client.CheckoutScreenMobile.ViewPage(Client.CheckoutScreenMobile.LoadingPage));

                        
                        return;

                        if (User.SelectedPaymentMethod.Type == PaymentMethod.PaymentMethodType.MasterCard.ToString()
                        ||  User.SelectedPaymentMethod.Type == PaymentMethod.PaymentMethodType.Visa      .ToString()
                        ||  User.SelectedPaymentMethod.Type == PaymentMethod.PaymentMethodType.Diners    .ToString()
                        ||  User.SelectedPaymentMethod.Type == PaymentMethod.PaymentMethodType.Discover  .ToString()
                        ||  User.SelectedPaymentMethod.Type == PaymentMethod.PaymentMethodType.Amex      .ToString())
                        {
                            _flow.AddChildStep(Client.CreditCardController.GetTokenizer());
                            _flow.AddChildStep(Client.CreditCardController.Tokenize());
                            _flow.AddChildStep(Client.CreditCardController.Charge());
                        }
                        else if (User.SelectedPaymentMethod.Type == PaymentMethod.PaymentMethodType.PayPal.ToString())
                        {
                            _flow.AddChildStep(Client.PaypalController.GetPaypalAccessToken());
                            _flow.AddChildStep(Client.PaypalController.CreatePaypalOrder());
                            _flow.AddChildStep(Client.PaypalController.OpenAndAwaitPaypalURL());
                            _flow.AddChildStep(Client.PaypalController.CapturePaypalPayment());
                        }
                        else if (User.SelectedPaymentMethod.Type == PaymentMethod.PaymentMethodType.PayPal.ToString())
                        {
                            _flow.AddChildStep(Client.GooglePayController.OpenAndAwaitGPayURL());
                        }
                        
                        _flow.AddChildStep("stop loading", async s => Client.CheckoutScreenMobile.LoadingPanelView.FinishLoading());

                        // var transaction = new Transaction(user: User, creditCardToken: User.MainToken);
                        // this.User.Transactions.Add(transaction);
                        // 
                        // // await this.CurrentTransaction.Purchase();
                        // 
                        // await Task.Yield();
                        // s.Log("Transaction.Purchase");
                        // await Task.Yield();
                        // s.Log("Transaction.ValidateReceipt");
                        // await Task.Yield();
                    });
        
        
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
        
        public class PaymentMethodData
        {
            public string ID;
            public string Type;
            public string DisplayName;
            
            public string[] actions = new[]
                                      {
                                          // Generic
                                          "start    https://www.galleon.com/start_url",
                                          "open_url https://www.galleon.com/web_url",
                                          "result   https://www.galleon.com/result_url",
                                      };
        }
        
        public class CreditCardFlowData
        {
            public string TokenizerURL;
            public string ChargeURL;
        }
        
        ////////////////////
        
        public class ConfigData
        {
            public bool IsCalifornia;
            
            public Dictionary<string, string> GenericConfigValues;
        }
        
        ////////////////////
        
        public class TaxData
        {
            public Dictionary<string, float> taxes = new();
        }
        
        
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}


