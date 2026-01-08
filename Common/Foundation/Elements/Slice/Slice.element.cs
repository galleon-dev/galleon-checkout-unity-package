using System;
using System.Threading.Tasks;
using Galleon.Checkout;
using Galleon.Checkout.ELEMENTS;
using Galleon.Checkout.Foundation;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout.ELEMENTS
{
    [Element("Slice")]
    public class Slice : Element
    {
        public Slice(string name) : base(name)
        {
        }

    }
}
