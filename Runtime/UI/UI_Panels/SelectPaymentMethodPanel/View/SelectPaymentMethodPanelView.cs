using System.Threading.Tasks;
using Galleon.Checkout.Foundation;
using UnityEngine;

namespace Galleon.Checkout.UI
{
    public class SelectPaymentMethodPanelView : View
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members

        public  GameObject SelectPaymentMethodItemPrefab;
        public  GameObject SelectPaymentMethodItemsHolder;

        private int        ScrollRectMaxSize   = 3;
        private float      PaymentPrefabHeight = 200f;
		private float      PaymentPrefabHeightLandscape = 125f;
        private float      SeparatorHeight     = 2f;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// View Result

        public ViewResult                   Result = ViewResult.None;
        public UnityEngine.UI.LayoutElement ScrollRectLayoutElement;

        public enum ViewResult
        {
            None,
            NewCard,
            Selected,
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Initialization

        public override void Initialize()
        {
            RefreshState();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Refresh

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
                var go   = Instantiate(original: SelectPaymentMethodItemPrefab, parent: SelectPaymentMethodItemsHolder.transform);
                var item = go.GetComponent<SelectPaymentMethodPanelItem>();
                item.Initialize(paymentMethodDefinition:null, SelectPaymentMethodPanelView: this);
                
                // Add ui separator
                Instantiate(original: CHECKOUT.Resources.UI_Seporator, parent: SelectPaymentMethodItemsHolder.transform);
            }

            // Add payment method definitions children
            var paymentMethodDefinitions = CHECKOUT.PaymentMethods.PaymentMethodsDefinitions;
            foreach (var paymentMethod in paymentMethodDefinitions)
            {
                var go   = Instantiate(original: SelectPaymentMethodItemPrefab, parent: SelectPaymentMethodItemsHolder.transform);
                var item = go.GetComponent<SelectPaymentMethodPanelItem>();

                item.Initialize(paymentMethodDefinition:paymentMethod, this);

                // Add ui separator
                Instantiate(original: CHECKOUT.Resources.UI_Seporator, parent: SelectPaymentMethodItemsHolder.transform);
            }
            
            // Add user payment methods children
            var userPaymentMethods = CHECKOUT.PaymentMethods.UserPaymentMethods;
            foreach (var userPaymentMethod in userPaymentMethods)
            {
                var go   = Instantiate(original: SelectPaymentMethodItemPrefab, parent: SelectPaymentMethodItemsHolder.transform);
                var item = go.GetComponent<SelectPaymentMethodPanelItem>();

                item.Initialize(userPaymentMethod:userPaymentMethod, this);

                // Add ui separator
                Instantiate(original: CHECKOUT.Resources.UI_Seporator, parent: SelectPaymentMethodItemsHolder.transform);
            }
            
            UpdateScrollRectMaxSize();
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// View Flow

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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Events

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
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helper Methods
        
        
        public void UpdateScrollRectMaxSize()
        {
            int PaymentMethodsAmount = CHECKOUT.PaymentMethods.UserPaymentMethods.Count;

            float PrefabHeight = PaymentPrefabHeight;

            if (CheckoutClient.Instance.CheckoutScreenMobile.IsLandscape)
            {
                PrefabHeight = PaymentPrefabHeightLandscape;
            }

            Debug.Log("<color=green>UpdateScrollRectMaxSize(): </color>" + PaymentMethodsAmount);

            if (PaymentMethodsAmount == 0)
            {
                ScrollRectLayoutElement.preferredHeight = 0;
            }
            else if (PaymentMethodsAmount <= ScrollRectMaxSize)
            {

                ScrollRectLayoutElement.preferredHeight = PaymentMethodsAmount * (PrefabHeight + SeparatorHeight) + 2;
            }
            else
            {
                ScrollRectLayoutElement.preferredHeight = ScrollRectMaxSize * (PrefabHeight + SeparatorHeight) + 2;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Test Scenarios
        
        public TestScenario scenario_2_part_1 => new TestScenario(expressions : new[] { $"{nameof(test_add_new_card)}()" });
        public TestScenario scenario_2_part_2 => new TestScenario(expressions : new[] { $"{nameof(test_back_to_checkout)}()" });
        
        public Step test_add_new_card()     => new Step(action : async (s) => { On_NewCardClicked(); });
        public Step test_back_to_checkout() => new Step(action : async (s) => { On_Select();         });
        
    }
}