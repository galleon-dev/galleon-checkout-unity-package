using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Galleon.Checkout.UI
{
    public class CheckoutPanelView : View
    {
        //////////////////////////////////////////////////////////////////////////// View Result
        
        public      ViewResult Result = ViewResult.None;
        public enum ViewResult
        {
            None,
            Back,
            Confirm,
            Settings,
            OtherPaymentMethods,
        }
        
        //////////////////////////////////////////////////////////////////////////// Members
        
        public TextMeshProUGUI ProductTitleText;
        public TextMeshProUGUI PriceText;
        
        //////////////////////////////////////////////////////////////////////////// View Flow
        
        public bool IsCompleted = false;
        
        public Step View()
        =>
            new Step(name   : $"View"
                    ,action : async (s) =>
                    {
                        IsCompleted = false;
                        
                        this.gameObject.SetActive(true);
                        
                        // TEMP
                        //await s.CaptureReport();
                        this.Flow.AddChildStep(Refresh());
                        this.Flow.AddChildStep(name:"capture report", s => s.CaptureReport());
                        this.Flow.AddChildStep(name: "wait a second", action: async s => await Task.Delay(5000));
                        
                        await this.Flow;
                        //while (!IsCompleted)
                        //    await Task.Yield();
                        
                        this.gameObject.SetActive(false);
                    });
        
        //////////////////////////////////////////////////////////////////////////// Refresh
        
        public Step Refresh()
        =>
            new Step(name   : $"Refresh"
                    ,action : async (s) =>
                    {
                        this.ProductTitleText.text = Checkout.CheckoutClient.Instance.CurrentSession.SelectedProduct.DisplayName;
                        this.PriceText.text        = Checkout.CheckoutClient.Instance.CurrentSession.SelectedProduct.PriceText;
                    });
        
        //////////////////////////////////////////////////////////////////////////// UI Events
        
        public void OnConfirmPurchaseClick()
        {
            Result      = ViewResult.Confirm;
            IsCompleted = true;
        }
        
        public void OnSettingsClick()
        {
            Result      = ViewResult.Settings;
            IsCompleted = true;
        }
        
        public void OnOtherPaymentMethodsClick()
        {
            Result      = ViewResult.OtherPaymentMethods;
            IsCompleted = true;
        }
    }
}
