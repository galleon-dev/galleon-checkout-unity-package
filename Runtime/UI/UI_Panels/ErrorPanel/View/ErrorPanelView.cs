using UnityEngine;

namespace Galleon.Checkout.UI
{
    public class ErrorPanelView : View
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// View Result

        public      ViewResult Result = ViewResult.None;
        public enum ViewResult
        {
            None,
            Back,
            Confirm,
        }
        
        //////////////////////////////////////////////////////////////////////////// UI Events

        public void On_TryAnotherMethodButtonClicked()
        {
            Result = ViewResult.Back;
            CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(Result.ToString());
        }
    }
}
