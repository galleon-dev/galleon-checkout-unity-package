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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Main Steps

        public Step CreateSliceAsset(VirtualEntity ve)
        =>
            new Step(name   : $"create_slice_asset"
                    ,action : async (s) =>
                    {
                        #if UNITY_EDITOR

                        #endif
                    });


        public Step CreateSliceHierarchy(VirtualEntity ve)
        =>
            new Step(name   : $"create_slice_hierarchy"
                    ,action : async (s) =>
                    {
                        #if UNITY_EDITOR

                        #endif
                    });


        public Step CreateSliceApp(VirtualEntity ve)
        =>
            new Step(name   : $"create_slice_app"
                    ,action : async (s) =>
                    {
                        #if UNITY_EDITOR

                        #endif
                    });

    }
}
