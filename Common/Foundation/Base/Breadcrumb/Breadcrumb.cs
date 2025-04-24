using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

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
            this.MemberName            = memberName;
            this.LineNumber            = lineNumber;
            this.SourceFilePath        = sourceFilePath;
            this.RealTimeSienceStartup = Time.realtimeSinceStartupAsDouble;
            this.DateTime              = DateTime.Now;
            this.FrameCount            = Time.frameCount;
            this.RenderedFrameCount    = Time.renderedFrameCount;
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
        
    }   
}
