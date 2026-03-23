using System;
using TMPro;
using UnityEngine;

namespace Galleon.Checkout
{
    public class OtherPaymentMethodsElement : MonoBehaviour
    {
        public TMP_Text OtherPaymentMethodsDescriptionText;

        public void Refresh()
        {
            this.OtherPaymentMethodsDescriptionText.text = CHECKOUT.Globals.OtherPaymentMethodsDescription;
        }
    }
}
