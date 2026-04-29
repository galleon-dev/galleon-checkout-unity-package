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
            Pay,
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
        public TMP_Text             PurchaseButtonText;

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
            CHECKOUT.PaymentMethods.SelectAppUserPaymentMethodForPreselection().Execute();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Refresh

        public async override void RefreshState()
        {
            if (CheckoutClient.Instance.CurrentSession == null) return;
            
            this.ProductTitleText.text = Checkout.CheckoutClient.Instance.CurrentSession.SelectedProduct.DisplayName;

            ///////////////

            // Remove children (if any)
            // Debug.Log("<color=orange>- Removing Payment Methods</color>");
            foreach (Transform child in PaymentMethodsPanel.transform)
            {
                // Debug.Log($"-Removing Item {child.gameObject.name}");
                Destroy(child.gameObject);
            }
            
            if (CHECKOUT.Globals.IsLastUsedPaymentMethodInPreselectionEnabled
            &&  CHECKOUT.PaymentMethods.LastUsedUserPaymentMethod != null)
            {
                var lastUsed = CHECKOUT.PaymentMethods.LastUsedUserPaymentMethod;
                
                // Instantiate
                var go   = Instantiate(original: PreselectionItemPrefab, parent: PaymentMethodsPanel.transform);
                var item = go.GetComponent<PreselectionPanelItemView>();
                item.Initialize(lastUsed, this);
            
                // Add ui separator
                Instantiate(original: CHECKOUT.Resources.UI_Seporator, parent: PaymentMethodsPanel.transform);
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
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Events

        public void OnConfirmPurchaseClick()
        {
            // CHECKOUT.User.SelectedUserPaymentMethod.Unselect();
            
            if (CHECKOUT.PaymentMethods.RealPaymentMethods.Contains(CHECKOUT.PaymentMethods.SelectedUserPaymentMethod))
                this.Result = ViewResult.Pay;
            else
                this.Result = ViewResult.Confirm;
            
            CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(Result.ToString());
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Methods

        public void SetPurchaseButtonSprite(UserPaymentMethod selectedPaymentMethod)
        {
            var image    = this.PurchaseButton.GetComponentInChildren<Image>();
            image.sprite = selectedPaymentMethod.GetButtonSprite();
            
            if (selectedPaymentMethod.Type == "app")
            {
                PurchaseButtonText.text = CHECKOUT.Config.GetString("preselection_app_button_text", defaultValue: "Continue with extra rolls");
            }
            else if (selectedPaymentMethod.Type == "native")
            {
                PurchaseButtonText.text = CHECKOUT.Config.GetString("preselection_native_button_text", defaultValue: "Continue");
            }
            else
            {
                PurchaseButtonText.text = CHECKOUT.Config.GetString("preselection_other_button_text", defaultValue: "");
            }
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Test Methods
        
        public Step test_preselect_checkout() => new Step(name : "preselection_panel_test_checkout", action : async (s) =>
                                                                                                            {
                                                                                                                CHECKOUT.PaymentMethods.SelectPaymentMethod(CHECKOUT.PaymentMethods.UserPaymentMethods.First(x => x.Type == "app"));
                                                                                                                OnConfirmPurchaseClick();
                                                                                                            });
    }
}

