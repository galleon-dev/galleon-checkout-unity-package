using System.Linq;
using UnityEditor;

namespace Galleon.Checkout.Foundation
{
    public class LiveOperationController : Entity
    {
        public void RegisterOperation(LiveOperation op)
        {
            
        }
        
        public Step ResumeAllOperations() 
        =>
            new Step(name   : $"resume_all_operations"
                    ,action : async (s) =>
                    {
                        foreach (var ve in Root.Instance.Node.GetEntitiesInDescendants<VirtualEntity>())
                            if (ve.HasPendingOperation())
                                await ve.ResumeOperation().Execute();
                    });
        
        public Step Resume() 
        => 
            new Step(name   : $"resume"
                    ,action : async (s) =>
                    {
                        if (!this.Node.PrefsStorage.HasKey("OpString"))
                            return;
                        
                        string   OpString = this.Node.PrefsStorage.Load<string>("OpString");
                        TextNode parsed   = TextNode.Parse(OpString);
                        
                        ///////////
                        
                        foreach (var textNode in parsed.Node.Descendants().OfType<TextNode>())
                        {
                            if (textNode.RawText.IsNullOrEmpty()
                            ||  textNode.RawText.ToLower().StartsWith("> origin") )
                                continue;
                            
                            var ve = new VirtualEntity(textNode.RawText);
                            this.Node.AddChild(ve);
                        }
                        
                        ///////////
                        
                        var ves = this.Node.Descendants().OfType<VirtualEntity>();

                        foreach (var ve in ves)
                        {
                            // await ve.DoNextLiveStep();
                        }
                    })
                    {
                        IsSilentLog = true,
                    };
        
        
        #if UNITY_EDITOR
        [InitializeOnLoadMethod]
        #endif
        public static async void InitializeOnLoad()
        {
            await Root.Instance.Context.SystemServices.LiveOperations.Resume().Execute();
        }
     
    }
}