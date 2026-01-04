using System;
using System.Collections.Generic;
using System.Linq;
using Galleon.Checkout.Foundation;

namespace Galleon.Checkout.ELEMENTS
{
    public class Elements : VirtualEntity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public Collection<Element> Collection = new();
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public Elements()
        {
            Element Folder    = new Element(name : "Folder"); this.Collection.Add(Folder);
            Folder.Definition = DefinitionNode.Parse(new []
                                                     {
                                                        "> Element Folder $name "
                                                     ,  "   > (Assets)          "
                                                     ,  "       > Folder $name  "
                                                     });
            
            Element Package                 = new Element(name : "Package"); this.Collection.Add(Package);
            Package.Definition = DefinitionNode.Parse(new []
                                                      {
                                                         "> Element Package             "
                                                      ,  "   > (Definitions)            "
                                                      ,  "       > Definition Folder    "
                                                      ,  "   > (Elements)               "
                                                      ,  "       > Element Folder       "
                                                      ,  "   > (Assets)                 "
                                                      ,  "       > Folder 'package1'    "
                                                      ,  "   > (Hierarchy)              "
                                                      ,  "       > Scene main           "
                                                      });
            
            Element Scene    = new Element(name : "Scene"); this.Collection.Add(Scene);
            Scene.Definition = DefinitionNode.Parse(new []
                                                     {
                                                        "> Element Scene $name  "
                                                     ,  "   > (Assets)          "
                                                     ,  "       > Scene $name   "
                                                     ,  "   > (Hierarchy)       "
                                                     ,  "       > Scene $name   "
                                                     });
            
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            
            
            Element Thing    = new Element(name : "Thing"); this.Collection.Add(Thing);
            Thing.Definition = DefinitionNode.Parse(new []
                                                      {
                                                         "> Element Thing $name              "
                                                      ,  "   > (Assets)                      "
                                                      ,  "       > Prefab $name              " 
                                                      ,  "       > Script $name              "
                                                      ,  "   > (Hierarchy)                   "
                                                      ,  "       > Prefab $name              "
                                                      ,  "          > Component $name        "
                                                      ,  "          > GO 'Model'             "
                                                      ,  "              > C U.Cube           "
                                                      ,  "   > (Setup)                       "
                                                      ,  "      #size                        "
                                                      ,  "      #pivot                       "
                                                      ,  "      #color                       "
                                                      ,  "      #collider                    "
                                                      });
            
        }
        
        public static Element GetElement(string typeName)
        {
            return GetAllElements().FirstOrDefault(e => e.Name == typeName);
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

