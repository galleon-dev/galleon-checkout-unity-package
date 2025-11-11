using Galleon.Checkout.Foundation;
using UnityEngine;

namespace Galleon.Checkout
{
    public class Project : Entity
    {        
        Collection<Package> Packages = new();
        
        public Package Package1 = new();
    }
}
