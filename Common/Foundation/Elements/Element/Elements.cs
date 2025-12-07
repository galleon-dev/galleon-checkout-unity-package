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
            Element Folder    = new Element(name : "Folder"); this.Collection.Add(Folder);
            Folder.Definition = DefinitionNode.Parse(new []
                                                     {
                                                        "> Element Folder "
                                                     ,  "   > (assets)    "
                                                     ,  "       > Folder  "
                                                     });
            
          //    Folder.Definition           = new DefinitionNode() { TextNode = "> Element Folder" }; 
          //    var folderAssetsNS          = new DefinitionNode() { TextNode = "   > (assets)" };                  Folder.Definition.Node.AddChild(folderAssetsNS);
          //        var Assets              = new DefinitionNode() { TextNode = "       > Folder" };                folderAssetsNS.Node.AddChild(Assets);
            
            Element Package                 = new Element(name : "Package"); this.Collection.Add(Package);
            Package.Definition = DefinitionNode.Parse(new []
                                                      {
                                                         "> Element Package             "
                                                      ,  "   > (definitions)            "
                                                      ,  "       > Definition Folder    "
                                                      ,  "   > (elements)               "
                                                      ,  "       > Element Folder       "
                                                      ,  "   > (assets)                 "
                                                      ,  "       > Folder top           "
                                                      ,  "   > (hierarchy)              "
                                                      ,  "       > Scene main           "
                                                      });
            
            
          //    Package.Definition          = new DefinitionNode() { TextNode = "> Element Package" }; 
          //    var packageDefinitionsNS    = new DefinitionNode() { TextNode = "   > (definitions)" };             Package.Definition.Node.AddChild(packageDefinitionsNS);
          //        var FolderDefinition    = new DefinitionNode() { TextNode = "       > Definition Folder" };     packageDefinitionsNS.Node.AddChild(FolderDefinition);
          //    var packageElementsNS       = new DefinitionNode() { TextNode = "   > (elements)" };                Package.Definition.Node.AddChild(packageElementsNS);
          //        var FolderElement       = new DefinitionNode() { TextNode = "       > Element Folder" };        packageElementsNS.Node.AddChild(FolderElement);
          //    var packageAssetsNS         = new DefinitionNode() { TextNode = "   > (assets)" };                  Package.Definition.Node.AddChild(packageAssetsNS);
          //        var AssetsFolder        = new DefinitionNode() { TextNode = "       > Folder top" };            packageAssetsNS.Node.AddChild(AssetsFolder);
          //    var packageHierarchyNS      = new DefinitionNode() { TextNode = "   > (hierarchy)" };               Package.Definition.Node.AddChild(packageHierarchyNS);
          //        var Scene               = new DefinitionNode() { TextNode = "       > Scene main" };            packageHierarchyNS.Node.AddChild(Scene);
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

