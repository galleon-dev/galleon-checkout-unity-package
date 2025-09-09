using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.Checkout.Foundation
{
    public class FolderAsset : Asset
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Consts
        
        public static readonly string PackageRootFolderPath = Application.dataPath + "/" + "package1/";
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
         public string FolderName   { get; set; } = "new_folder";
                                    //{
                                    //    get => System.IO.Path.GetFileName(Path);
                                    //    set => Rename(value); 
                                    //}
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// CRUD Actions
        
        public bool DoesFolderExist()
        {
            return Directory.Exists(Path);   
        }
        
        public void CreateFolder()
        {
            Directory.CreateDirectory(Path);
        }
        
        public void DeleteFolder()
        {
            #if UNITY_EDITOR
            if (File.Exists($"{Path}.meta"))
                File.Delete($"{Path}.meta");
            #endif
            
            Directory.Delete(Path);
        }
        
        public void OnAddedToParent(IEntity parent)
        {
            if (parent is not Asset parentAsset)
                throw new Exception("FolderElement.OnAddedToParent: Parent is not Asset");

            if (this.Node.CRUDCommonName != null)
                this.FolderName = this.Node.CRUDCommonName;
            
            this.Path = System.IO.Path.Combine(parentAsset.Path, this.FolderName);
        }
        
        public void Rename(string folderName)
        {
            //Debug.Log($"Renaming {FolderName} to {folderName}");
            
            //////////////////////////////////////////
            
            var oldPath = Path;
            var newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(oldPath) ?? string.Empty, folderName);
            
            if (Directory.Exists(newPath))
                Debug.Log("Destination already exists!");
            
            //Debug.Log($"old path = {oldPath}");
            //Debug.Log($"new path = {newPath}");
            
            Directory.Move(oldPath, newPath);
            
            //Debug.Log($"Exists1 = {Directory.Exists(oldPath)}");
            //Debug.Log($"Exists2 = {Directory.Exists(newPath)}");
            
            this.Path = newPath;
            
            //////////////////////////////////////////
            
            // Rename the meta file as well
            var oldMetaPath = $"{oldPath}.meta";
            var newMetaPath = $"{newPath}.meta";
            
            if (File.Exists(oldMetaPath))
            {
                File.Move(oldMetaPath, newMetaPath);
                //Debug.Log($"Renamed meta file from {oldMetaPath} to {newMetaPath}");
            }
            else
            {
                //Debug.LogError($"Meta file not found at {oldMetaPath}, skipping rename.");
            }
            
            //////////////////////////////////////////
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// CRUD Handler
        
        public class FolderCrudHandler : CrudHandler<FolderAsset>
        {
            public override void Create()                          => target.CreateFolder();
            public override void Delete()                          => target.DeleteFolder();
            public override void OnAddedToParent(IEntity Parent)   => target.OnAddedToParent(Parent);
            public override bool DoesExist()                       => target.DoesFolderExist();
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Scan Handler
        
        public class FolderScanHandler : ScanHandler<FolderAsset>
        {
            public override void Scan()
            {
                // Debug.Log($"-> scanning {this.target.GetType().Name}");
                
                var folder       = target;
                var path         = folder.Path;
                
                if (path == null)
                    return;
                
                var childFolders = Directory.EnumerateDirectories(folder.Path);
                
                foreach (var childFolderPath in childFolders)
                {
                    // Debug.Log($"--> scanned : {childFolderPath}");
                    EmitScannedItem(parent:target, itemCategory:"folder", item: childFolderPath);
                }
            }

            public override void Register()
            {
                OnItemScanned -= HandleItemScanned;
                OnItemScanned += HandleItemScanned;
                
                void HandleItemScanned(object parent, string itemCategory, object item)
                {
                    if (parent      is FolderAsset parentFolder
                    && itemCategory == "folder"
                    && item         is string path)
                    {
                        var child = new FolderAsset() { FolderPath = path, Path = path };
                        parentFolder.Node.AddChild(child);
                    }
                }
            }
        }
    }
}


