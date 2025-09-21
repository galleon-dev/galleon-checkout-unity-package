using Galleon.Checkout;
using Galleon.Checkout.Shared;
using Newtonsoft.Json;
using UnityEngine.UIElements;

namespace Galleon.Checkout
{
    public class PaymentMethod : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public PaymentMethodData        Data;
        
        public PaymentMethodDefinition  Definition;
        public UserPaymentMethod        UserData;
        
        public bool                     IsSelected;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties
        
        public bool                     IsUserPaymentMethod      => UserData != null;
        public bool                     IPaymentMethodDefinition => UserData != null;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public PaymentMethod(PaymentMethodData data)
        {
            if (data.data_type == "definition_data")
            {
                this.Definition        = new PaymentMethodDefinition();
                this.Definition.Data   = data.data as PaymentMethodDefinitionData;
            }
            else if (data.data_type == "user_data")
            {
                this.UserData          = new UserPaymentMethod();
                this.UserData.Data     = data.data as UserPaymentMethodData;
            }
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Inspector
        
        public class Inspector : Inspector<PaymentMethod>
        {
            public Inspector(PaymentMethod target) : base(target)
            {
                this.Add(new Label($"Payment Method Type : {target.Data.payment_method_type}" ));
                this.Add(new Label($"Data Type :           {target.Data.data_type}"           ));
            }
        }
    }
}

