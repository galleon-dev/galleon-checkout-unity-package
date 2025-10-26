using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Galleon.Checkout
{
    [Serializable]
    public class CheckoutSprites
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        [Header("Built in")]
        // Icons
        public Sprite AddCreditCardIconSprite;
        public Sprite VisaIconSprite;
        public Sprite MasterCardIconSprite;
        public Sprite AmexIconSprite;
        public Sprite DinersIconSprite;
        public Sprite DiscoverIconSprite;
        public Sprite GPayIconSprite;
        public Sprite GPlayIconSprite;
        public Sprite PaypalIconSprite;
        public Sprite AppleIconSprite;
        public Sprite KlarnaIconSprite;
        public Sprite WebCheckoutIconSprite;
        public Sprite CashAppIconSprite;
        public Sprite AmazonPayIconSprite;
        // Buttons
        public Sprite CheckoutButtonSprite;
        public Sprite GpaybuttonSprite;
        public Sprite PaypalbuttonSprite;
        public Sprite AppleButtonSprite;
        
        [Header("Cloud")]
        public List<SpriteResource> SpriteResources = new();
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// API Methods
        
        public Sprite GetIconSprite(string paymentMethodActualType)
        {
            if (SpriteResources.Any(x => x.Name == paymentMethodActualType + "_icon"))
            {
                return SpriteResources.FirstOrDefault(x => x.Name == paymentMethodActualType)?.Sprite;
            }
            else
            {
                switch (paymentMethodActualType)
                {
                    case "card" :        return AddCreditCardIconSprite;   
                    case "visa":         return VisaIconSprite;
                    case "mastercard":   return MasterCardIconSprite;
                    case "amex":         return AmexIconSprite;
                    case "diners":       return DinersIconSprite;
                    case "discover":     return DiscoverIconSprite;
                    case "google_pay":   return GPayIconSprite;
                    case "google_play":  return GPlayIconSprite;
                    case "paypal":       return PaypalIconSprite;
                    case "apple":        return AppleIconSprite;
                    case "klarna":       return KlarnaIconSprite;
                    case "web_checkout": return WebCheckoutIconSprite;
                    case "cashapp":      return CashAppIconSprite;
                    case "amazon_pay":   return AmazonPayIconSprite;
                    case "native":
                        #if UNITY_ANDROID
                            return GPlayIconSprite;
                        #elif UNITY_IOS
                            return AppleIconSprite;
                        #else
                            return AddCreditCardIconSprite
                        #endif
                    default:            return AddCreditCardIconSprite;
                }
            }
        }
        
        public Sprite GetButtonSprite(string paymentMethodActualType)
        {
            if (SpriteResources.Any(x => x.Name == paymentMethodActualType + "_button"))
            {
                return SpriteResources.FirstOrDefault(x => x.Name == paymentMethodActualType + "_button")?.Sprite;
            }
            else
            {
                switch (paymentMethodActualType)
                {
                    case "google_pay": return GpaybuttonSprite;
                    case "paypal":     return PaypalbuttonSprite;
                    case "apple":      return AppleButtonSprite;
                    default:           return CheckoutButtonSprite;
                }
            }
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Resource Methods
        
        public async Task<Sprite> LoadSprite(string name_or_url)
        {
            try
            {
                // Definitions
                var cached   = SpriteResources.FirstOrDefault(x => x.Name == name_or_url);
                var filePath = Path.Combine(Application.persistentDataPath, name_or_url);

                // Check if cached in memory
                if (cached != null && cached.Sprite != null)
                {
                    return cached.Sprite;
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
                    var       sprite  = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    
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
    
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helper Types
    
    [Serializable]
    public class SpriteResource
    {
        public string Name;
        public Sprite Sprite;
    }
}