//     public class FolderElement : Entity
//     {
//         //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
//         
//         //////// Folder
//         
//         public string Path                 = "";
//         
//         public string FolderName           {
//                                                get => System.IO.Path.GetFileName(Path);
//                                                set => Rename(value); // Path = value != null
//                                                                      //               ? System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path) ?? string.Empty, value)
//                                                                      //               : null;
//                                            }
//         
//         public string DataPathRelativePath =>  Path.Replace(Application.dataPath, "").TrimStart('/');
//         
//         //////// Element
//         
//         public bool IsDraft = false;
//         
//         public List<string> Edits = new List<string>();
//
//         
//         //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Root Folder
//         
//         private static FolderElement folder1;
//         public  static FolderElement Folder1 
//         {
//             get
//             {
//                 if (folder1 != null)
//                     return FolderElement.folder1;
//                 
//                 // Create folder 1
//                 folder1                  = new FolderElement();
//                 folder1.Path             = Application.dataPath + "/" + "Folder1";
//                 folder1.IsDraft          = true;
//                 folder1.Node.DisplayName = $"(FolderElement) {folder1.FolderName}";
//                 
//                 // Add folder 1 as a child of Assets
//                 folder1.Node.SetParent(Root.Instance.Context.Package1.Assets);
//                 
//                 // Create Actual physical folder
//                 if (!folder1.DoesExist())
//                     folder1.Create();
//                 
//                 Debug.Log($"Folder 1 at path : {folder1.Path}");
//                 
//                 return folder1;
//             }
//         }
//         
//         //////////////////////////////////////////////////////////////////////////////////////////////////////////////// TEMP
//
//         #if UNITY_EDITOR
//         [MenuItem("Tools/TetFolder")]
//         #endif 
//         static void TestFolder()
//         {
//             // Create folder 1
//             FolderElement folder1    = new FolderElement();
//             folder1.Path             = Application.dataPath + "/" + "Folder1";
//             folder1.IsDraft          = true;
//             folder1.Node.DisplayName = $"(FolderElement) {folder1.FolderName}";
//             
//             // Add folder 1 as a child of Assets
//             Root.Instance.Context.Package1.Assets.Node.Children.Clear();
//             folder1.Node.SetParent(Root.Instance.Context.Package1.Assets);
//         }
//         
//         //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Element Actions
//
//         public void Report()
//         {
//             Debug.Log($"FolderElement.Report:");
//             Debug.Log($"-path = {Path}");
//             Debug.Log($"-relative path = {DataPathRelativePath}");
//             Debug.Log($"-Exists = {DoesExist()}");
//         }
//
//         public void Scan()
//         {
//             // Check if the path exists
//             if (!System.IO.Directory.Exists(Path))
//             {
//                 Debug.LogError($"Path does not exist: {Path}");
//                 return;
//             }
//             
//             try
//             {
//                 // Get all directories in the current path
//                 string[] directories = System.IO.Directory.GetDirectories(Path);
//                 
//                 // Create a child FolderElement for each directory
//                 foreach (string directory in directories)
//                 {
//                     // make sure it doesnt already exist as a child
//                     if (Node.Children.OfType<FolderElement>().Any(child => child.Path == directory))
//                     {
//                         Debug.Log($"- skipping'{directory}' - already exists.");
//                         continue;
//                     }
//                     
//                     // Create a new folder element
//                     FolderElement childFolder = new FolderElement();
//                     childFolder.Path          = directory;
//                     
//                     // Add the child folder to this folder's children
//                     childFolder.Node.SetParent(this);
//                 }
//             }
//             catch (System.Exception e)
//             {
//                 Debug.LogError($"Error scanning folder {Path}: {e.Message}");
//             }
//         }
//
//
//         //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Physical Actions
//         
//         public bool DoesExist()
//         {
//             return Directory.Exists(Path);
//         }
//
//         public void Create()
//         {
//             Debug.Log($"Creating folder {Path}, Exists = {DoesExist()}");
//             Directory.CreateDirectory(Path);
//             IsDraft = false;
//         }
//         
//         public void Rename(string folderName)
//         {
//             Debug.Log($"Renaming {FolderName} to {folderName}");
//             
//             //////////////////////////////////////////
//             
//             var oldPath = Path;
//             var newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(oldPath) ?? string.Empty, folderName);
//             
//             if (Directory.Exists(newPath))
//                 Debug.Log("Destination already exists!");
//             
//             Debug.Log($"old path = {oldPath}");
//             Debug.Log($"new path = {newPath}");
//             
//             Directory.Move(oldPath, newPath);
//             
//             Debug.Log($"Exists1 = {Directory.Exists(oldPath)}");
//             Debug.Log($"Exists2 = {Directory.Exists(newPath)}");
//             
//             this.Path = newPath;
//             
//             //////////////////////////////////////////
//             
//             // Rename the meta file as well
//             var oldMetaPath = $"{oldPath}.meta";
//             var newMetaPath = $"{newPath}.meta";
//             
//             if (File.Exists(oldMetaPath))
//             {
//                 File.Move(oldMetaPath, newMetaPath);
//                 Debug.Log($"Renamed meta file from {oldMetaPath} to {newMetaPath}");
//             }
//             else
//             {
//                 //Debug.LogError($"Meta file not found at {oldMetaPath}, skipping rename.");
//             }
//             
//             //////////////////////////////////////////
//         }
//         
//         public void Delete()
//         {
//             Debug.Log($"Deleting folder {Path}");
//             Directory.Delete(path: Path, recursive: true);
//             
//             Debug.Log($"Deleting meta {Path}.meta");
//             File.Delete(path: $"{Path}.meta");
//         }
//         
//         
//         //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Live Actions
//         
//         public FolderElement Add()
//         {   
//             FolderElement child = new FolderElement();
//             child.Path          = System.IO.Path.Combine(this.Path, "New Folder");
//             child.IsDraft       = true;
//             
//             child.Node.SetParent(this);
//             
//             return child;
//         }
//         
//         public void Remove()
//         {
//             this.Node.Parent.Node.Children.Remove(this);
//             // And also delete (if not draft)
//         }
//         
//         public void ApplyEdits()
//         {
//             foreach (var edit in this.Edits)
//             {
//                 ApplySingleEdit(edit);
//             }
//         }
//         
//         public void ApplySingleEdit(string edit)
//         {
//             try
//             {
//                 // Split the input edit string into field name and value using '=' as the delimiter
//                 var parts = edit.Split('=');
//                 
//                 if (parts.Length != 2)
//                 {
//                     // Log an error if the format is invalid
//                     Debug.LogError($"Invalid edit format: '{edit}'. Expected format is 'field=value'.");
//                     return;
//                 }
//                 else
//                 {
//                     var fieldName  = parts[0].Trim(); // Get the field name and trim whitespace
//                     var fieldValue = parts[1].Trim(); // Get the field value and trim whitespace
//
//                     // Try to find a field with the specified name
//                     var field    = this.GetType().GetField(fieldName);
//
//                     // Try to find a property with the specified name
//                     var property = this.GetType().GetProperty(fieldName);
//
//                     if (field != null)
//                     {
//                         // If a field is found, convert the value to the appropriate type
//                         var convertedValue = System.Convert.ChangeType(fieldValue, field.FieldType);
//
//                         // Set the field value
//                         field.SetValue(this, convertedValue);
//                     }
//                     else if (property != null && property.CanWrite)
//                     {
//                         // If a writable property is found, convert the value to the appropriate type
//                         var convertedValue = System.Convert.ChangeType(fieldValue, property.PropertyType);
//
//                         // Set the property value
//                         property.SetValue(this, convertedValue);
//                     }
//                     else
//                     {
//                         // Log an error if neither a field nor a writable property is found
//                         Debug.LogError($"Field or writable property '{fieldName}' not found on FolderElement.");
//                     }
//                 }
//             }
//             catch (System.Exception ex)
//             {
//                 // Log any exception that occurs during the edit application
//                 Debug.LogError($"Error applying edit '{edit}': {ex.Message}");
//             }
//         }
//         
//     }
// }
