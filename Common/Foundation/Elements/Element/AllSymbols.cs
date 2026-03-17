using System;
using System.Collections.Generic;
using System.Linq;
using Galleon.Checkout.Foundation;

namespace Galleon.Checkout.Symbols
{
    public class AllSymbols : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public Collection<Symbol> Collection = new();
        
        public Symbols.ThingSymbol thingSymbol = new ThingSymbol();
        public Symbols.Slice       sliceSymbol = new Slice("slice");
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public AllSymbols()
        {
            Symbol Folder    = new Symbol(name : "Folder"); this.Collection.Add(Folder);
            Folder.Definition = DefinitionNode.Parse(new []
                                                     {
                                                        "> Element Folder $name "
                                                     ,  "   > (Assets)          "
                                                     ,  "       > Folder $name  "
                                                     });
            
            Symbol Package                 = new Symbol(name : "Package"); this.Collection.Add(Package);
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
            
            Symbol Scene    = new Symbol(name : "Scene"); this.Collection.Add(Scene);
            Scene.Definition = DefinitionNode.Parse(new []
                                                     {
                                                        "> Element Scene $name  "
                                                     ,  "   > (Assets)          "
                                                     ,  "       > Scene $name   "
                                                     ,  "   > (Hierarchy)       "
                                                     ,  "       > Scene $name   "
                                                     });
            
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            
            
            // Element Thing    = new Element(name : "Thing"); this.Collection.Add(Thing);
            // Thing.Definition = DefinitionNode.Parse(new []
            //                                           {
            //                                              "> Element Thing $name              "
            //                                           ,  "   > (Assets)                      "
            //                                           ,  "       > Prefab $name              " 
            //                                           ,  "       > Script $name              "
            //                                           ,  "   > (Hierarchy)                   "
            //                                           ,  "       > Prefab $name              "
            //                                           ,  "          > Component $name        "
            //                                           ,  "          > GO 'Model'             "
            //                                           ,  "              > C U.Cube           "
            //                                           ,  "   > (Setup)                       "
            //                                           ,  "      #size                        "
            //                                           ,  "      #pivot                       "
            //                                           ,  "      #color                       "
            //                                           ,  "      #collider                    "
            //                                           });
            
        }
        
        public static Symbol GetElement(string typeName)
        {
            return GetAllElements().FirstOrDefault(e => e.Name == typeName);
        }
        public static Symbol GetElement(Type type)
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
        
        public static IEnumerable<Symbol> GetAllElements()
        {
            return Root.Instance.Context.Project.Package1.AllSymbols.Collection;
        }
        public static Symbol GetElementByName(string elementName)
        {
            return Root.Instance.Context.Project.Package1.AllSymbols.Collection.FirstOrDefault(x => x.Name == elementName); 
        }
    }
}

