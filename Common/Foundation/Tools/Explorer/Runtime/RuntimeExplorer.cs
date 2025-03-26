using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Galleon.Checkout
{
    public class RuntimeExplorer : MonoBehaviour
    {
        public  UIDocument  doc;
        private EventSystem primaryEventSystem;

        private void OnEnable()
        {
            try
            {
                // Stop clicks from passing through
                this.primaryEventSystem    = EventSystem.current;
                primaryEventSystem.enabled = false;

                doc.enabled = true;
                
                // Initialize UI
                var root = doc.rootVisualElement;
                root.style.backgroundColor = Color.gray;
                root.Clear();

                // Scroll view
                var scrollView            = new ScrollView(); root.Add(scrollView);
                scrollView.mode           = ScrollViewMode.VerticalAndHorizontal;
                
                // scrollView.style.flexGrow = 1;
                
                // Create Panel
                // var panel                   = new VisualElement(); scrollView.Add(panel);
                // panel.style.flexBasis       = new StyleLength(new Length(100, LengthUnit.Percent));
                // panel.style.flexGrow        = 1;
                // panel.style.flexDirection   = FlexDirection.Column;
                // panel.pickingMode           = PickingMode.Ignore;
              
                // scrollView.Add(new Label(""));
                // scrollView.Add(new Label(""));
                
                try
                {
                    scrollView.Add(new Label("Runtime Explorer"));   
                    scrollView.Add(new Divider());   
                    scrollView.Add(new Explorer());   
                }
                catch (Exception ex) { Debug.LogException(ex); }
                
            
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void OnDisable()
        {
            try
            {
                primaryEventSystem.enabled = true;
                this.doc.rootVisualElement?.Clear();

                doc.enabled = false;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
