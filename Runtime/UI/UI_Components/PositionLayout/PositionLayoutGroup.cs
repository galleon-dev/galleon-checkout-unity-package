using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{
    [ExecuteInEditMode]
    [AddComponentMenu("Layout/Position Layout Group")]
    public class PositionLayoutGroup : LayoutGroup
    {
        public float spacing        = 0;
        public bool  isVertical     = true;
        public bool  centerChildren = true;
        

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
        }

        public override void CalculateLayoutInputVertical()
        {
            // Do nothing
        }

        public override void SetLayoutHorizontal()
        {
            PositionChildren();
        }

        public override void SetLayoutVertical()
        {
            PositionChildren();
        }

        private void PositionChildren()
        {
            float offset = isVertical ? -padding.top : padding.left;

            for (int i = 0; i < rectChildren.Count; i++)
            {
                RectTransform child = rectChildren[i];

                float size = isVertical
                             ? LayoutUtility.GetPreferredHeight(child)
                             : LayoutUtility.GetPreferredWidth(child);

                if (isVertical)
                {
                    SetChildAlongAxis(child, 1, offset);
                    
                    if (centerChildren)
                    {       
                        // Also Center the child's center horizontally within the parent
                        float childWidth       = child.rect.width;
                        float parentInnerWidth = rectTransform.rect.width - padding.left - padding.right;
                        float xOffset          = padding.left + (parentInnerWidth - childWidth) * 0.5f;
                        SetChildAlongAxis(child, 0, xOffset);
                    }
                }
                else
                {
                    SetChildAlongAxis(child, 0, offset);

                    if (centerChildren)
                    {
                        // Also Center the child's center vertically within the parent                    
                        float childHeight       = child.rect.height;
                        float parentInnerHeight = rectTransform.rect.height - padding.top - padding.bottom;
                        float yOffset           = padding.top + (parentInnerHeight - childHeight) * 0.5f;
                        SetChildAlongAxis(child, 1, yOffset);
                    }
                }

                offset += size + spacing;
            }
        }
        
        public float CalculateHeight()
        {
            float totalHeight = 0f;
            int activeChildCount = 0;

            // Iterate over each child in this transform
            foreach (RectTransform child in transform)
            {
                if (child.TryGetComponent(out LayoutElement layoutElement) && layoutElement.ignoreLayout)
                    continue;
                
                // Skip inactive children
                if (!child.gameObject.activeInHierarchy)
                    continue;

                // Add the child's preferred height
                totalHeight += LayoutUtility.GetPreferredHeight(child);
                activeChildCount++;
            }

            // Add spacing between children (only between them, not after the last one)
            float totalSpacing = Mathf.Max(0, activeChildCount - 1) * spacing;

            // Add top and bottom padding
            float totalPadding = padding.top + padding.bottom;

            // Final result: content height + spacing + padding
            return totalHeight + totalSpacing + totalPadding;
        }

        public override float preferredHeight => CalculateHeight();
    }
}