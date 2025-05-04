using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{
    public class UI_Panel : UI_Element
    {
        public LayoutGroup LayoutGroup;
        public Image       img_Background;
        
        [ContextMenu("Refresh Layout")]
        public void RefreshLayout()
        {
            if (this.LayoutGroup is HorizontalLayoutGroup h)
            {
                h.childAlignment = this.LayoutDirection == LayoutDirection.LeftToRight
                                 ? TextAnchor.MiddleLeft
                                 : TextAnchor.MiddleRight;
            }
            else if (this.LayoutGroup is VerticalLayoutGroup v)
            {
                v.reverseArrangement = !v.reverseArrangement;
            }

            var texts = this.gameObject.GetComponentsInChildren<TMP_Text>();
            foreach (var text in texts)
            {
                text.alignment = this.LayoutDirection == LayoutDirection.LeftToRight
                               ? TextAlignmentOptions.Left
                               : TextAlignmentOptions.Right;
                
            }
            
            foreach (var child in this.ChildUIElements)
                child.RefreshLayout();
        }
    }
}
