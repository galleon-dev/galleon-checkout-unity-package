using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Galleon.Checkout.UI
{
    public class FooterPanelView : View
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
            
            if (this.State == STATE.long_terms_of_service.ToString())
            {
                LongTermsOfServiceelement.SetActive(true);
            }
            else if (this.State == STATE.terms_privacy_return.ToString())
            {
                TermsPrivacyReturnElement.SetActive(true);
            }
            else if (this.State == STATE.none.ToString())
            {
                DisableAllPanels();
            }
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// helper Methods
        
        private void DisableAllPanels()
        {
            this.TermsPrivacyReturnElement.SetActive(false);
            this.LongTermsOfServiceelement.SetActive(false);
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
