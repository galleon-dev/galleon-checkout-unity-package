using UnityEngine.UIElements;

namespace Galleon.Checkout.ExplorerTests
{
    public class Title : ExplorerVisualEntity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI

        private Label label;

        public void RefreshUI(VisualElement parent)
        {
            if (label == null)
            {
                label = new Label();
                parent.Add(label);
            }

            UpdateContent();
        }

        private void UpdateContent()
        {
            if (ExplorerItem?.Target == null)
            {
                label.text = "";
                return;
            }

            if (ExplorerItem.Target is IEntity entity)
            {
                label.text = entity.Node.DisplayName;
            }
            else
            {
                label.text = ExplorerItem.Target.ToString();
            }
        }
    }
}
