using System.Collections.Generic;

namespace Galleon.Checkout
{
    public class PaymentMethodsController
    {
        /////////////////////////////////////////////////////////////// Members
        
        public List<PaymentMethod> PaymentMethods { get; set; }
        
        /////////////////////////////////////////////////////////////// Lifecycle
        
        public Step Initialize() 
        => 
            new Step(name   : "initialize_payment_methods_controller"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {
                        this.PaymentMethods.Add(new CreditCardPaymentMethod());
                        this.PaymentMethods.Add(new PayPalPaymentMethod());
                        this.PaymentMethods.Add(new GPayPaymentMethod());
                    });
    }
}