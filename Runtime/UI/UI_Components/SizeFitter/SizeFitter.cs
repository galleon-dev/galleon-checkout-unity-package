using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{
    [ExecuteInEditMode]
    public class SizeFitter : LayoutElement
    {
        public override float preferredHeight => ((RectTransform)this.transform).sizeDelta.y;
    }
}
