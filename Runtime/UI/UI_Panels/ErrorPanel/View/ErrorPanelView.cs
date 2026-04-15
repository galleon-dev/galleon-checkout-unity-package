using UnityEngine;
using TMPro;

namespace Galleon.Checkout.UI
{
    public class ErrorPanelView : View
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members

        public TMP_Text ErrorText;
        public TMP_Text ErrorDescriptionText;
        public TMP_Text ErrorButtonText;

        public string   ErrorMessage     = "Payment Failed";
        public string   ErrorDescription = "Try again or use another payment method.";
        public string   ButtonText       = "Back To Payment Options";

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// View Result

        public      ViewResult Result = ViewResult.None;
        public enum ViewResult
        {
            None,
            Back,
            Confirm,
            Checkout,
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public override void Initialize()
        {
            base.Initialize();
            // Reset to default values on initialization
            ResetToDefaults();
        }

        public override void RefreshState()
        {
            if (ErrorText != null)
                ErrorText.text = ErrorMessage;

            if (ErrorDescriptionText != null)
                ErrorDescriptionText.text = ErrorDescription;

            if (ErrorButtonText != null)
                ErrorButtonText.text = ButtonText;

            // Reset to defaults after displaying, ready for next time
            ResetToDefaults();
        }

        private void ResetToDefaults()
        {
            ErrorMessage     = "Payment Failed";
            ErrorDescription = "Try again or use another payment method.";
            ButtonText       = "Back To Payment Options";
        }

        
        //////////////////////////////////////////////////////////////////////////// UI Events

        public void On_TryAnotherMethodButtonClicked()
        {
            Result = ViewResult.Confirm;
            CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(Result.ToString());
        }
    }
}
