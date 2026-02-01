using Galleon.Checkout;
using UnityEngine.UIElements;

namespace ExplorerTests
{
    /// <summary>
    /// ExplorerItem represents a single entity item in the Explorer tree view.
    /// </summary>
    public class ExplorerItem : VisualElement, IEntity
    {
        //////////////////////////////////////////////////////////////////////// Members

        public EntityNode Node { get; set; }
        
        public Galleon.Checkout.IEntity Entity { get; private set; }

        public VisualElement ChildrenContainer { get; private set; }
        public VisualElement ContentContainer  { get; private set; }

        private Button  itemButton;
        private Foldout itemFoldout;

        //////////////////////////////////////////////////////////////////////// Lifecycle

        public ExplorerItem(Galleon.Checkout.IEntity entity)
        {
            this.Node = new EntityNode(this);
            
            Entity = entity;

            InitializeUI();
            PopulateChildren();
        }

        //////////////////////////////////////////////////////////////////////// Initialization

        private void InitializeUI()
        {
            // Create foldout for this item
            itemFoldout       = new Foldout();
            itemFoldout.text  = Entity?.Node.DisplayName ?? "Unknown Entity";
            itemFoldout.value = false; // Collapsed by default

            // Create content container
            ContentContainer      = new VisualElement();
            ContentContainer.name = "ContentContainer";

            // Create children container
            ChildrenContainer                   = new VisualElement();
            ChildrenContainer.name              = "ChildrenContainer";
            ChildrenContainer.style.paddingLeft = 20; // Indent children

            // Add to foldout
            itemFoldout.Add(ContentContainer);
            itemFoldout.Add(ChildrenContainer);

            // Add foldout to this element
            this.Add(itemFoldout);

            // Register foldout change event
            itemFoldout.RegisterValueChangedCallback(evt =>
                                                     {
                                                         if (evt.newValue)
                                                         {
                                                             OnExpanded();
                                                         }
                                                         else
                                                         {
                                                             OnCollapsed();
                                                         }
                                                     });
        }

        //////////////////////////////////////////////////////////////////////// Children Management

        private void PopulateChildren()
        {
            if (Entity == null)
                return;

            ChildrenContainer.Clear();

            // Populate child entities
            foreach (var child in Entity.Node.Children)
            {
                var childItem = new ExplorerItem(child);
                ChildrenContainer.Add(childItem);
            }
        }

        public void RefreshChildren()
        {
            PopulateChildren();
        }

        //////////////////////////////////////////////////////////////////////// Events

        private void OnExpanded()
        {
            // Refresh children when expanded
            RefreshChildren();
        }

        private void OnCollapsed()
        {
            // Optional: could clear children to save memory
        }

        //////////////////////////////////////////////////////////////////////// Helper Methods

        public void SetSelected(bool selected)
        {
            if (selected)
            {
                itemFoldout.style.backgroundColor = new StyleColor(new UnityEngine.Color(0.3f, 0.5f, 0.7f, 0.3f));
            }
            else
            {
                itemFoldout.style.backgroundColor = StyleKeyword.Null;
            }
        }

    }
}