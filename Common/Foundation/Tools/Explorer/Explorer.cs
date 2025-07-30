using UnityEngine;
using UnityEngine.UIElements;

namespace Galleon.Checkout
{
    public class Explorer : VisualElement
    {
        //////////////////////////////////////////////////////////////////////// Instance
        
        public static Explorer Instance { get; private set; }
        
        //////////////////////////////////////////////////////////////////////// Members
        
        public IEntity RootObject     { get; set; } = Root.Instance;
        public IEntity SelectedEntity { get; set; } = Root.Instance;
        
        //////////////////////////////////////////////////////////////////////// UI
        
        public TwoPaneSplitView splitView;
        public VisualElement    TreePanel;
        public ScrollView       TreeScrollView;
        
        public VisualElement    InspectorPanel;
        public ScrollView       InspectorScrollView;
        
        
        public      DisplayMode Mode = DisplayMode.Horizontal;
        public enum DisplayMode
        {
            Tree, 
            Mobile, 
            Horizontal,
        }
        
        //////////////////////////////////////////////////////////////////////// Lifecycle
        
        public Explorer()
        {            
            Instance = this;
            
            // Make the root element fill its container
            this.style.flexGrow                     = 1;
            this.style.height                       = Length.Percent(100);
            
            splitView                               = new TwoPaneSplitView(0 ,300 ,TwoPaneSplitViewOrientation.Horizontal); this.Add(splitView);
            
            TreePanel                               = new VisualElement(); this.splitView.Add(TreePanel);
            
            TreeScrollView                          = new ScrollView(); this.TreePanel.Add(TreeScrollView);          
            TreeScrollView.mode                     = ScrollViewMode.VerticalAndHorizontal;
            
            InspectorPanel                          = new VisualElement(); this.splitView.Add(InspectorPanel);
            
            InspectorScrollView                     = new ScrollView(); this.InspectorPanel.Add(InspectorScrollView);
            InspectorScrollView.mode                = ScrollViewMode.VerticalAndHorizontal;
            
            // Tree View 
            ExplorerItem root = new ExplorerItem(RootObject); 
            TreeScrollView.Add(root);
            
            // Inspector View
            SetSelectedInspectorUI(SelectedEntity.Node.Inspector);
            
            RefreshMode();
        }
        
        //////////////////////////////////////////////////////////////////////// Refresh Methods
        
        public void RefreshMode()
        {
            if (Mode == DisplayMode.Horizontal)
            {
                splitView.orientation = TwoPaneSplitViewOrientation.Horizontal;
            }
            else if (Mode == DisplayMode.Mobile)
            {
                splitView.orientation = TwoPaneSplitViewOrientation.Vertical;
            }
        }
        
        //////////////////////////////////////////////////////////////////////// Methods
        
        public void SetSelectedEntity(IEntity entity)
        {
            if (this.SelectedEntity.Node.Inspector != null)
                this.SelectedEntity.Node.Inspector.IsSelected = false;
            
            if (entity.Node.Inspector != null)
                entity.Node.Inspector.IsSelected = true;
            
            this.SelectedEntity = entity;
            
            SetSelectedInspectorUI(SelectedEntity.Node.Inspector);
        }
        
        public void SetSelectedInspectorUI(VisualElement inspectorUI)
        {
            if (inspectorUI == null)
            {
                this.InspectorScrollView.Clear();
                this.InspectorScrollView.Add(new Label($"Selected : {SelectedEntity}"));
            }
            else
            {
                this.InspectorScrollView.Clear();
                this.InspectorScrollView.Add(inspectorUI);
            }
            
            
            var genericInspector = new GenericInspector(target:SelectedEntity);
            
            this.InspectorScrollView.Add(new EntityNode.EntityInspector(SelectedEntity));
            this.InspectorScrollView.Add(genericInspector);
            
        }
    }
}

