using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Galleon.Checkout
{
    public class ExplorerItem : VisualElement
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public object        Target;
        
        public VisualElement ChildrenHolder;
        public VisualElement LinkedChildrenHolder;
        public VisualElement InspectorHolder;
        
        public ButtonFoldout ButtonFoldout;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties
        
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
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public ExplorerItem(object target)
        {
            // Item Name
            
            this.Target = target;
            
            // General
            InitializeSelf();

            // Nested Inspectors
            InitializeInspector();

            // Populate 
            PopulateChildren();
        }
        
        public void InitializeSelf()
        {
            // Name
            string entityDisplayName = Target.GetType().Name;
            
            if (Target is IEntity e)
                entityDisplayName = e.Node.DisplayName;
            
            // Button Foldout
            this.ButtonFoldout      = new ButtonFoldout(); this.Add(this.ButtonFoldout);            
            this.ButtonFoldout.Text = entityDisplayName;
            
            if (Target is IEntity en)
            {
                en.Node.ExplorerItem = this;
            }
            
            // Child holder
            ChildrenHolder          = new VisualElement(); this.ButtonFoldout.Add(ChildrenHolder);
            
            // Linked-Children holder
            LinkedChildrenHolder    = new VisualElement(); this.ButtonFoldout.Add(LinkedChildrenHolder);
          //LinkedChildrenHolder.style.backgroundColor = ChildrenHolder.resolvedStyle.backgroundColor * 0.9f;
            
            // Inspector holder
            InspectorHolder         = new VisualElement();
            
            // On Button Click
            ButtonFoldout.Button.clicked += () =>
                                            {
                                                Explorer.SetSelectedEntity(Target as IEntity);
                                                this.RefrehsChildren();
                                            };

                        
            // Schedule OnAutoRefresh to be called every second
            this.schedule.Execute(OnAutoRefresh).Every(1000);
        }
        
        public void InitializeInspector()
        {
            try
            {
                // Check if the target type has a custom Inspector
                var targetType = Target.GetType();
                
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
                    // Create Inspector Instance
                    var inspectorType = inspectorTypes.First();
                    var inspector     = Activator.CreateInstance(inspectorType, Target) as Inspector;
                    
                    inspector.ExplorerItem = this;
                    
                    // Assign
                    if (Target is IEntity entity)
                    {
                        entity.Node.Inspector = inspector;
                        this.InspectorHolder.Clear();
                        this.InspectorHolder.Add(inspector);
                    }
                    
                    // Show
                    // if (inspector != null)
                    // {
                    //     inspector.Target = Target;
                    //     
                    //     VisualElement parentPanel = this.ButtonFoldout;
                    //     if (Explorer?.Mode == Explorer.DisplayMode.Horizontal)
                    //     {
                    //         parentPanel = this.InspectorHolder; //Explorer.InspectorScrollView;
                    //         parentPanel.Clear();
                    //     }
                    //     
                    //     parentPanel.Add(inspector);
                    // }
                }
                
            }
            catch (Exception ex)
            {
                Add(new HelpBox($"error with nested inspectors : \n{ex.ToString()}", HelpBoxMessageType.Error));
            }
            
        }
        
        public void PopulateChildren()
        {   
            if (Target is IEntity e)
            {
                // Predefined Child Entities
                foreach (var child in e.Node.Children)
                {
                    var childItem = new ExplorerItem(child);
                    
                    // #if UNITY_EDITOR
                    // string header = child.Node.editorExtras.HeaderAttributeText;
                    // if (header != null)
                    //     this.ButtonFoldout.Add(new Label(header));
                    // #endif
                    
                    this.ChildrenHolder.Add(childItem);
                }
                
                // Linked Children
                foreach (var linkedChild in e.Node.LinkedChildren)
                {
                    if (linkedChild.TryGetTarget(out var target))
                    {
                        var childItem = new ExplorerItem(target);
                      //childItem.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
                        
                        this.LinkedChildrenHolder.Add(childItem);
                    }
                }
            }
            
        }
        
        public void RefrehsChildren()
        {
            if (this.ButtonFoldout.Foldout.value == false)
                return;
            
            this.ChildrenHolder.Clear();
            this.LinkedChildrenHolder.Clear();
            
            PopulateChildren();
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Inspector Events
        
        public void OnRefreshChildren()
        {
            RefrehsChildren();
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Refresh
        
        public void OnAutoRefresh()
        {
            if (this.Target is IEntity e)
            {
                this.ButtonFoldout.Text = e.Node.DisplayName;
                
                e.Node.Inspector?.OnExplorerItemAutoRefresh();
            }            
        }
    }
}

namespace Galleon.Checkout
{
    public class ExplorerItem2 : VisualElement
    {
        // UI
        ButtonFoldout2 ItemElement;
        VisualElement  InspectorsElement;
        VisualElement  ChildrenElement;
        
        // Data
        public object Target;
        
        // Lifecycle
        void Refresh() {}
        
        void RefreshItem()       {}
        void RefreshInspectors() {}
        void RefreshChildren()   {}
    }
    
    public class ButtonFoldout2 : VisualElement
    {
        VisualElement FoldButton;
        VisualElement MainButton;
        VisualElement WidgetsElement;
    }
}


/// > Explorer Window
///     - Create an editor window called EditorExplorer
///    (- Create a runtime ui doc called RuntimeExplorer)
/// > Explorer
///     - Create a VisualElement called Explorer
///       - the explorer will show the entity tree, and will contain the root visual element that will handle its children, etc.
/// > Explorer Modes
///     - Tree
///     - Split Portrait
///     - Split Landscape
/// > Widget
///     > Widget UI
///         - Create a VisualElement class called "Widget"
///         - it acts similar to a foldout, where it has a main item (button) and content that you can toggle, but more complicated then a foldout.
///         - the main item :
///             - Children Foldout Button
///             - Main button )opens content foldout)
///             - Extras
///                 - Indicators
///         - Children foldout
///         - Content foldout
///             - Inspectors
///             - Entity Inspector
///             - Raw Inspector
///     > Widget Code
///         - Data
///         - Refresh
/// > Explorer Item
///     - Create a Class called ExplorerItem


