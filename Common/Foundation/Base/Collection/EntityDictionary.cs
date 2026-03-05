using System.Collections.Generic;

namespace Galleon.Checkout.Foundation
{
    public class EntityDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IEntity where TValue : IEntity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties

        public EntityNode Node { get; }

        public bool IsReferenceDictionary = false;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public EntityDictionary(bool isReferenceDictionary = false)
        {
            Node                       = new EntityNode(this);
            this.IsReferenceDictionary = isReferenceDictionary;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Dictionary API

        public new void Add(TKey key, TValue value)
        {
            base.Add(key, value);

            if (!IsReferenceDictionary)
                this.Node.AddChild(value);
        }

        public new bool Remove(TKey key)
        {
            if (TryGetValue(key, out var value))
            {
                if (!IsReferenceDictionary)
                    Node.RemoveChild(value);

                return base.Remove(key);
            }

            return false;
        }

        public new void Clear()
        {
            if (!IsReferenceDictionary)
            {
                foreach (var value in this.Values)
                    Node.RemoveChild(value);
            }

            base.Clear();
        }

        public new TValue this[TKey key]
        {
            get => base[key];
            set
            {
                if (ContainsKey(key))
                {
                    var oldValue = base[key];
                    if (!IsReferenceDictionary)
                        Node.RemoveChild(oldValue);
                }

                base[key] = value;

                if (!IsReferenceDictionary)
                    Node.AddChild(value);
            }
        }
    }
}