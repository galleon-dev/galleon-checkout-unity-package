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
            
            splitView                               = new TwoPaneSplitView(0, 300, TwoPaneSplitViewOrientation.Horizontal); this.Add(splitView);
            splitView.style.flexGrow                = 1;
            splitView.style.height                  = Length.Percent(100);
            
            
            TreePanel                               = new VisualElement(); splitView.Add(TreePanel);
            TreePanel.style.flexGrow                = 1;
            TreePanel.style.height                  = Length.Percent(100);
            
            
            TreeScrollView                          = new ScrollView(); TreePanel.Add(TreeScrollView);          
            TreeScrollView.style.flexGrow           = 1;
            TreeScrollView.style.height             = Length.Percent(100);
            
            
            InspectorPanel                          = new VisualElement(); splitView.Add(InspectorPanel);
            InspectorPanel.style.flexGrow           = 1;
            InspectorPanel.style.height             = Length.Percent(100);
            
            
            InspectorScrollView                     = new ScrollView(); InspectorPanel.Add(InspectorScrollView);
            InspectorScrollView.style.flexGrow      = 1;
            InspectorScrollView.style.height        = Length.Percent(100);
            
            
            // Left 
            ExplorerItem root = new ExplorerItem(RootObject); 
            TreeScrollView.Add(root);
            
            // Right
            SetSelectedInspectorUI(SelectedEntity.Node.Inspector);
            
            RefreshMode();
        }
        
        //////////////////////////////////////////////////////////////////////// Refresh Methods
        
        public void RefreshMode()
        {
            if (Mode == DisplayMode.Horizontal)
            {
                splitView.orientation = TwoPaneSplitViewOrientation.Horizontal;
                //splitView.fixedPaneInitialDimension = 300;
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
                return;
            }
            
            this.InspectorScrollView.Clear();
            this.InspectorScrollView.Add(inspectorUI);
        }
    }
}
