using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galleon.Checkout
{
    public class ParentPanel : MonoBehaviour
    {
        private RectTransform RectTransform;
        public  float         originalHeightAnchorTop;
        public  float         fullscreenHeightAnchorTop;

        private void Awake()
        {
            this.RectTransform             = this.GetComponent<RectTransform>();
            this.originalHeightAnchorTop   = this.RectTransform.rect.height;
            this.fullscreenHeightAnchorTop = 0;
            
            // TEMP 
            SetFullscreenHeight();
        }

        [ContextMenu("Set Fullscreen")]
        public void SetFullscreenHeight()
        {
            Vector2 offsetMax            = this.RectTransform.offsetMax;
            offsetMax.y                  = this.fullscreenHeightAnchorTop;
            this.RectTransform.offsetMax = offsetMax;
        }
    
        [ContextMenu("Set Original Size")]
        public void SetOriginalHeight()
        {
            Vector2 offsetMax            = this.RectTransform.offsetMax;
            offsetMax.y                  = -this.originalHeightAnchorTop;
            this.RectTransform.offsetMax = offsetMax;

        }
    }
}