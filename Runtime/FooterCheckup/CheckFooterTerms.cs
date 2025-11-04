using Galleon.Checkout.UI;
using UnityEngine;

namespace Galleon.Footer
{
    public class CheckFooterTerms : MonoBehaviour
    {
        public FooterPanelView FooterPanelView;
        
        private void OnEnable()
        {
            if (FooterPanelView)
            {
                FooterPanelView.ShowTermsOfService(false);
            }
        }

        private void OnDisable()
        {
            if (FooterPanelView)
            {
                FooterPanelView.ShowTermsOfService(true);
            }
        }
    }
}