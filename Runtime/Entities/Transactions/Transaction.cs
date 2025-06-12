using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galleon.Checkout
{
    public class Transaction
    {
        //////////////////////////////////////// Members
        
        public Guid       ID               = new Guid();
        public bool       IsDone           = false;
        
        public List<Step> TransactionSteps = new List<Step>();

        //////////////////////////////////////// Lifecycle
        
        public Transaction()
        {
        }
    }
}
