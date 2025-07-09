using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Galleon.Checkout.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Galleon.Checkout
{
    public class CreditCardPaymentMethodDefinition : PaymentMethodDefinition
    {
        public string cards = "master_card, visa";
    }
}


