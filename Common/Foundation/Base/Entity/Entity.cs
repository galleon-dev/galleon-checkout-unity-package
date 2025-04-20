using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout
{
    public interface IEntity
    {
        EntityNode Node { get; }
    }
    
    public partial class Entity : IEntity
    {
        [SerializeField]
        private EntityNode entityNode   =  null;
        public  EntityNode Node         => entityNode ??= new EntityNode(this);  
    }

    public partial class EntityNode
    {
        public EntityNode(IEntity entity)
        {
            if (entity == null)
                throw new Exception("entity is null in EntityNode constructor");
            
            this.Entity      = entity;
            this.DisplayName = entity.GetType().Name;
            
            PopulatePredefinedChildren();
        }
        
        ///////////////////////////////////////////////////////////////////////// Helper Methods
        
        public void PopulatePredefinedChildren()
        {
            // Auto Child Entities
            var type    = Entity.GetType();
            var members = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            
            foreach (var member in members)
            {
                var value = member switch
                {
                    System.Reflection.FieldInfo    field => field.GetValue(Entity),
                  //System.Reflection.PropertyInfo prop  => prop.GetValue(entity), // Not Properties!
                    _ => null
                };

                if (value != null && value is IEntity e)
                {
                    this.Children.Add(e);
                    
                    #if UNITY_EDITOR
                    var header = member.GetCustomAttribute<HeaderAttribute>();
                    if (header != null)
                    {
                        e.Node.editorExtras.HeaderAttributeText = header.header;
                    }
                    #endif
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - General

        [SerializeReference] [HideInInspector] public IEntity Entity;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Info

        public EntityID ID   = new EntityID();
        public Tags     Tags = new Tags();
        
        public string DisplayName;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Node

        [SerializeReference] public IEntity       Parent   = null;
        [SerializeReference] public List<IEntity> Children = new List<IEntity>();

        public                      IEnumerable<IEntity> Ancestors()   => EnumerateUp  (this.Entity);
        public                      IEnumerable<IEntity> Descendants() => EnumerateDown(this.Entity);


        public void SetParent(IEntity parent)
        {
            if (parent == this)
                return;

            this.Parent = parent;

            if (!parent.Node.Children.Contains(this.Entity))
                parent.Node.Children.Add(this.Entity);
        }


        // Static helper methods
        
        public static IEnumerable<IEntity> EnumerateDown(IEntity origin)
        {
            yield return origin;

            foreach (var child in origin.Node.Children)
            {
                var tree = EnumerateDown(child);

                foreach (var item in tree)
                {
                    yield return item;
                }
            }

            //var entries = new List<Element>();
            //entries.Add(origin);
            //foreach (var child in origin.Children)
            //{
            //    entries.AddRange(EnumerateDownstream(child));
            //}
            //return entries;
        }

        public static IEnumerable<IEntity> EnumerateUp(IEntity origin)
        {
            var current = origin;
            yield return current;

            while (current.Node.Parent != null)
            {
                current = current.Node.Parent;
                yield return current;
            }

            yield break;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Editor
        #if UNITY_EDITOR
        
        public EditorExtras editorExtras = new EditorExtras();
        public class EditorExtras
        {
            public string HeaderAttributeText;
            
        }
        
        #endif // UNITY_EDITOR
        
    }
}