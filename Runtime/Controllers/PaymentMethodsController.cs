using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;

namespace Galleon.Checkout
{
    public class PaymentMethodsController
    {
        /////////////////////////////////////////////////////////////// Members
        
        public List<PaymentMethod> PaymentMethods { get; set; } = new ();
        
        /////////////////////////////////////////////////////////////// Lifecycle
        
        public Step Initialize() 
        => 
            new Step(name   : "initialize_payment_methods_controller"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {
                        var response = await CHECKOUT.Network.Get($"{CHECKOUT.Network.SERVER_BASE_URL}/payment-methods-test");
                        
                        
                        this.PaymentMethods.Add(new CreditCardPaymentMethod());
                        this.PaymentMethods.Add(new PayPalPaymentMethod());
                        this.PaymentMethods.Add(new GPayPaymentMethod());
                    });
    }
}