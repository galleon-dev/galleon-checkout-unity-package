using System.Threading.Tasks;
using UnityEngine;

namespace Galleon.Checkout.UI
{
    public class SelectPaymentMethodPanelView : View
    {
        //////////////////////////////////////////////////////////////////////////// Members

        public GameObject SelectPaymentMethodItemPrefab;
        public GameObject SelectPaymentMethodItemsHolder;


        //////////////////////////////////////////////////////////////////////////// View Result

        public ViewResult Result = ViewResult.None;
        public UnityEngine.UI.LayoutElement ScrollRectLayoutElement;
        int ScrollRectMaxSize = 3;
        float PaymentPrefabHeight = 200f;
        float SeparatorHeight = 2f;

        public enum ViewResult
        {
            None,
            NewCard,
            Selected,
        }

        //////////////////////////////////////////////////////////////////////////// Initialization

        public override void Initialize()
        {
            // Remove children (if any)
            foreach (Transform child in SelectPaymentMethodItemsHolder.transform)
            {
                Debug.Log($"-Removing Item {child.gameObject.name}");
                Destroy(child.gameObject);
            }

            // Add first child (with null payment method = "Add new credit card")
            {
                var go = Instantiate(original: SelectPaymentMethodItemPrefab, parent: SelectPaymentMethodItemsHolder.transform);
                var item = go.GetComponent<SelectPaymentMethodPanelItem>();
                item.Initialize(userPaymentMethod:null, SelectPaymentMethodPanelView: this);
                
                // Add ui separator
                Instantiate(original: CHECKOUT.Resources.UI_Seporator, parent: SelectPaymentMethodItemsHolder.transform);
            }

            // Add children
            var paymentMethods = CheckoutClient.Instance.CurrentUser.PaymentMethods;
            foreach (var paymentMethod in paymentMethods)
            {
                var go = Instantiate(original: SelectPaymentMethodItemPrefab, parent: SelectPaymentMethodItemsHolder.transform);
                var item = go.GetComponent<SelectPaymentMethodPanelItem>();

                item.Initialize(paymentMethod, this);

                // Add ui separator
                Instantiate(original: CHECKOUT.Resources.UI_Seporator, parent: SelectPaymentMethodItemsHolder.transform);
            }
            UpdateScrollRectMaxSize();
        }

        //////////////////////////////////////////////////////////////////////////// Refresh

        public override void RefreshState()
        {
            // Remove children (if any)
            foreach (Transform child in SelectPaymentMethodItemsHolder.transform)
            {
                Debug.Log($"-Removing Item {child.gameObject.name}");
                Destroy(child.gameObject);
            }

            // Add first child (with null payment method = "Add new credit card")
            {
                var go = Instantiate(original: SelectPaymentMethodItemPrefab, parent: SelectPaymentMethodItemsHolder.transform);
                var item = go.GetComponent<SelectPaymentMethodPanelItem>();
                item.Initialize(userPaymentMethod:null, SelectPaymentMethodPanelView: this);
                
                // Add ui separator
                Instantiate(original: CHECKOUT.Resources.UI_Seporator, parent: SelectPaymentMethodItemsHolder.transform);
            }

            // Add children
            var paymentMethods = CheckoutClient.Instance.CurrentUser.PaymentMethods;
            foreach (var paymentMethod in paymentMethods)
            {
                var go = Instantiate(original: SelectPaymentMethodItemPrefab, parent: SelectPaymentMethodItemsHolder.transform);
                var item = go.GetComponent<SelectPaymentMethodPanelItem>();

                item.Initialize(paymentMethod, this);

                // Add ui separator
                Instantiate(original: CHECKOUT.Resources.UI_Seporator, parent: SelectPaymentMethodItemsHolder.transform);
            }

            UpdateScrollRectMaxSize();
        }

        public void UpdateScrollRectMaxSize()
        {
            int PaymentMethodsAmount = CheckoutClient.Instance.CurrentUser.PaymentMethods.Count;

            Debug.Log("<color=green>UpdateScrollRectMaxSize(): </color>" + PaymentMethodsAmount);

            if (PaymentMethodsAmount == 0)
            {
                ScrollRectLayoutElement.preferredHeight = 0;
            }
            else if (PaymentMethodsAmount <= ScrollRectMaxSize)
            {
                ScrollRectLayoutElement.preferredHeight = PaymentMethodsAmount * (PaymentPrefabHeight + SeparatorHeight) + 2;
            }
            else
            {
                ScrollRectLayoutElement.preferredHeight = ScrollRectMaxSize * (PaymentPrefabHeight + SeparatorHeight) + 2;
            }
        }


        //////////////////////////////////////////////////////////////////////////// View Flow

        public bool IsCompleted = false;

        public Step View()
        =>
            new Step(name: $"view_select_payment_methods_panel"
                    , action: async (s) =>
                    {
                        IsCompleted = false;

                        this.gameObject.SetActive(true);

                        while (!IsCompleted)
                            await Task.Yield();

                        this.gameObject.SetActive(false);
                    });

        //////////////////////////////////////////////////////////////////////////// UI Events

        public void On_NewCardClicked()
        {
            this.Result = ViewResult.NewCard;
            CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(this.Result.ToString());
        }

        public void On_Select()
        {
            this.Result = ViewResult.Selected;
            CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(this.Result.ToString());
        }
    }
}