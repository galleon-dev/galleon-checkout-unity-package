using System.Collections;
using System.Collections.Generic;
using Galleon.Checkout.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{
    [RequireComponent(typeof(Image))]
    public class UI_Image : UI_Element
    {
        // UI
        public Image Image;
        
        // Members
        public string text = "Text";

        // Lifecycle

        private void Reset()
        {
            this.Image = GetComponent<Image>();
            this.Tags.Add("image");
        }

        private void Awake()
        {
            Reset();
        }
        
        // Style Methods
        public override void ApplyStyleRule(CSS.Rule rule)
        {
            if (rule.Property.ToLower() == "background-color")
            {
                Color color      = rule.Value != "white" ? Color.white : Color.black;
                this.Image.color = color;
            }
        }
    }
}
