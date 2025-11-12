using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout.Foundation;
using UnityEngine;

namespace Galleon.Checkout
{
    [Element("element")]   
    public partial class Element : Entity
    {
        //// Members
        
        public string Name;

        //// Lifecycle
        
        public Element(string name)
        {
            this.Name = name;
        }
    }
    
    public class DefinitionNode : Entity
    {
        public string Value = "";
        
        public bool IsNamespace => Value.StartsWith("(") && Value.EndsWith(")");
    }
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    [AttributeUsage(AttributeTargets.All)]
    public class ElementAttribute : Attribute
    {
        public string ElementName;
        
        public ElementAttribute(string ElementName)
        {
            this.ElementName = ElementName;
        }
    }
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region BS
    
    public class ScanHandler
    {
        public object target;
        
        public delegate void OnItemScannedDelegate(object parent, string itemCategory, object item);
        public static event  OnItemScannedDelegate OnItemScanned; 
        
        public virtual void Scan()
        {
        }
        
        public void EmitScannedItem(object parent, string itemCategory, object item)
        {
            OnItemScanned?.Invoke(parent, itemCategory, item);
        }
        
        public virtual void Register()
        {
        }
    }
    public class ScanHandler<T> : ScanHandler where T : class
    {
        public new T target
        {
            get => base.target as T;
            set => base.target = value;
        }
    }
    
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    public class CrudHandler
    {
        public object target; 
        public virtual void Create()    {}
        public virtual void Delete()    {}
        public virtual bool DoesExist() { return default; }
        
        public virtual void Update(string path, object value)
        {
            DynamicExpression.SetValue(origin:target, expression:path, value:value);
        }
        
        public virtual void OnAddedToParent(IEntity Parent) {}
    }
    public class CrudHandler<T> : CrudHandler where T : class
    {
        public new T target
        {
            get => base.target as T;
            set => base.target = value;
        }
    }
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    
    public partial class EntityNode
    {
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Print
     
        
        public        PRINTING Printing => new(Entity);
        public struct PRINTING
        {
            IEntity Entity;
            public PRINTING(IEntity entity) => this.Entity = entity;
            
            public async void Print(string id, string text)
            {
                var   op = new PrintOperation(ID:id, parent:Entity, text:text);
                await op.Print().Execute();   
            }
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Live 
     
        public        LIVE2 Live2 => new(Entity);
        public struct LIVE2
        {
            IEntity Entity;
            public LIVE2(IEntity entity) => this.Entity = entity;
            
            public void Plus(IEntity entity)
            {
                entity.Node.IsCrudDraft = true;
                this.Entity.Node.AddChild(entity);
                entity.Node.Crud.OnAddedToParent(parent : this.Entity);
                entity.Node.Crud.Create();
                entity.Node.IsCrudDraft = false;
            }
            public void Minus()
            {
                Entity.Node.Crud.Delete();
                Entity.Node.ParentNode.RemoveChild(Entity);
            }
            public void Edit(string path, string value)
            {
                Entity.Node.Crud.Update(path, value);
            }
            
            ////////////////////////////////////////////////
            
            public async Task CRUD_PLUS(IEntity entity)
            {
                this.Entity.Node.AddChild(entity);
                entity.Node.Crud.OnAddedToParent(parent : this.Entity);
                entity.Node.Crud.Create();
            }
            public async Task STEP_PLUS(IEntity entity)
            {
                var Parent = this.Entity;
                
                Step plus = new Step(name : "plus"
                                    ,action : async s =>
                                    {
                                        s.AddChildStep(name   : "add_child"
                                                      ,action : async x =>
                                                              {
                                                                  Parent.Node     .AddChild       (child  : entity);
                                                                  Parent.Node.Crud.OnAddedToParent(parent : Parent);
                                                              });
                                        
                                        s.AddChildStep(name    : "create"
                                                      ,action : async x =>
                                                                {
                                                                    entity.Node.Crud.Create();
                                                                });
                                    });
                
                await plus.Execute();
            }
            public async Task OP_PLUS(IEntity entity)
            {
                var Parent = this.Entity;
                var op     = new Operation(ID:"op_id");
                
                op.Flow.AddChildStep(name   : "add_child"
                                    ,action : async x =>
                                            {
                                                Parent.Node     .AddChild       (child  : entity);
                                                Parent.Node.Crud.OnAddedToParent(parent : Parent);
                                            });
                
                op.Flow.AddChildStep(name   : "create"
                                    ,action : async x =>
                                            {
                                                entity.Node.Crud.Create();
                                            });
            }
            public async Task P_OP_PLUS(IEntity entity)
            {
                var op = new PrintOperation(ID     : "op_id"
                                           ,parent : this.Entity
                                           ,text   : "op_text");;
            }
            public async Task LIVE_PLUS(IEntity entity)
            {
                
            }
        }    
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Element
     
