using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{
    public class LoadingPanelView : View
    {
        public Image    LadingImage;
        public TMP_Text LadingText;
        
        public float rotationSpeed = 100f;

        ///////
        
        public      ViewResult Result = ViewResult.None;
        public enum ViewResult
        {
            None,
            Success,
            Error,
        }
        
        ////////
        
        private void Update()
        {
            LadingImage.transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }

        public void FinishLoading()
        {
            this.Result = ViewResult.Success;
            CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(Result.ToString());
        }
    }
}
