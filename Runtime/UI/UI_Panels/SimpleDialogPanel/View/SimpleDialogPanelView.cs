using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{
    public class SimpleDialogPanelView : View
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public TMP_Text MainText;
        public TMP_Text ConfirmButtonText;
        public TMP_Text DeclineButtonText;
        
        //// Members
        
        [Header("Remove Payment Method - UI")]
        public GameObject RemovePaymentMethodGameObject;
        public Image      RemovePaymentMethodIcon;
        public TMP_Text   RemovePaymentMethodLabel;
        
        [Header("Remove Payment Method - Sprites")]
        public Sprite   VisaSprite;
        public Sprite   MasterCardSprite;
        public Sprite   GPaySprite;
        public Sprite   PaypalSprite;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Result
        
        public      DialogResult Result = DialogResult.None;
        public enum DialogResult
        {
            None,
            Confirm,
            Decline,
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Refresh

        public override void RefreshState()
        {
            var paymentMethod                  = CheckoutClient.Instance.CurrentSession.PaymentMethodToDelete;
            this.RemovePaymentMethodLabel.text = paymentMethod.DisplayName;
            
            if      (paymentMethod.Type == PaymentMethod.PaymentMethodType.Visa.ToString())
                this.RemovePaymentMethodIcon.sprite = VisaSprite;
            else if (paymentMethod.Type == PaymentMethod.PaymentMethodType.MasterCard.ToString())
                this.RemovePaymentMethodIcon.sprite = MasterCardSprite;
            else if (paymentMethod.Type == PaymentMethod.PaymentMethodType.GPay.ToString())
                this.RemovePaymentMethodIcon.sprite = GPaySprite;
            else if (paymentMethod.Type == PaymentMethod.PaymentMethodType.PayPal.ToString())
                this.RemovePaymentMethodIcon.sprite = PaypalSprite;
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
