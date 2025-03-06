using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Galleon.Checkout
{
    public class User
    {
        public string                ID     = new Guid().ToString();
        public string                Name   = "Fake User";
        public List<CreditCardToken> Tokens = new List<CreditCardToken>();
        
        public CreditCardToken       MainToken    => Tokens.FirstOrDefault();
    }
}

