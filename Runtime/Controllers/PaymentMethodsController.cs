using System.Collections.Generic;
using System.Linq;
using Galleon.Checkout.Shared;
using Newtonsoft.Json;
using UnityEngine;

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
                        var result   = await CHECKOUT.Network.Get($"{CHECKOUT.Network.SERVER_BASE_URL}/payment-methods-test");
                        var response = JsonConvert.DeserializeObject<PaymentMethodsResponse>(result.ToString());
                        var methods  = response.payment_methods;
                        
                        Debug.Log($"payment methods count: {methods.Count()}");
                        foreach (var method in methods)
                            Debug.Log($"payment method : {method.type}");
                        
                        this.PaymentMethods.Add(new CreditCardPaymentMethod());
                        this.PaymentMethods.Add(new PayPalPaymentMethod());
                        this.PaymentMethods.Add(new GPayPaymentMethod());
                    });
    }
}