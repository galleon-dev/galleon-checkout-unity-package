using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout.UI;
using UnityEngine;

namespace Galleon.Checkout
{
    public class CheckoutSession : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public CheckoutProduct                    SelectedProduct;
        
        public string                             LastDialogRequest     = null;
        public SimpleDialogPanelView.DialogResult LastDialogResult      = SimpleDialogPanelView.DialogResult.None;
        public PaymentMethod                      PaymentMethodToDelete = null;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties
        
        public CheckoutClient Client => CheckoutClient.Instance; // TODO : should be : Node.Ancestors().OfType<CheckoutClient>().First();
        public User           User   => Client.CurrentUser;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Temp
        
        public static event Action Report;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Steps
        
        public Step Flow()
        =>
            new Step(name   : $"checkout_session_flow"
                    ,action : async (s) =>
                    {
                        await Client.OpenCheckoutScreenMobile();//s.AddChildStep(Client.OpenCheckoutScreenMobile());
                      //s.AddChildStep(Client.CheckoutScreenMobile.ViewSuccessPanel());
                        s.AddChildStep(Client.CheckoutScreenMobile.ViewPage(Client.CheckoutScreenMobile.CheckoutPage));
                        return;
                        s.AddChildStep(Client.CheckoutScreenMobile.ViewTestPanel());
                        s.AddChildStep(new Step("wait a second", action: async (s) => {await Task.Delay(1000);}));
                        s.AddChildStep(Client.CheckoutScreenMobile.ViewCheckoutPanel());
                        s.AddChildStep(new Step("wait a second", action: async (s) => {await Task.Delay(1000);}));
                        s.AddChildStep(Client.CheckoutScreenMobile.ViewCreditCardPanel());
                        s.AddChildStep(new Step("wait a second", action: async (s) => {await Task.Delay(1000);}));
                        s.AddChildStep(Client.CurrentUser.RunTestTransaction());
                        s.AddChildStep(Client.CheckoutScreenMobile.ViewSuccessPanel());
                        s.AddChildStep(new Step("wait a second", action: async (s) => {await Task.Delay(1000);}));
                        s.AddChildStep(Client.CloseCheckoutScreenMobile());
                        
                      //s.AddChildStep(new Step("Report", action: async (s) => { Report?.Invoke(); await Task.Delay(5000); }));
                        
                    });
    }
}
