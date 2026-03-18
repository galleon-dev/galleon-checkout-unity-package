using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Galleon.Checkout.Foundation;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UIElements;

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

        public Entity()
        {
            entityNode = new EntityNode(this);
        }
        
    }
    
    public class EntityFP : Entity {}

    public partial class EntityNode
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public EntityNode(                   IEntity entity
                         ,[CallerMemberName] string  callerName = ""
                         ,[CallerLineNumber] int     callerLine = 0
                         ,[CallerFilePath  ] string  callerPath = "")
        {
            if (entity == null)
                throw new Exception("entity is null in EntityNode constructor");
            
            this.Entity      = entity;
            //this.DisplayName = entity.GetType().Name;
            
            this.Breadcrumbs.Add(new Breadcrumb(callerName, callerLine, callerPath, displayName : "creation_breadcrumb"));
            
            //Setup();
        }
        
        public void Initialize()
        {
            foreach (var child in this.Descendants())
                child.Node.Setup();
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Setup
        
        bool didSetup     = false;
        bool didLateSetup = false;
        
        public void Setup()
        {
            PopulatePredefinedChildren();
            didSetup = true;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - General

        [SerializeReference] [HideInInspector] public IEntity Entity;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - ID

        public        EntityID ID => new EntityID(this.Entity);
        public struct EntityID
        {
            private IEntity Entity; 
            public  EntityID(IEntity entity) { this.Entity = entity; }
            
            public string SelfPathID  => this.Entity.Node.DisplayName;
            public string PathID      => string.Join(".", Entity.Node.Ancestors()
                                                          .Reverse()
                                                          .ToList()
                                                          .Select(p => p.Node.ID.SelfPathID)
                                                          );
            
            public string StorageID   => Entity.Node.CustomStorageID?.Invoke() ?? PathID; 
        }
        
        public Func<string> CustomStorageID = null;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Tags
        
        public Tags Tags = new Tags();
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Info
        
        public string CustomDisplayName = null;
        public string DisplayName => CustomDisplayName ?? 
                                     Entity?.ToString()?
                                    .Replace($"{Entity?.GetType().Namespace ?? string.Empty}.", "") 
                                    ?? "Null";
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Breadcrumbs

        public List<Breadcrumb> Breadcrumbs = new();

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Node

        [SerializeReference] public IEntity             Parent          = null;
                             public EntityNode          ParentNode      => Parent.Node;
        [SerializeReference] public List<IEntity>       Children        = new();
                             public List<EntityNode>    ChildNodes      => Children.Select(c => c.Node).ToList();
        
        public List<WeakReference<IEntity>>             LinkedChildren  { get; set; } = new();

        public IEnumerable<IEntity>                     Ancestors()     => EnumerateUp  (this.Entity);
        public IEnumerable<IEntity>                     Descendants()   => EnumerateDown(this.Entity);

        public void SetParent(IEntity parent)
        {
            if (parent == this)
                return;

            this.Parent = parent;

            if (!parent.Node.Children.Contains(this.Entity))
                parent.Node.Children.Add(this.Entity);
        }
        
        public void RemoveFromParent()
        {
            Parent.Node.RemoveChild(this.Entity);
        }

        public void AddChild(IEntity child)
        {
            if (child.Node.Parent != null)
            {
                child.Node.Parent.Node.RemoveChild(child);
            }
            
            this.Children.Add(child);
            child.Node.SetParent(this.Entity);
            
            if (child.Node.didSetup == false)
            {
                child.Node.Setup();
            }
        }
        
        public void RemoveChild(IEntity child)
        {
            this.Children.Remove(child);
            child.Node.Parent = null;
        }
        
        public void AddLinkedChild(IEntity linkedChild)
        {
            this.LinkedChildren.Add(new WeakReference<IEntity>(linkedChild));
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
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Node Suger
        
        public T                    GetEntityInChildren            <T>()                => this.Children.OfType<T>().FirstOrDefault();
        public T                    GetEntityInChildrenAndSelf     <T>()                => this.Entity is T self ? self : this.Children.OfType<T>().FirstOrDefault();
        public T                    GetEntityInDescendants         <T>()                => this.Descendants().Skip(1).OfType<T>().FirstOrDefault();
        public T                    GetEntityInDescendantsAndSelf  <T>()                => this.Descendants().OfType<T>().FirstOrDefault();

        public IEntity              GetEntityInChildren            (Type type)          => this.Children.FirstOrDefault(c => type.IsInstanceOfType(c));
        public IEntity              GetEntityInChildrenAndSelf     (Type type)          => type.IsInstanceOfType(this.Entity) ? this.Entity : this.Children.FirstOrDefault(c => type.IsInstanceOfType(c));
        public IEntity              GetEntityInDescendants         (Type type)          => this.Descendants().Skip(1).FirstOrDefault(e => type.IsInstanceOfType(e));
        public IEntity              GetEntityInDescendantsAndSelf  (Type type)          => this.Descendants().FirstOrDefault(e => type.IsInstanceOfType(e));

        public IEnumerable<T>       GetEntitiesInChildren          <T>()                => this.Children.OfType<T>();
        public IEnumerable<T>       GetEntitiesInChildrenAndSelf   <T>()                => (this.Entity is T self ? new[] { self } : Enumerable.Empty<T>()).Concat(this.Children.OfType<T>());
        public IEnumerable<T>       GetEntitiesInDescendants       <T>()                => this.Descendants().Skip(1).OfType<T>();
        public IEnumerable<T>       GetEntitiesInDescendantsAndSelf<T>()                => this.Descendants().OfType<T>();

        public IEnumerable<IEntity> GetEntitiesInChildren          (Type type)          => this.Children.Where(c => type.IsInstanceOfType(c));
        public IEnumerable<IEntity> GetEntitiesInChildrenAndSelf   (Type type)          => (type.IsInstanceOfType(this.Entity) ? new[] { this.Entity } : Enumerable.Empty<IEntity>()).Concat(this.Children.Where(c => type.IsInstanceOfType(c)));
        public IEnumerable<IEntity> GetEntitiesInDescendants       (Type type)          => this.Descendants().Skip(1).Where(e => type.IsInstanceOfType(e));
        public IEnumerable<IEntity> GetEntitiesInDescendantsAndSelf(Type type)          => this.Descendants().Where(e => type.IsInstanceOfType(e));

        public T                    GetEntityInParent              <T>()                => this.Parent is T parent ? parent : default;
        public T                    GetEntityInParentAndSelf       <T>()                => this.Entity is T self ? self : (this.Parent is T parent ? parent : default);
        public T                    GetEntityInAncestors           <T>()                => this.Ancestors().Skip(1).OfType<T>().FirstOrDefault();
        public T                    GetEntityInAncestorsAndSelf    <T>()                => this.Ancestors().OfType<T>().FirstOrDefault();

        public IEntity              GetEntityInParent              (Type type)          => type.IsInstanceOfType(this.Parent) ? this.Parent : null;
        public IEntity              GetEntityInParentAndSelf       (Type type)          => type.IsInstanceOfType(this.Entity) ? this.Entity : (type.IsInstanceOfType(this.Parent) ? this.Parent : null);
        public IEntity              GetEntityInAncestors           (Type type)          => this.Ancestors().Skip(1).FirstOrDefault(e => type.IsInstanceOfType(e));
        public IEntity              GetEntityInAncestorsAndSelf    (Type type)          => this.Ancestors().FirstOrDefault(e => type.IsInstanceOfType(e));

        public IEnumerable<T>       GetEntitiesInParent            <T>()                => this.Parent is T parent ? new[] { parent } : Enumerable.Empty<T>();
        public IEnumerable<T>       GetEntitiesInParentAndSelf     <T>()                => (this.Entity is T self ? new[] { self } : Enumerable.Empty<T>()).Concat(this.Parent is T parent ? new[] { parent } : Enumerable.Empty<T>());
        public IEnumerable<T>       GetEntitiesInAncestors         <T>()                => this.Ancestors().Skip(1).OfType<T>();
        public IEnumerable<T>       GetEntitiesInAncestorsAndSelf  <T>()                => this.Ancestors().OfType<T>();

        public IEnumerable<IEntity> GetEntitiesInParent            (Type type)          => this.Parent != null && type.IsInstanceOfType(this.Parent) ? new[] { this.Parent } : Enumerable.Empty<IEntity>();
        public IEnumerable<IEntity> GetEntitiesInParentAndSelf     (Type type)          => (type.IsInstanceOfType(this.Entity) ? new[] { this.Entity } : Enumerable.Empty<IEntity>()).Concat(this.Parent != null && type.IsInstanceOfType(this.Parent) ? new[] { this.Parent } : Enumerable.Empty<IEntity>());
        public IEnumerable<IEntity> GetEntitiesInAncestors         (Type type)          => this.Ancestors().Skip(1).Where(e => type.IsInstanceOfType(e));
        public IEnumerable<IEntity> GetEntitiesInAncestorsAndSelf  (Type type)          => this.Ancestors().Where(e => type.IsInstanceOfType(e));
        
        public bool                 ContainsChild                  (IEntity child)      => this.Children.Contains(child);
        public bool                 ContainsDescendant             (IEntity entity)     => this.Descendants().Contains(entity);
        public IEntity              GetChildAt                     (int index)          => (index < 0 || index >= this.Children.Count) ? null : this.Children[index];
        public int                  GetChildCount                  ()                   => this.Children.Count;
        public IEnumerable<IEntity> GetSiblings                    ()                   => this.Parent?.Node.Children.Where(c => c != this.Entity) ?? Enumerable.Empty<IEntity>();
        public T                    GetSibling<T>                  ()                   => this.GetSiblings().OfType<T>().FirstOrDefault();
        public IEnumerable<T>       GetSiblings<T>                 ()                   => this.GetSiblings().OfType<T>();
        public int                  GetDepth                       ()                   => this.Ancestors().Count() - 1;
        public bool                 IsAncestorOf                   (IEntity entity)     => entity.Node.Ancestors().Contains(this.Entity);
        public bool                 IsDescendantOf                 (IEntity entity)     => this.Ancestors().Contains(entity);


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Initialization
        
        public void PopulatePredefinedChildren()
        {
            // Auto Child Entities
            var type    = Entity.GetType();
            var members = type.GetFields(BindingFlags.Instance | BindingFlags.Public)
                              .Where(m => !(typeof(Step).IsAssignableFrom(m.FieldType)));
            
            foreach (var member in members)
            {
                try
                {
                    var value = member switch
                                {
                                    FieldInfo field => field.GetValue(Entity),
                                    //System.Reflection.PropertyInfo prop  => prop.GetValue(entity), // Not Properties!
                                    _ => null
                                };
                    
                    
                    if (value != null && value is IEntity e)
                    {
                        e.Node.CustomDisplayName = member.Name;
                        this.AddChild(e);
                        
                        #if UNITY_EDITOR
                        var header = member.GetCustomAttribute<HeaderAttribute>();
                        if (header != null)
                        {
                            e.Node.editorExtras.HeaderAttributeText = header.header;
                        }
                        #endif
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.ToString());
                }

            }
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Quick Debug
        
        public void LogDumpTree()
        {
            DumpTreeNode(this.Entity, 0);
            void DumpTreeNode(IEntity entity, int level)
            {
                var indent = new string(' ', level * 2);
                Debug.Log($"{indent}{entity}");

                foreach (var child in entity.Node.Children)
                    DumpTreeNode(child, level + 1);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Data
        
        private Dictionary<string, object> Data;
        
        public void SetData(string key, object value)
        {
            if (Data is null)
                Data = new Dictionary<string, object>();
            
            this.Data[key] = value;
        }

        public object GetData(string key)
        {
            if (Data is null)
                return null;
            
            return this.Data[key];
        }

        public T GetData<T>(string key)
        {
            if (Data is null)
                return default;
            
            return (T)this.Data[key];
        }

        public void RemoveData(string key)
        {
            if (Data is null)
                return;
            
            this.Data.Remove(key);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Prefs Storage
        
        public        EntityPrefsStorage PrefsStorage => new(Entity);
        public struct EntityPrefsStorage
        {
            private IEntity Entity; public  EntityPrefsStorage(IEntity entity) => Entity = entity;

            public void         Store         (string key, object value) => Root.Instance.Context.SystemServices.PrefsStorageService.Store           ($"entity_storage_{Entity.Node.ID.StorageID}_{key}", value );
            public T            Load<T>       (string key)               => Root.Instance.Context.SystemServices.PrefsStorageService.Load<T>         ($"entity_storage_{Entity.Node.ID.StorageID}_{key}"        );
            public object       Load          (string key)               => Root.Instance.Context.SystemServices.PrefsStorageService.Load            ($"entity_storage_{Entity.Node.ID.StorageID}_{key}"        );
            public bool         HasKey        (string key)               => Root.Instance.Context.SystemServices.PrefsStorageService.HasKey          ($"entity_storage_{Entity.Node.ID.StorageID}_{key}"        );
            public void         Remove        (string key)               => Root.Instance.Context.SystemServices.PrefsStorageService.Remove          ($"entity_storage_{Entity.Node.ID.StorageID}_{key}"        );
            public void         AddToList     (string key, object value) => Root.Instance.Context.SystemServices.PrefsStorageService.AddToList       ($"entity_storage_{Entity.Node.ID.StorageID}_{key}", value );
            public void         RemoveFromList(string key, object value) => Root.Instance.Context.SystemServices.PrefsStorageService.RemoveFromList  ($"entity_storage_{Entity.Node.ID.StorageID}_{key}", value );
            public List<T>      LoadList<T>   (string key)               => Root.Instance.Context.SystemServices.PrefsStorageService.LoadList<T>     ($"entity_storage_{Entity.Node.ID.StorageID}_{key}"        );
            public List<string> GetAllStoredKeys()                       => Root.Instance.Context.SystemServices.PrefsStorageService.GetAllStoredKeys(beginningWith:$"entity_storage_{Entity.Node.ID.StorageID}_");
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - SessionState
        
        public        EntitySessionStorage SessionStorage => new(Entity);
        public struct EntitySessionStorage
        {
            private IEntity Entity; public  EntitySessionStorage(IEntity entity) => Entity = entity;

            public void         Store           (string key, object value) => Root.Instance.Context.SystemServices.SessionStorageService.Store           ($"entity_storage_{Entity.Node.ID.StorageID}_{key}", value );
            public T            Load<T>         (string key)               => Root.Instance.Context.SystemServices.SessionStorageService.Load<T>         ($"entity_storage_{Entity.Node.ID.StorageID}_{key}"        );
            public object       Load            (string key)               => Root.Instance.Context.SystemServices.SessionStorageService.Load            ($"entity_storage_{Entity.Node.ID.StorageID}_{key}"        );
            public bool         HasKey          (string key)               => Root.Instance.Context.SystemServices.SessionStorageService.HasKey          ($"entity_storage_{Entity.Node.ID.StorageID}_{key}"        );
            public void         Remove          (string key)               => Root.Instance.Context.SystemServices.SessionStorageService.Remove          ($"entity_storage_{Entity.Node.ID.StorageID}_{key}"        );
            public void         AddToList       (string key, object value) => Root.Instance.Context.SystemServices.SessionStorageService.AddToList       ($"entity_storage_{Entity.Node.ID.StorageID}_{key}", value );
            public void         RemoveFromList  (string key, object value) => Root.Instance.Context.SystemServices.SessionStorageService.RemoveFromList  ($"entity_storage_{Entity.Node.ID.StorageID}_{key}", value );
            public List<T>      LoadList<T>     (string key)               => Root.Instance.Context.SystemServices.SessionStorageService.LoadList<T>     ($"entity_storage_{Entity.Node.ID.StorageID}_{key}"        );
            public List<string> GetAllStoredKeys()                         => Root.Instance.Context.SystemServices.SessionStorageService.GetAllStoredKeys(beginningWith:$"entity_storage_{Entity.Node.ID.StorageID}_");
        }
        

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Disk Storage

        public        EntityDiskStorage DiskStorage => new(Entity);
        public struct EntityDiskStorage
        {
            private IEntity Entity; public  EntityDiskStorage(IEntity entity) => Entity = entity;

            public void         Store         (string key, object value) => Root.Instance.Context.SystemServices.DiskStorageService.Store           ($"entity_storage_{Entity.Node.ID.StorageID}_{key}", value  );
            public T            Load<T>       (string key)               => Root.Instance.Context.SystemServices.DiskStorageService.Load<T>         ($"entity_storage_{Entity.Node.ID.StorageID}_{key}"         );
            public object       Load          (string key)               => Root.Instance.Context.SystemServices.DiskStorageService.Load            ($"entity_storage_{Entity.Node.ID.StorageID}_{key}"         );
            public bool         HasKey        (string key)               => Root.Instance.Context.SystemServices.DiskStorageService.HasKey          ($"entity_storage_{Entity.Node.ID.StorageID}_{key}"         );
            public void         Remove        (string key)               => Root.Instance.Context.SystemServices.DiskStorageService.Remove          ($"entity_storage_{Entity.Node.ID.StorageID}_{key}"         );
            public void         AddToList     (string key, object value) => Root.Instance.Context.SystemServices.DiskStorageService.AddToList       ($"entity_storage_{Entity.Node.ID.StorageID}_{key}", value  );
            public void         RemoveFromList(string key, object value) => Root.Instance.Context.SystemServices.DiskStorageService.RemoveFromList  ($"entity_storage_{Entity.Node.ID.StorageID}_{key}", value  );
            public List<T>      LoadList<T>   (string key)               => Root.Instance.Context.SystemServices.DiskStorageService.LoadList<T>     ($"entity_storage_{Entity.Node.ID.StorageID}_{key}"         );
            public List<string> GetAllStoredKeys()                       => Root.Instance.Context.SystemServices.DiskStorageService.GetAllStoredKeys(beginningWith:$"entity_storage_{Entity.Node.ID.StorageID}_");
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Reflection
        
        public       EntityReflection Reflection => new(Entity);
        public class EntityReflection
        {
            private IEntity Entity; public  EntityReflection(IEntity entity) => Entity = entity;
            
            public IEnumerable<Step> Steps()
            {
                var type = this.Entity.GetType();
                
                // Retrieve all methods in the type that have a return type of Step
                var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                  .Where(m => m.GetParameters().Length == 0)
                                  .Where(m => m.ReturnType == typeof(Step));
                
                foreach (var method in methods)
                {
                    yield return (Step)method.Invoke(this.Entity, null);
                }
                
                yield break;
            }
                        
            public IEnumerable<Step> Steps(object parameterObject)
            {
                var type = this.Entity.GetType();
                
                // Retrieve all methods in the type
                var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                  .Where(m => m.GetParameters().Length > 0)
                                  .Where(m => m.ReturnType == typeof(Step));
                
                foreach (var method in methods)
                {
                    var parameters          = method.GetParameters();
                    var parameterObjectType = parameterObject.GetType();
                    
                    // Check if all parameters in the method can be matched by the members of the parameterObject
                    bool parametersMatch = parameters.All(p =>
                                                          {
                                                              var    member    = (MemberInfo)parameterObjectType.GetProperty(p.Name) ?? parameterObjectType.GetField(p.Name);
                                                              var    fieldType = member is PropertyInfo propertyInfo ? propertyInfo.PropertyType : ((FieldInfo)member).FieldType;
                                                              return member != null && fieldType == p.ParameterType;
                                                          });

                    if (parametersMatch)
                    {
                        // Prepare the arguments by retrieving values from the parameterObject
                        var arguments = parameters.Select(p =>
                                                          {
                                                              var    member = (MemberInfo)parameterObjectType.GetProperty(p.Name) ?? (MemberInfo)parameterObjectType.GetField(p.Name);
                                                              return member is PropertyInfo property ? property.GetValue(parameterObject) : ((FieldInfo)member)?.GetValue(parameterObject);
                                                          }).ToArray();
                        
                        yield return (Step)method.Invoke(this.Entity, arguments);
                    }
                }
                
                yield break;
            }
            
            public IEnumerable<Func<Step>> StepMethods()
            {
                var type = this.Entity.GetType();

                // Retrieve all methods in the type that have a return type of Step
                var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                  .Where(m => m.GetParameters().Length == 0)
                                  .Where(m => m.ReturnType == typeof(Step));

                foreach (var method in methods)
                {
                    var origin = this;
                    yield return () => (Step)method.Invoke(origin.Entity, null);
                }

                yield break;
            }
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Inspector
        
        private System.WeakReference<Inspector>    _Inspector    = new WeakReference<Inspector>(null);
        public  Inspector                          Inspector     {
                                                                     get { _Inspector.TryGetTarget(out var i); return i;     } 
                                                                     set { _Inspector = new WeakReference<Inspector>(value); }
                                                                 }
        
        private System.WeakReference<ExplorerItem> _ExplorerItem = new WeakReference<ExplorerItem>(null);
        public  ExplorerItem                       ExplorerItem  {
                                                                     get { _ExplorerItem.TryGetTarget(out var e); return e;        } 
                                                                     set { _ExplorerItem = new WeakReference<ExplorerItem>(value); }
                                                                 }
        
        public class EntityInspector : Inspector<IEntity>
        {
            public EntityInspector(IEntity target) : base(target)
            {
                //////////////////////////////////////////////////////////// Main
                Foldout foldout = new Foldout() { text = $"{target.Node.DisplayName} entity" , value = true}; this.Add(foldout);
                //foldout.Add(new Label(" "));
                //foldout.Add(new Label(" "));
                foldout.contentContainer.style.backgroundColor = new Color(0.3f,0.3f,0.3f);
                
                //////////////////////////////////////////////////////////// Breadcrumbs
                
                var breadcrumbsFoldout = new Foldout() { text = "Breadcrumbs", value = false }; foldout.Add(breadcrumbsFoldout);
                breadcrumbsFoldout?.Clear();
                foreach (var breadcrumb in Target.Node.Breadcrumbs)
                {
                    var breadcrumbInspector = new Breadcrumb.Inspector(breadcrumb, breadcrumb.DisplayName);
                    breadcrumbsFoldout.Add(breadcrumbInspector);
                }
                
                //////////////////////////////////////////////////////////// Session Storage
                
                var sessionStorageFoldout = new Foldout() { text = "Session Storage", value = false }; foldout.Add(sessionStorageFoldout);
                sessionStorageFoldout.RegisterValueChangedCallback(evt =>
                {
                    if (evt.previousValue == false && evt.newValue == true)
                    {
                        RefreshSessionStorage();
                    }
                });
                                
                
                RefreshSessionStorage();
                void RefreshSessionStorage()
                {
                    sessionStorageFoldout.Clear();

                    // Add Dump Storage button
                    Button dumpButton = new Button(() =>
                    {
                        var keys = Target.Node.SessionStorage.GetAllStoredKeys();
                        foreach (var key in keys)
                        {
                            object value;
                            if (key.StartsWith(SessionStorageService.KEY_PREFIX))
                                value = Root.Instance.Context.SystemServices.SessionStorageService.Load(key);
                            else
                                value = Target.Node.SessionStorage.Load(key);
                            
                            Debug.Log($"  {key}: {value ?? "null"}");
                        }
                    });
                    dumpButton.text = "Dump Storage";
                    sessionStorageFoldout.Add(dumpButton);

                    // Add Clear Storage button
                    Button clearButton = new Button(() =>
                    {
                        var keys = Target.Node.SessionStorage.GetAllStoredKeys();
                        foreach (var key in keys)
                            Target.Node.SessionStorage.Remove(key);
                        Debug.Log($"Cleared {keys.Count} session storage keys for {Target}");
                        RefreshSessionStorage();
                    });
                    clearButton.text = "Clear Storage";
                    sessionStorageFoldout.Add(clearButton);

                    ////////
    
                    var keys = Target.Node.SessionStorage.GetAllStoredKeys();
                    foreach (var key in keys)
                    {
                        var value = Target.Node.SessionStorage.Load(key);
                        sessionStorageFoldout.Add(new Label($"{key}: {value}"));
                    }
                }

                //////////////////////////////////////////////////////////// Virtual Entities
                
                var virtualEntitiesFoldout = new Foldout() { text = "Virtual Entities", value = false }; foldout.Add(virtualEntitiesFoldout);
                virtualEntitiesFoldout.RegisterValueChangedCallback(evt =>
                {
                    if (evt.previousValue == false && evt.newValue == true)
                    {
                        RefreshVirtualEntities();
                    }
                });

                RefreshVirtualEntities();
                void RefreshVirtualEntities()
                {
                    virtualEntitiesFoldout.Clear();
                    var virtualEntityIDs = Target.Node.SessionStorage.LoadList<string>("virtual_entities");

                    if (virtualEntityIDs == null || virtualEntityIDs.Count == 0)
                    {
                        virtualEntitiesFoldout.Add(new Label("No virtual entities"));
                        return;
                    }

                    foreach (var veID in virtualEntityIDs)
                    {
                        var veString = Root.Instance.Context.SystemServices.SessionStorageService.Load<string>($"virtual_entity_{veID}");
                        virtualEntitiesFoldout.Add(new Label($"ID: {veID}"));
                        virtualEntitiesFoldout.Add(new Label($"  Data: {(string.IsNullOrEmpty(veString) ? "(empty)" : veString.Substring(0, System.Math.Min(50, veString.Length)) + "...")}"));
                    }
                }
                

                //////////////////////////////////////////////////////////// Steps
                #region STEPS
                
                // get all methods that return void and have no params
                var steps = target.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                            .Where     (m => m is PropertyInfo p && p.PropertyType == typeof(Step) 
                                                          || m is MethodInfo mi && mi.GetParameters().Length == 0 && mi.ReturnType == typeof(Step) && !mi.Name.StartsWith("get_"))
                                            .ToArray   ();
                
                if (steps.Length > 0)
                {
                    ButtonFoldout StepsFoldout               = new ButtonFoldout(); foldout.Add(StepsFoldout);
                    StepsFoldout.Button.style.maxWidth       = 120;
                    StepsFoldout.Text                        = $"STEPS ({steps.Count()})";
                    StepsFoldout.Expanded                    = false; // Closed by default
                    
                    foreach (var step in steps)
                    {
                        try
                        {
                            Button button                = new Button(); StepsFoldout.Content.Add(button);
                            button.text                  =  step.Name;
                            button.style.unityTextAlign  = TextAnchor.MiddleLeft;
                            button.clicked              += () =>
                                                           {
                                                               if (step is MethodInfo m)
                                                               {
                                                                   Step s;
                                                                   
                                                                   if (m.IsStatic)
                                                                     s = m.Invoke(null, null) as Step;
                                                                   else
                                                                     s = m.Invoke(target, null) as Step;
                                                                   
                                                                   s.Execute();
                                                               }
                                                               if (step is PropertyInfo p)
                                                               {
                                                                   Step s;
                                                                   if (p.GetMethod?.IsStatic ?? false)
                                                                       s = p.GetValue(null) as Step;
                                                                   else
                                                                       s = p.GetValue(target) as Step;
                                                                       
                                                                   s.Execute();
                                                               }
                                                           };
                            
                            bool isPublic = (step is MethodInfo methodInfo && methodInfo.IsPublic) || (step is PropertyInfo propertyInfo && propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsPublic);
                            if (!isPublic)
                                button.text += " (private)";
                            bool isStatic =  (step is MethodInfo methodInfoCheck && methodInfoCheck.IsStatic || step is PropertyInfo propertyInfoCheck && propertyInfoCheck.GetMethod?.IsStatic == true);
                            if (isStatic)
                                button.text += " (static)";
                        }
                        catch (Exception e)
                        {
                            TextField errorText = new TextField(); StepsFoldout.Add(errorText);
                            errorText.value = $"(error : {e.Message})";
                            errorText.label = $"method = [{step?.Name.ToSafeString()}]";
                        }
                    }
                }

                
                #endregion  // STEPS
            }
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Unity Editor
        #if UNITY_EDITOR
        
        public       EditorExtras editorExtras = new EditorExtras();
        public class EditorExtras
        {
            public string HeaderAttributeText;   
        }
        
        #endif // UNITY_EDITOR
        
    }    
}
