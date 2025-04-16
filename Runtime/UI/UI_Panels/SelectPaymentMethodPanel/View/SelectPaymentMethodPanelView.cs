using System.Threading.Tasks;
using UnityEngine;

namespace Galleon.Checkout.UI
{
    public class SelectPaymentMethodPanelView : View
    {
        //////////////////////////////////////////////////////////////////////////// View Flow
    
        public bool IsCompleted = false;
        
        public Step View()
        =>
            new Step(name   : $"view_select_payment_methods_panel"
                    ,action : async (s) =>
                    {
                        IsCompleted = false;
                        
                        this.gameObject.SetActive(true);
                        
                        while (!IsCompleted)
                            await Task.Yield();
                        
                        this.gameObject.SetActive(false);
                    });
    }
}