using System.Collections.Generic;
using System.Threading.Tasks;

namespace Galleon.Checkout
{
    public class NetworkEndpoint<TRequest, TResponse> : Entity
    {
        public string                     URL     { get; set; }
        public string                     Method  { get; set; }
        public Dictionary<string, string> Headers { get; set; }
    }
}