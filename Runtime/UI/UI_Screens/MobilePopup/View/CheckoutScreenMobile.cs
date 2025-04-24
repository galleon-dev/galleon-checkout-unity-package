using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Galleon.Checkout;
using Galleon.Checkout.UI;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace Galleon.Checkout.UI
{
    public class CheckoutScreenMobile : View
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public CheckoutViewModel ViewModel => CheckoutClient.Instance.CheckoutViewModel;
            
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members

        // Panels
        
        [Header("Parent Panel")]
        public  ParentPanel                  ParentPanel;
        [Header("Panels")]
        public  CheckoutPanelView            CheckoutPanel;
        public  CreditCardInfoPanelView      CreditCardPanel;
        public  SettingsPanelView            SettingsPanelView;
        public  SuccessPanelView             SuccessPanelView;
        public  ErrorPanelView               ErrorPanelView;
        public  SelectCurrencyPanelView      SelectCurrencyPanelView;
        public  SelectPaymentMethodPanelView SelectPaymentMethodPanelView;
        public  SimpleDialogPanelView        SimpleDialogPanelView;

        private bool                         isPending = false;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        private void Start()
        {
            ViewCheckoutPanel().Execute();
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Steps
        
        public Step ViewCheckoutPanel()
        =>
            new Step(name   : $"view_checkout_panel"
                    ,action : async (s) =>
                    {
                        DisableAllPanels();
                        await CheckoutPanel.View().Execute();

                        switch (CheckoutPanel.Result)
                        {
                            case CheckoutPanelView.ViewResult.Confirm             : ViewSuccessPanel()            .Execute(); break;
                            case CheckoutPanelView.ViewResult.Settings            : ViewSettingsPanel()           .Execute(); break;
                            case CheckoutPanelView.ViewResult.OtherPaymentMethods : ViewSelectPaymentMethodPanel().Execute(); break;
                        }

                    });
        
        public Step ViewSuccessPanel()
        =>
            new Step(name   : $"view_success_panel"
                    ,action : async (s) =>
                    {
                        DisableAllPanels();
                        await SuccessPanelView.View().Execute();

                        switch (SuccessPanelView.Result)
                        {
                            case SuccessPanelView.ViewResult.Confirm : Close(); break;
                        }
                    });
        
        public Step ViewSettingsPanel()
        =>
            new Step(name   : $"view_settings_panel"
                    ,action : async (s) =>
                    {
                        DisableAllPanels();
                        await SettingsPanelView.View().Execute();
                        
                        switch (SettingsPanelView.Result)
                        {
                            case SettingsPanelView.ViewResult.Close : ViewCheckoutPanel().Execute(); break;
                        }
                    });
        
        public Step ViewSelectPaymentMethodPanel()
        =>
            new Step(name   : $"view_select_payment_methods_panel"
                    ,action : async (s) =>
                    {
                        DisableAllPanels();
                        await CreditCardPanel.View().Execute();
                        
                        switch (CreditCardPanel.Result)
                        {
                            case CreditCardInfoPanelView.ViewResult.Confirm : Close(); break;
                        }
                    });

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Events (OLD)

        #region OLD
        
        [ContextMenu("Test")]
        public async void OnPurchaseClick()
        {
            if (isPending)
                return;
            
            isPending = true;
            
            // fake request 
            // UnityWebRequest requet = UnityWebRequest.Get("http://localhost:5007/purchase");
            // var asyncOp = requet.SendWebRequest();
            // while (!asyncOp.isDone)
            //     await Task.Yield();
            // bool isSuccess = asyncOp.webRequest.error == null && asyncOp.webRequest.downloadHandler.text == "true";
            // if (!isSuccess)
            // {
            //     CheckoutPanel.gameObject.SetActive(false);
            //     ErrorPanel   .gameObject.SetActive(true);
            //     
            //     if (asyncOp.webRequest.error != null)
            //         Debug.LogError(asyncOp.webRequest.error);
            // }
            // else
            // {        
            //     CheckoutPanel  .gameObject.SetActive(false);
            //     CreditCardPanel.gameObject.SetActive(true);   
            // }
            
            bool isSuccess = true;
            isPending      = false;
            
            SetCreditCardPanelActive();
        }

        public void OnCreditCardConfirmClick()
        {
            CreditCardPanel .gameObject.SetActive(false);
            SuccessPanelView.gameObject.SetActive(true);
        }

        public void OnSuccessConfirmClick()
        {
            Close();
        }

        public void OnErrorConfirmClick()
        {
            ErrorPanelView.gameObject.SetActive(false);
            CheckoutPanel .gameObject.SetActive(true);
        }

        #endregion // OLD
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Events

        public void OnShadeClick()
        {
            Close();
        }

        public void OnBackClick()
        {
            Close();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Actions

        public void SetCheckoutPanelActive()
        {
            DisableAllPanels();
            CheckoutPanel.gameObject.SetActive(true);
        }

        public void SetCreditCardPanelActive()
        {
            DisableAllPanels();
            CreditCardPanel.gameObject.SetActive(true);
        }

        public void SetSuccessPanelActive()
        {
            DisableAllPanels();
            SuccessPanelView.gameObject.SetActive(true);
        }

        public void Close()
        {
            this.gameObject.SetActive(false);
            Destroy(this.gameObject);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helper UI Actions

        private void DisableAllPanels()
        {
            CheckoutPanel               .gameObject.SetActive(false);
            CreditCardPanel             .gameObject.SetActive(false);
            SettingsPanelView           .gameObject.SetActive(false);
            SuccessPanelView            .gameObject.SetActive(false);
            ErrorPanelView              .gameObject.SetActive(false);
            SelectCurrencyPanelView     .gameObject.SetActive(false);
            SelectPaymentMethodPanelView.gameObject.SetActive(false);
            SimpleDialogPanelView       .gameObject.SetActive(false);
        }

        void Update()
        {
            // if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
            // {
            //     if (Input.GetKeyDown(KeyCode.Escape))
            //     {
            //         // Handle back button press
            //         OnBackClick();
            //     }
            // }
        }
    }
}