using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Galleon.Checkout
{
    public class Runtime : Entity
    {
        [RuntimeInitializeOnLoadMethod]
        public static async Task RuntimeEntryPoint()
        {
            Debug.Log("Runtime.RuntimeEntryPoint()");
            
            Root.Instance.Runtime.Node.Children.Clear();
        }
    }
}
