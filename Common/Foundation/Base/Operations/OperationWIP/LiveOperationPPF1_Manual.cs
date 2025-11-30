using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout.Foundation;
using Galleon.Checkout.Foundation.LiveOperationPPF1M;

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
                    ,action : async (s) =>
                    {
                        await ManuallyCreateFullVirtualTree().Execute();
        
                        foreach (var vNode in FullVirtualTree.Node.Descendants().OfType<PPF1M_LiveNode>())
                        {
                            if (vNode.DoesNeedToDoAction)
                                vNode.DoAction();
                        }
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Main Steps
        
        public Step ManuallyCreateFullVirtualTree() 
        =>
            new Step(action : async (s) =>
            {
                this.FullVirtualTree = new PPF1M_LiveNode()        { TextNode = new TextNode("root")};                                                              // Root
                    var pf              = new PPF1M_LiveNode()         { TextNode = new TextNode("> Assets.Folder package1") }; FullVirtualTree.Node.AddChild(pf);      // package_1
                        var f1              = new PPF1M_LiveNode()         { TextNode = new TextNode("> Assets.Folder package1") }; pf.Node.AddChild(f1);                   // f1 
                
                
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
    
    public class PPF1M_LiveNode : Entity
    {
        ////////////////// Members
        
        public PPF1M_LiveOperation   Operation               { get; set; }
        
        public TextNode              TextNode                = new TextNode();
        public bool                  DoesNeedToDoAction      = false;
        public IEntity               LinkedCreatedEntity;

        ////////////////// Properties
        
        public string action_type => TextNode == null ? "plus" : "plus";
        
        ////////////////// Main Actions
        
        public void DoAction()
        {
            switch (action_type)
            {
                case "plus" : DoPlus(); return;
                default     :           return;
            }
        }
        
        public void DoPlus()
        {
            var parent = this.Node.Parent;
            var child  = this;
            
            parent.Node.AddChild(child);
            child.Node.Live.LiveHandler.OnAddedToParent(parent);
            child.Node.Live.LiveHandler.Create();
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
        public PPF1M_LiveNode PPFE1M_Get_AssetFolderNode() =>  new PPF1M_LiveNode()
        {
            TextNode = new TextNode("> Assets.Folder f1") 
        };
    }
}

