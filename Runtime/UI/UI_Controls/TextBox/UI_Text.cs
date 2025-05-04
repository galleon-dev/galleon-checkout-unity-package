using TMPro;
using UnityEngine;

namespace Galleon.Checkout.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UI_Text : UI_Element
    {
        // UI
        public TextMeshProUGUI Text;
        
        // Members
        public string text = "Text";

        // Lifecycle

        private void Reset()
        {
            this.Text = GetComponent<TextMeshProUGUI>();
            this.Tags.Add("text");
        }

        private void Awake()
        {
            Reset();
        }
        
        
        // Style Methods
        public override void ApplyStyleRule(CSS.Rule rule)
        {
            if (rule.Property.ToLower() == "color")
            {
                Color color     = rule.Value != "white" ? Color.white : Color.black;
                this.Text.color = color;
            }
        }
    }
}
