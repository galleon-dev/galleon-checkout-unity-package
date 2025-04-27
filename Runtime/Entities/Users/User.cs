using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Galleon.Checkout
{
    public class User
    {
        // Members
        public string                ID     = new Guid().ToString();
        public string                Name   = "Fake User";
        public List<CreditCardToken> Tokens = new List<CreditCardToken>();
        
        public List<Transaction>     Transactions = new List<Transaction>();
        public Transaction           CurrentTransaction;
        
        // Properties
        public CreditCardToken       MainToken    => Tokens.FirstOrDefault();
        
        // Methods
        public Step RunTestTransaction()
        =>
            new Step(name   : $"run_test_transaction"
                    ,action : async (s) =>
                    {
                        this.CurrentTransaction = new Transaction(user : this, creditCardToken: this.MainToken);
                        this.Transactions.Add(this.CurrentTransaction);
                        
                        // await this.CurrentTransaction.Purchase();
                        
                        await Task.Yield();
                        s.Log("Transaction.Purchase");
                        await Task.Yield();
                        s.Log("Transaction.ValidateReceipt");
                        await Task.Yield();
                    });
    }
}

