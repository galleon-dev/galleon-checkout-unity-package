#define GALLEON_DEV

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galleon.Checkout
{
    #if GALLEON_DEV
    [CreateAssetMenu(fileName = "Element", menuName = "Galleon/Element")]
    #endif
    public partial class Element : ScriptableObject, IEntity
    {
        /////////////////////////////////////////////////// Members
        
        public string       Name;
        public List<string> Tags = new List<string>();
        public Element      Parent;
        
        /////////////////////////////////////////////////// Entity
        
        public EntityNode Node { get; set; }
        
        /////////////////////////////////////////////////// Lifecycle

        public void Reset()
        {
            if (Name == null)
                Name = name;
            
            // Get the name of the scriptable object file
            #if UNITY_EDITOR
            if (string.IsNullOrEmpty(Name))
            {
                string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                    Name = fileName;
                }
            }
            Name = Name.Replace(".element", "");
            this.Node.DisplayName = Name;
            #endif

            
            // Assign the Parent field to the Project instance
            if (Parent == null)
            {
                #if UNITY_EDITOR
                var projectGuid = UnityEditor.AssetDatabase.FindAssets("t:Element Project")[0];
                if (!string.IsNullOrEmpty(projectGuid))
                {
                    var projectPath = UnityEditor.AssetDatabase.GUIDToAssetPath(projectGuid);
                    Parent = UnityEditor.AssetDatabase.LoadAssetAtPath<Element>(projectPath);
                }
                #endif
            }
        }

        private void OnEnable()
        {
            this.Node.DisplayName = Name;
        }

        public Element()
        {
            Node = new EntityNode(this);
            Root.Instance.Project.Node.Children.Add(this);
        }
    }
}

