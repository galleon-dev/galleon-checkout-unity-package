using Galleon.Checkout.UI;
using UnityEngine;

namespace Galleon.Footer
{
    public class CheckFooter : MonoBehaviour
    {
        public FooterPanelView FooterPanelView;
        
        private void OnEnable()
        {
            if (FooterPanelView)
            {
                //FooterPanelView.ShowViewPaymentMethods(true);
            }
        }

        private void OnDisable()
        {
            if (FooterPanelView)
            {
                //FooterPanelView.ShowViewPaymentMethods(false);
            }
        }
    }
}