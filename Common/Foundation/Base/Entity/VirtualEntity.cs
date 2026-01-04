using System.Collections.Generic;
using System.Linq;
using Galleon.Checkout.Assets;
using Galleon.Checkout.Foundation;

namespace Galleon.Checkout.Foundation
{
    public class VirtualEntity : Entity
    {
        public string Namespace
        {
            get
            {
                if (this is Asset) return "asset";
                else               return "";
            }
        }
        
        public IEnumerable<string> GetAllChildNamespaces()
        {
            return this.Node.Descendants()
                            .OfType<VirtualEntity>()
                            .Where   (x => x.Namespace != "")
                            .Select  (x => x.Namespace)
                            .Distinct();
        }
        
        public VirtualEntity GetDefaultParentForNamespace(string ns)
        {
            var target = this.Node.Descendants().OfType<VirtualEntity>().First(x => x.Namespace == ns);
            return target;
        }
        
        public VirtualEntity GetTopNodeForNamespace(string ns)
        {
            var target = this.Node.Descendants().OfType<VirtualEntity>().First(x => x.Namespace == ns);
            return target;
        }
    }
}