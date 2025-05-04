using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEditor;

namespace Galleon.Checkout.UI
{
    public static class CSS 
    {
        public static List<Rule> ParseCSS(string css)
        {
            List<Rule> rules = new List<Rule>();
            
            // Remove comments and normalize whitespace
            css = RemoveComments(css);
            css = NormalizeWhitespace(css);
            
            // Split into rule blocks
            int openBraceIndex  = 0;
            int closeBraceIndex = 0;
            
            while (openBraceIndex < css.Length && openBraceIndex != -1)
            {
                openBraceIndex = css.IndexOf('{', openBraceIndex);
                if (openBraceIndex == -1) break;
                
                closeBraceIndex = css.IndexOf('}', openBraceIndex);
                if (closeBraceIndex == -1) break;
                
                string selector     = css.Substring(0, openBraceIndex).Trim();
                string declarations = css.Substring(openBraceIndex + 1, closeBraceIndex - openBraceIndex - 1).Trim();
                
                // Process selector
                string id  = null;
                string tag = null;
                
                if (selector.StartsWith("#"))
                {
                    id = selector.Substring(1);
                }
                else if (selector.StartsWith("."))
                {
                    tag = selector.Substring(1);
                }
                
                // Process declarations
                string[] declarationPairs = declarations.Split(';');
                foreach (string declaration in declarationPairs)
                {
                    if (string.IsNullOrWhiteSpace(declaration)) 
                        continue;
                    
                    string[] parts = declaration.Split(':');
                    
                    if (parts.Length != 2) 
                        continue;
                    
                    string property = parts[0].Trim();
                    string value    = parts[1].Trim();
                    
                    Rule rule = new Rule
                    {
                        ID       = id,
                        Tag      = tag,
                        Property = property,
                        Value    = value
                    };
                    
                    rules.Add(rule);
                }
                
                // Move past this rule block
                css             = css.Substring(closeBraceIndex + 1);
                openBraceIndex  = 0;
                closeBraceIndex = 0;
            }
            
            return rules;
        }

        private static string RemoveComments(string css)
        {
            // Remove CSS comments (/* ... */)
            return Regex.Replace(css, @"/\*[\s\S]*?\*/", string.Empty);
        }

        private static string NormalizeWhitespace(string css)
        {
            css = Regex.Replace(css, @"\s+", " "); // Replace multiple spaces with single space
            css = Regex.Replace(css, @"\s*([{};:])\s*", "$1"); // Remove spaces around brackets and semicolons
            return css.Trim(); // Remove leading/trailing whitespace
        }

        public class Rule
        {
            public string ID;
            public string Tag;
            public string Property;
            public string Value;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// TESTs
        
        public class CSSTest : MonoBehaviour
        {
            [TextArea(3, 10)]
            public static string testCSS = 
            @"#button {
                background-color: red;
                width: 100px;
            }
            .container {
                padding: 10px;
                margin: 20px;
            }";

            private void Start()
            {
                TestCSSParser();
            }

            #if UNITY_EDITOR
            [MenuItem("Tools/Galleon/Tests/Test CSS Parser")]
            #endif
            public static void TestCSSParser()
            {
                var rules = ParseCSS(testCSS);
                Debug.Log($"Found {rules.Count} CSS rules:");
                
                foreach (var rule in rules)
                {
                    string selector = !string.IsNullOrEmpty(rule.ID) ? $"#{rule.ID}" : $".{rule.Tag}";
                    Debug.Log($"> {selector} {{ {rule.Property}: {rule.Value}; }}");
                }
            }
        }
    }
}
