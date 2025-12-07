using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout.Foundation;
using Galleon.Checkout.Foundation.LiveOperationPPF1M;
using UnityEngine;

namespace Galleon.Checkout.Foundation.LiveOperationPPF1M
{
    public class PPF1M_LiveOperation : Entity
    {   
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public string            ID;
        public IEntity           Parent;
        
        public PPF1M_LiveNode    OriginalTree;
        public PPF1M_LiveNode    ChildVirtualTree;
        public PPF1M_LiveNode    FullVirtualTree;
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public PPF1M_LiveOperation(string id, IEntity parent, string definition)
        {
            this.ID                     = id;
            this.Parent                 = parent;
            this.OriginalTree           = new ()
                                        {
                                            TextNode = new TextNode(definition)
                                        };
            this.OriginalTree.Operation = this;
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Main Flow
        
        public Step Flow() 
        =>
            new Step(name   : $"execute_PPF1M"
                    ,action : async (flow) =>
                    {
                        flow.AddChildStep(DumpChildElement()              );
                        flow.AddChildStep(DumpParentElement()             );
                        flow.AddChildStep(ManuallyCreateChildVirtualTree());
                        flow.AddChildStep(ManuallyCreateFullVirtualTree() );
                        flow.AddChildStep(DumpVirtualTree()               );
        
                        flow.AddChildStep("setup_flow"
                                         ,async s =>
                                                  {
                                                      foreach (var vNode in FullVirtualTree.Node.Descendants().OfType<PPF1M_LiveNode>())
                                                      {
                                                          if (vNode.DoesNeedToDoAction)
                                                              flow.AddChildStep(vNode.DoAction());
                                                      }
                                                  
                                                  });
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Main Steps
        
        public Step DumpChildElement() 
        =>
            new Step(action : async (s) =>
            {
                 Element childElement = Elements.GetElementByName("Folder");
                 var dump = childElement.DumpDefinition();
                 foreach (var line in dump)
                     s.Log(line);
            });
        
        public Step DumpParentElement() 
        =>
            new Step(action : async (s) =>
            {
                 Element parentElement = this.Parent.Node.GetElement();
                 var dump = parentElement.DumpDefinition();
                 foreach (var line in dump)
                     s.Log(line);
            });
        
        public Step ManuallyCreateChildVirtualTree() 
        =>
            new Step(action : async (s) =>
            {
                this.ChildVirtualTree = new PPF1M_LiveNode()        { TextNode = new TextNode("> Assets.Folder f1"), Operation = this }; ChildVirtualTree.Node.AddChild(ChildVirtualTree); // f1
            });
        
        public Step ManuallyCreateFullVirtualTree() 
        =>
            new Step(action : async (s) =>
            {
                this.FullVirtualTree = PPF1M_LiveNode.ParseTree(new []
                                       {
                                           "> Assets.Folder package1    "
                                       ,   "    > Assets.Folder f1      "
                                       });

                foreach (var textNode in FullVirtualTree.TextNode.Node.Descendants().OfType<TextNode>())
                {
                    s.Log(textNode.RawText);
                }
                
                // this.FullVirtualTree = new PPF1M_LiveNode()        { TextNode = new TextNode("> root"), Operation = this};                                                                              // Root
                //     var pf              = new PPF1M_LiveNode()         { TextNode = new TextNode("> Assets.Folder package1"), Operation = this }; FullVirtualTree.Node.AddChild(pf);                      // package_1
                //         var f1              = new PPF1M_LiveNode()         { TextNode = new TextNode("> Assets.Folder f1"),   Operation = this,  DoesNeedToDoAction = true}; pf.Node.AddChild(f1);         // f1 
            });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helper debug steps
       
        public Step DumpVirtualTree()
        =>
            new Step(action : async (s) =>
            {
                foreach (var item in FullVirtualTree.Node.Descendants().OfType<PPF1M_LiveNode>())
                {
                    var indent = item.Node.Ancestors().Count();
                    var prefix = new string(' ', indent * 4);
                    s.Log($"{prefix}{item.TextNode.FullText}");
                }
            });
    }
    
    /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// ///
    /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// ///
    /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// ///
    /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// ///
    /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// ///
    /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// ///
    /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// ///
    /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// ///
    /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// ///
    /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// ///
    
    public class PPF1M_LiveNode : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public PPF1M_LiveOperation   Operation               { get; set; }
        
        public TextNode              TextNode                = new TextNode();
        public bool                  DoesNeedToDoAction      = false;
        public IEntity               LinkedCreatedEntity;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties
        
        public string action_type => TextNode == null ? "plus" : "plus";
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Static
        
        public static PPF1M_LiveNode ParseTree(string lines) => ParseTree(lines.Split('\n'));
        public static PPF1M_LiveNode ParseTree(IEnumerable<string> lines)
        {
            PPF1M_LiveNode rootLiveNode = null;
            TextNode       rootTextNode = TextNode.Parse(lines);
            
            Debug.Log(">>>");
            foreach (var textNode in rootTextNode.Node.Descendants().OfType<TextNode>())
            {
                Debug.Log(textNode.RawText);
            }
            Debug.Log("<<<");
            
            foreach (var textNode in rootTextNode.Node.Descendants().OfType<TextNode>())
            {
                var liveNode = ParseNode(textNode.RawText);
                textNode.Node.Data["live_node"] = liveNode;
                
                if (textNode.Node.Parent != null)
                {
                    var parentText     = textNode  .Node.Parent;
                    var parentLiveNode = parentText.Node.Data["live_node"] as PPF1M_LiveNode;
                    liveNode.Node.SetParent(parentLiveNode);
                }
                else
                {
                    rootLiveNode = liveNode;
                }
            }
            
            // cleanup
            foreach (var textNode in rootTextNode.Node.Descendants().OfType<TextNode>())
            {
                textNode.Node.Data.Remove("live_node");
            }
            
            return rootLiveNode;
        }
        
        public static PPF1M_LiveNode ParseNode(string nodeText)
        {
            var node      = new PPF1M_LiveNode();
            node.TextNode = TextNode.Parse(nodeText);
            return node;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Main Actions
        
        public Step DoAction() 
        =>
            new Step(name   : $"do_action"
                    ,action : async (s) =>
                    {
                        switch (action_type)
                        {
                            case "plus" : s.AddChildStep(DoPlus()); return;
                            default     :                           return;
                        }
                        
                    });
        
        public Step DoPlus()
        =>
            new Step(action: async s =>
            {
                // Definitions
                var parentLiveNode = this.Node.Parent;
                var childLiveNode  = this;
                
                // Parent Entity
                var parentEntity = (Operation.Parent as Package).Assets.rootFolder;
                
                // Child Entity
                var  childEntityTypeName = "Galleon.Checkout." + childLiveNode.TextNode.LineWords.First();
                Type entityType          = Type.GetType(childEntityTypeName);
                var  childEntity         = (IEntity)Activator.CreateInstance(entityType);
                
                // Add child entity
                parentEntity.Node.AddChild(childEntity);
                childEntity.Node.Live.LiveHandler.OnAddedToParent(parentEntity);
                
                // Create child entity
                childEntity.Node.Live.LiveHandler.Create();
            });
    }
}

    /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// ///
    /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// ///
    /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// ///
    /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// ///
    /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// ///
    /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// ///
    /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// ///
    /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// ///
    /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// ///
    /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// ///

namespace Galleon.Checkout
{
    public partial class Element
    {
        public PPF1M_LiveNode PPFE1M_Get_AssetFolderNode() =>  new PPF1M_LiveNode()
        {
            TextNode = new TextNode("> Assets.Folder f1") 
        };
    }
}

