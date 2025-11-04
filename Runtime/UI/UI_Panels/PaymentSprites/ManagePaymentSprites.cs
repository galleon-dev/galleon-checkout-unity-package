using Galleon.Checkout;
using Galleon.Checkout.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{
    public class ManagePaymentSprites : View
    {
        public Sprite GetPaymentIcon(UserPaymentMethod UserPaymentMethod)
        {
            Sprite Icon = UserPaymentMethod.GetIconSprite();
            return Icon;
        }
    }
}