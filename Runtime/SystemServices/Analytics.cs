using UnityEngine;

namespace Galleon.Checkout
{
    public class Analytics
    {
        public Step Initialize => new Step(name   : "initialize_analytics"
                                          ,tags   : new[] { "init"}
                                          ,action : async s =>
                                          {
                                              
                                          });
    }
}