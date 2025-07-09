using System;
using System.Collections.Generic;
using Galleon.Checkout.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Galleon.Checkout
{
    public class PaymentMethodDefinition
    {
        //// Members
        
        public string                         Type;
        public Shared.PaymentMethodDefinitionData Data;
        
        //// Transaction Steps
        
        public List<string> InitializationSteps  = new();
        public List<string> VaultingSteps        = new();
        public List<string> TransactionSteps     = new();
        public List<string> PostTransactionSteps = new();
        
        //// Sderialization
        
        public class PMDJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => typeof(PaymentMethodDefinition).IsAssignableFrom(objectType);

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var jo   = JObject.Load(reader);
                var type = jo["type"]?.ToString();

                PaymentMethodDefinition obj = type switch
                                            {
                                                "credit_card" => new CreditCardPaymentMethodDefinition(),
                                                "gpay"        => new GPayPaymentMethodDefinition(),
                                                "paypal"      => new PayPalPaymentMethodDefinition(),
                                                _             => throw new Exception($"Unknown type: {type}")
                                            };

                serializer.Populate(jo.CreateReader(), obj);
                return obj;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                JObject jo = JObject.FromObject(value, serializer);
                jo.WriteTo(writer);
            }
        }
    }
}