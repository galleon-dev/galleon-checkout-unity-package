using System.Collections;
using System.Collections.Generic;
using Galleon.Checkout;
using TMPro;
using UnityEngine;

namespace Galleon.SampleApp
{
    public class StoreViewItem : MonoBehaviour
    {
        // Members
        
        public CheckoutProduct Product;
        public TextMeshProUGUI TitleText;

        public async void OnItemClick()
        {
            // CheckoutAPI.Purchase(CheckoutProduct);
        }
    }
}


