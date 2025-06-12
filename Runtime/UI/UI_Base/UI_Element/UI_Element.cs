using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Galleon.Checkout.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Color = UnityEngine.Color;

namespace Galleon.Checkout.UI
{
    public class UI_Element : MonoBehaviour, IEntity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// IEntity
        
        public EntityNode Node { get; set; }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        [Header("UI Element")]
        public string          ID              = "";
        public List<string>    Tags            = new List<string>();
        
        [Header("Settings")]
        public LayoutDirection LayoutDirection = LayoutDirection.LeftToRight;
        public string          Local           = "en-us";
        public Appearance      appearance      = Appearance.System;
        
        [Space(10)]
        public string TypeSpace;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties
        
        public IEnumerable<UI_Element> ChildUIElements => gameObject.GetComponentsInChildren<UI_Element>(true).Except(new[] { this });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public UI_Element()
        {
            this.Node = new EntityNode(this);
        }

        private void Awake()
        {
            if (this.ID.IsNullOrEmpty())
                this.ID = this.gameObject.name;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Refresh methods
        
        [ContextMenu("Refresh UI Element")]
        public void Refresh()
        {
            RefreshLayout();
        }
        
        [ContextMenu("Refresh Layout")]
        public virtual void RefreshLayout()
        {
        }
        
        [ContextMenu("RefreshStyle")]
        public void RefreshStyle()
        {
            foreach (var child in this.ChildUIElements)
                child.RefreshStyle();
        }
        
        [ContextMenu("Refresh Locale")]
        public void RefreshLocale()
        {
            foreach (var child in this.ChildUIElements)
                child.RefreshLocale();
        }
        
        [ContextMenu("Refresh Config")]
        public void RefreshConfig()
        {
            foreach (var child in this.ChildUIElements)
                child.RefreshConfig();
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Style Methods
        
        public virtual void ApplyStyleRule(CSS.Rule rule)
        {
            
        }
        
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    }
    
    ///
    /// Helper Types
    /// 
    
    public enum LayoutDirection
    {
        LeftToRight,
        RightToLeft
    }
    
    public enum Appearance
    {
        System,
        Light,
        Dark,
    }
}