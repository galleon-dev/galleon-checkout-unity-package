using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Galleon.Checkout.Foundation;
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
    }

    public partial class EntityNode
    {
        ///////////////////////////////////////////////////////////////////////// Lifecycle
        
        public EntityNode(IEntity entity
                        ,[CallerMemberName] string callerName = ""
                        ,[CallerLineNumber] int    callerLine = 0
                        ,[CallerFilePath  ] string callerPath = "")
        {
            if (entity == null)
                throw new Exception("entity is null in EntityNode constructor");
            
            this.Entity      = entity;
            this.DisplayName = entity.GetType().Name;
            
            this.Breadcrumbs.Add(new Breadcrumb(callerName, callerLine, callerPath, displayName : "creation_breadcrumb"));
            
            Setup();
        }
        
        ///////////////////////////////////////////////////////////////////////// Setup
        
        public void Setup()
        {
            PopulatePredefinedChildren();
            
            #if UNITY_EDITOR
          //if (Application.isPlaying) 
            #endif
            {
                PopulateTestScenarios();
            }
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - General

        [SerializeReference] [HideInInspector] public IEntity Entity;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Info

        public EntityID ID   = new EntityID();
        public Tags     Tags = new Tags();
        
        public string DisplayName;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Breadcrumbs

        public List<Breadcrumb> Breadcrumbs = new();

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Testing
        
        public List<TestScenario> TestScenarios = new();
        
        public static string CurrentTestScenario = "scenario_1";
        
        protected void PopulateTestScenarios()
        {
            IEnumerable<TestScenario> scenarios = this.Reflection.TestScenarios();

            foreach (var scenario in scenarios)
            {
                scenario.Target = this.Entity;
                this.TestScenarios.Add(scenario);
            }
        }
        
        public async Task RunCurrentTestScenario()
        {
            await RunTestScenario(CurrentTestScenario);
        }
        
        public async Task RunTestScenario(string scenarioName)
        {
            var scenario = this.TestScenarios.First(x => x.Name == scenarioName);
            
            if (scenario == null)
                return;
            
            await scenario.RunScenario().Execute();
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Node

        [SerializeReference] public IEntity       Parent   = null;
        [SerializeReference] public List<IEntity> Children = new List<IEntity>();
        
        public List<WeakReference<IEntity>>       LinkedChildren { get; set; } = new();

        public IEnumerable<IEntity>               Ancestors()   => EnumerateUp  (this.Entity);
        public IEnumerable<IEntity>               Descendants() => EnumerateDown(this.Entity);

        public void SetParent(IEntity parent)
        {
            if (parent == this)
                return;

            this.Parent = parent;

            if (!parent.Node.Children.Contains(this.Entity))
                parent.Node.Children.Add(this.Entity);
        }

        public void AddChild(IEntity child)
        {
            this.Children.Add(child);
            child.Node.SetParent(this.Entity);
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
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Reflection
        
        public        EntityReflection Reflection => new(Entity);
        public struct EntityReflection
        {
            private IEntity Entity;
            public  EntityReflection(IEntity entity) => Entity = entity;
            
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
            
            public IEnumerable<TestScenario> TestScenarios()
            {
                var type = this.Entity.GetType();
                
                // Retrieve all properties and fields in the type that are of type TestScenario
                var members = type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                  .Where     (m => (m is FieldInfo    fi && fi.FieldType    == typeof(TestScenario)) 
                                                || (m is PropertyInfo pi && pi.PropertyType == typeof(TestScenario)));

                foreach (var member in members)
                {
                    var value = member switch
                    {
                        FieldInfo    field => field.GetValue(this.Entity),
                        PropertyInfo prop  => prop .GetValue(this.Entity),
                        _ => null
                    };

                    if (value is TestScenario scenario)
                    {
                        yield return scenario;
                    }
                }
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
                Foldout foldout = new Foldout() { text = $"{target.Node.DisplayName} entity" , value = false}; this.Add(foldout);
                foldout.Add(new Label(" "));
                
                // Breadcrumbs
                var breadcrumbsFolderout = new Foldout() { text = "Breadcrumbs", value = true }; foldout.Add(breadcrumbsFolderout);
                breadcrumbsFolderout?.Clear();
                foreach (var breadcrumb in Target.Node.Breadcrumbs)
                {
                    var breadcrumbInspector = new Breadcrumb.Inspector(breadcrumb, breadcrumb.DisplayName);
                    breadcrumbsFolderout.Add(breadcrumbInspector);
                }
            }
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Unity Editor
        #if UNITY_EDITOR
        
        public EditorExtras editorExtras = new EditorExtras();
        public class EditorExtras
        {
            public string HeaderAttributeText;
            
        }
        
        #endif // UNITY_EDITOR
        
    }
}