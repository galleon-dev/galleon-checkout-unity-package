using System.Collections.Generic;
using Galleon.Checkout.Shared;

namespace Galleon.Checkout
{
    public class CheckoutActionsController : Entity
    {
        public Step Charge()
        =>
            new Step(name   : $"charge"
                    ,action : async (s) =>
                    {                                               
                        var selectedUserPaymentMethod = CHECKOUT.User.SelectedUserPaymentMethod;
                        
                        var response = await CHECKOUT.Network.Post<ChargeResponse>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/charge"
                                                                                  ,headers  : new ()
                                                                                            {
                                                                                                { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                            }
                                                                                  ,body     : new Shared.ChargeRequest()
                                                                                            {
                                                                                                session_id              = CHECKOUT.Session.SessionID,
                                                                                                is_new_payment_method   = selectedUserPaymentMethod.IsNewPaymentMethod,
                                                                                                payment_method          = new PaymentMethodDetails()
                                                                                                                        {
                                                                                                                             id   = selectedUserPaymentMethod.Data.id,
                                                                                                                             data = new ()
                                                                                                                                  {
                                                                                                                             //          { "token", ((CreditCardUserUserPaymentMethod)selectedUserPaymentMethod).TokenID }
                                                                                                                                  }
                                                                                                                        },
                                                                                                save_payment_method     = selectedUserPaymentMethod.IsNewPaymentMethod,
                                                                                            });
                        
                        CheckoutClient.Instance.CurrentSession.lastChargeResult = new ChargeResultData()
                                                                                {
                                                                                    errors      = new [] { "error" },
                                                                                    is_canceled = false,
                                                                                    is_success  = true,
                                                                                    charge_id   = "12345",
                                                                                };
                        
                        if (response.next_actions != null)
                        {
                            var        flow        = s.ParentStep;
                            List<Step> nextActions = new();

                            foreach (var step in nextActions)
                            {
                                flow.AddChildStep(step);
                            }
                        }
                        else
                        {
                            // NO TRANSACTION RESULT AND NO NEXT ACTION . ERROR .
                        }
                    });      
        
        public Step SetEmail() 
        =>
            new Step(name   : $"set_email"
                    ,action : async (s) =>
                    {
                        var email     = CHECKOUT.Session.User.Email;
                        var sessionID = CHECKOUT.Session.SessionID;
                        
                        var response  = await CHECKOUT.Network.Post<UpdateEmailResponse>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/update-email"
                                                                                        ,headers  : new ()
                                                                                                  {
                                                                                                      { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                                  }
                                                                                        ,body     : new Shared.UpdateEmailRequest()
                                                                                                  {
                                                                                                      email      = email,
                                                                                                      session_id = sessionID,
                                                                                                  });
                    });
    }
}

