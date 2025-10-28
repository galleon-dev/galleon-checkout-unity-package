using Galleon.Checkout;
using Galleon.Checkout.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{
    public class SetDropdownOption : MonoBehaviour
    {
        public CheckoutPanelView CheckoutPanelView;
        public Toggle Toggle;
        public checkoutPanelPaymentMethodItemView checkoutPanelPaymentMethodItemView;

        public void SetSelectionInfo(checkoutPanelPaymentMethodItemView _checkoutPanelPaymentMethodItemView)
        {
            if (Toggle.isOn)
            {
                int index = _checkoutPanelPaymentMethodItemView.transform.GetSiblingIndex() - 1;
                Debug.Log("SetSelectionInfo(): " + index);
                checkoutPanelPaymentMethodItemView.SelectDropdownPaymentMethod(CHECKOUT.PaymentMethods.UserPaymentMethods[index], CheckoutPanelView);
            }
        }
    }
}