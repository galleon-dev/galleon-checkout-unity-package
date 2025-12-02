using System;
using System.Collections.Generic;
using System.Linq;

namespace Galleon.Checkout.Foundation
{
    public class Elements : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public Collection<Element> Collection = new();
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public Elements()
        {
            Element Folder              = new Element(name : "Folder");                     this.Collection.Add(Folder);
                var folderAssetsNS      = new DefinitionNode() { Value = "(assets)" };      Folder.Node.AddChild(folderAssetsNS);
                    var Assets          = new DefinitionNode()     { Value = "Folder" };    folderAssetsNS.Node.AddChild(Assets);
            
            Element Package             = new Element(name : "Package");                    this.Collection.Add(Package);
                var packageAssetsNS     = new DefinitionNode() { Value = "(assets)" };      Package.Node.AddChild(packageAssetsNS);
                    var AssetsFolder    = new DefinitionNode()     { Value = "Folder" };    packageAssetsNS.Node.AddChild(AssetsFolder);
                var packageHierarchyNS  = new DefinitionNode() { Value = "(hierarchy)" };   Package.Node.AddChild(packageHierarchyNS);
                    var Scene           = new DefinitionNode()     { Value = "Scene" };     packageHierarchyNS.Node.AddChild(Scene);
        }
        
        public static Element GetElement(Type type)
        {
            var elements = GetAllElements();

            // Check for Element attribute
            var elementAttribute = type.GetCustomAttributes(typeof(ElementAttribute), true).FirstOrDefault() as ElementAttribute;
            if (elementAttribute != null)
                return elements.FirstOrDefault(e => e.Name == elementAttribute.ElementName);

            // Check for IEntity implementation
            if (typeof(IEntity).IsAssignableFrom(type))
                return elements.FirstOrDefault(e => e.Name == type.Name);

            // not found
            return null;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Static
        
        public static IEnumerable<Element> GetAllElements()
        {
            return Root.Instance.Context.Project.Package1.Elements.Collection;
        }
        public static Element GetElementByName(string elementName)
        {
            return Root.Instance.Context.Project.Package1.Elements.Collection.FirstOrDefault(x => x.Name == elementName);
        }
    }
}

