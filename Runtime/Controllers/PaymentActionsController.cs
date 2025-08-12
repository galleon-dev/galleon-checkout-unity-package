using System.Collections.Generic;
using Galleon.Checkout.Shared;

namespace Galleon.Checkout
{
    public class PaymentActionsController : Entity
    {
        public Step Charge()
        =>
            new Step(name   : $"charge"
                    ,action : async (s) =>
                    {                                               
                        var response = await CHECKOUT.Network.Post<ChargeResponse>(url      : $"{CHECKOUT.Network.SERVER_BASE_URL}/charge"
                                                                                  ,headers  : new ()
                                                                                            {
                                                                                                { "Authorization", $"Bearer {CHECKOUT.Network.GalleonUserAccessToken}" }
                                                                                            }
                                                                                  ,body     : new Shared.ChargeRequest()
                                                                                            {
                                                                                                session_id          = CHECKOUT.Session.SessionID,
                                                                                                is_new_payment_method = CHECKOUT.User.SelectedUserPaymentMethod.IsNewPaymentMethod,
                                                                                                payment_method      = new PaymentMethodDetails()
                                                                                                                   {
                                                                                                                        id   = "1",
                                                                                                                        data = new ()
                                                                                                                             {
                                                                                                                                  { "token", "bla" }
                                                                                                                             }
                                                                                                                   },
                                                                                                save_payment_method  = CHECKOUT.User.SelectedUserPaymentMethod.IsNewPaymentMethod,
                                                                                            });
                        
                        CheckoutClient.Instance.CurrentSession.lastChargeResult = new ChargeResultData()
                                                                                {
                                                                                    errors         = new []{"error"},
                                                                                    is_canceled     = false,
                                                                                    is_success      = true,
                                                                                    charge_id = "12345",
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
        
    }
}