using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Galleon.Checkout.Foundation;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{
    public class CheckoutPanelViewLandscape : View
    {

        [Header("Shop Item")]
        public TextMeshProUGUI ProductTitleText;
        public TextMeshProUGUI PriceText;
        public TextMeshProUGUI TaxText;

        public override void Initialize()
        {
            Debug.Log("Initialize - CheckoutPanelViewLandscape()");
            RefreshState();
        }

        public override void RefreshState()
        {
            if (CheckoutClient.Instance.CurrentSession == null)
                return;

            this.ProductTitleText.text = Checkout.CheckoutClient.Instance.CurrentSession.SelectedProduct.DisplayName;
            this.PriceText.text = Checkout.CheckoutClient.Instance.CurrentSession.SelectedProduct.PriceText;
        }

    }
}