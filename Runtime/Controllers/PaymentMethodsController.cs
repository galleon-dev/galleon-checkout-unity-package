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
        
        public List<PaymentMethodDefinition> PaymentMethodsDefinitions { get; set; } = new ();
        
        /////////////////////////////////////////////////////////////// Lifecycle
        
        public Step Initialize() 
        => 
            new Step(name   : "initialize_payment_methods_controller"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {
                        var _result   = await CHECKOUT.Network.Get($"{CHECKOUT.Network.SERVER_BASE_URL}/payment-method-definitions-test");
                        var _response = JsonConvert.DeserializeObject<PaymentMethodDefinitionsResponse>(value : _result.ToString(), settings: PaymentMethodsJsonHelper.JsonSettings);
                        var _methods  = _response.payment_method_definitions;
                        
                        Debug.Log("=====================");
                        Debug.Log($"{_result}");
                        Debug.Log("=====================");
                        
                        var result   = await CHECKOUT.Network.Get($"{CHECKOUT.Network.SERVER_BASE_URL}/payment-methods-test");
                        var response = JsonConvert.DeserializeObject<PaymentMethodsResponse>(value : result.ToString(), settings: PaymentMethodsJsonHelper.JsonSettings);
                        var methods  = response.payment_methods;
                        
                        Debug.Log($"payment methods count: {methods.Count()}");
                        foreach (var method in methods)
                            Debug.Log($"payment method : {method.type}");
                        
                        this.PaymentMethodsDefinitions.Add(new PaymentMethodDefinition()
                                                           {
                                                               Type             = "credit_card",
                                                               VaultingSteps    = { "get_tokenizer", "tokenize" },
                                                               TransactionSteps = { "charge" },
                                                           });
                        
                        this.PaymentMethodsDefinitions.Add(new PaymentMethodDefinition()
                                                           {
                                                               Type                = "paypal",
                                                               InitializationSteps = { "check_availability" },
                                                               TransactionSteps    =
                                                                                   {
                                                                                        "create_order",
                                                                                        "open_url",
                                                                                   },
                                                           });
                        
                        this.PaymentMethodsDefinitions.Add(new PaymentMethodDefinition()
                                                           {
                                                               Type                = "google_pay",
                                                               InitializationSteps = { "check_availability" },
                                                               TransactionSteps    =
                                                                                   {
                                                                                        "create_order",
                                                                                        "open_url",
                                                                                   },
                                                           });
                    });
        
    }
}