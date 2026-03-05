
namespace Galleon.Checkout.Foundation
{
    public partial class Context : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public Home                Home             = new Home();
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

