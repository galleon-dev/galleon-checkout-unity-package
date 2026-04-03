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
        public Sprite GalleonIconSprite;
        public Sprite AppIconSprite;
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
        public Sprite VenmoIconSprite;
        public Sprite ZelleIconSprite;
        public Sprite LinkIconSprite;

        // Buttons
        public Sprite CheckoutButtonSprite;
        public Sprite GpaybuttonSprite;
        public Sprite PaypalbuttonSprite;
        public Sprite AppleButtonSprite;
        public Sprite AmazonPayButtonSprite;
        public Sprite AmexButtonSprite;
        public Sprite CashAppButtonSprite;
        public Sprite DinersButtonSprite;
        public Sprite DiscoverButtonSprite;
        public Sprite KlarnaButtonSprite;
        public Sprite MastercardButtonSprite;
        public Sprite VenmoButtonSprite;
        public Sprite VisaButtonSprite;
        public Sprite ZelleButtonSprite;
        public Sprite LinkButtonSprite;

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
                    case "empty_card":           return AddCreditCardIconSprite;
                    case "card":                 return AddCreditCardIconSprite;
                    case "visa":                 return VisaIconSprite;
                    case "mastercard":           return MasterCardIconSprite;
                    case "amex":                 return AmexIconSprite;
                    case "diners":               return DinersIconSprite;
                    case "discover":             return DiscoverIconSprite;
                    case "google_pay_browser":   return GPayIconSprite;
                    case "google_pay":           return GPayIconSprite;
                    case "google_play":          return GPlayIconSprite;
                    case "empty_paypal":         return PaypalIconSprite;
                    case "paypal":               return PaypalIconSprite;
                    case "apple":                return AppleIconSprite;
                    case "klarna":               return KlarnaIconSprite;
                    case "web_checkout":         return WebCheckoutIconSprite;
                    case "cashapp":              return CashAppIconSprite;
                    case "amazon_pay":           return AmazonPayIconSprite;
                    case "venmo":                return VenmoIconSprite;
                    case "zelle":                return ZelleIconSprite;
                    case "link":                 return LinkIconSprite;
                    case "native":
                        #if UNITY_ANDROID
                        return GPlayIconSprite;
                    #elif UNITY_IOS
                            return AppleIconSprite;
                    #else
                            return AddCreditCardIconSprite;
                    #endif
                    case "app": return AppIconSprite;
                    default:
                    {
                        if (paymentMethodActualType.ToLower().Contains("paypal"))
                            return PaypalIconSprite;

                        return AddCreditCardIconSprite;
                    }
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
                    case "google_pay":           return GpaybuttonSprite;
                    case "google_pay_browser":   return GpaybuttonSprite;
                    case "paypal":               return PaypalbuttonSprite;
                    case "empty_paypal":         return PaypalbuttonSprite;
                    case "apple":                return AppleButtonSprite;
                    case "amazon_pay":           return AmazonPayButtonSprite;
                    case "amex":                 return AmexButtonSprite;
                    case "cashapp":              return CashAppButtonSprite;
                    case "diners":               return DinersButtonSprite;
                    case "discover":             return DiscoverButtonSprite;
                    case "klarna":               return KlarnaButtonSprite;
                    case "mastercard":           return MastercardButtonSprite;
                    case "venmo":                return VenmoButtonSprite;
                    case "visa":                 return VisaButtonSprite;
                    case "zelle":                return ZelleButtonSprite;
                    case "link":                 return LinkButtonSprite;

                    default: return CheckoutButtonSprite;
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Resource Methods

        //public Step LoadSprite(string name_or_url) 
        //=>
        //    new Step(name   : $"load_sprite_{name_or_url}"
        //            ,action : async (s) =>
        //            {
        //                
        //            });
        
        public async Task<Sprite> LoadSprite(string name_or_url)
        {     
            try
            {
                // Definitions
                var cached = SpriteResources.FirstOrDefault(x => x.Name == name_or_url);

                // Check if cached in memory
                if (cached != null && cached.Sprite != null)
                {
                    return cached.Sprite;
                }

                // Definitions
                string fileName = name_or_url.StartsWith("http") ? GetFileNameFromUrl(name_or_url) : name_or_url;
                string filePath = Path.Combine(Application.persistentDataPath, fileName);

                // Check if cached on disk
                if (File.Exists(filePath))
                {
                    Debug.Log("Load From Cache: " + filePath);
                    byte[] bytes   = File.ReadAllBytes(filePath);
                    var    texture = new Texture2D(2, 2);
                    texture.LoadImage(bytes);

                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                    return sprite;
                }

                // Download if URL
                if (name_or_url.StartsWith("http"))
                {
                    return await DownloadAndCacheSprite(name_or_url, filePath);
                }

                return null;
            }
            catch (Exception e)
            {
                return default;
            }
        }

        public async Task<Sprite> DownloadAndCacheSprite(string url, string filePath)
        {
            try
            {
                // Start download
                using UnityWebRequest request   = UnityWebRequestTexture.GetTexture(url);
                var                   operation = request.SendWebRequest();

                // Await
                while (!operation.isDone)
                    await Task.Yield();

                // Check result
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Download failed: " + request.error);
                    return null;
                }

                // Save to disk
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                byte[]    bytes   = texture.EncodeToPNG();
                File.WriteAllBytes(filePath, bytes);
                
                // Get Sprite
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                // Save in memory list
                SpriteResources.Add(new SpriteResource { Name = url, Sprite = sprite });
                
                //Debug.Log("Save To Cache: " + filePath);

                return sprite;
            }
            catch (Exception e)
            {
                Debug.LogError($"Download error: {e}");
                return null;
            }

        }

        private string GetFileNameFromUrl(string url)
        {
            return Path.GetFileName(new Uri(url).AbsolutePath);
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