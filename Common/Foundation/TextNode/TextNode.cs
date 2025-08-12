using System.Collections.Generic;
using System.Linq;
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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Unity Test Menu

        #if UNITY_EDITOR
        [MenuItem("Tools/Test Parse Tree")]
        #endif
        public static void TestParseTree()
        {
            string mockText = 
@"
parent line #main .alpha
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
    }
}
