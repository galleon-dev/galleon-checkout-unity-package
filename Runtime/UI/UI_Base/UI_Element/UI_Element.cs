using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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

#region EXTRAS

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

/// <summary>
/// A lightweight CSS parser that extracts style properties from CSS text
/// </summary>
public class SimpleCSSParser
{
    /// <summary>
    /// Represents a CSS style with key-value property pairs
    /// </summary>
    public class Style
    {
        public Dictionary<string, string> properties = new Dictionary<string, string>();
    }

    /// <summary>
    /// Parses CSS text into a dictionary of selectors and their styles
    /// </summary>
    /// <param name="cssText">Raw CSS text to parse</param>
    /// <returns>Dictionary mapping CSS selectors to their style properties</returns>
    public static Dictionary<string, Style> Parse(string cssText)
    {
        // Remove CSS comments /* ... */ before parsing
        cssText = RemoveComments(cssText);
        
        // Store all parsed styles with their selectors
        var styles = new Dictionary<string, Style>();
        
        // Split CSS into blocks (each ending with '}')
        string[] blocks = cssText.Split('}');
        
        foreach (var block in blocks)
        {
            // Skip empty blocks
            if (string.IsNullOrWhiteSpace(block)) continue;
            
            // Split into selector and style body
            var parts = block.Split('{');
            if (parts.Length != 2) continue;

            // Extract and clean the selector and body
            string selector = parts[0].Trim();
            string body     = parts[1];

            // Create a new style for this selector
            var style = new Style();
            
            // Process each property declaration (prop: value;)
            string[] declarations = body.Split(';');
            foreach (var declaration in declarations)
            {
                if (string.IsNullOrWhiteSpace(declaration)) continue;
                
                // Split into property name and value
                var kv = declaration.Split(':');
                if (kv.Length != 2) continue;
                
                string key            = kv[0].Trim();
                string value          = kv[1].Trim();
                style.properties[key] = value;
            }

            styles[selector] = style;
        }

        return styles;
    }

    private static string RemoveComments(string cssText)
    {
        if (string.IsNullOrEmpty(cssText))
            return cssText;

        int    startIndex = 0;
        string result     = cssText;

        // Find and remove all /* ... */ comments
        while (true)
        {
            startIndex = result.IndexOf("/*", startIndex);
            if (startIndex == -1) 
                break;

            int endIndex = result.IndexOf("*/", startIndex + 2);
            if (endIndex == -1) 
                break; // Unclosed comment, just leave it

            // Remove the comment
            result = result.Remove(startIndex, (endIndex + 2) - startIndex);
            // Don't increment startIndex since we've removed the comment
            // and the text has shifted
        }

        return result;
    }
    
    public void ApplyStyle(Dictionary<string, Style> styles)
    {
        List<string> tags = new ();
        
        foreach (var style in styles)
        {
            var selector = style.Key;
            
            if (!tags.Contains($".{selector}"))
                continue;

            foreach (var property in style.Value.properties)
            {
                if (property.Key == "background_color")
                {
                    Image target = default;
                    target.color = (Color)new ColorConverter().ConvertFromString(property.Value)!;
                }
                if (property.Key == "font_color")
                {
                    Text target = default;
                    target.color = (Color)new ColorConverter().ConvertFromString(property.Value)!;
                }
                if (property.Key == "layout_direction")
                {
                    
                    HorizontalLayoutGroup target = default;
                    target.childAlignment = (TextAnchor)(property.Value == "left" 
                                                                         ? LayoutDirection.LeftToRight 
                                                                         : LayoutDirection.RightToLeft);  
                }
            }
        }
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public class Tags : MonoBehaviour
{
    public List<string> tags = new List<string>();
}


#endregion // Extras