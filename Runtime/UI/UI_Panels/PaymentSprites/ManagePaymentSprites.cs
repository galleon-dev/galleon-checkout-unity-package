using Galleon.Checkout;
using Galleon.Checkout.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{
    public class ManagePaymentSprites : View
    {
        [Header("Sprites")]
        public Sprite AddCreditCardSprite;
        public Sprite VisaSprite;
        public Sprite MasterCardSprite;
        public Sprite AmexSprite;
        public Sprite DinersSprite;
        public Sprite DiscoverSprite;
        public Sprite GPaySprite;
        public Sprite PaypalSprite;
        public Sprite AppleSprite;

        public Sprite GetPaymentIcon(UserPaymentMethod UserPaymentMethod)
        {
            Sprite Icon = null;

            Debug.Log("GetPaymentIcon(). Type: " + UserPaymentMethod.Type + "  IsSelected: " + UserPaymentMethod.IsSelected);

            /* if (this.UserPaymentMethod == null)
             {
                 this.Label.text = "Add Credit or Debit Card";
                 this.Icon.sprite = AddCreditCardSprite;
             }
            */
            if (UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.Visa.ToString())
                Icon = VisaSprite;
            else if (UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.MasterCard.ToString())
                Icon = MasterCardSprite;
            else if (UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.Amex.ToString())
                Icon = AmexSprite;
            else if (UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.Diners.ToString())
                Icon = DinersSprite;
            else if (UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.Discover.ToString())
                Icon = DiscoverSprite;
            else if (UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.GPay.ToString())
                Icon = GPaySprite;
            else if (UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.PayPal.ToString())
                Icon = PaypalSprite;
            else if (UserPaymentMethod.Type == UserPaymentMethod.PaymentMethodType.Apple.ToString())
                Icon = AppleSprite;

            return Icon;
        }
    }
}