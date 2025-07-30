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
                                                                                                SessionId          = CHECKOUT.Session.SessionID,
                                                                                                IsNewPaymentMethod = CHECKOUT.User.SelectedUserPaymentMethod.IsNewPaymentMethod,
                                                                                                PaymentMethod      = new PaymentMethodDetails()
                                                                                                                   {
                                                                                                                        id   = "1",
                                                                                                                        data = new ()
                                                                                                                             {
                                                                                                                                  { "token", "bla" }
                                                                                                                             }
                                                                                                                   },
                                                                                                SavePaymentMethod  = CHECKOUT.User.SelectedUserPaymentMethod.IsNewPaymentMethod,
                                                                                            });
                        
                        if (response.Success)
                        {
                            CheckoutClient.Instance.CurrentSession.lastTransactionResult = new TransactionResultData()
                                                                                         {
                                                                                             errors         = response.Errors,
                                                                                             isCanceled     = false,
                                                                                             isSuccess      = true,
                                                                                             transaction_id = "12345",
                                                                                         };
                        }
                        else if (response.NextActions != null)
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