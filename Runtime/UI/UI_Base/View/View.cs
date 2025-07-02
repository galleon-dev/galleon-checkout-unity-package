using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Galleon.Checkout.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Color = UnityEngine.Color;

namespace Galleon.Checkout.UI
{
    public class View : MonoBehaviour, IEntity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// IEntity
        
        public EntityNode Node { get; set; }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public UI UI;
        
        public string State = "";
        public string Style = "";
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Flow
        
        public Step Flow;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public View()
        {
            this.Node = new EntityNode(this);
            this.Flow = new Step(name: $"{this.GetType().Name}_Flow", tags: new []{"view_flow"});
        }

        public virtual void Awake()
        {
            this.UI = new UI(this.gameObject);
            
            Initialize();
        }
        
        public virtual void Initialize()
        {
            
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Refresh

        
        [ContextMenu("Refresh")]
        public void DoRefresh() => Refresh();
        public void Refresh()
        {
            RefreshState();
           // RefreshUI();
        }
        
        
        [ContextMenu("Refresh UI")]
        public void DoRefreshUI() => RefreshUI();
        public void RefreshUI()
        {
            // Style
            
            var style = this.Style;
            var rules = CSS.ParseCSS(style);
            
            foreach (var element in UI.UI_Elements)
            {
                foreach (var rule in rules)
                {
                    if (element.Tags.Contains(rule.Tag) || element.ID == rule.ID)
                    {
                        element.ApplyStyleRule(rule);
                    }
                }
            }
            
            // Layout
            
            // Locale
        }
        
        [ContextMenu("Refresh State")]
        public void DoRefreshState() => RefreshState();
        public virtual void RefreshState()
        {   
        }
    }
}
