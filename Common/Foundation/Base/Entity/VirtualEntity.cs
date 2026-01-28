using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout.Assets;
using Galleon.Checkout.ELEMENTS;
using Galleon.Checkout.Foundation;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace Galleon.Checkout.Foundation
{
    public class VirtualEntity : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// General
        
        public TextNode TextNode  { get; set; }
        
        public string   ThingName => TextNode?.LineWords != null && TextNode.LineWords.Count() >= 2 
                                   ? TextNode.LineWords.ElementAt(1) 
                                   : string.Empty;
        
        public Thing    Element   => Root.Instance.Context.Project.Package1.Elements.ThingElement;
        
        
        public string[] Tags => TextNode.Hashtags.Select(x => x.Trim('#')).ToArray();
        
        public string Prompt => TextNode.Equals.ContainsKey("prompt") ? TextNode.Equals["prompt"] : string.Empty;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public VirtualEntity()
        {
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// State
        
        public string State         =  "assets";
        public string StateFilePath => Path.Combine(Application.dataPath, "TEMP", $"ve_state_{ThingName}.json");
        
        public async Task SaveState()
        {
            #if UNITY_EDITOR
            var key = $"ve_{ThingName}_State";
            SessionState.SetString(key, State);
          //Debug.Log($"saved : {key} = {State}");
            return;
            #endif
            
          //File.WriteAllText(path : StateFilePath, contents: State);
        }
        
        public async Task LoadState()
        {
            #if UNITY_EDITOR
            var key = $"ve_{ThingName}_State";
            this.State = SessionState.GetString($"ve_{ThingName}_State", defaultValue:"default");
          //Debug.Log($"loaded : {key} = {State}");
            return;
            #endif
            
          //try
          //{
          //    this.State = await File.ReadAllTextAsync(path : StateFilePath);
          //}
          //catch (Exception e)
          //{
          //    this.State = "error";
          //}
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Operation
        
        public async Task DoNextLiveStep()
        {
            // Debug.Log(StateFilePath);
            await LoadState();
            
            if (this.State == "assets")
            {
                #if UNITY_EDITOR
                
                AssetDatabase.StartAssetEditing();
                
                await Element.CreateThingAsset(ve : this).Execute();
                
                State = "hierarchy";
                await SaveState();
                
                
                AssetDatabase.StopAssetEditing();
                
                #endif // UNITY_EDITOR
            }
            else if (this.State == "hierarchy")
            {
                State = "done";
                await SaveState();
                
                await Element.CreateThingHierarchy(ve : this).Execute();
                
            }
            else if (this.State == "app")
            {
                await Element.CreateThingApp(ve: this).Execute();
            }
            else if (this.State == "done")
            {
                
            }
        }
        
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

