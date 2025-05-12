using TMPro;
using UnityEngine;

namespace Galleon.Checkout.UI
{
    public class SimpleDialogPanelView : View
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public TMP_Text MainText;
        public TMP_Text ConfirmButtonText;
        public TMP_Text DeclineButtonText;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Result
        
        public      DialogResult Result = DialogResult.None;
        public enum DialogResult
        {
            None,
            Confirm,
            Decline,
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Events
        
        public void On_ConfirmClicked()
        {
            Debug.Log("On Confirm Clicked");
            
            Debug.Log($"Removing Payment Method : {CheckoutClient.Instance.CurrentSession.PaymentMethodToDelete.DisplayName}");
            CheckoutClient.Instance.CurrentSession.User.RemovePaymentMethod(CheckoutClient.Instance.CurrentSession.PaymentMethodToDelete);
            
            this.Result = DialogResult.Confirm;
            CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(this.Result.ToString());
        }
        
        public void On_DeclineClicked()
        {
            Debug.Log("On Decline Clicked");
            this.Result = DialogResult.Decline;
            CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(this.Result.ToString());
        }
    }
}