        public        ELEMENT Element => new(Entity);
        public struct ELEMENT
        {
            IEntity Entity;
            public ELEMENT(IEntity entity) => this.Entity = entity;
            
            // Definitions
            public IEntity GetDefinition()   { return default; }
            public IEntity GetElement()      { return default; }
            public IEntity GetAssetsFolder() { return default; }
            
            // Scan
            public void Scan() {}
            
            // CRUD
            public void Create() {}
            public void Delete() {}
            public void Update() {}
            public void OpenForEdit()  {}
            public void CloseForEdit() {}
            
            // Live
            public void Plus()   {}
            public void Minus()  {}
            public void Equals() {}
            
            // Print
            public void Print() {}
            
            // Extras
            public object MCVParent() => default;
            
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - Scan
        
        public        SCAN Scan => new(Entity);
        public struct SCAN
        {
            IEntity Entity;
            public SCAN(IEntity entity) => this.Entity = entity;
            
            public bool SupportsScan => Entity.GetType().GetNestedTypes().Any(x => x.IsSubclassOf(typeof(ScanHandler)));
            
            public ScanHandler GetScanHandler()
            {
                if (!SupportsScan)
                    throw new Exception($"type {Entity.GetType().FullName} does not support Scan");
                
                var type            = Entity.GetType();
                var scanType        = type.GetNestedTypes().FirstOrDefault(x => x.IsSubclassOf(typeof(ScanHandler)));
                var scanInstance    = Activator.CreateInstance(scanType, true) as ScanHandler;
                scanInstance.target = Entity;
                
                return scanInstance;
            }
            
            public void Register()
            {
                var scanHandler = GetScanHandler();
                scanHandler.Register();
            }
            public void ScanSelf()
            {
                var scanHandler = GetScanHandler();
                scanHandler.Scan();
            }
            public void ScanRecursive()
            {
                this.ScanSelf();
                
                var children = this.Entity?.Node?.Children;
                if (children == null) return;

                foreach (var child in children)
                {
                    if (child == null) continue;
                    child.Node.Scan.ScanRecursive();
                }
            }
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Aspect - self CRUD
        
        public bool   IsCrudDraft = true;
        public string CRUDCommonName;
        
        public        CRUD Crud => new(Entity);
        public struct CRUD
        {
            public IEntity Entity;
            public CRUD(IEntity entity) => this.Entity = entity;
            
            public bool isDraft => Entity.Node.Tags.Contains("crud_draft") || Entity.Node.IsCrudDraft;
            
            public bool SupportsCRUD => Entity.GetType().GetNestedTypes().Any(x => x.IsSubclassOf(typeof(CrudHandler)));
            
            public void Create()
            {
                GetCrudHandler().Create();
            }
            
            public void OnAddedToParent(IEntity parent)
            {
                GetCrudHandler().OnAddedToParent(parent);
            }
            
            public void Delete()
            {
                GetCrudHandler().Delete();
            }
            
            public void Update(string path, string value)
            {
                GetCrudHandler().Update(path, value);
            }
            
            public bool DoesExist()
            {
                return GetCrudHandler().DoesExist();
            }
            
            public void OpenForEdit()  {}
            public void CloseForEdit() {}
            
            ////////////////////////////// Helpers
            
            public CrudHandler GetCrudHandler()
            {
                if (!SupportsCRUD)
                    throw new Exception($"type {Entity.GetType().FullName} does not support CRUD");
                
                var type            = Entity.GetType();
                var crudType        = type.GetNestedTypes().FirstOrDefault(x => x.IsSubclassOf(typeof(CrudHandler)));
                var crudInstance    = Activator.CreateInstance(crudType, true) as CrudHandler;
                crudInstance.target = Entity;
                
                return crudInstance;
            }
        }
        
            
    } // end of ENTITY
    
    #endregion // BS
}

