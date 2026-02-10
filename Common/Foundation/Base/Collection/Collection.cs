using System;
using System.Collections.Generic;

namespace Galleon.Checkout.Foundation
{
    public class Collection<T> : List<T>, IEntity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties
        
        public EntityNode Node { get; }
        
        public bool IsReferenceCollection = false;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public Collection(bool isReferenceCollection = false)
        {
            Node                       = new EntityNode(this);
            this.IsReferenceCollection = isReferenceCollection;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// List API

        public new void Add(T item)
        {
            base.Add(item);
            
            if (!IsReferenceCollection 
            &&  item is IEntity entity)
                this.Node.AddChild(entity);
        }

        public new void AddRange(IEnumerable<T> collection)
        {
            foreach (var item in collection)
                this.Add(item);
        }

        public new bool Remove(T item)
        {
            if (!IsReferenceCollection
            &&  item is IEntity entity)
                Node.RemoveChild(entity);

            return base.Remove(item);
        }

        public new void Clear()
        {
            foreach (var item in this)
            {
                if (!IsReferenceCollection
                &&  item is IEntity entity)
                    Node.RemoveChild(entity);
            }
            base.Clear();
        }

        public new void Insert(int index, T item)
        {
            base.Insert(index, item);
            
            if (!IsReferenceCollection
            &&  item is IEntity entity)
                Node.AddChild(entity);
        }

        public new void InsertRange(int index, IEnumerable<T> collection)
        {
            foreach (var item in collection)
                this.Insert(index++, item);
        }

        public new void RemoveAt(int index)
        {
            var item = this[index];
            
            if (!IsReferenceCollection
            &&  item is IEntity entity)
                Node.RemoveChild(entity);

            base.RemoveAt(index);
        }

        public new int RemoveAll(Predicate<T> match)
        {
            var itemsToRemove = this.FindAll(match);
            foreach (var item in itemsToRemove)
            {
                if (!IsReferenceCollection
                &&  item is IEntity entity)
                    Node.RemoveChild(entity);
            }
            return base.RemoveAll(match);
        }


    }
}