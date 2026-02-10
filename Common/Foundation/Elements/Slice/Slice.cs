using System.Collections.Generic;
using System.Linq;
using Galleon.Checkout.Foundation;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout
{
    public class Slice : Entity
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public Step RuntimeStart() 
        =>
            new Step(name   : $"slice_runtime_start"
                    ,action : async (s) =>
                    {   
                    });
    }
}
