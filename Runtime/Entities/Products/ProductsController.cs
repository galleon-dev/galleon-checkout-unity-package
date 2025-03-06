using System.Collections.Generic;
using UnityEngine;

namespace Galleon.Checkout
{
    public class ProductsController
    {
        public List<Product> products = new();
        
        public Step Initialize => new Step(name   : "initialize_products_controller"
                                          ,tags   : new[] { "init"}
                                          ,action : async s =>
                                          {
                                              
                                          });
    }
}
