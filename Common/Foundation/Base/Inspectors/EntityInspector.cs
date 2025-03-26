using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Galleon.Checkout
{
    public class EntityInspector : VisualElement
    {
        public object Target;
        
        public EntityInspector(object target)
        {
            this.Target = target;
            
            string entityDisplayName = target.GetType().Name;
            
            if (target is IEntity e)
                entityDisplayName = e.Node.DisplayName;
            
            var item  = new ButtonFoldout(); this.Add(item);            
            item.Text = entityDisplayName;

            // Predefined Child Entities
            if (target is IEntity e2)
            {
                foreach (var child in e2.Node.Children)
                {
                    var childItem = new EntityInspector(child);
                    
                    #if UNITY_EDITOR
                    string header = child.Node.editorExtras.HeaderAttributeText;
                    if (header != null)
                        item.Add(new Label(header));
                    #endif
                    
                    item.Add(childItem);
                }
            }
        }
    }
}
