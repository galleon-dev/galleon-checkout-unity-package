using System;
using TMPro;
using UnityEngine;

namespace Galleon.Checkout.UI
{
    public class CheckoutTitleBarView : View
    {
        /////////////////////////////////////////////////////// Members
        
        public TMP_Text title;
        
        public static event Action GalleonLogoClicked;
        
        /////////////////////////////////////////////////////// UI Events
        
        public async void OnGearsButtonClick()
        {
            await OpenCheckoutSettingsView().Execute();
        }
        
        public async void OnGalleonLogoClick()
        {
            GalleonLogoClicked?.Invoke();
        }
        
        /////////////////////////////////////////////////////// Steps
        
        public Step OpenCheckoutSettingsView()
        =>
            new Step(name   : $"open_checkout_settings_view"
                    ,action : async (s) =>
                    {
                        
                    });
    }
}
