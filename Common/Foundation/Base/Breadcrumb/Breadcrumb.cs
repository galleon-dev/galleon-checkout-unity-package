using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout
{
    public class Breadcrumb
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public string       MemberName              = "";
        public int          LineNumber              = -1;
        public string       SourceFilePath          = "";
        public string       FileName                => Path.GetFileName(SourceFilePath);
            
        public string       InternalMemberName      = ""; 
        public int          InternalLineNumber      = 0; 
        public string       InternalFilePath        = "";
            
        public double       RealTimeSienceStartup   = 0;
        public DateTime     DateTime                = default;
        public int          FrameCount              = -1;
        public int          RenderedFrameCount      = -1;
        
        public List<string> ActiveSteps             = new();
        public string       ActiveStepsSTR          = "";
        
        public string       Note                    = "";
        public string       DisplayName             = "";

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public Breadcrumb(string                    memberName     = ""
                         ,int                       lineNumber     = 0
                         ,string                    sourceFilePath = ""
                         ,string                    note           = ""
                         ,string                    displayName    = "breadcrumb"
                         ,[CallerMemberName] string callerMember   = ""
                         ,[CallerLineNumber] int    callerLine     = 0
                         ,[CallerFilePath  ] string callerPath     = "")
        {
            #if UNITY_EDITOR
          //return;
            #endif
           
            this.MemberName            = memberName;
           this.LineNumber            = lineNumber;
           this.SourceFilePath        = sourceFilePath;
         //this.RealTimeSienceStartup = Time.realtimeSinceStartupAsDouble;
         //this.DateTime              = DateTime.Now;
         //this.FrameCount            = Time.frameCount;
         //this.RenderedFrameCount    = Time.renderedFrameCount;
           this.Note                  = note;
           this.DisplayName           = displayName;
           
            this.InternalMemberName    = callerMember;
            this.InternalLineNumber    = callerLine;
            this.InternalFilePath      = callerPath;
            
            
            if (memberName.IsNullOrEmpty() && lineNumber == 0 && sourceFilePath.IsNullOrEmpty())
            {
                this.MemberName     = callerMember;
                this.LineNumber     = callerLine;
                this.SourceFilePath = callerPath;
            }
            
            // // Active steps at time of creation of breadcrumb
            // if (StepManager.Instance != null)
            // {
            //     foreach (var s in StepManager.Instance.ActiveSteps)
            //         ActiveSteps.Add(s?.Name.ToSafeString() + " (" + s?.InstanceID.ToSafeString() + ")");
            // }
            // ActiveStepsSTR = string.Join("\n", ActiveSteps);
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Utility Methods
        
        public string ToShortString()  => $"<color=#00FFFF>{FileName}</color>.<b>{MemberName}</b>:<color=yellow>{LineNumber}</color>";
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Editor Methods
        
        public void OpenCodeAtBreadcrumb()
        {
            #if UNITY_EDITOR
            
            try
            {
                // Load asset
                string scriptPath = FileUtil.GetProjectRelativePath(this.SourceFilePath);
                var    asset      = AssetDatabase.LoadAssetAtPath<Object>(scriptPath);
                
                // Open asset
                AssetDatabase.OpenAsset(asset, lineNumber:this.LineNumber);

            }
            catch (Exception e)
            {
                Debug.Log("<color=red>Cannot open this breadcrumb</color>");
            }
            
            #endif // UNITY_EDITOR
        }
        
        ////////////////////////////////////////////////
        
        public class Inspector : Inspector<Breadcrumb>
        {
            // members
            Breadcrumb breadcrumb;
            string     displayName;
            
            // UI
            Button        button;
            Foldout       foldout;
            VisualElement content;

            public Inspector(Breadcrumb breadcrumb, string displayName) : base(breadcrumb)
            {
                this.breadcrumb  = breadcrumb;
                this.displayName = displayName;
                
                // Foldout
                foldout = new Foldout(); this.Add(foldout);
                foldout.text = this.displayName.Replace("_breadcrumb", "");
                
                Toggle  toggle                       = foldout.Q<Toggle>();
                toggle.style.flexGrow                = 1;
                var defaultElement                   = toggle.Children().FirstOrDefault();
                defaultElement.style.flexGrow        = 0;
                foldout.value                        = false; // Close by default;
                Label foldoutLabel                   = toggle.Q<Label>();
                foldoutLabel.style.minWidth          = 120;
                
                // Main Button 
                button                               = new Button(); toggle.Add(button);
                button.text                          = this.breadcrumb?.ToShortString() ?? "";
                button.style.unityTextAlign          = TextAnchor.MiddleLeft;
                button.style.paddingLeft             = 10;
                button.style.fontSize                = 11; // default is 12 // be just a little smaller than the default 
                button.style.marginLeft              = 4;
                button.style.flexGrow                = 1;
                button.clicked                      += OnButtonClicked; void OnButtonClicked()
                {
                    Debug.Log($"Opening code at breadcrumb : {displayName}");
                    
                    #if UNITY_EDITOR
                    this.breadcrumb?.OpenCodeAtBreadcrumb();
                    #endif
                }
                
                // Content
                content = new VisualElement(); foldout.Add(content);
                if (breadcrumb != null)
                {
                    var members = breadcrumb.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                                      .Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property);

                    foreach (var member in members)
                    {
                        try
                        {
                            var memberName  = member.Name;
                            var memberValue = member.MemberType switch { MemberTypes.Field    => ((FieldInfo   )member).GetValue(breadcrumb),
                                                                         MemberTypes.Property => ((PropertyInfo)member).GetValue(breadcrumb), };
                            
                            
                            TextField text = new TextField(); content.Add(text);
                            text.SetEnabled(false);
                            text.multiline = true;
                            text.label     = memberName;
                            text.value     = memberValue?.ToString() ?? ""; 
                        }
                        catch (Exception e)
                        {
                        }
                    }
                }
            }
        }
        
    }   
}
