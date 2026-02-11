using System.Linq;
using UnityEngine;

namespace Galleon.Checkout.Foundation
{
    public class LiveOperation : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public string  ID;
        public string  OpString;
        public string  TargetEntityID;
        public IEntity TargetEntity;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public LiveOperation(string ID, IEntity targetEntity, string OpString)
        {
            this.ID             = ID;
            this.OpString       = OpString;
            this.TargetEntity   = targetEntity;
            this.TargetEntityID = targetEntity.Node.ID.PathID;
        }
        
        public LiveOperation(string ID, string TargetEntityID)
        {
            this.ID             = ID;
            this.TargetEntityID = TargetEntityID;
            this.TargetEntity   = Root.Instance.Node.Descendants().FirstOrDefault(x => x.Node.ID.PathID == TargetEntityID) as IEntity;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Steps
        
        public Step Flow() 
        =>
            new Step(name   : $"Flow"
                    ,action : async (s) =>
                    {
                        // Store operation
                        TargetEntity.Node.PrefsStorage.Store("OpString", OpString);

                        // Parse 
                        TextNode parsed = TextNode.Parse(OpString);

                        // Create VirtualEntity children
                        foreach (var textNode in parsed.Node.Descendants().OfType<TextNode>())
                        {
                            if (textNode.RawText.IsNullOrEmpty()
                            ||  textNode.RawText.ToLower().StartsWith("> origin") )
                                continue;

                            var ve = new VirtualEntity() {TextNode = textNode};
                            TargetEntity.Node.Live.AddVirtualEntity(ve);
                        }

                        // Get created VirtualEntities
                        var ves = TargetEntity.Node.Descendants().OfType<VirtualEntity>();

                        foreach (var ve in ves)
                        {
                            Debug.Log(ve.TextNode?.RawText ?? "> NULL");
                        }

                        
                        return;
                        foreach (var ve in ves)
                        {
                            // set state
                            ve.State = "assets";
                            await ve.SaveState();
                            
                            // do next op step
                            await ve.DoNextLiveStep();
                        }
                        
                    });
    }
}
