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
            Debug.Log("Header X button clicked");
            CheckoutClient.Instance.CheckoutScreenMobile.On_CloseClicked();
        }

        public async void OnBackButtonClicked()
        {
            Debug.Log("Header Back button clicked");
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
            new Step(name: $"open_checkout_settings_view"
                    , action: async (s) =>
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

        public GameObject HeaderSeperator;
        
        public GameObject CheckoutAndSettingsPanel;
        public GameObject BackAndTitlePanel;
        public GameObject XButtonPanel;
        public GameObject PaymentMethodPanel;

        // For Landscape Right
        public GameObject CheckoutAndSettingsPanelRight;
        public GameObject BackAndTitlePanelRight;
        public GameObject XButtonPanelRight;
        public GameObject PaymentMethodPanelRight;

        // To avoid inconsistencies with header apperance, I've added a container that I can hide/unhide during loading panel appearance, otherwise RefreshStates are triggered multiple times during the galleon button click and screen states switches in between 
        public GameObject HeaderContainerLeft;
        public GameObject HeaderContainerRight;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Refresh

        public override void RefreshState()
        {
            // Debug.Log("HEADER - RefreshState: " + this.State);
            DisableAllPanels();

            if (this.State == STATE.checkout_and_settings.ToString())
            {
                CheckoutAndSettingsPanel.SetActive(true);
                HeaderSeperator.SetActive(true);

                if (CheckoutAndSettingsPanelRight)
                {
                    CheckoutAndSettingsPanelRight.SetActive(true);
                }
            }
            if (this.State == STATE.back_and_text.ToString())
            {
                BackAndTitlePanel.SetActive(true);
                HeaderSeperator.SetActive(true);

                if (BackAndTitlePanelRight)
                {
                    BackAndTitlePanelRight.SetActive(true);
                }
            }
            if (this.State == STATE.x_button.ToString())
            {
                XButtonPanel.SetActive(true);
                HeaderSeperator.SetActive(true);

                if (XButtonPanelRight)
                {
                    XButtonPanelRight.SetActive(true);
                }
            }
            if (this.State == STATE.credit_card_info.ToString())
            {
                PaymentMethodPanel.SetActive(true);
                HeaderSeperator.SetActive(true);

                if (PaymentMethodPanelRight)
                {
                    PaymentMethodPanelRight.SetActive(true);
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// helper Methods

        private void DisableAllPanels()
        {
            HeaderSeperator.SetActive(false);
            
            this.CheckoutAndSettingsPanel.SetActive(false);
            this.BackAndTitlePanel.SetActive(false);
            this.XButtonPanel.SetActive(false);
            this.PaymentMethodPanel.SetActive(false);

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

        public void EnableCheckoutHeader(bool enabled)
        {
            if(HeaderContainerLeft) 
                HeaderContainerLeft.SetActive(enabled);

            if (HeaderContainerRight)
                HeaderContainerRight.SetActive(enabled);

            if (enabled == true)
            {
                this.CheckoutAndSettingsPanel.SetActive(true);

                if (CheckoutAndSettingsPanelRight)
                {
                    CheckoutAndSettingsPanelRight.SetActive(true);
                }
            }
        }

    }
}
