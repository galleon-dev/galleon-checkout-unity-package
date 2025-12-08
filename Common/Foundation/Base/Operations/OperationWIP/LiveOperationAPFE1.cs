using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout.Foundation;
using Galleon.Checkout.Foundation.LiveOperationAPFE1;

namespace Galleon.Checkout.Foundation.LiveOperationAPFE1
{
    public class APFE1_LiveOperation : Entity
    {   
        ///////// Members
        
        public string           ID;
        public IEntity          Parent;
        
        public APFE1_LiveNode   OriginalTree;
        public APFE1_LiveNode   ChildVirtualTree;
        
        
        ///////// Lifecycle
        
        public APFE1_LiveOperation(string id, IEntity parent, string definition)
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
            new Step(name   : $"execute_APFE1"
                    ,action : async (s) =>
                    {
                        ChildVirtualTree.DO_APFE1_CreateVTree();

                        foreach (var vNode in ChildVirtualTree.Node.Descendants().OfType<APFE1_LiveNode>())
                        {
                            if (vNode.DoesNeedToDoAction)
                                vNode.DO_APFE1_PLusAction_Per_Node();
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
    
    public class APFE1_LiveNode : Entity
    {
        //////////////// Members
        
        public APFE1_LiveOperation  Operation { get; set; }
        public TextNode             TextNode = new TextNode();
        
        public bool                 DoesNeedToDoAction => Node.Parent is APFE1_LiveNode;
        
        public IEntity              LinkedCreatedEntity;
        
        //////////////// Lifecycle
       
        public APFE1_LiveNode()
        {
        }
        
        //////////////// Main Methods
        
        /// <summary>
        /// This is APFE1
        /// this (root) node is : ["> Folder f1"]
        /// Operation.Parent is : Root.Instance.Context.Package1.Assets.rootFolder
        ///
        /// This method creates the following vtree under this (root) node :
        ///     > (this node) Elements.Folder "f1"
        ///         > Assets.Folder "f1"
        /// </summary>
        public void DO_APFE1_CreateVTree()
        {
            // Definitions
            string               targetElementName   = this.TextNode.LineSplits.First();
            
            // Get Target Element
            IEnumerable<Element> AllElements        = Elements.GetAllElements();
            Element              targetElement      = AllElements.Single(x => x.Name == targetElementName);
            
            // Add Folder Asset Live Node To VTree
            APFE1_LiveNode       folderAssetLiveNode = targetElement.APFE1_Get_AssetFolderNode();
            folderAssetLiveNode.Operation            = this.Operation;
            
            // Add Assets.Folder node to this
            this.Node.AddChild(folderAssetLiveNode);
        }
        
        public void DO_APFE1_PLusAction_Per_Node()
        {
            if (!this.DoesNeedToDoAction) 
                return;
            
            IEntity Parent     = this.Operation.Parent; var p = this.Node.Ancestors().OfType<APFE1_LiveNode>().FirstOrDefault(n => n.LinkedCreatedEntity != null);
            string  type       = this.TextNode.LineContent.Split(' ').First(); // "Folder"
            Type    targetType = Type.GetType("Galleon.Checkout." + type);
            IEntity target     = (IEntity)Activator.CreateInstance(targetType);
            
            Parent.Node.AddChild(target);
            target.Node.Live.LiveHandler.OnAddedToParent(Parent);
            target.Node.Live.LiveHandler.Create();
            
            LinkedCreatedEntity = target;
        }
        
        //////////////// Helper Methods
        
        public APFE1_LiveNode CloneNode()
        {
            var clone        = new APFE1_LiveNode();
            clone.TextNode   = TextNode.CloneNode();
            clone.Operation  = Operation;
            return clone;
        }
        
        public APFE1_LiveNode CloneTree()
        {
            var nodeMap   = new Dictionary<APFE1_LiveNode, APFE1_LiveNode>();
            var clone     = this.CloneNode();
            nodeMap[this] = clone;

            foreach (var descendant in this.Node.Descendants().OfType<APFE1_LiveNode>())
            {
                var descendantClone = descendant.CloneNode();
                nodeMap[descendant] = descendantClone;

                var parent = descendant.Node.Parent as APFE1_LiveNode;
                if (parent != null && nodeMap.ContainsKey(parent))
                {
                    nodeMap[parent].Node.AddChild(descendantClone);
                }
            }

            return clone;
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
        public APFE1_LiveNode  APFE1_Get_AssetFolderNode() =>  new APFE1_LiveNode()
        {
            TextNode = new TextNode( "> Assets.Folder f1" ) 
        };
    }
}

