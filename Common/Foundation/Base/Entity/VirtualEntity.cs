using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout.Assets;
using Galleon.Checkout.ELEMENTS;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace Galleon.Checkout.Foundation
{
    public class VirtualEntity : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// TEMP
        
        public        ThingData thingData => new ThingData(this);
        public struct ThingData
        {
            public VirtualEntity ve;
            public ThingData(VirtualEntity ve) { this.ve = ve; }
            
            public ELEMENTS.ThingElement   Element     => Root.Instance.Context.Project.Package1.allElements.ThingElement;
            
            public string           ThingName   => ve.TextNode?.LineWords != null && ve.TextNode.LineWords.Count() >= 2 
                                                 ? ve.TextNode.LineWords.ElementAt(1) 
                                                 : string.Empty;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// General
        
        public TextNode         TextNode    { get; set; }
        
        public string[]         Tags        => TextNode.Hashtags.Select(x => x.Trim('#')).ToArray();
        
        public string           Prompt      => TextNode.Equals.ContainsKey("prompt") ? TextNode.Equals["prompt"] : string.Empty;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public VirtualEntity(string text)
        {
            this.TextNode             = new TextNode(text);
            
            var id                    = CreateStorageID();
            this.Node.CustomStorageID = GetStorageID;         
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// ToString

        public override string ToString() => $"(ve) " + this.TextNode?.Line ?? "Virtual Entity (null)";
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// ID
        
        public string GetStorageID()
        {
            var prefix = "veid_";
            var id = this.TextNode.Hashtags.FirstOrDefault(x => x.StartsWith(prefix));

            if (id != null)
                return id.Substring(prefix.Length);

            return null;
        }

        public string CreateStorageID()
        {
            var id = DateTime.UtcNow.Ticks.ToString("X"); 
            TextNode.RawText += $" #veid_{id} ";
            this.StoreState();
            return id;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - CRUD actions
        
        public Step DeleteVirtualEntity() 
        =>
            new Step(name   : $"delete_virtual_entity"
                    ,action : async (s) =>
                    {
                        this.Node.Parent.Node.Live.RemoveVirtualEntity(this);
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
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Storage
        
        public void StoreState()
        {
            var id  = GetStorageID();
            var key = $"virtual_entity_{id}";
            Root.Instance.Context.SystemServices.SessionStorageService.Store(key, this.TextNode.RawText);
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Operation

        public Step ExecuteOperation() 
        =>
            new Step(name   : $"execute_operation"
                    ,action : async (s) =>
                    {
                        this.TextNode.RawText += "state:pending";
                        StoreState();
                    });
        
        public string State
        {
            get
            {
                return this.TextNode.Colons.ContainsKey("state") ? this.TextNode.Colons["state"] : "undefined";
            }
            set
            {
                if (this.State != "undefined")
                    this.TextNode.RawText = this.TextNode.RawText.Replace($"state:{this.State}", $"state:{value}");
                else
                    this.TextNode.RawText += $"state:{value}";
            }
        }

        public bool HasPendingOperation() => this.TextNode.Colons.ContainsKey("state") && this.TextNode.Colons["state"] == "pending";

        public Step ResumeOperation() 
        =>
            new Step(name   : $"resume_operation"
                    ,action : async (s) =>
                    {
                        if (!HasPendingOperation()) 
                            return;
                        
                        this.TextNode.RawText = this.TextNode.RawText.Replace("state:pending", "state:assets");
                        StoreState();
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Temp resume op
        
        
        #if UNITY_EDITOR
        [InitializeOnLoadMethod]
        #endif
        public static async void InitializeOnLoad()
        {
            var ves = Root.Home.SliceHub.Node.Descendants().OfType<VirtualEntity>();
            
            foreach (var ve in ves)
            {
                if (ve.State != "undefined" && ve.State != "done") 
                await ve.PrintThing().Execute();
            }
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - TEMP PRINT
        
        public Step PrintThing() 
        =>
            new Step(name   : $"print_thing"
                    ,action : async (s) =>
                    {
                        s.Log($"state = {State}");
                        
                        var thingElement = new ELEMENTS.ThingElement();
                        
                        if (State == "undefined"
                        ||  State == "pending")
                        {
                            thingElement.CreateThingAsset(this).Execute();
                            State = "assets";
                            StoreState();
                        }
                        if (State == "assets")
                        {
                            await Task.Delay(5000);
                            #if UNITY_EDITOR
                            AssetDatabase.Refresh(options: ImportAssetOptions.ForceUpdate);
                            #endif
                            State = "reload";
                            StoreState();
                        }
                        if (State == "reload")
                        {
                            thingElement.CreateThingHierarchy(this).Execute();
                            State = "hierarchy";
                            StoreState();
                        }
                        if (State == "hierarchy")
                        {
                            thingElement.CreateThingApp(this).Execute();
                            State = "done";
                            StoreState();
                        }
                        if (State == "done")
                        {
                            this.DeleteVirtualEntity().Execute();
                        }
                    });
    }
}

