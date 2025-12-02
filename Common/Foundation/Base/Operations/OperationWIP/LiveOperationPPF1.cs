using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout.Foundation;
using Galleon.Checkout.Foundation.LiveOperationPPF1;

namespace Galleon.Checkout.Foundation.LiveOperationPPF1
{
    public class PPF1_LiveOperation : Entity
    {   
        ///////// Members
        
        public string           ID;
        public IEntity          Parent;
        
        public PPF1_LiveNode    OriginalTree;
        public PPF1_LiveNode    ChildVirtualTree;
        public PPF1_LiveNode    ParentFullVirtualTree;
        
        
        ///////// Lifecycle
        
        public PPF1_LiveOperation(string id, IEntity parent, string definition)
        {
            this.ID                     = id;
            this.Parent                 = parent;
            this.OriginalTree           = new ()
                                        {
                                            TextNode = new TextNode(definition)
                                        };
            this.OriginalTree.Operation = this;
            this.ChildVirtualTree       = OriginalTree.CloneTree();
        }
        
        
        ///////// Main API
        
        public Step Flow() 
        =>
            new Step(name   : $"execute_PPF1"
                    ,action : async (s) =>
                    {
                        ChildVirtualTree.DO_PPF1_CreateChildVTree();

                        foreach (var vNode in ChildVirtualTree.Node.Descendants().OfType<PPF1_LiveNode>())
                        {
                            if (vNode.DoesNeedToDoAction)
                                vNode.DO_PPF1_PLusAction_Per_Node();
                        }
                    });
        
    }
    
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    
    public class PPF1_LiveNode : Entity
    {
        //////////////// Members
        
        public PPF1_LiveOperation   Operation { get; set; }
        public TextNode             TextNode = new TextNode();
        
        public bool                 DoesNeedToDoAction => Node.Parent is PPF1_LiveNode;
        
        public IEntity              LinkedCreatedEntity;
        
        //////////////// Lifecycle
       
        public PPF1_LiveNode()
        {
        }
        
        //////////////// Main Methods
        
        public void DO_PPFE1_MANUALLY_CREATE_TEST_VTREE()
        {
            
        }
        
        public void DO_PPFE1_CreateFullVTree()
        {
            // first clone parent tree
            this.Operation.ParentFullVirtualTree = this.PPF1_CloneActualTree(this.Operation.Parent);
            
            // Add Child V Tree
            this.Operation.ChildVirtualTree.Node.Descendants().OfType<LiveNode>().ToList().ForEach(x => x.ActuallyExists = false);
            this.Operation.ParentFullVirtualTree.PPF1_AddChildVTree(this.Operation.ChildVirtualTree.PPF1_CloneLiveTree());
        }
        
        public void DO_PPF1_CreateChildVTree()
        {
            ////////////////////////////////////////////////////////////////////////
            /// Folder Element Definition:
            /// > Folder
            ///     > (Assets)
            ///         > Assets.Folder
            ////////////////////////////////////////////////////////////////////////
            /// Package Element Definition:
            /// > Package
            ///     > (Assets)
            ///         > Assets.Folder "package_root_folder"
            ///     > (Hierarchy)
            ///         > H.Scene "package_main_scene"
            ///     > (whatever)
            ///         > Whatever "..."
            ////////////////////////////////////////////////////////////////////////
            /// Expected v tree result :
            /// > (this node) Elements.Folder "f1"
            ///     > (Assets)
            ///         > Assets.Folder "f1"
            ////////////////////////////////////////////////////////////////////////
            /// And then :
            /// > (this node) Elements.Folder "f1"
            ///     > (Assets)
            ///         > Assets.Folder "f1" <---- this needs to get printed under package root asset
            ////////////////////////////////////////////////////////////////////////
            /// need :
            ///     -> print parent per category
            ///////////////////////////////////////////////////////////////////////
            
            // Definitions
            string                  targetElementName             = this.TextNode.LineWords.First();
            
            // Get Parent Element
            Element                 parentElement                 = this.Operation.Parent.Node.GetElement();
            
            // Get Target Element
            Element                 targetElement                 = Elements.GetElementByName(targetElementName);
            
            // Get relevant namespaces
            var                     parentElementNamespaceNodes   = parentElement.Node.Descendants().OfType<DefinitionNode>().Where(x => x.IsNamespace); // get all namespace nodes under parent
            var                     targetElementNamespaceNodes   = targetElement.Node.Descendants().OfType<DefinitionNode>().Where(x => x.IsNamespace); // get all namespace nodes under target element
            var                     intersectionNodes             = parentElementNamespaceNodes.Intersect(targetElementNamespaceNodes);                  // get the intersection of the two
            
            // copy relevent nodes to vtree
            foreach (var node in intersectionNodes)
            {
                
            }
            
            // Add Folder Asset Live Node To VTree
            PPF1_LiveNode           folderAssetLiveNode           = targetElement.PPFE1_Get_AssetFolderNode();
            folderAssetLiveNode.Operation                         = this.Operation;
            
            // Add Assets.Folder node to this
            this.Node.AddChild(folderAssetLiveNode);
        }
        
        public void DO_PPF1_PLusAction_Per_Node()
        {
            //////// Validation
            
            if (!this.DoesNeedToDoAction) 
                return;
            
            //////// Definitions
            
            string  type       = this.TextNode.LineContent.Split(' ').First(); // "Folder"
            Type    childType  = Type.GetType("Galleon.Checkout." + type);
            IEntity child      = (IEntity)Activator.CreateInstance(childType);
            IEntity Parent     = this.Node.Parent;
          //IEntity Parent     = this.Operation.Parent; var p = this.Node.Ancestors().OfType<PPF1_LiveNode>().FirstOrDefault(n => n.LinkedCreatedEntity != null);
        
            //////// Actual Action
            
            Parent.Node.AddChild(child);
            child.Node.Live.LiveHandler.OnAddedToParent(Parent);
            child.Node.Live.LiveHandler.Create();
            LinkedCreatedEntity = child;
        }
        
        //////////////// Helper Methods
        
        public PPF1_LiveNode CloneNode()
        {
            var clone        = new PPF1_LiveNode();
            clone.TextNode   = TextNode.Clone();
            clone.Operation  = Operation;
            return clone;
        }
        
        public PPF1_LiveNode CloneTree()
        {
            var nodeMap   = new Dictionary<PPF1_LiveNode, PPF1_LiveNode>();
            var clone     = this.CloneNode();
            nodeMap[this] = clone;

            foreach (var descendant in this.Node.Descendants().OfType<PPF1_LiveNode>())
            {
                var descendantClone = descendant.CloneNode();
                nodeMap[descendant] = descendantClone;

                var parent = descendant.Node.Parent as PPF1_LiveNode;
                if (parent != null && nodeMap.ContainsKey(parent))
                {
                    nodeMap[parent].Node.AddChild(descendantClone);
                }
            }

            return clone;
        }
        
        //////////////////////////
        
        public PPF1_LiveNode PPF1_CloneActualTree(IEntity origin)
        {
            return new PPF1_LiveNode();
        }
        
        public PPF1_LiveNode PPF1_CloneLiveTree()
        {
            return new PPF1_LiveNode();
        }
        
        public void PPF1_AddChildVTree(IEntity childVTreeRoot)
        {
            
        }

    }
}

    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///

namespace Galleon.Checkout
{
    public partial class Element
    {
        public PPF1_LiveNode PPFE1_Get_AssetFolderNode() =>  new PPF1_LiveNode()
        {
            TextNode = new TextNode("> Assets.Folder f1") 
        };
    }
}

