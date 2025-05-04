using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Galleon.Checkout.UI
{
    public class TestPanelView : View
    {
        //////////////////////////////////////////////////////////////////////////// View Result
        
        public      ViewResult Result = ViewResult.None;
        public enum ViewResult
        {
            None,
            Confirm,
        }
        
        //////////////////////////////////////////////////////////////////////////// UI Events
        
        public void On_OKButtonClicked()
        {    
            this.Result = ViewResult.Confirm;
            CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(Result.ToString());
        }
        
        public void On_SwapUIButtonClicked()
        {
            this.SwapUI();
        }
        
        //////////////////////////////////////////////////////////////////////////// Test Methods
        
        [ContextMenu("Test Swap Style")]
        public void SwapUI()
        {
            string lightStyle = 
            @".bg_img {
                background-color: white;
            }
            .text {
                color: black;
            }";
            
            string darkStyle = 
            @".bg_img {
                background-color: black;
            }
            .text {
                color: white;
            }";
            
            if (this.Style == lightStyle)
                this.Style = darkStyle;
            else if (this.Style == darkStyle)
                this.Style = lightStyle;
            else
                this.Style = lightStyle;
            
            RefreshUI();
        }
        
    }
}
