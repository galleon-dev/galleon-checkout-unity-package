
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Galleon.Checkout
{
    public class Utility
    {
        
    }
    
    public static class StringExtentions
    {
        public static string Color(this string text, string color)
        {
            return $"<color={color}>{text}</color>";
        }

        public static string Color(this string text, UnityEngine.Color color)
        {
            string hex = ColorUtility.ToHtmlStringRGB(color);
            return $"<color=#{hex}>{text}</color>";
        }

        public static bool IsNullOrEmpty(this string text)
        {
            return string.IsNullOrEmpty(text);
        }
        
        public static string ToSafeString(this object target)
        {
            if (target == null)
                return "";

            return target.ToString();
        }

        /// <summary>
        /// Truncates <paramref name="text"/> so that its total length never exceeds <paramref name="maxLength"/>.
        /// When truncation occurs the result ends with an ellipsis, which is counted inside the budget.
        /// A <paramref name="maxLength"/> of 0 (or less) disables truncation.
        /// </summary>
        public static string Truncate(this string text, int maxLength, string ellipsis = "...")
        {
            if (maxLength    <= 0)             return text;
            if (text          == null)         return text;
            if (text.Length   <= maxLength)    return text;

            ellipsis = ellipsis ?? "";

            // Not enough room for the ellipsis itself - hard cut.
            if (maxLength <= ellipsis.Length)
                return text.Substring(0, maxLength);

            return text.Substring(0, maxLength - ellipsis.Length).TrimEnd() + ellipsis;
        }
    }
    
    public static class IEnumerableExtentions
    {
        public static T RandomItem<T>(this IEnumerable<T> items)
        {
            if (items == null || !items.Any())
                return default;

            return items.ElementAt(UnityEngine.Random.Range(0, items.Count()));
        }
        
        public static string ToLineString<T>(this IEnumerable<T> items)
        {
            if (items == null || !items.Any())
                return string.Empty;

            return string.Join("'", items);
        }
        public static string ToMultilineString<T>(this IEnumerable<T> items)
        {
            if (items == null || !items.Any())
                return string.Empty;

            return string.Join("\n", items);
        }
    }
}