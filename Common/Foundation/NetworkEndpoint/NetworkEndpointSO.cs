using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout.Foundation
{
    #if UNITY_EDITOR
    [CreateAssetMenu(fileName = "new_network_endpoint", menuName = "Galleon/Network Endpoint")]
    #endif
    public class NetworkEndpointSO : ScriptableObject, IEntity
    {
        //////////////////////////////////////////////////////////////// IEntity

        public EntityNode Node { get; }
        
        //////////////////////////////////////////////////////////////// Members
        
        public string URL;
        public string Method;
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(NetworkEndpointSO))]
    public class NetworkEndpointEditor : Editor
    {
        
        // Create a VisualElement for the custom editor UI
        public override VisualElement CreateInspectorGUI()
        {
            return new IMGUIContainer(() =>
            {
                DrawDefaultInspector();
            });
        }

    }
    #endif
}
