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

        public async void OnAddPaymentMethodClicked()
        {
          //  CheckoutClient.Instance.CheckoutScreenMobile.UI_PaymentMethods();
            CheckoutClient.Instance.CheckoutScreenMobile.On_BackFromCreditCardInfo();
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
            credit_card_info
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public GameObject CheckoutAndSettingsPanel; 
        public GameObject BackAndTitlePanel;
        public GameObject XButtonPanel;
        public GameObject PaymentMethodPanel;

        // For Landscape Right
        public GameObject CheckoutAndSettingsPanelRight;
        public GameObject BackAndTitlePanelRight;
        public GameObject XButtonPanelRight;
        public GameObject PaymentMethodPanelRight;
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Refresh

        public override void RefreshState()
        {
            Debug.Log("HEADER - RefreshState: " + this.State);
            DisableAllPanels();

            if (this.State == STATE.checkout_and_settings.ToString())
            {
                CheckoutAndSettingsPanel.SetActive(true);

                if(CheckoutAndSettingsPanelRight)
                {
                    CheckoutAndSettingsPanelRight.SetActive(true);
                }
            }
            if (this.State == STATE.back_and_text.ToString())
            {
                BackAndTitlePanel.SetActive(true);

                if (BackAndTitlePanelRight)
                {
                    BackAndTitlePanelRight.SetActive(true);
                }
            }
            if (this.State == STATE.x_button.ToString())
            {
                XButtonPanel.SetActive(true);

                if (XButtonPanelRight)
                {
                    XButtonPanelRight.SetActive(true);
                }
            }
            if (this.State == STATE.credit_card_info.ToString())
            {
                PaymentMethodPanel.SetActive(true);

                if (PaymentMethodPanelRight)
                {
                    PaymentMethodPanelRight.SetActive(true);
                }
            }
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// helper Methods
        
        private void DisableAllPanels()
        {
            this.CheckoutAndSettingsPanel.SetActive(false);
            this.BackAndTitlePanel       .SetActive(false);
            this.XButtonPanel            .SetActive(false);
            this.PaymentMethodPanel      .SetActive(false);

            if (CheckoutAndSettingsPanelRight)
            {
                CheckoutAndSettingsPanelRight.SetActive(false);
            }

            if (BackAndTitlePanelRight)
            {
                BackAndTitlePanelRight.SetActive(false);
            }

            if (XButtonPanelRight)
            {
                XButtonPanelRight.SetActive(false);
            }

            if (PaymentMethodPanelRight)
            {
                PaymentMethodPanelRight.SetActive(false);
            }
        }
    }
}
