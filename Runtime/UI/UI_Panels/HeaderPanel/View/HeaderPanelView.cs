using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galleon.Checkout.UI
{
    public class HeaderPanelView : View
    {   
        /////////////////////////////////////////////////////// UI Events
        
        public async void OnGearsButtonClick()
        {
            Debug.Log("Gears button clicked");
            CheckoutClient.Instance.CheckoutScreenMobile.On_SettingsClicked();
        }
        
        public async void OnGalleonLogoClick()
        {
            CheckoutClient.Instance.CheckoutScreenMobile.On_GalleonLogoClicked();
        }
        
        public async void OnXButtonClicked()
        {
            CheckoutClient.Instance.CheckoutScreenMobile.On_CloseClicked();
        }
        
        public async void OnBackButtonClicked()
        {
            CheckoutClient.Instance.CheckoutScreenMobile.On_BackClicked();
        }
        
        /////////////////////////////////////////////////////// Steps
        
        public Step OpenCheckoutSettingsView()
        =>
            new Step(name   : $"open_checkout_settings_view"
                    ,action : async (s) =>
                    {
                        
                    });
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// State
        
        public enum STATE
        {
            none,
            checkout_and_settings,
            back_and_text,
            x_button,
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public GameObject CheckoutAndSettingsPanel; 
        public GameObject BackAndTitlePanel;
        public GameObject XButtonPanel;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Refresh

        public override void RefreshState()
        {
            DisableAllPanels();

            if (this.State == STATE.checkout_and_settings.ToString())
            {
                CheckoutAndSettingsPanel.SetActive(true);
            }
            if (this.State == STATE.back_and_text.ToString())
            {
                BackAndTitlePanel.SetActive(true);
            }
            if (this.State == STATE.x_button.ToString())
            {
                XButtonPanel.SetActive(true);
            }
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// helper Methods
        
        private void DisableAllPanels()
        {
            this.CheckoutAndSettingsPanel.SetActive(false);
            this.BackAndTitlePanel       .SetActive(false);
            this.XButtonPanel            .SetActive(false);
        }
    }
}
