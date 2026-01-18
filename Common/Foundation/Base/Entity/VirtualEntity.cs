using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout.Assets;
using Galleon.Checkout.ELEMENTS;
using Galleon.Checkout.Foundation;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace Galleon.Checkout.Foundation
{
    public class VirtualEntity : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// General
        
        public TextNode TextNode { get; set; }
        
        public string ThingName => TextNode?.LineWords != null && TextNode.LineWords.Count() >= 2 
                                  ? TextNode.LineWords.ElementAt(1) 
                                  : string.Empty;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public VirtualEntity()
        {
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Operation
        
        
        public Operation CreateOp => new Operation(id: $"operation_{ThingName}")
                                         .AddStep(CreateAssets())
                                         .AddStep(DomainReload())
                                         .AddStep(CreateHierarchy());
                                        
        public Step CreateAssets() 
        =>
            new Step(name   : $"create_assets"
                    ,action : async (s) =>
                    {
                            #if UNITY_EDITOR
                                                                       
                           AssetDatabase.StartAssetEditing();
                           
                           #endif
                           
                           // create a new default script at path
                           string scriptPath = "Assets/TEMP/domain_reload.cs";
                           System.IO.File.WriteAllText(scriptPath, "");
                    });
        public Step DomainReload() 
        =>
            new Step(name   : $"domain_reload"
                    ,action : async (s) =>
                    {
                        #if UNITY_EDITOR
                        
                        AssetDatabase.StopAssetEditing();
                                                                       
                        AssetDatabase.Refresh(options: ImportAssetOptions.ForceUpdate);
                        await Task.Delay(5000);
                       
                        #endif
                    });
        public Step CreateHierarchy() 
        =>
            new Step(name   : $"create_hierarchy"
                    ,action : async (s) =>
                    {
                        
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Semantics

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

