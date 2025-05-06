using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace Galleon.Checkout
{
    public class ButtonFoldout : VisualElement
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public Foldout       Foldout;
        public Toggle        Toggle;
        public Button        Button;
        public VisualElement Content;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties
        
        public string Text
        {
            get => Button.text;
            set => Button.text = value;
        }
        
        public bool Expanded
        {
            get => Foldout.value;
            set => Foldout.value = value;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public ButtonFoldout()
        {
            // Main Foldout 
            Foldout                     = new Foldout(); this.contentContainer.Add(Foldout);
            Foldout.value               = false; // Close by default;
            {
                // inner elements
                this.Toggle                     = Foldout.Q<Toggle>();
                var     defaultElement          = Toggle.Children().FirstOrDefault();
                defaultElement.style.flexGrow   = 0;
                
                // Main Button
                Button                      = new Button(); Toggle.Add(Button);
                Button.text                 = $"...";
                Button.style.height         = 20;
                Button.style.flexGrow       = 1;
                Button.style.flexDirection  = FlexDirection.RowReverse;
                Button.style.unityTextAlign = TextAnchor.MiddleLeft;
                Button.style.justifyContent = Justify.FlexStart;
                Button.style.paddingLeft    = 10;
                Button.clicked             += MainFoldoutButtonOnclicked; void MainFoldoutButtonOnclicked()
                                              {
                                                  Foldout.value = !Foldout.value;
                                              }
                
                // Content
                Content = new VisualElement(); Foldout.Add(Content);
            }
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Methods
        
        public new void Add (VisualElement element)
        {
            this.Content.Add(element);
        }
    }
}
