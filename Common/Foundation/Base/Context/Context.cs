using UnityEngine.Serialization;

namespace Galleon.Checkout.Foundation
{
    public class Context : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public Project             Project  = new Project();
        
        public OperationController Operations     = new();
        public StepController      StepController = new StepController();
        
        public StorageService      StorageService = new();
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public Context()
        {
            // Search for packages
        }
        
    }
}

