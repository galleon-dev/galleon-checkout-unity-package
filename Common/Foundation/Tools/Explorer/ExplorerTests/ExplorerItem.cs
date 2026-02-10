using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Galleon.Checkout.ExplorerTests
{
    public class ExplorerItem : ExplorerVisualEntity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members

        public object Target { get; set; }

        public Title                Title           { get; private set; }
        public List<Indicator>      Indicators      { get; private set; } = new List<Indicator>();
        public List<FoldoutArrow>   FoldoutArrows   { get; private set; } = new List<FoldoutArrow>();
        public List<Panel>          Panels          { get; private set; } = new List<Panel>();

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI

        private VisualElement visualElement;

        public void BuildUI(VisualElement parent)
        {
            if (visualElement == null)
            {
                visualElement = new VisualElement();
                parent.Add(visualElement);
            }

            RefreshUI();
        }

        public void RefreshUI()
        {
            if (Title == null)
            {
                Title = new Title { ExplorerItem = this };
            }

            Title.RefreshUI(visualElement);
        }
    }
}
