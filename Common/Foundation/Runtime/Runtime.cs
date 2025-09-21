using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Galleon.Checkout.Foundation.Tests;
using UnityEngine;

namespace Galleon.Checkout
{
    public class Runtime : Entity
    {
        ////////////////// Members
        
        public TestController TestController = new TestController();
        
        ////////////////// Lifecycle
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static async Task RuntimeEntryPoint()
        {
            Debug.Log("Runtime.RuntimeEntryPoint()");
            
          //Root.Instance.Runtime.Node.Children.Clear(); // For now its handled by the domain reload
        }
        
        ////////////////// Steps
        
        public static event Action OnRuntimeCreate;
        public Step RuntimeCreatee()
        =>
            new Step(name   : $"runtime_create"
                    ,action : async (s) =>
                    {
                        OnRuntimeCreate?.Invoke();
                    });
    }
}

