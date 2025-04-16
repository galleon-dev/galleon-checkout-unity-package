using System.Text;
using System.Threading.Tasks;
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
        
        //////////////////////////////////////////////////////////////////////////// View Flow
        
        public bool IsCompleted = false;
        
        public Step View()
        =>
            new Step(name   : $"View"
                    ,action : async (s) =>
                    {
                        IsCompleted = false;
                        
                        this.gameObject.SetActive(true);
                        
                        while (!IsCompleted)
                            await Task.Yield();
                        
                        this.gameObject.SetActive(false);
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
