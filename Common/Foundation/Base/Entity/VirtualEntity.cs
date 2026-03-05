using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout.Assets;
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
            
            public ELEMENTS.Thing   Element     => Root.Instance.Context.Project.Package1.Elements.ThingElement;
            
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
                        this.TextNode.RawText += "#pending";
                        StoreState();
                    });
        
        public bool HasPendingOperation() => this.Tags.Contains("pending");
        
        public Step ResumeOperation() 
        =>
            new Step(name   : $"resume_operation"
                    ,action : async (s) =>
                    {
                        if (!HasPendingOperation()) return;
                        
                        this.TextNode.RawText = this.TextNode.RawText.Replace("#pending", "");
                        StoreState();
                        
                        var state = this.TextNode.RawText.Contains($"pending") ? "pending" : "not-pending";
                        Debug.Log($"Resumed operation for {this.TextNode.Line} with state of {state}");
                    });
    }
}

