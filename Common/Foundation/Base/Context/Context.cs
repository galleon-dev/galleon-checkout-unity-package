using UnityEngine.Serialization;

namespace Galleon.Checkout.Foundation
{
    public class Context : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        // public Package             Package1       = new();
        
        public OperationController Operations     = new();
        public StepController      StepController = new StepController();
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public Context()
        {
            // Search for packages
        }
        
    }
}