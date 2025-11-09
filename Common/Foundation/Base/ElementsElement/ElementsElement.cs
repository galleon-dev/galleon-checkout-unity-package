using System;

namespace Galleon.Checkout.Foundation
{
    public class Elements : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public Collection<Element> Collection = new();
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public Elements()
        {
            Element Folder = new Element(name:"Folder");
            this.Collection.Add(Folder);
        }
    }
}
