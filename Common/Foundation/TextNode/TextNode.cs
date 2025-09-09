using System;
using System.Collections.Generic;
using System.Linq;
using Galleon.Checkout;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout.Foundation
{
    public class TextNode : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members

        /// <summary>
        /// The full raw text block for this node, including all lines.
        /// </summary>
        public string RawText;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Text Properties

        /// <summary>
        /// All lines of the node, trimmed of leading spaces.
        /// </summary>
        public List<string> FullTextLines => RawText?.Split('\n')
                                                     .Select(line => line.TrimStart())
                                                     .ToList() ?? new List<string>();

        /// <summary>
        /// Full raw text as a single string, including all original formatting.
        /// </summary>
        public string FullText => RawText;

        /// <summary>
        /// The first line
        /// </summary>
        public string Line => FullTextLines.FirstOrDefault() ?? "";
        
        /// <summary>
        /// The first line content, without the leading '>' or leading/trailing spaces.
        /// </summary>
        public string LineContent => FullTextLines.FirstOrDefault() is { } l && l.TrimStart().StartsWith(">") && l.TrimStart().Length > 1 
                                                                    ? l.TrimStart().Substring(1).Trim() 
                                                                    : "";

        
        /// <summary>
        /// List of words in the LineContent, split by whitespace.
        /// </summary>
        public List<string> LineWords => LineContent.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
        
        /// <summary>
        /// All lines after the first, combined into a single string.
        /// </summary>
        public string Content => string.Join("\n", ContentLines);

        /// <summary>
        /// All lines after the first line, each trimmed of leading spaces.
        /// </summary>
        public List<string> ContentLines => FullTextLines.Skip(1)
                                                         .Select(line => line.TrimStart())
                                                         .ToList();

        /// <summary>
        /// Position of the '>' character in the first line, used to infer tree depth.
        /// </summary>
        public int Indent
        {
            get
            {
                var    firstLine = RawText?.Split('\n').FirstOrDefault();
                return firstLine?.IndexOf('>') ?? int.MaxValue;
            }
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Specific Properties

        /// <summary>
        /// List of all hashtags (words starting with '#') found in the full text. Extracted without the '#' and trimmed at whitespace.
        /// </summary>
        public List<string> Hashtags
        {
            get
            {
                var tags = new List<string>();
                foreach (var line in FullTextLines)
                {
                    var words = line.Split(' ');
                    foreach (var word in words)
                    {
                        if (word.StartsWith("#") && word.Length > 1)
                        {
                            var tag = new string(word.Skip(1).TakeWhile(char.IsLetterOrDigit).ToArray());
                            if (!string.IsNullOrEmpty(tag))
                                tags.Add(tag);
                        }
                    }
                }
                return tags;
            }
        }

        /// <summary>
        /// The first hashtag found in the full text, or empty string if none exist.
        /// </summary>
        public string Hashtag => Hashtags.FirstOrDefault() ?? "";

        /// <summary>
        /// List of all dot-tags (words starting with '.') found in the full text. Extracted without the '.' and trimmed at whitespace.
        /// </summary>
        public List<string> Dots
        {
            get
            {
                var dots = new List<string>();
                foreach (var line in FullTextLines)
                {
                    var words = line.Split(' ');
                    foreach (var word in words)
                    {
                        if (word.StartsWith(".") && word.Length > 1)
                        {
                            var dot = new string(word.Skip(1).TakeWhile(char.IsLetterOrDigit).ToArray());
                            if (!string.IsNullOrEmpty(dot))
                                dots.Add(dot);
                        }
                    }
                }
                return dots;
            }
        }

        /// <summary>
        /// The first dot-tag found in the full text, or empty string if none exist.
        /// </summary>
        public string Dot => Dots.FirstOrDefault() ?? "";

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Parse API

        public static TextNode  Parse(string text)
        {
            return Parse(text.Split('\n').ToList());
        }

        public static TextNode Parse(IEnumerable<string> textLines)
        {
            /////// Definitions
            
            List<TextNode> nodes        = new List<TextNode>();
            List<string>   currentBlock = new List<string>();

            /////// Parse
            
            foreach (var line in textLines)
            {
                var trimmed = line.TrimStart();

                if (trimmed.StartsWith(">"))
                {
                    if (currentBlock.Count > 0)
                    {
                        nodes.Add(new TextNode { RawText = string.Join("\n", currentBlock) });
                        currentBlock.Clear();
                    }
                }

                currentBlock.Add(line);
            }

            if (currentBlock.Count > 0)
                nodes.Add(new TextNode { RawText = string.Join("\n", currentBlock) });

            TextNode rootNode = new TextNode { RawText = "> root" };

            /////// Link
            
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    TextNode currentNode         = nodes[i];
                    TextNode potentialParentNode = nodes[j];

                    if (potentialParentNode.Indent < currentNode.Indent)
                    {
                        currentNode.Node.Parent = potentialParentNode;
                        potentialParentNode.Node.Children.Insert(0, currentNode);
                        break;
                    }
                }
            }

            var rootNodes = nodes.Where(n => n.Node.Parent == null).ToList();
            foreach (var topNode in rootNodes)
                topNode.Node.SetParent(rootNode);

            return rootNode;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Write API

        /// <summary>
        /// Returns a string representation of the current node only.
        /// </summary>
        public string SelfToString()
        {
            return RawText ?? "";
        }

        /// <summary>
        /// Returns a string representation of the whole tree from the current node downwards.
        /// </summary>
        public string TreeToString()
        {
            var result = new List<string>();
            AddNodeToString(this, result, 0);
            return string.Join("\n", result);

            void AddNodeToString(TextNode node, List<string> result, int depth)
            {
                // Skip empty nodes
                if (string.IsNullOrEmpty(node.RawText)) return;
                
                var lines = node.RawText.Split('\n');
                if (lines.Length == 0) return;
                
                // Find the position of '>' in the first line to determine alignment for continuation lines
                string firstLine = lines[0].TrimStart();
                int    arrowPos  = firstLine.IndexOf('>');
                int    alignPos  = arrowPos >= 0 ? arrowPos + 2 : 0;
                
                // Create base indentation based on tree depth (4 spaces per level)
                string depthIndent = new string(' ', depth * 4);
                
                // Process each line in the node
                foreach (var line in lines)
                {
                    string trimmed = line.TrimStart();
                 
                    // Add alignment indentation only for continuation lines (not starting with '>')
                    string indent  = depthIndent + (trimmed.StartsWith(">") ? "" : new string(' ', alignPos));
                    result.Add(indent + trimmed);
                }

                // Recursively process all child nodes
                foreach (var child in node.Node.Children.OfType<TextNode>())
                {
                    AddNodeToString(child, result, depth + 1);
                }
            }

        }


        
        /// <summary>
        /// Writes the tree to the file at the specified path.
        /// </summary>
        public void WriteToFile(string path)
        {
            try
            {
                string content = TreeToString();
                System.IO.File.WriteAllText(path, content);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to write TextNode tree to file '{path}': {ex.Message}");
                throw;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// General

        public override string ToString()
        {
            return SelfToString();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Unity Test Menu

        #if UNITY_EDITOR
        [MenuItem("Tools/Test Parse Tree")]
        #endif
        public static void TestParseTree()
        {
            string mockText = 
@"
> parent line #main .alpha
  second line of parent
  third line of parent #secondary_tag .beta
    > child #nested .childdot
      child continuation line 1
      child continuation line 2 #child_tag
        > subchild .deepest
          subchild details
    > another_child
      another_child notes line 1
      another_child notes line 2 #another_tag .bottom
> second_root .rooted
     > deep
         > deeper
           even deeper content #deep_tag .last
           second line of deeper
";

            TextNode root = Parse(mockText);

            Debug.Log("Parsed Tree:");
            foreach (var child in root.Node.Descendants().OfType<TextNode>())
            {
                Debug.Log("---");
                foreach (var line in child.FullTextLines)
                {
                    Debug.Log(line);
                }
                Debug.Log("Hashtags: [" + string.Join(", ", child.Hashtags) + "]");
                Debug.Log("Dots: [" + string.Join(", ", child.Dots) + "]");
            }
        }

        #if UNITY_EDITOR
        [MenuItem("Tools/Test Write API")]
        #endif
        public static void TestWriteAPI()
        {
            string mockText = 
@"
> parent line #main .alpha
  second line of parent
  third line of parent #secondary_tag .beta
    > child #nested .childdot
      child continuation line 1
      child continuation line 2 #child_tag
        > subchild .deepest
          subchild details
    > another_child
      another_child notes line 1
      another_child notes line 2 #another_tag .bottom
> second_root .rooted
    > deep
        > deeper
          even deeper content #deep_tag .last
          second line of deeper
";

            TextNode root = Parse(mockText);

            Debug.Log("=== Testing Write API ===");
            
            // Test SelfToString on a specific child node
            var childNode = root.Node.Children.OfType<TextNode>().FirstOrDefault();
            if (childNode != null)
            {
                Debug.Log("SelfToString() for child node:");
                Debug.Log(childNode.SelfToString());
                Debug.Log("---");
            }

            // Test TreeToString on the root
            Debug.Log("TreeToString() for entire tree:");
            string treeString = root.TreeToString();
            Debug.Log(treeString);
            Debug.Log("---");

            // Test WriteToFile (write to a temporary file)
            string tempPath = System.IO.Path.Combine(Application.temporaryCachePath, "TextNode_Test_Output.txt");
            try
            {
                root.WriteToFile(tempPath);
                Debug.Log($"Successfully wrote tree to: {tempPath}");
                
                // Verify the file was written correctly
                string fileContent = System.IO.File.ReadAllText(tempPath);
                Debug.Log("File content verification:");
                Debug.Log(fileContent);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"WriteToFile test failed: {ex.Message}");
            }
        }
    }
}
