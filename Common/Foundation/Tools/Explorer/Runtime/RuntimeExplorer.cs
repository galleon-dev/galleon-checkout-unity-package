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
                scrollView.style.flexGrow = 1;
                
                try
                {
                    scrollView.Add(new Label("Runtime Explorer"));   
                    scrollView.Add(new Divider());   
                    
                    var explorer = new Explorer();
                    explorer.Mode = Explorer.DisplayMode.Mobile;
                    explorer.RefreshMode();
                    scrollView.Add(explorer);   
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
