
namespace Galleon.Checkout.Foundation
{
    public class Context : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public Project             Project          = new Project();
        public Products            Products         = new Products();
        
        public SystemServices      SystemServices   = new SystemServices();
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public Context()
        {
            // Search for packages
        }
        
    }
}

