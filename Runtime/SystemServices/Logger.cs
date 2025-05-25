using UnityEngine;

namespace Galleon.Checkout
{
    public class Logger : Entity
    {
        public Step Initialize() 
        => 
            new Step(name   : "initialize_logger"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {    
                    });
    }
}
