using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout.Foundation
{
    public class PrintOperation : Operation
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public string    PrintText;
        public TextNode  RootTextNode;
        public PrintNode RootPrintNode;
        
        public IEntity   Parent;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// State
        
        public STATE State
        {
            get => this.Data["state"] as STATE ?? null;
            set => this.Data["state"] = value;       
        }
        
        [Serializable]
        public class STATE
        {
            public PrintPhase Phase = PrintPhase.Pending;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Types
        
        public enum PrintPhase
        {
            Pending,
            Prep,
            Parse,
            Assets,
            Refresh,
            Hierarchy,
            Done
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public PrintOperation(string ID, IEntity parent, string text) : base(ID)
        {
            this.PrintText = text;
            this.Parent    = parent;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// API
        
        public Step Print() 
        =>
            new Step(name   : $"print_{this.ID}"
                    ,action : async (s) =>
                    {
                        this.AddStep(Prep     ());
                        this.AddStep(Parse    ());
                        this.AddStep(Assets   ());
                        this.AddStep(Refresh  ());
                        this.AddStep(Hierarchy());
                        this.AddStep(Done     ());
                        
                        await this.Execute();
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Phases


        public Step Prep() 
        =>
            new Step(name   : $"prep"
                    ,action : async (s) =>
                    {    
                    });

        public Step Parse() 
        =>
            new Step(name   : $"parse"
                    ,action : async (s) =>
                    {    
                        this.RootTextNode = TextNode.Parse(this.PrintText);
                        RootTextNode.Node.LogDumpTree();
                    });

        public Step Assets() 
        =>
            new Step(name   : $"assets"
                    ,action : async (s) =>
                    {    
                        this.RootPrintNode = PrintNode.Parse(this.RootTextNode);
                        RootPrintNode.Node.LogDumpTree();

                        foreach (var node in RootPrintNode.Node.Descendants().Skip(1).OfType<PrintNode>())
                        {
                            var typeName      = node.TextNode.LineWords.First();
                            var assetTypeName = $"{typeName}Asset";
                            
                            var commonName = node.TextNode.LineWords.ElementAt(1);
                            
                            var assetType     = Type.GetType($"Galleon.Checkout.Foundation.{assetTypeName}");
                            var asset         = Activator.CreateInstance(assetType) as IEntity;
                            
                            asset.Node.CRUDCommonName = commonName;
                            
                            this.Parent.Node.Live.Plus(asset as IEntity);
                            
                            node.CreatedEntity = asset as IEntity;
                        }
                    });

        public Step Refresh() 
        =>
            new Step(name   : $"refresh"
                    ,action : async (s) =>
                    {  
                        #if UNITY_EDITOR
                        AssetDatabase.Refresh();
                        #endif
                    });

        public Step Hierarchy() 
        =>
            new Step(name   : $"hierarchy"
                    ,action : async (s) =>
                    {  
                    });

        public Step Done() 
        =>
            new Step(name   : $"done"
                    ,action : async (s) =>
                    {    
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Phase Helpers
        
        public async Task PostPhase()
        {
            
        }
    }
}

