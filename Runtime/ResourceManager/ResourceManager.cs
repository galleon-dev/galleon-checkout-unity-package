using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Galleon.Checkout
{
    public class ResourceManager : Entity
    {
        public async Task<Sprite> LoadSprite(string name_or_url)
        {
            try
            {
                // Definitions
                var cached   = CHECKOUT.Resources.Sprites.FirstOrDefault(x => x.name == name_or_url);
                var filePath = Path.Combine(Application.persistentDataPath, name_or_url);

                // Check if cached in memory
                if (cached != null && cached.sprite != null)
                {
                    return cached.sprite;
                }
                // Check if cached on disk
                else
                {
                    if (File.Exists(filePath))
                    {
                        byte[] bytes   = File.ReadAllBytes(filePath);
                        var    texture = new Texture2D(2, 2);
                        texture.LoadImage(bytes);
                        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    }
                }

                return default;
            }
            catch (Exception e)
            {
                return default;
            }
        } 
        
        public async Task<Sprite> DownloadSprite(string url)
        {
            var filePath = Path.Combine(Application.persistentDataPath, url);
            
            try
            {
                using UnityWebRequest request   = UnityWebRequestTexture.GetTexture(url);
                var                   operation = request.SendWebRequest();
                
                while (!operation.isDone)
                    await Task.Yield();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                    var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    
                    byte[] bytes = texture.EncodeToPNG();
                    File.WriteAllBytes(filePath, bytes);

                    return sprite;
                    
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        
        }
    }
    
    [Serializable]
    public class SpriteResource
    {
        public string name;
        public Sprite sprite;
    }
}
