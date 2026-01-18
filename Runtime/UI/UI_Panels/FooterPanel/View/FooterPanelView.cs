using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Galleon.Checkout.UI
{
    public class FooterPanelView : View, IPointerClickHandler
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// State
        
        public enum STATE
        {
            none,
            terms_privacy_return,
            long_terms_of_service,
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public GameObject TermsPrivacyReturnElement; 
        public GameObject LongTermsOfServiceelement;

        public GameObject ViewPaymentMethodsPanel;
        public GameObject TermsOfServicePanel;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Refresh

        public override void RefreshState()
        {
            DisableAllPanels();
            
            if (this.State == STATE.terms_privacy_return.ToString())
            {
                if (CHECKOUT.Globals.ShowLongFooter)
                    TermsPrivacyReturnElement.SetActive(true);
                else
                    LongTermsOfServiceelement.SetActive(true);
            }
            else if (this.State == STATE.none.ToString())
            {
                DisableAllPanels();
            }
            
            
            ShowViewPaymentMethods(CHECKOUT.Screen.CurrentPage == CHECKOUT.Screen.CheckoutPage);    
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Events
        
        
        public void OnPointerClick(PointerEventData eventData)
        {
            try
            {
                var text      = GetComponentInChildren<TextMeshProUGUI>();
                var linkIndex = TMP_TextUtilities.FindIntersectingLink(text, eventData.position, null);
                var linkId    = text.textInfo.linkInfo[linkIndex].GetLinkID();
                
                if (CHECKOUT.Config.ConfigData.ContainsKey($"url_{linkId}"))
                {
                    var url = CHECKOUT.Config.GetString($"$url_{linkId}");
                    Application.OpenURL(url);
                }
            }
            catch (Exception e)
            {
            }
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// helper Methods
        
        private void DisableAllPanels()
        {
            if (CheckoutScreenMobile.IsLandscape == false)
            {
                this.TermsPrivacyReturnElement.SetActive(false);
                this.LongTermsOfServiceelement.SetActive(false);
            }
        }

        public void ShowViewPaymentMethods(bool _Status)
        {
            // Debug.Log("Show View Payment Methods Panel: " + _Status);
            ViewPaymentMethodsPanel.SetActive(_Status);
        }

        public void ShowTermsOfService(bool _Status)
        {
            // Debug.Log("Show Terms Of Service Panel: " + _Status);
            TermsOfServicePanel.SetActive(_Status);
        }

    }
}