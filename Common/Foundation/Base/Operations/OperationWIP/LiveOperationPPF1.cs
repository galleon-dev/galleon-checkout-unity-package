using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout.Foundation;
using Galleon.Checkout.Foundation.LiveOperationPPF1;
using UnityEngine;

namespace Galleon.Checkout.Foundation.LiveOperationPPF1
{
    public class PPF1_LiveOperation : Entity
    {   
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public string           ID;
        public IEntity          OperationParent;
        
        public PPF1_LiveNode    OriginalTree;
        public PPF1_LiveNode    ChildVirtualTree;
        public PPF1_LiveNode    FullVirtualTree;
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public PPF1_LiveOperation(string id, IEntity operationParent, string definitionText)
        {
            this.ID                     = id;
            this.OperationParent                 = operationParent;
            this.OriginalTree           = new()
                                        {
                                            DefinitionNode = new DefinitionNode(definitionText), 
                                            Operation      = this
                                        };
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Main Flow
        
        public Step Flow() 
        =>
            new Step(name   : $"execute_PPF1"
                    ,action : async (flow) =>
                    {
                        flow.AddChildStep(DumpChildElement()       );
                        flow.AddChildStep(DumpParentElement()      );
                        flow.AddChildStep(CreateChildVirtualTree() );
                        flow.AddChildStep(CreateParentVirtualTree());
                        flow.AddChildStep(CreateZipedTree()        );
                        flow.AddChildStep(CreateFullVirtualTree()  );
                        flow.AddChildStep(DumpVirtualTree()        );
                        flow.AddChildStep(RunLiveFlow()            );
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
                
                 Element parentElement = this.OperationParent.Node.GetElement();
                 var dump = parentElement.DumpDefinition();
                 foreach (var line in dump)
                     s.Log(line);
            });
        
        public Step CreateChildVirtualTree() 
        =>
            new Step(action : async (s) =>
            {
                this.ChildVirtualTree = OriginalTree.CloneTree();
                
                foreach (var node in ChildVirtualTree.TextNode.Node.Descendants().OfType<TextNode>())
                    s.Log(node.RawText);
            });        
        
        public Step CreateParentVirtualTree() 
        =>
            new Step(action : async (s) =>
            {
                Element parentElement = this.OperationParent.Node.GetElement();
                var     parentTree    = parentElement.Definition.CloneTree();
                
                foreach (var node in parentTree.TextNode.Node.Descendants().OfType<TextNode>())
                    s.Log(node.RawText);
            });        
        
        public Step CreateZipedTree() 
        =>
            new Step(action : async (s) =>
            {
                // Create zip tree
                var zipTree = CreateZippedTree();
                
                // Log zip tree
                foreach (var node in zipTree.Node.Descendants().OfType<DefinitionNode>())
                {
                    var expanded = node.DoesNeedToExpandToFullLine() ? node.ExpandToFullLine() : node.TextNode;
                    s.Log($"{node.TextNode.RawText} - (ns:{node.GetNamespace() + ")", -15} ---> | {expanded.RawText}");
                }        
            });
        
        public Step CreateFullVirtualTree() 
        =>
            new Step(action : async (s) =>
            {
                // Create Zipped Tree
                var zipTree = CreateZipedTree();
                
                // Insert Child
                var childTree               = ChildVirtualTree.Node.Descendants().OfType<DefinitionNode>();
                var childTreeNamespaceNodes = childTree.Where(n => n.IsNamespaceNode()).ToList();
                
                foreach (var childNamespaceNode in childTreeNamespaceNodes)
                {
                    var namespaceNodeInZipTree = zipTree.Node.Descendants().OfType<DefinitionNode>().FirstOrDefault();
                }
                
                
                foreach (var node in ChildVirtualTree.TextNode.Node.Descendants().OfType<TextNode>())
                    s.Log(node.RawText);
            });
        
