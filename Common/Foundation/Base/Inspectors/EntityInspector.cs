using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Galleon.Checkout
{
    public class EntityInspector : VisualElement
    {
        public object Target;
        
        public Explorer Explorer
        {
            get
            {
                return Explorer.Instance;
                
                // Search up the visual element hierarchy to find an Explorer
                VisualElement current = this;
                while (current != null)
                {
                    if (current is Explorer e)
                    {
                        return e;
                    }
                    current = current.parent;
                }
                return null;
            }
        }
        
        
        public EntityInspector(object target)
        {
            this.Target = target;
            
            string entityDisplayName = target.GetType().Name;
            
            if (target is IEntity e)
                entityDisplayName = e.Node.DisplayName;
            
            var item  = new ButtonFoldout(); this.Add(item);            
            item.Text = entityDisplayName;

            #region NESTED_INSPECTORS
            ////////////////////////////////////////////////////////////////////////////

            try
            {
                // Check if the target type has a custom Inspector
                var targetType = target.GetType();
                
                // Look for inspector types directly in the target type as nested classes first
                var inspectorTypes = targetType.GetNestedTypes()
                                               .Where(t => t.IsSubclassOf(typeof(Inspector)) 
                                                        && !t.IsAbstract                    
                                                      //&& t.BaseType.IsGenericType         
                                                      //&& t.BaseType.GetGenericArguments()[0] == targetType)
                                                      )
                                               .ToList();
                
                if (inspectorTypes.Any())
                {
                    var inspectorType = inspectorTypes.First();
                    var inspector     = Activator.CreateInstance(inspectorType, target) as Inspector;
                    
                    if (target is IEntity entity)
                    {
                        entity.Node.Inspector = inspector;
                    }
                    
                    if (inspector != null)
                    {
                        inspector.Target = target;
                        
                        VisualElement parentPanel = item;
                        if (Explorer?.Mode == Explorer.DisplayMode.Horizontal)
                        {
                            parentPanel = Explorer.InspectorScrollView;
                            parentPanel.Clear();
                        }
                        
                        
                        parentPanel.Add(inspector);
                    }
                }

            }
            catch (Exception ex)
            {
                Add(new HelpBox($"error with nested inspectors : \n{ex.ToString()}", HelpBoxMessageType.Error));
            }
            
            
            item.Button.clicked += () =>
                                   {
                                       Explorer.SetSelectedEntity(target as IEntity);
                                   };
            
            
            ////////////////////////////////////////////////////////////////////////////
            #endregion // NESTED_INSPECTORS
            
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
