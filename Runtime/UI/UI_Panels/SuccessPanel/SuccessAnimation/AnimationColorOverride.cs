using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class AnimationColorOverride : MonoBehaviour
{
    public Image Image;
    public Color Color;
    
    [MenuItem("Tool/Do Thing")]
    public static void DoThing()
    {
        var path       = GetPath();
        var folderPath = Path.GetDirectoryName(path);
        var files      = Directory.GetFiles(folderPath, "*.png");
        
        foreach (var file in files)
        {
            var relativePath = file.Replace("/Users/levanb/galleon/Projects/Galleon_Checkout_Development/Packages/", "Packages/");
            var sprite       = AssetDatabase.LoadAssetAtPath<Sprite>(relativePath);
            
            if (sprite == null)
            {
                Debug.LogError("Null Sprite at path: " + relativePath);
                continue;
            }
            
            Debug.Log($"{sprite.name, -3} : {sprite.texture.width} x {sprite.texture.height}");
            
            
        
        }
        
    }
    
    public static string GetPath([CallerFilePath] string path = "")
    {
        return path;
    }

}
