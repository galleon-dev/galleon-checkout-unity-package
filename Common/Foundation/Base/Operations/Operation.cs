using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout.Assets;
using Galleon.Checkout.Foundation;
using Newtonsoft.Json;
using UnityEngine;
using Random = System.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout.Foundation
{
    public class Operation : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public OPERATION_STATE OperationState = new OPERATION_STATE();
        public Step            Flow;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties
        
        public string ID
        {
            get => OperationState.Op.ID;
            set => OperationState.Op.ID = value;
        }
        
        public Dictionary<string, object> Data
        {
            get => OperationState.Data;
            set => OperationState.Data = value;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// State
        
        public string OperationStateFileRelativePath => $"TEMP/operation_{ID}_state.json";
        
        [Serializable] 
        public class OPERATION_STATE
        {
            public OperationData              Op   = new OperationData();
            public Dictionary<string, object> Data = new();
        }
        [Serializable]
        public class OperationData
        {
            public string                     ID               { get; set; } = "";
            public List<string>               CompletedStepIDs { get; set; } = new();
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public Operation(string ID)
        {
            this.ID                       = ID;
            this.Flow                     = new Step(name: $"operation_{ID}");
            this.Flow.PreChildStepAction  = (completedChildStep) =>
                                          {       
                                              this.OperationState.Op.CompletedStepIDs.Add(completedChildStep.Name);
                                              this.Save();
                                              Root.Instance.Context.Operations.Save();
                                          };
            this.Flow.PostStepAction      = (completedStep) =>
                                          {
                                              Root.Instance.Context.Operations.OngoingOperations.Remove(ID);
                                              Root.Instance.Context.Operations.Save();
                                          };
        }
        
        public Operation AddStep(Step step)
        {
            this.Flow.AddChildStep(step);
            return this;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// API
        
        public async Task Execute()
        {
            Root.Instance.Context.Operations.OngoingOperations.Add(ID);
            
            await Flow.Execute();
        }
        
        public async Task Resume()
        {
            await Flow.Execute();
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Events
        
        #if UNITY_EDITOR
        [InitializeOnLoadMethod]
        #else
        [RuntimeInitializeOnLoadMethod]
        #endif
        public static void InitializeOnLoad()
        {
            // Debug.Log("OperationController.InitializeOnLoad()");
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Storage
        
        public void Save()
        {
            var json = JsonConvert.SerializeObject(this.OperationState);
            var path = Path.Combine(Application.dataPath, OperationStateFileRelativePath);
            File.WriteAllText(path, json);
        }
        
        public static Operation Load(string ID)
        {
            var operation = Root.Instance.Context.Operations.AllRegisteredOperations.Single(x => x.ID == ID);
            var path      = Path.Combine(Application.dataPath, operation.OperationStateFileRelativePath);

            if (!File.Exists(path))
                throw new Exception($"operation {ID} state file not found at path: " + path);

            var json                 = File.ReadAllText(path);
            operation.OperationState = JsonConvert.DeserializeObject<OPERATION_STATE>(json);

            foreach (var step in operation.Flow.ChildSteps)
            {
                if (operation.OperationState.Op.CompletedStepIDs.Contains(step.Name))
                    step.StepState = Step.STEP_STATE.PreviouslyCompleted;
            }
            
            return operation;
        }
        
        public async void Test()
        {
            /// a.crud_plus(f1)
            ///     add_child()
            ///     create()
            ///
            /// a.op_plus(f1)
            ///     new step("plus")
            ///         new step("add_child")
            ///         new step("create")
            ///
            
            await this.Node.Live2.CRUD_PLUS(new Folder() {FolderName = "f1"});
            await this.Node.Live2.STEP_PLUS(new Folder() {FolderName = "f1"});
            await this.Node.Live2.P_OP_PLUS(new Folder() {FolderName = "f1"});
            await this.Node.Live2.OP_PLUS  (new Folder() {FolderName = "f1"});
            await this.Node.Live2.LIVE_PLUS(new Folder() {FolderName = "f1"});
        }
    }
    
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    
    public class LiveOperation
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public LiveNode OriginalTree;
        public LiveNode ChildVirtualTree;
        public LiveNode ParentFullVirtualTree;
        
        public TextNode OriginalTextNode;
        
        public string   ID;
        public IEntity  Parent;
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public LiveOperation(string id, IEntity parent, LiveNode definition)
        {
            this.ID                     = id;
            this.Parent                 = parent;
            this.OriginalTree           = definition;
            this.OriginalTree.Operation = this;
        }
        public LiveOperation(string id, IEntity parent, string text)
        {
            this.ID                     = id;
            this.Parent                 = parent;
            this.OriginalTextNode       = TextNode.Parse(text).Node.Children.OfType<TextNode>().First(); // first one under "root"
            this.OriginalTree           = new LiveNode();
                                        {
                                            this.OriginalTree.TextNode = this.OriginalTextNode;
                                        }
            this.OriginalTree.Operation = this;
                                        
            this.ChildVirtualTree       = OriginalTree.CloneTree();
            
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public void SaveWithNode(){}
        public void LoadWithNode(){}
        
        public async Task Execute()
        {
            await CreateVirtualTree();
            await CreateActualContent();
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// WIP
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// WIP
        
        ////
        
        /// foreach node in full_v_tree
        ///     if (node needs action)
        ///         node.DoAction()
        ///         (plus) =>
        ///             var parent = node.getParent();
        ///             var child  = node.getElement().CreateActualChild();
        /// 
        ///             parent.AddChild(child);
        ///             child.Create();
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
        
        public async Task CreateVirtualTree()
        {
            // Create virtual tree
            ChildVirtualTree = new LiveNode();
            
            // With Parent
            LiveNode Parent = new LiveNode();
            ChildVirtualTree.Node.AddChild(Parent);
            
            // Find categories
            List<string> categories = new();
            foreach (var child in OriginalTree.Node.Descendants().OfType<LiveNode>())
                if (child.ToString() == "(category)")
                    categories.Add(child.ToString());
            
            // Clone Categories sub-trees
            foreach (var category in categories)
            {
                var originalSubTreeRoot = OriginalTree.Node.Descendants().OfType<LiveNode>().FirstOrDefault(x => x.ToString() == category);
                var clonedTreeRoot      = originalSubTreeRoot.CloneTree();
                var TargetTreeRoot      = Parent;

                TargetTreeRoot.Node.AddChild(clonedTreeRoot);
            }
            
            // Remove irrelevant nodes per category
            foreach (var lineNode in ChildVirtualTree.Node.Descendants().OfType<LiveNode>())
            {
                // Get Category
                var category = lineNode.GetCategoryName();
                if (category == null) continue;
                
                // is supported in category
                var element      = lineNode.GetElement();
                var isInCategory = element.SupportsCategory(category);
                
                // Remove node if not in category
                if (!isInCategory)
                {
                    // Get Parent
                    var parent = lineNode.Node.Parent as LiveNode;
                    
                    // Move all of my children to my parent
                    foreach (var child in lineNode.Node.Children)
                        parent.Node.AddChild(child);
                    
                    // remove this node
                    parent.Node.RemoveChild(lineNode);
                }
            }
        }
        
        public async Task CreateActualContent()
        {
            var virtualTreeRoot = ChildVirtualTree;

            foreach (var liveNode in virtualTreeRoot.Node.Descendants().OfType<LiveNode>())
            {
                if (liveNode.IsDraft())
                {
                    var virtualParent = liveNode.Node.Parent as LiveNode;
                    var actualParent  = virtualParent.GetActualEntity().Node;
                    
                    var actualNode = liveNode.GetElement().CreateActualNode();
                    actualParent.AddChild(actualNode);
                    actualNode.Node.Crud.Create();
                }
            }
        }
    }
    
    
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
     
    
    public partial class LiveNode : Entity
    {   
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public TextNode TextNode = new TextNode();
        public string   Text
        {
            get => TextNode.FullText;
            set => TextNode.RawText = value;
        }
        
        public string TargetText;
        public string ActionText;
        
        public LiveOperation Operation;
        
        public IEntity LinkedCreatedEntity;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Main Action
        
        public Step DoAction() 
        =>
            new Step(name   : $"DoAction"
                    ,action : async (s) =>
                    {
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lave Load
        
        public        string   Save ()            { return "string"; }
        public static LiveNode Parse(string text) { return default;  }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// API Methods
        
        public void DoPlusCreateAfterVTree()
        {
                        
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// WIP
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// WIP
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// WIP
        
        /// [plus1]   -> simple-op + 1 node + direct-action.
        /// [plus1e]  -> plus1 + element + live + node + code + fake vtree.
        ///     [AF + EF]
        ///     [EF + EF]
        /// [plus1ev] -> plus1e + vtree.
        /// [...]     -> test. save-load. 2 nodes. n nodes. suger/refs.
        
        /// APFE1 (AF + EF1)
        ///     Assets.plusButton
        ///         Assets.Node.Plus('f1')
        ///             new Op("f1").Execute()
        ///                 CreateVTree()
        ///                     Node "f1" should become : "f1" > "A.F1"
        ///                         GetElement(F)."CopeyTreeToParent()"
        ///                 ApplyVTree()
        ///                     Foreach actual node => actualNode.Plus
        
        /// PPFE1 (EF + EF1)
        ///     Package.PlusButton
        ///         P.Element.Plus('f1')
        ///             ... 
        
        /// PPF
        ///     real element + real vtree
        
        /// split 12
        
        /// 1 = >F1>F11>F12 , Save+Load, Scan+Minus+Equals, etc.
        /// 2 = s + go 
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// WIP
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// WIP
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// WIP
        

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Category Methods
        
        public bool IsCategoryNode()
        {
            if (Text.Trim().StartsWith("(") 
            &&  Text.Trim().EndsWith(")"))
            {
                return true;
            }
            
            return false;
        }
        
        public string GetCategoryName()
        {
            if (IsCategoryNode())
            {
                return Text.Trim().Substring(1, Text.Trim().Length - 2);
            }
            
            return "";
        }
        
        public string GetNodeCategory()
        {
            foreach (var parentNode in this.Node.Ancestors().OfType<LiveNode>())
            {
                if (parentNode.IsCategoryNode())
                    return parentNode.GetCategoryName();
            }
            
            return null;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Real Tree Methods
        
        public bool IsDraft()
        {
            return true;
        }
        
        public Entity GetActualEntity()
        {
            return null;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Clone Methods
        
        public LiveNode CloneNode()
        {
            var clone        = new LiveNode();
            clone.TextNode   = TextNode.CloneNode();
            clone.TargetText = new string(TargetText);
            clone.ActionText = new string(ActionText);
            clone.Operation  = Operation;
            return clone;
        }
        
        public LiveNode CloneTree()
        {
            var nodeMap   = new Dictionary<LiveNode, LiveNode>();
            var clone     = this.CloneNode();
            nodeMap[this] = clone;

            foreach (var descendant in this.Node.Descendants().OfType<LiveNode>())
            {
                var descendantClone = descendant.CloneNode();
                nodeMap[descendant] = descendantClone;

                var parent = descendant.Node.Parent as LiveNode;
                if (parent != null && nodeMap.ContainsKey(parent))
                {
                    nodeMap[parent].Node.AddChild(descendantClone);
                }
            }

            return clone;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Element
        
        public LiveElement GetElement()
        {
            return new LiveElement();
        }
    }
    
    
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    /// /// ///
    
    
    public class LiveElement
    {
        public bool SupportsCategory(string categoryName)
        {
            return new Random().Next(2) == 1;
        }
        
        public Entity CreateActualNode()
        {
            return default;
        }
    }
    
}

namespace Galleon.Checkout
{
    namespace Foundation
    {
        partial class LiveNode
        {
            public bool APFE_DoesNeedToDoAction = false;
            public bool PPFE_DoesNeedToDoAction => !ActuallyExists;
            public bool ActuallyExists = false;
        }
    }
    
}

/// > Elements
///     > Element
///     > Package
///     > (Assets)
///         > Folder
///     > (Hierarchy)
///         > Scene