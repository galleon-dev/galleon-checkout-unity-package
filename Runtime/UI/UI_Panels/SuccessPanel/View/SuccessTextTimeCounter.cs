using System.Collections;
using UnityEngine;

namespace Galleon.Checkout.UI
{
    public class SuccessTextTimeCounter : MonoBehaviour
    {
        public Galleon.Checkout.UI.SuccessPanelView SuccessPanelView;

        float DelayTime = 1.0f;
        bool SendReceipt = false;

        void OnEnable()
        {
            StartCoroutine(ClosePanel());
        }

        IEnumerator ClosePanel()
        {
            yield return new WaitForSeconds(DelayTime);

            if (SendReceipt)
            {
                Debug.Log("Auto Send Receipt");
                SuccessPanelView.OnConfirmEmailButtonClick();
            }
            else
            {
                Debug.Log($"Auto Close in {DelayTime} seconds");
                CheckoutClient.Instance.CheckoutScreenMobile.On_CloseClicked();
            }
        }
    }
}