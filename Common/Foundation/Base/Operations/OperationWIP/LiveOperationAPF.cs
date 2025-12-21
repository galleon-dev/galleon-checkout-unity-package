using System;
using System.Linq;
using System.Threading.Tasks;

namespace Galleon.Checkout.Foundation.LiveOperationAPF
{
    public class LiveOperation : Entity
    {   
        ///////// Members
        
        public string       ID;
        public IEntity      Parent;
        
        public APF_LiveNode OriginalTree;
        
        ///////// Lifecycle
        
        public LiveOperation(string id, IEntity parent, APF_LiveNode definition)
        {
            this.ID                     = id;
            this.Parent                 = parent;
            this.OriginalTree           = definition;
            this.OriginalTree.Operation = this;
        }
        
        ///////// Main API
        
        public Step Flow() 
        =>
            new Step(name   : $"execute_APF"
                    ,action : async (s) =>
                    {
                        OriginalTree.DO_APF();
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
    
    public class APF_LiveNode : Entity
    {
        //////////////// Members
        
        public LiveOperation Operation;
        public string            TargetText;
        public string            ActionText;
        
        //////////////// Lifecycle
       
        public APF_LiveNode()
        {
        }
        
        public APF_LiveNode(string targetText, string actionText)
        {
            this.TargetText = targetText;
            this.ActionText = actionText;
        }
        
        //////////////// Main Methods
        
        public void DO_APF()
        {
            IEntity Parent     = this.Operation.Parent;
            
            string  type       = this.TargetText.Split(' ').First(); // "Folder"
            Type    targetType = Type.GetType("Galleon.Checkout." + type);
            
            IEntity target     = (IEntity)Activator.CreateInstance(targetType);
            
            Parent.Node.AddChild(target);
            target.Node.Live.LiveComponent.OnAddedToParent(Parent);
            target.Node.Live.LiveComponent.Create();            
        }
    }
}