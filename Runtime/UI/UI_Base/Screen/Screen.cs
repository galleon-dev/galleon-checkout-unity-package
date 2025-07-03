using System.Threading.Tasks;
using UnityEngine;

namespace Galleon.Checkout.UI
{
    public class Screen : MonoBehaviour, IEntity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// IEntity
        
        public EntityNode Node { get; set; }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public bool IsActive = true;
        
        public View RootView;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public Screen()
        {
            this.Node = new EntityNode(this);
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Flow API
        
        public void OpenScreen()
        {
            this.View().Execute();
        }
        
        public void CloseScreen()
        {
            IsActive = false;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Flow Steps
        
        
        public Step View()
        =>
            new Step(name   : $"view"
                    ,action : async (s) =>
                    {
                        s.AddChildStep(Open());
                        s.AddChildStep(Active());
                        s.AddChildStep(Close());
                    });
        
        public Step Open()
        =>
            new Step(name   : $"open"
                    ,action : async (s) =>
                    {
                        IsActive = true;
                    });
        
        public Step Active()
        =>
            new Step(name   : $"active"
                    ,action : async (s) =>
                    {
                        while (IsActive)
                            await Task.Yield();
                    });
        
        public Step Close()
        =>
            new Step(name   : $"close"
                    ,action : async (s) =>
                    {
                        IsActive = false;
                    });
    }
}
