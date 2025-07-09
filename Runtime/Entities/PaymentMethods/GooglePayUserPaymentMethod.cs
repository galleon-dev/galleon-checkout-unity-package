namespace Galleon.Checkout
{
    public class GooglePayUserUserPaymentMethod : UserPaymentMethod
    {
        public Step CheckIfGPayIsAvailable()
        =>
            new Step(name   : $"check_if_g_pay_is_available"
                    ,action : async (s) =>
                    {
                        
                    });
        
        public Step CreateGPayOrder()
        =>
            new Step(name   : $"create_g_pay_order"
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