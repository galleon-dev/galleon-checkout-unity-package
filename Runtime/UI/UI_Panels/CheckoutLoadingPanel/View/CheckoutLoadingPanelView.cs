namespace Galleon.Checkout.UI
{
    public class CheckoutLoadingPanelView : View
    {
        public ViewResult Result = ViewResult.None;
        public enum ViewResult
        {
            None,
            Success,
            Error,
        }

        public void FinishLoading()
        {
            this.Result = ViewResult.Success;
            CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(Result.ToString());
        }
    }
}