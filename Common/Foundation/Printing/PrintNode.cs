using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Galleon.Checkout;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout.Foundation
{
    public class PrintNode : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public TextNode TextNode { get; set; }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties
        
        public PrintNode RootPrintNode => this.Node.Ancestors().OfType<PrintNode>().Last();
        
        public string NodePrintPhase => "default";
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Static Parse
        
        public static PrintNode Parse(string text)
        {
            var textNode  = TextNode.Parse(text);
            var printNode = PrintNode.Parse(textNode);
            return printNode;
        }
        public static PrintNode Parse(TextNode textNode)
        {
            return ParseTextNode(textNode);
            
            PrintNode ParseTextNode(TextNode _txtNode)
            {
                PrintNode printNode = new PrintNode();
                printNode.TextNode  = _txtNode;

                foreach (var childTextNode in _txtNode.Node.Children.OfType<TextNode>())
                {
                    var childPrintNode = ParseTextNode(childTextNode); 
                    printNode.Node.AddChild(childPrintNode);
                }
                
                return printNode;
            }
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// State
        
        public static readonly string PRINT_STATE_FILE_PATH = System.IO.Path.Combine(Application.dataPath, "TEMP", "printState.txt");
        
        public static State PrintState = new();
        
        public enum PrintPhase
        {
            None,
            Assets,
            Refresh,
            Hierarchy,
        }
        [Serializable]
        public class State
        {
            public PrintPhase Phase { get; set; } = PrintPhase.None;
            
            public static void Save()
            {
                var    settings = new JsonSerializerSettings { Converters = { new StringEnumConverter() } };
                string json     = JsonConvert.SerializeObject(PrintState, settings);
                File.WriteAllText(PRINT_STATE_FILE_PATH, json);
            }
            
            public static void Load()
            {
                try
                {
                    var settings = new JsonSerializerSettings { Converters = { new StringEnumConverter() } };
                    var json     = File.ReadAllText(PRINT_STATE_FILE_PATH);
                    PrintState   = JsonConvert.DeserializeObject<State>(json, settings);
                }
                catch (FileNotFoundException e)
                {
                    // Do Nothing
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Print Test
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        
        #if UNITY_EDITOR
        [MenuItem("Tools/Galleon/Test Print Node THING")]
        public static async Task TestPrintThing()
        {
            string text    = "> ThingElement t";
            Debug.Log($"Printing :'{text}'");
            
            PrintNode root = PrintNode.Parse(text);
            
            Debug.Log($"Parsed : {root.TextNode.Line}");
            await root.Print();
        }
        #endif
        
        #if UNITY_EDITOR
        [MenuItem("Tools/Galleon/Test Print Node Start")]
        public static async Task TestPrintStart()
        {
            PrintState.Phase = PrintPhase.None;
            State.Save();
            
            #if UNITY_EDITOR
            AssetDatabase.StartAssetEditing();
            #endif
            
            Debug.Log("Test Print Start");
            
            PrintState.Phase = PrintPhase.Assets;
            State.Save();
            
            PrintNode printNode = new PrintNode();
            printNode.CreateScriptAsset();
            
            PrintState.Phase = PrintPhase.Refresh;
            State.Save();
            
            #if UNITY_EDITOR
            AssetDatabase.StopAssetEditing();
            #endif
            
            printNode.RefreshAssetDataBase();
        }
        [MenuItem("Tools/Galleon/Test Print Node Resume")]
        public static async Task TestPrintResume()
        {
            Debug.Log("Test Print Resume");
            
            PrintState.Phase = PrintPhase.Hierarchy;
            State.Save();
            
            PrintNode printNode = new PrintNode();   
            printNode.CreateGO();
            printNode.AttachComponentToGameObject();
        }
        #endif

        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Print Test Actions
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public string ScriptAssetPath = System.IO.Path.Combine(Application.dataPath, "TEMP", "TempThing.cs");
        public string GameObjectName = "TempThingGO";
        
        public async Task CreateScriptAsset()
        {
            System.IO.File.WriteAllText(ScriptAssetPath 
                                       ,$@"using UnityEngine;

                                       public class TempThing : MonoBehaviour
                                       {{
                                           void Start()
                                           {{
                                               Debug.Log(""Hello From TempThing"");
                                           }}
                                       }}");
        }
        
        public async Task RefreshAssetDataBase()
        {
            #if UNITY_EDITOR
            AssetDatabase.Refresh();
            #endif
        }
        
        public async Task CreateGO()
        {
            GameObject newGameObject = new GameObject(GameObjectName);
        }
        
        public async Task AttachComponentToGameObject()
        {
            // Find the GameObject by name
            GameObject targetGameObject = GameObject.Find(GameObjectName);
            if (targetGameObject == null)
            {
                Debug.LogError($"GameObject with name '{GameObjectName}' not found.");
                return;
            }
        
            // Load the compiled script type
            Type scriptType = Type.GetType("TempThing");
            if (scriptType == null)
            {
                Debug.LogError("Script type 'TempThing' not found.");
                return;
            }
        
            // Attach the component to the GameObject
            targetGameObject.AddComponent(scriptType);
            Debug.Log($"Component 'TempThing' attached to GameObject '{GameObjectName}'.");
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle Events
        
        #if UNITY_EDITOR
        [InitializeOnLoadMethod]
        #endif
        private static async void InitOnLoad()
        {
            //Logger.Log("Remote03.PrintService.InitOnLoad()");
          
            Debug.Log("PrintNode.InitOnLoad()");
            
            try
            {
                #if UNITY_CLOUD_BUILD
                return;
                #endif
                
                State.Load();
                
                bool isAwaitingAssetRefresh = PrintState.Phase == PrintPhase.Refresh;
                if (isAwaitingAssetRefresh)
                {
                    // Log
                    Debug.Log($"PrintNode.InitOnLoad() : resuming print operation, because printState is {PrintState.Phase}");
                    
                    // Do nothing if we are in build,
                    // the Builder will call the ContinuePrintingAfterAssetRefresh()
                    bool didPrintStartFromBuild = false; 
                  //if (State.Instance.PrintStartedFromBuild)
                    if (didPrintStartFromBuild)
                    {
                        Debug.Log("returning because print started from build");
                        return;
                    }
                    
                    #if UNITY_EDITOR
                    if (BuildPipeline.isBuildingPlayer)
                    {
                        Debug.Log("returning building player");
                        return;
                    }
                    #endif
                    
                    // Resume print
                    #if UNITY_EDITOR
                    await TestPrintResume();
                    #endif
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helpers
        
        public static List<Type> AllRegisteredTypes = new();
        public void InitTypes()
        {
            if (AllRegisteredTypes.Count != 0)
                return;
            
            var types = AppDomain.CurrentDomain.GetAssemblies()
                                 .SelectMany(x => x.GetTypes())
                               //.Where(x => typeof(Entity).IsAssignableFrom(x) && x != typeof(Entity))
                                  ;
            
            AllRegisteredTypes.AddRange(types);
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Print
        
        public async Task Print()
        {
            InitTypes();
            
            foreach (var node in this.Node.Descendants().OfType<PrintNode>())
            {
                await node.PrintSelf();
            }
        }
        
        public async Task ResumePrint()
        {
            InitTypes();

            foreach (var node in this.Node.Descendants().OfType<PrintNode>())
            {
                await node.PrintSelf();
            }
        }
        
        public async Task PrintSelf()
        {
            Debug.Log($"- printing node {TextNode.Line}");
            
            if (TextNode.Line == "> root")
                return;
            
            
            var elementTypeName = this.TextNode.LineWords.First();
            var elementType     = AllRegisteredTypes.FirstOrDefault(x => x.Name == elementTypeName);
            var element         = Activator.CreateInstance(elementType);

            
            if (element is not IPrintable)
                return;
            
            var printable = (IPrintable)element;
            await printable.OpenDependenciesEdit(this);
            await printable.Print(this);
            await printable.CloseDependenciesEdit(this);    
        }
    }
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Temp - IPrintable
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    
    public interface IPrintable
    {        
        public Task OpenDependenciesEdit(PrintNode printNode);
        public Task CloseDependenciesEdit(PrintNode printNode);
        
        public Task OpenSelfForEdit(PrintNode printNode);
        public Task CloseSelfForEdit(PrintNode printNode);
        
        public Task Print(PrintNode printNode);
    }
}

public class OpNode : Entity
{
    public string       op_id;
    public string       op_instructions;
    public int          op_state;
    
    public OpNode[]     StoredOperations => new [] { new OpNode() };
    
    
    public List<Phase>  phases = new List<Phase>();
    public class        Phase
    {
        public string       phase;
        public List<Action> PhaseActions;
    }
    
    public List<Action> actions = new List<Action>();
    
    ////////////////////////////////////////////////////////////////////// Actual actions
    
    void doThing_1()
    {
        // do the thing
        
        RefreshAssetDB();
    }
    
    void do_thing_2()
    {
        
    }
    
    ////////////////////////////////////////////////////////////////////// Lifecycle
    
    private void StartOrResume()
    {
        string currentPhase = op_state.ToString();
        Phase p             = phases.First(x => x.phase == currentPhase);
        
        this.actions.AddRange(p.PhaseActions);
    }
    
    ////////////////////////////////////////////////////////////////////// Helpers
    
    void RefreshAssetDB(){}
    
    void Save() {}
    void Load() {}
    
    void InitOnLoad() {OnReload();}
    void OnReload()
    {
        foreach (var op in StoredOperations)
        {
            op.StartOrResume();
        }
    }
    
    /// Assets.Node.Print("> Folder f1"); 
    /// =>
    ///     PrintNode.NewPrintOperation(test : raw_text, parent : Assets)
    ///     =>
    ///         op.state        = "start";
    ///         op.instructions = raw_text;
    ///         op.parentEntity = Assets;
    ///         op.id           = parent.id + date_time; 
    ///
    ///         op.StartOrResume()
    ///         =>
    ///             phase p = op.Phases[op.State];
    ///
    ///             foreach action in p.actions
    ///                 action.Execute
    ///                 state.Update
    ///                 op.Save 
}
