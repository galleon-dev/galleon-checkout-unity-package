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
            Element Folder                  = new Element(name : "Folder");                                     this.Collection.Add(Folder);
                Folder.Definition           = new DefinitionNode() { Text = "> Element Folder" }; 
                var folderAssetsNS          = new DefinitionNode() { Text = "   > (assets)" };                  Folder.Definition.Node.AddChild(folderAssetsNS);
                    var Assets              = new DefinitionNode() { Text = "       > Folder" };                folderAssetsNS.Node.AddChild(Assets);
            
            Element Package                 = new Element(name : "Package");                                    this.Collection.Add(Package);
                Package.Definition          = new DefinitionNode() { Text = "> Element Package" }; 
                var packageDefinitionsNS    = new DefinitionNode() { Text = "   > (definitions)" };             Package.Definition.Node.AddChild(packageDefinitionsNS);
                    var FolderDefinition    = new DefinitionNode() { Text = "       > Definition Folder" };     packageDefinitionsNS.Node.AddChild(FolderDefinition);
                var packageElementsNS       = new DefinitionNode() { Text = "   > (elements)" };                Package.Definition.Node.AddChild(packageElementsNS);
                    var FolderElement       = new DefinitionNode() { Text = "       > Element Folder" };        packageElementsNS.Node.AddChild(FolderElement);
                var packageAssetsNS         = new DefinitionNode() { Text = "   > (assets)" };                  Package.Definition.Node.AddChild(packageAssetsNS);
                    var AssetsFolder        = new DefinitionNode() { Text = "       > Folder top" };            packageAssetsNS.Node.AddChild(AssetsFolder);
                var packageHierarchyNS      = new DefinitionNode() { Text = "   > (hierarchy)" };               Package.Definition.Node.AddChild(packageHierarchyNS);
                    var Scene               = new DefinitionNode() { Text = "       > Scene main" };            packageHierarchyNS.Node.AddChild(Scene);
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

