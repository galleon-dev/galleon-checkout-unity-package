using Galleon.Checkout;
using Galleon.Checkout.ELEMENTS;

namespace Galleon.Checkout.ELEMENTS
{
    [Element("Scene")]
    public class Scene : Element
    {
        public Scene(string name) : base(name)
        {
        }
    }
}

namespace Galleon.Checkout.RT
{
    [Element("Scene")]
    public class Scene : Entity
    {
        
    }
}

namespace Galleon.Checkout.Assets
{
    [Element("Scene")]
    public class Scene : Asset
    {
        
    }
}

namespace Galleon.Checkout.Hierarchy
{
    [Element("Scene")]
    public class Scene
    {
        
    }
}