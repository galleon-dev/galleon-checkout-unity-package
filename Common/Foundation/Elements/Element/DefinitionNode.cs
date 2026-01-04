using System;
using System.Collections.Generic;
using System.Linq;
using Galleon.Checkout;
using Galleon.Checkout.Foundation;

namespace Galleon.Checkout.Foundation
{
    public class DefinitionNode : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public TextNode TextNode;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties
        
        public string   EntityType  => TextNode.LineFirstWord;
        public bool     IsNamespace => TextNode.LineSplits.Any(s => s.StartsWith("(") && s.EndsWith(")"));
        public string   Namespace   => TextNode.FirstLineParenthesisContent;
        public string   EntityName  => TextNode.LineWords.Count > 1 ? TextNode.LineWords.ElementAt(1) : "";
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public DefinitionNode()
        {
        }

        public DefinitionNode(TextNode textNode)
        {
            this.TextNode = textNode;
        }
        
        public DefinitionNode(string text)
        {
            this.TextNode = new TextNode(new string(text));
        }
        
        public DefinitionNode CloneNode()
        {
            return new DefinitionNode
                   {
                       TextNode = this.TextNode.CloneNode()
                   };
        }
        
        public DefinitionNode CloneTree()
        {
            var clone = CloneNode();

            foreach (var child in Node.Children.OfType<DefinitionNode>())
            {
                var childClone = child.CloneTree();
                childClone.Node.SetParent(clone);
                childClone.TextNode.Node.SetParent(clone.TextNode);
            }

            return clone;
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Static Parse
        
        public static DefinitionNode Parse(string lines) => Parse(lines.Split('\n'));
        public static DefinitionNode Parse(IEnumerable<string> lines)
        {
            DefinitionNode rootDefinition = null;
            TextNode       rootTextNode   = TextNode.Parse(lines);
            
            foreach (var textNode in rootTextNode.Node.Descendants().OfType<TextNode>())
            {
                var def = new DefinitionNode() { TextNode = textNode };
                textNode.Node.SetData("definition_node", def);
                
                if (textNode.Node.Parent != null)
                {
                    var parentText = textNode.Node.Parent;
                    var parentDef  = parentText.Node.GetData<DefinitionNode>("definition_node");
                    def.Node.SetParent(parentDef);
                }
                else
                {
                    rootDefinition = def;
                }
            }
            
            // cleanup
            foreach (var textNode in rootTextNode.Node.Descendants().OfType<TextNode>())
            {
                textNode.Node.RemoveData("definition_node");
            }
            
            return rootDefinition;
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helper Utility Methods
        
        ////////// Namespace
        
        public DefinitionNode GetNamespaceNode()
        {
            var parents = this.Node.Ancestors().OfType<DefinitionNode>();
            return parents.FirstOrDefault(p => p.TextNode.HasParenthesis);
        }
        
        public string GetNamespace()
        {
            return GetNamespaceNode()?.Namespace;
        }
        
        public bool IsNamespaceNode() => TextNode.HasParenthesis;
        
        ////////// Fill To Full Line
        
        public TextNode ExpandToFullLine()
        {
            // e.g. "> Folder 'package 1'" --> becomes --> "> Assets.Folder 'package 1'"
            
            TextNode lineNode = this.TextNode;
            
            var ns            = GetNamespace();
            var newLine       = ns == null 
                              ? lineNode.RawText
                              : lineNode.RawText.Replace($"> {lineNode.FirstSplit}"
                                                        ,$"> {ns}.{lineNode.FirstSplit}");
            
            return new TextNode(newLine);
        }
        
        public bool DoesNeedToExpandToFullLine() => !IsNamespaceNode() && GetNamespace() != null;

        ////////// Insert
        
        public void InsertNodeIntoTree(DefinitionNode childNamespaceNode)
        {
            try
            {
                // Get all nodes in current tree
                var thisTree                 = this.Node.Descendants().OfType<DefinitionNode>();
                
                // Find namespace nodes in current tree matching child's namespace
                var namespaceNodesInThisTree = thisTree.Where(n => n.IsNamespace && n.GetNamespace() == childNamespaceNode.GetNamespace());  
                var namespaceNodeInThisTree  = namespaceNodesInThisTree.FirstOrDefault();
                
                // Get parent node where child will be inserted
                var parentNodeInThisTree     = namespaceNodeInThisTree.Node.Children.Count > 0 
                                             ? namespaceNodeInThisTree.Node.Children.First()
                                             : namespaceNodeInThisTree.Node.Entity;
                
                // Insert child nodes
                foreach (DefinitionNode child in childNamespaceNode.Node.Children.OfType<DefinitionNode>())
                {
                    var cloneTree = child.CloneTree();
                    parentNodeInThisTree.Node.AddChild(cloneTree);
                    
                    // Add indentation to text nodes
                    foreach (var textNode in cloneTree.TextNode.Node.Descendants().OfType<TextNode>())
                    {
                        textNode.RawText = "    " + textNode.RawText;
                    }
                }
            }
            catch (Exception e)
            {
                e.ToString();
            }
            
        }

        public void ApplyVariable(string variableName, string value)
        {
            foreach (var textNode in TextNode.Node.Descendants().OfType<TextNode>())
            {
                textNode.RawText = textNode.RawText.Replace(variableName, value);
            }
        }

        public void AddTag(string tag)
        {
            // Prep
            var tagText = new string(tag);
            if (!tagText.StartsWith("#")) tagText = "#" + tagText;
            
            // Insert
            TextNode.RawText = TextNode.RawText + " " + tagText;
        }
    }
}