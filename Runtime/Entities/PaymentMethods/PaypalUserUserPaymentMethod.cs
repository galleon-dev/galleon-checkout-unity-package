namespace Galleon.Checkout
{
    public class PaypalUserPaymentMethod : PaymentMethod
    {
        public Step CreatePaypalOrder()
        =>
            new Step(name   : $"create_paypal_order"
                    ,action : async (s) =>
                    {
                        
                    });
        
        public Step open_url_and_await_response()
        =>
            new Step(name   : $"open_url_and_await_response"
                    ,action : async (s) =>
                    {
                        
                    });
        
        public Step GetPurchaseResult()
        =>
            new Step(name   : $"GetPurchaseResult"
                    ,action : async (s) =>
                    {
                        
                    });
    }
}