using TMPro;
using UnityEngine;

namespace Galleon.Checkout.UI
{
    public class CheckoutTitleBarView : View
    {
        /////////////////////////////////////////////////////// Members
        
        public TMP_Text title;
        
        /////////////////////////////////////////////////////// UI Events
        
        public async void OnGearsButtonClick()
        {
            await OpencheckoutSettingsView().Execute();
        }
        
        /////////////////////////////////////////////////////// Steps
        
        public Step OpencheckoutSettingsView()
        =>
            new Step(name   : $"open_checkout_settings_view"
                    ,action : async (s) =>
                    {
                        
                    });
    }
}