        public Step RunLiveFlow() 
            =>
            new Step(name   : $"run_live_flow"
                    ,action : async (s) =>
                    {   
                        foreach (var vNode in FullVirtualTree.Node.Descendants().OfType<PPF1_LiveNode>())
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
        
        public PPF1_LiveNode ParseTree(string[] lines)
        {
            var root = PPF1_LiveNode.ParseTree(lines);

            foreach (var node in root.Node.Descendants().OfType<PPF1_LiveNode>())
                node.Operation = this;
            
            return root;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helper Methods
        
        public DefinitionNode CreateZippedTree()
        {
            // Definitions
            Element parentElement = this.OperationParent.Node.GetElement();
            Element childElement  = Elements.GetElement(this.ChildVirtualTree.DefinitionNode.EntityType); 
            var     parentTree    = parentElement.Definition.CloneTree();
            var     childTree     = childElement.Definition.CloneTree();
            
            // Get child namespaces
            var childNamespacesNodes = childTree.Node.Descendants().OfType<DefinitionNode>().Where(n => n.IsNamespaceNode()).ToList();
            var childNamespaces      = childNamespacesNodes.Select(n => n.TextNode.FirstLineParenthesisContent).ToList();
            
            // Get parent namespaces
            var parentNamespaceNodes = parentTree.Node.Descendants().OfType<DefinitionNode>().Where(n => n.IsNamespaceNode()).ToList();
            var parentNamespaces     = parentNamespaceNodes.Select(n => n.TextNode.FirstLineParenthesisContent).ToList();
            
            // Clone zip tree from parent tree 
            var zipTree = parentTree.CloneTree();

            // Remove irrelevant namespaces from zip tree
            var zipNamespaceNodes = zipTree.Node.Descendants().OfType<DefinitionNode>().Where(n => n.IsNamespaceNode()).ToList();
            foreach(var namespaceNode in zipNamespaceNodes)
            {
                if (!childNamespaces.Contains(namespaceNode.TextNode.FirstLineParenthesisContent))
                    namespaceNode.Node.RemoveFromParent();
            }
            
            // return result
            return zipTree;
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
    
    public class PPF1_LiveNode : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public PPF1_LiveOperation    Operation               { get; set; }
        
        public DefinitionNode        DefinitionNode          = new DefinitionNode();
        public TextNode              TextNode                => DefinitionNode.TextNode;
        public bool                  DoesNeedToDoAction      = false;
        public IEntity               LinkedCreatedEntity;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties
        
        public string action_type => TextNode == null ? "plus" : "plus";
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Static
        
        public static PPF1_LiveNode ParseTree(string lines) => ParseTree(lines.Split('\n'));
        public static PPF1_LiveNode ParseTree(IEnumerable<string> lines)
        {
            PPF1_LiveNode  rootLiveNode = null;
            TextNode       rootTextNode = TextNode.Parse(lines);
            
            foreach (var textNode in rootTextNode.Node.Descendants().OfType<TextNode>())
            {
                var liveNode = ParseNode(textNode);
                textNode.Node.SetData(key : "live_node", value : liveNode);
                
                if (textNode.Node.Parent != null)
                {
                    var parentText     = textNode  .Node.Parent;
                    var parentLiveNode = parentText.Node.GetData<PPF1_LiveNode>(key : "live_node");
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
                textNode.Node.RemoveData(key : "live_node");
            }
            
            return rootLiveNode;
        }
        
        public static PPF1_LiveNode ParseNode(TextNode textNode)
        {
            var node            = new PPF1_LiveNode();
            node.DefinitionNode = new DefinitionNode(textNode.RawText);
            
            if (node.TextNode.Hashtags.Contains("plus"))
                node.DoesNeedToDoAction = true;
            
            return node;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public PPF1_LiveNode()
        {
        }
        
        public PPF1_LiveNode CloneNode()
        {
            var clone                 = new PPF1_LiveNode();
            clone.Operation           = this.Operation;
            clone.DefinitionNode      = this.DefinitionNode.CloneNode();
            clone.DoesNeedToDoAction  = this.DoesNeedToDoAction;
            clone.LinkedCreatedEntity = this.LinkedCreatedEntity;
            
            return clone;
        }
        
        public PPF1_LiveNode CloneTree()
        {
            var clone = CloneNode();

            foreach (var child in this.Node.Children.OfType<PPF1_LiveNode>())
            {
                var childClone = child.CloneTree();
                childClone.Node.SetParent(clone);
                childClone.DefinitionNode.Node.SetParent(clone.DefinitionNode);
                childClone.DefinitionNode.TextNode.Node.SetParent(clone.DefinitionNode.TextNode);
            }

            return clone;
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
                var parentEntity = (Operation.OperationParent as Package).Assets.rootFolder;
                
                // Child Entity
                var  childEntityTypeName = "Galleon.Checkout." + childLiveNode.TextNode.LineSplits.First();
                Type entityType          = Type.GetType(childEntityTypeName);
                var  childEntity         = (IEntity)Activator.CreateInstance(entityType);
                
                // Add child entity
                parentEntity.Node.AddChild(childEntity);
                childEntity.Node.Live.LiveHandler.OnAddedToParent(parentEntity);
                
                // Store CRUD params
                EntityNode.CRUD_Params crud = new ()
                                              {
                                                 Name = childLiveNode.TextNode.LineWords.Last().Trim('\''), 
                                              };
                
                childEntity.Node.SetData("CRUD_params", crud);
                s.Log($"name : {crud.Name}");
                
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
        public PPF1_LiveNode PPFE1_Get_AssetFolderNode() =>  new PPF1_LiveNode()
        {
            DefinitionNode = new DefinitionNode( "> Assets.Folder f1" ),
        };
    }
}

