using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout.Foundation;
using Galleon.Checkout.Foundation.LiveOperationPPF1;
using UnityEngine;

namespace Galleon.Checkout.Foundation.LiveOperationPPF1
{
    public class LiveOperation : Entity
    {   
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public string   ID;
        public IEntity  OperationParent;
       
        public string   OperationType = "plus";
        
        public LiveNode OriginalTree;
        public LiveNode ChildVirtualTree;
        public LiveNode FullVirtualTree;
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public LiveOperation(string id, IEntity operationParent, string definitionText)
        {
            this.ID              = id;
            this.OperationParent = operationParent;
            this.OriginalTree    = new()
                                 {
                                     DefinitionNode = new DefinitionNode(definitionText), 
                                     Operation      = this
                                 };
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Main Flow
        
        public Step Flow() 
        =>
            new Step(name   : $"execute_Flow"
                    ,action : async (flow) =>
                    {
                        flow.AddChildStep(LogOriginalTree()        );
                        flow.AddChildStep(DumpChildElement()       );
                        flow.AddChildStep(DumpParentElement()      );
                        flow.AddChildStep(CreateChildVirtualTree() );
                        flow.AddChildStep(CreateParentVirtualTree());
                        flow.AddChildStep(CreateZipTree()          );
                        flow.AddChildStep(CreateFullVirtualTree()  );
                        flow.AddChildStep(DumpVirtualTree()        );
                        flow.AddChildStep(RunLiveFlow()            );
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Main Steps
        
        
        public Step LogOriginalTree() 
        =>
            new Step(action : async (s) =>
            {
                /// > Folder f1
                
                s.Log(OriginalTree.TextNode.ToTreeString());
            });
        
        public Step DumpChildElement() 
        =>
            new Step(action : async (s) =>
            {
                /// > Element Folder $name
                ///     > (Assets)
                ///         > Folder $name 
                
                Element childElement = Elements.GetElementByName(OriginalTree.DefinitionNode.EntityType);
                var     dump         = childElement.DumpDefinition();
                 
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
                var     dump          = parentElement.DumpDefinition();
                 
                foreach (var line in dump)
                    s.Log(line);
            });
        
        public Step CreateChildVirtualTree() 
        =>
            new Step(action : async (s) =>
            {
                /// > Element Folder f1
                ///     > (Assets)
                ///         > Folder f1
                
                Element childElement  = Elements.GetElementByName(OriginalTree.DefinitionNode.EntityType);
                 
                // Create tree from Element 
                this.ChildVirtualTree = new LiveNode()
                                      {
                                          DefinitionNode = childElement.Definition.CloneTree(),
                                          Operation      = this
                                      };
                
                // Set name from Operation Original Tree
                var childInstanceName = OriginalTree.DefinitionNode.EntityName;
                ChildVirtualTree.DefinitionNode.ApplyVariable("$name", childInstanceName);
                
                // Log
                foreach (var node in ChildVirtualTree.TextNode.Node.Descendants().OfType<TextNode>())
                    s.Log(node.RawText);
            });        
        
        public Step CreateParentVirtualTree() 
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
                var     parentTree    = parentElement.Definition.CloneTree();
                
                foreach (var node in parentTree.TextNode.Node.Descendants().OfType<TextNode>())
                    s.Log(node.RawText);
            });        
        
        public Step CreateZipTree() 
        =>
            new Step(action : async (s) =>
            {
                /// > Package
                ///     > (Assets)
                ///         > Folder "package1"
                
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
                /// > Package                       #exists
                ///     > (Assets)                  #exists
                ///         > Folder "package1"     #exists
                ///             > Folder f1         #plus
                
                
                // Create Zipped Tree
                DefinitionNode fullTree = CreateZippedTree();
                
                // Add #exists Tag
                foreach (var node in fullTree.Node.Descendants().OfType<DefinitionNode>())
                    node.AddTag("#exists");
                
                foreach (var node in fullTree.Node.Descendants().OfType<DefinitionNode>())
                   s.Log(node.TextNode.RawText);
                
                // Insert Child
                var childTree               = ChildVirtualTree.DefinitionNode.Node.Descendants().OfType<DefinitionNode>();
                var childTreeNamespaceNodes = childTree.Where(n => n.IsNamespaceNode()).ToList();
                foreach (var childNamespaceNode in childTreeNamespaceNodes)
                    fullTree.InsertNodeIntoTree(childNamespaceNode);
                
                // Mark Live Action for inserted child nodes
                string actionTag = $"#{OperationType}";
                foreach (var node in fullTree.Node.Descendants().OfType<DefinitionNode>())
                    if (node.TextNode.Hashtags.Count == 0)
                        node.AddTag(actionTag);
                
                // Log
                s.Log("---");
                foreach (var node in fullTree.Node.Descendants().OfType<DefinitionNode>())
                    s.Log(node.TextNode.RawText);
                
                // Done
                this.FullVirtualTree = new LiveNode(definitionRoot: fullTree);
            });
        
        public Step DumpVirtualTree()
        =>
            new Step(action : async (s) =>
            {
                /// > Package                       #exists
                ///     > (Assets)                  #exists
                ///         > Folder "package1"     #exists
                ///             > Folder f1         #plus
                
                foreach (var node in FullVirtualTree.DefinitionNode.Node.Descendants().OfType<DefinitionNode>())
                    s.Log(node.TextNode.RawText);
            });
       
        
        public Step RunLiveFlow() 
        =>
            new Step(name   : $"run_live_flow"
                    ,action : async (s) =>
                    {   
                        foreach (var vNode in FullVirtualTree.Node.Descendants().OfType<LiveNode>())
                        {
                            s.Log($"- {vNode.TextNode.RawText} { (vNode.DoesNeedToDoAction ? "<-- action" : "") } ");
                            
                            if (vNode.DoesNeedToDoAction)
                                s.ParentStep.AddChildStep(vNode.DoAction());
                        }                          
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helper Parse Methods
        
        public LiveNode ParseTree(string[] lines)
        {
            var root = LiveNode.ParseTree(lines);

            foreach (var node in root.Node.Descendants().OfType<LiveNode>())
                node.Operation = this;
            
            return root;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helper Methods
        
        public DefinitionNode CreateZippedTree()
        {
            // Definitions
            Element        parentElement    = this.OperationParent.Node.GetElement();
            Element        childElement     = Elements.GetElement(this.ChildVirtualTree.DefinitionNode.TextNode.LineWords.ElementAt(1)); 
            DefinitionNode parentTree       = parentElement.Definition.CloneTree();
            DefinitionNode childTree        = childElement.Definition.CloneTree();
            
            
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
                {
                    namespaceNode         .Node.RemoveFromParent();
                    namespaceNode.TextNode.Node.RemoveFromParent();
                }
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
    
    public class LiveNode : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public LiveOperation    Operation               { get; set; }
        
        public DefinitionNode   DefinitionNode          = new DefinitionNode();
        public TextNode         TextNode                => DefinitionNode.TextNode;
        public bool             DoesNeedToDoAction      = false;
        public IEntity          LinkedCreatedEntity;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties
        
        public string action_type => TextNode == null ? "plus" : "plus";
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Static
        
        public static LiveNode ParseTree(string lines) => ParseTree(lines.Split('\n'));
        public static LiveNode ParseTree(IEnumerable<string> lines)
        {
            // Initialize root variables
            LiveNode rootLiveNode = null;
            TextNode rootTextNode = TextNode.Parse(lines);
    
            // Process each text node to create corresponding live nodes
            foreach (var textNode in rootTextNode.Node.Descendants().OfType<TextNode>())
            {
                // Parse and create live node from text node
                var liveNode = ParseNode(textNode);
                textNode.Node.SetData(key : "live_node", value : liveNode);
        
                // Set parent-child relationship
                if (textNode.Node.Parent != null)
                {
                    var parentText     = textNode  .Node.Parent;
                    var parentLiveNode = parentText.Node.GetData<LiveNode>(key : "live_node");
                    liveNode.Node.SetParent(parentLiveNode);
                }
                else
                {
                    rootLiveNode = liveNode;
                }
            }
    
            // cleanup temporary live node references from text nodes
            foreach (var textNode in rootTextNode.Node.Descendants().OfType<TextNode>())
            {
                textNode.Node.RemoveData(key : "live_node");
            }
    
            return rootLiveNode;
        }

        public static LiveNode ParseNode(TextNode textNode)
        {
            // Create new live node and set definition
            var node            = new LiveNode();
            node.DefinitionNode = new DefinitionNode(textNode.RawText);
    
            // Set action flag if node has "plus" tag
            if (node.TextNode.Hashtags.Contains("plus"))
                node.DoesNeedToDoAction = true;
    
            return node;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public LiveNode()
        {
        }

        public LiveNode(DefinitionNode definitionRoot)
        {
            DefinitionNode = definitionRoot;   
        }
        
        public LiveNode CloneNode()
        {
            var clone                 = new LiveNode();
            clone.Operation           = this.Operation;
            clone.DefinitionNode      = this.DefinitionNode.CloneNode();
            clone.DoesNeedToDoAction  = this.DoesNeedToDoAction;
            clone.LinkedCreatedEntity = this.LinkedCreatedEntity;
            
            return clone;
        }
        
        public LiveNode CloneTree()
        {
            var clone = CloneNode();

            foreach (var child in this.Node.Children.OfType<LiveNode>())
            {
                var childClone = child.CloneTree();
                childClone.Node.SetParent(clone);
                childClone.DefinitionNode.Node.SetParent(clone.DefinitionNode);
                childClone.DefinitionNode.TextNode.Node.SetParent(clone.DefinitionNode.TextNode);
            }

            return clone;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Parse methods
        
        
        
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
                var parentLiveNode          = this.Node.Parent;
                var childLiveNode           = this;
                
                // Parent Entity
                var parentEntity            = (Operation.OperationParent as Package).Assets.rootFolder;
                
                // Child Entity
                var  childEntityTypeName    = "Galleon.Checkout." + childLiveNode.TextNode.LineSplits.First();
                Type entityType             = Type.GetType(childEntityTypeName);
                var  childEntity            = (IEntity)Activator.CreateInstance(entityType);
                
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
