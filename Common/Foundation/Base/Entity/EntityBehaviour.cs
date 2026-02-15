using UnityEngine;

namespace Galleon.Checkout
{
    public class EntityBehaviour : MonoBehaviour, IEntity
    {
        public EntityNode Node { get; }

        public EntityBehaviour()
        {
            Node = new EntityNode(this);
        }
    }
}