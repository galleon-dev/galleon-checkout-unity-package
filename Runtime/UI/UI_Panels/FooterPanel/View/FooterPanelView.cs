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
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Refresh

        public override void RefreshState()
        {
            DisableAllPanels();
            
            if (this.State == STATE.long_terms_of_service.ToString())
            {
                LongTermsOfServiceelement.SetActive(true);
            }
            if (this.State == STATE.terms_privacy_return.ToString())
            {
                TermsPrivacyReturnElement.SetActive(true);
            }
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// helper Methods
        
        private void DisableAllPanels()
        {
            this.TermsPrivacyReturnElement.SetActive(false);
            this.LongTermsOfServiceelement.SetActive(false);
        }
    }
}
