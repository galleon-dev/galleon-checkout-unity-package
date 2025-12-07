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
                        flow.AddChildStep(DumpChildElement()               );
                        flow.AddChildStep(DumpParentElement()              );
                        flow.AddChildStep(ManuallyCreateChildVirtualTree() );
                        flow.AddChildStep(ManuallyCreateFullVirtualTree()  );
                        flow.AddChildStep(ManuallyCreateZipedTree()        );
                        flow.AddChildStep(DumpVirtualTree()                );
                        flow.AddChildStep(RunLiveFlow()                    );
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
                /// > Package
                ///     > (definition)
                ///         > definition
                ///     > (Elements)
                ///         > Element
                ///     > (Assets)
                ///         > Folder "package1"
                ///     > (Hierarchy)
                ///         > Scene "main"
                ///     > (whatever)
                ///         > Whatever "..."
                
                 Element parentElement = this.Parent.Node.GetElement();
                 var dump = parentElement.DumpDefinition();
                 foreach (var line in dump)
                     s.Log(line);
            });
        
        public Step ManuallyCreateChildVirtualTree() 
        =>
            new Step(action : async (s) =>
            {
                this.ChildVirtualTree = ParseTree(new []
                                        {
                                           "> Assets.Folder f1"
                                        });

                foreach (var node in ChildVirtualTree.TextNode.Node.Descendants().OfType<TextNode>())
                    s.Log(node.RawText);
                
            });        
        
        public Step ManuallyCreateZipedTree() 
        =>
            new Step(action : async (s) =>
            {
                var zipTree = ParseTree(new []
                              {
                                 "> Assets.Folder f1"
                              });

                foreach (var node in zipTree.TextNode.Node.Descendants().OfType<TextNode>())
                    s.Log(node.RawText);
                
            });
        
        public Step ManuallyCreateFullVirtualTree() 
        =>
            new Step(action : async (s) =>
            {
                this.FullVirtualTree = ParseTree(new []
                                       {
                                           "> Package package                    #exists "
                                       ,   "    > (Assets)                       #exists "
                                       ,   "        > Assets.Folder 'package 1'  #exists "
                                       ,   "            > Assets.Folder 'f1'     #plus   "
                                       });

                
                
                foreach (var node in ChildVirtualTree.TextNode.Node.Descendants().OfType<TextNode>())
                    s.Log(node.RawText);
            });
        
        public Step RunLiveFlow() 
            =>
            new Step(name   : $"run_live_flow"
                    ,action : async (s) =>
                    {   
                        foreach (var vNode in FullVirtualTree.Node.Descendants().OfType<PPF1M_LiveNode>())
                        {
                            s.Log($"- {vNode.TextNode.RawText} { (vNode.DoesNeedToDoAction ? "<-- action" : "") } ");
                            if (vNode.DoesNeedToDoAction)
                                s.ParentStep.AddChildStep(vNode.DoAction());
                        }                          
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helper debug steps
       
        public Step DumpVirtualTree()
        =>
            new Step(action : async (s) =>
            {
                foreach (var textNode in FullVirtualTree.TextNode.Node.Descendants().OfType<TextNode>())
                    s.Log(textNode.RawText);
            });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helper Parse Methods
        
        public PPF1M_LiveNode ParseTree(string[] lines)
        {
            var root = PPF1M_LiveNode.ParseTree(lines);

            foreach (var node in root.Node.Descendants().OfType<PPF1M_LiveNode>())
                node.Operation = this;
            
            return root;
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
            
            foreach (var textNode in rootTextNode.Node.Descendants().OfType<TextNode>())
            {
                var liveNode = ParseNode(textNode);
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
        
        public static PPF1M_LiveNode ParseNode(TextNode textNode)
        {
            var node      = new PPF1M_LiveNode();
            node.TextNode = textNode;
            
            if (node.TextNode.Hashtags.Contains("plus"))
                node.DoesNeedToDoAction = true;
            
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

