using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif 

namespace Galleon.Checkout.UI
{
    public class CheckboxButton : MonoBehaviour
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        // Fields
        private bool _isChecked = false;
        
        
        public bool IsChecked
        {
            get 
            {
                return _isChecked;    
            }
            set
            {
                this.CheckedMark  .SetActive( value);
                this.UncheckedMark.SetActive(!value);
                _isChecked = value;
            } 
        }
        
        // UI
        public GameObject CheckedMark;
        public GameObject UncheckedMark;
        public TMP_Text   Text;  
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Events
        
        public void On_Click()
        {
            this.IsChecked = !this.IsChecked;
        }
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(CheckboxButton))]
    public class MyComponentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Get the target object
            CheckboxButton myComponent = (CheckboxButton)target;

            // Draw the custom toggle UI
            EditorGUI.BeginChangeCheck();
            bool newValue = EditorGUILayout.Toggle("IsChecked", myComponent.IsChecked);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(myComponent, "Change IsChecked");
                myComponent.IsChecked = newValue;
                EditorUtility.SetDirty(myComponent);
            }
            
            // Default Inspector
            DrawDefaultInspector();
        }
    }
    #endif // UNITY_EDITOR
}
