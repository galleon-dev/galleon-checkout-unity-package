using System.Collections;
using System.Collections.Generic;
using Galleon.Checkout.Samples;
using UnityEngine;

namespace Galleon.SampleApp
{
    public class SampleAppController : MonoBehaviour
    {
        ////////////////////////////////////////////////////////////////////// Members
        
        public StoreView StoreView;
        
        ////////////////////////////////////////////////////////////////////// Lifecycle
        
        void Awake()
        {
            Debug.Log($"SampleAppController.Awake");
        }
        
        void Start()
        {
            // Debug.Log($"SampleAppController.Start");
        }
    }
}