using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{
    [RequireComponent(typeof(Button))]
    public class UI_Button : UI_Element
    {
        // UI
        public Button Button;
        
        // Members
        public string text = "Button";

        // Lifecycle

        private void Reset()
        {
            this.Button = GetComponent<Button>();
            this.Tags.Add("button");
        }

        private void Awake()
        {
            Reset();
        }
    }
}
