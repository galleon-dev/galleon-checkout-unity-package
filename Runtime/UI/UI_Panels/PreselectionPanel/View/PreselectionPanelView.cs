using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{
    public class PreselectionPanelView : View
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// View Result

        public      ViewResult Result = ViewResult.None;
        public enum ViewResult
        {
            None,
            Back,
            Confirm,
            Settings,
            AddCard,
            OtherPaymentMethods,
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members

        public PositionLayoutGroup  PositionLayoutGroup;

        [Header("Shop Item")]
        public TextMeshProUGUI      ProductTitleText;
        public TextMeshProUGUI      PriceText;
        public TextMeshProUGUI      TaxText;

        [Header("Payment Methods")]
        public GameObject           PaymentMethodsPanel;
        public GameObject           PreselectionItemPrefab;        

        [Header("Payment Buttons")]
        public GameObject           PurchaseButton;
        public GameObject           GooglePayButton;
        public GameObject           PaypalPayButton;
        public GameObject           ApplePayButton;

        public  Config              Configutation;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helper Types
        
        [Serializable]
        public class Config
        {
            public bool ShowMinimalOptions = false;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Links

        public IEnumerable<PreselectionPanelItemView> PaymentMethodItemViews => GetComponentsInChildren<PreselectionPanelItemView>();

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public override void Initialize()
        {
            
        }

        public void OnEnable()
        {
            CHECKOUT.PaymentMethods.UserPaymentMethods.First(x => x.Type == "app").SelectExclusive();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Refresh

        public async override void RefreshState()
        {
            if (CheckoutClient.Instance.CurrentSession == null) return;
            
            this.ProductTitleText.text = Checkout.CheckoutClient.Instance.CurrentSession.SelectedProduct.DisplayName;
            this.PriceText.text        = Checkout.CheckoutClient.Instance.CurrentSession.SelectedProduct.PriceText;

            ///////////////

            // Remove children (if any)
            // Debug.Log("<color=orange>- Removing Payment Methods</color>");
            foreach (Transform child in PaymentMethodsPanel.transform)
            {
                // Debug.Log($"-Removing Item {child.gameObject.name}");
                Destroy(child.gameObject);
            }

            // Add children
            var paymentMethods = CHECKOUT.PaymentMethods.PreselectionUserPaymentMethods;
            paymentMethods.Reverse();
            foreach (var paymentMethod in paymentMethods)
            {
                // Instantiate
                var go   = Instantiate(original: PreselectionItemPrefab, parent: PaymentMethodsPanel.transform);
                var item = go.GetComponent<PreselectionPanelItemView>();
                item.Initialize(paymentMethod, this);
            
                // Add ui separator
                Instantiate(original: CHECKOUT.Resources.UI_Seporator, parent: PaymentMethodsPanel.transform);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)PaymentMethodsPanel.transform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.gameObject.transform as RectTransform);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Radio Buttons

        public void OnRadiobuttonSelected(PreselectionPanelItemView SelectedItem)
        {
            foreach (var item in PaymentMethodItemViews)
            {
                if (item == SelectedItem)
                    continue;

                item.Unselect();
            }
            
            //ShowPurchaseButton();
            var image    = PurchaseButton.gameObject.GetComponent<Image>();
            image.sprite = CHECKOUT.PaymentMethods.PaymentMethodsDefinitions.First().LogoSprite;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Events

        public void OnConfirmPurchaseClick()
        {
            // CHECKOUT.User.SelectedUserPaymentMethod.Unselect();
            
            this.Result = ViewResult.Confirm;
            CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(Result.ToString());
        }

        public void OnSettingsClick()
        {
            this.Result = ViewResult.Settings;
            CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(Result.ToString());
        }

        public void OnOtherPaymentMethodsClick()
        {
            this.Result = ViewResult.OtherPaymentMethods;
            CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(Result.ToString());
        }

        public void On_AddCardClicked()
        {
            this.Result = ViewResult.AddCard;
            CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(Result.ToString());
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Methods

        public void SetPurchaseButtonSprite(Sprite sprite)
        {
            var image    = this.PurchaseButton.GetComponentInChildren<Image>();
            image.sprite = sprite;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Test Methods
        
        public Step test_preselect_checkout() => new Step(name : "preselection_panel_test_checkout", action : async (s) =>
                                                                                                            {
                                                                                                                Debug.Log($"Aaaaa {DateTime.Now}");
                                                                                                                CHECKOUT.User.SelectPaymentMethod(CHECKOUT.PaymentMethods.UserPaymentMethods.First(x => x.Type == "app"));
                                                                                                                OnConfirmPurchaseClick();
                                                                                                            });
    }
}

