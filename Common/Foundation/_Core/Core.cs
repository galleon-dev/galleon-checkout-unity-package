using System;
using System.Linq;
using UnityEngine.UIElements;

namespace Galleon.Checkout.Foundation
{
    public class Core : Entity
    {
        public void Plus(IEntity entity)
        {
            if (entity is Folder f)
            {
                var package = this.Node.Parent as Package;
                var Assets  = package.Assets;
                Assets.Node.Live.Plus(f);
            }
            else
            {
                var definition     = entity.Node.Element.GetDefinition();
                var treeDefinition = ""; // var tree = definition.treeDefinition;
                var tree           = this.Node.Descendants().First(x => x.GetType().Name == treeDefinition);
                tree.Node.Live.Plus(entity);
            }
        }
        
        //////////////////////////////////////////////////////////////////////////////////// TEMP
        
        public Step Do_Core_Plus_Folder() 
        =>
            new Step(action : async (s) =>
                    {
                        var Package = this.Node.Parent as Package;
                        var assets = Package.Assets;
                        assets.Do_Assets_Plus_Folder();
                    });
        
        //////////////////////////////////////////////////////////////////////////////////// Inspector
        public class Inspector : Inspector<Core>
        {
            public Inspector(Core target) : base(target)
            {
                Button btn_CorePlusFolder   = new Button(); this.Add(btn_CorePlusFolder);
                btn_CorePlusFolder.clicked += () => target.Do_Core_Plus_Folder().Execute(); 
                btn_CorePlusFolder.text     = "Assets + Folder";
            }
        }
    }
}

