using UnityEngine.UIElements;

namespace ExplorerTests
{
    /// <summary>
    /// Explorer is a development tool to browse and inspect entities in the project.
    /// It supports multiple display modes for viewing entity hierarchies.
    /// </summary>
    public class Explorer : VisualElement, Galleon.Checkout.IEntity
    {
        //////////////////////////////////////////////////////////////////////// IEntity Implementation

        private Galleon.Checkout.EntityNode _entityNode;
        public  Galleon.Checkout.EntityNode Node => _entityNode ??= new Galleon.Checkout.EntityNode(this);

        //////////////////////////////////////////////////////////////////////// Display Mode

        public ExplorerMode Mode { get; set; } = ExplorerMode.Vertical;

        //////////////////////////////////////////////////////////////////////// UI Panels

        public VisualElement ContentPanel  { get; private set; }
        public VisualElement ChildrenPanel { get; private set; }

        //////////////////////////////////////////////////////////////////////// Lifecycle

        public Explorer()
        {
            InitializePanels();
            RefreshLayout();
        }

        //////////////////////////////////////////////////////////////////////// Initialization

        private void InitializePanels()
        {
            // Make the root element fill its container
            this.style.flexGrow = 1;
            this.style.height   = Length.Percent(100);

            // Initialize panels
            ContentPanel      = new VisualElement();
            ContentPanel.name = "ContentPanel";

            ChildrenPanel      = new VisualElement();
            ChildrenPanel.name = "ChildrenPanel";
        }

        //////////////////////////////////////////////////////////////////////// Layout Management

        public void RefreshLayout()
        {
            // Clear existing children
            this.Clear();

            switch (Mode)
            {
                case ExplorerMode.Vertical:
                    SetupVerticalLayout();
                    break;

                case ExplorerMode.Split:
                    SetupSplitLayout();
                    break;
            }
        }

        private void SetupVerticalLayout()
        {
            // In vertical mode, all items are displayed in a single vertical tree
            // The children panel contains the tree structure
            ChildrenPanel.style.flexGrow      = 1;
            ChildrenPanel.style.flexDirection = FlexDirection.Column;

            this.Add(ChildrenPanel);
        }

        private void SetupSplitLayout()
        {
            // In split mode, create a two-pane view:
            // - Left pane: tree of entities (ChildrenPanel)
            // - Right pane: inspector for selected entity (ContentPanel)

            var splitView = new TwoPaneSplitView(0, 300, TwoPaneSplitViewOrientation.Horizontal);

            // Configure children panel for tree view
            ChildrenPanel.style.flexGrow      = 1;
            ChildrenPanel.style.flexDirection = FlexDirection.Column;

            // Configure content panel for inspector view
            ContentPanel.style.flexGrow = 1;

            splitView.Add(ChildrenPanel);
            splitView.Add(ContentPanel);

            this.Add(splitView);
        }

        //////////////////////////////////////////////////////////////////////// Mode Management

        public void SetMode(ExplorerMode mode)
        {
            if (Mode != mode)
            {
                Mode = mode;
                RefreshLayout();
            }
        }
    }

    //////////////////////////////////////////////////////////////////////// Explorer Mode Enum

    public enum ExplorerMode
    {
        /// <summary>
        /// All items are displayed in a vertical tree (like Unity hierarchy window)
        /// </summary>
        Vertical,

        /// <summary>
        /// View is split into two panes: tree on the left, inspector on the right
        /// </summary>
        Split
    }
}

