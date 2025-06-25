using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{
    public class CheckoutPanelView : View
    {
        //////////////////////////////////////////////////////////////////////////// View Result
        
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
        
        //////////////////////////////////////////////////////////////////////////// Members
        
        [Header("Shop Item")]
        public TextMeshProUGUI ProductTitleText;
        public TextMeshProUGUI PriceText;
        
        [Header("Payment Methods")]
        public GameObject      PaymentMethodsPanel;
        public GameObject      PaymentMethodItemPrefab;
        
        public GameObject      AddCreditCardButtonElement;


        [Header("Payment Buttons")]
        public GameObject PurchaseButton;
        public GameObject GooglePayButton;

        //////////////////////////////////////////////////////////////////////////// Links

        public IEnumerable<checkoutPanelPaymentMethodItemView> PaymentMethodItemViews => GetComponentsInChildren<checkoutPanelPaymentMethodItemView>();

        //////////////////////////////////////////////////////////////////////////// Initialization

        public override void Initialize()
        {
            // Remove children (if any)
            foreach (Transform child in PaymentMethodsPanel.transform)
            {
                Debug.Log($"-Removing Item {child.gameObject.name}");
                Destroy(child.gameObject);
            }
            
            // Add children
            var paymentMethods = CheckoutClient.Instance.CurrentUser.PaymentMethods;

            Instantiate(original: CHECKOUT.Resources.UI_Seporator, parent: PaymentMethodsPanel.transform);
            
            foreach (var paymentMethod in paymentMethods)
            {
                var go   = Instantiate(original : PaymentMethodItemPrefab, parent : PaymentMethodsPanel.transform);
                var item = go.GetComponent<checkoutPanelPaymentMethodItemView>();
                item.Initialize(paymentMethod, this);

                // Add ui separator
                Instantiate(original : CHECKOUT.Resources.UI_Seporator, parent : PaymentMethodsPanel.transform);
            }

            // Add defult add card button
            this.AddCreditCardButtonElement.SetActive(paymentMethods.Count == 0);
        }
        
        //////////////////////////////////////////////////////////////////////////// View Flow
        
        public bool IsCompleted = false;
        
        public Step View()
        =>
            new Step(name   : $"View"
                    ,action : async (s) =>
                    {
                        IsCompleted = false;
                        
                        Refresh();
                        
                        this.gameObject.SetActive(true);
                        
                        // TEMP
                        //await s.CaptureReport();
                      //this.Flow.AddChildStep(Refresh());
                        this.Flow.AddChildStep(name:"capture report", s => s.CaptureReport());
                        this.Flow.AddChildStep(name: "wait a second", action: async s => await Task.Delay(5000));
                        
                        await this.Flow;
                        //while (!IsCompleted)
                        //    await Task.Yield();
                        
                        this.gameObject.SetActive(false);
                    });
        
        //////////////////////////////////////////////////////////////////////////// Refresh

        public override void RefreshState()
        {
            this.ProductTitleText.text = Checkout.CheckoutClient.Instance.CurrentSession.SelectedProduct.DisplayName;
            this.PriceText.text        = Checkout.CheckoutClient.Instance.CurrentSession.SelectedProduct.PriceText;
         
            ///////////////
            
            // Remove children (if any)
            foreach (Transform child in PaymentMethodsPanel.transform)
            {
                Debug.Log($"-Removing Item {child.gameObject.name}");
                Destroy(child.gameObject);
            }
            
            // Add children
            var paymentMethods = CheckoutClient.Instance.CurrentUser.PaymentMethods;
            foreach (var paymentMethod in paymentMethods)
            {
                var go   = Instantiate(original : PaymentMethodItemPrefab, parent : PaymentMethodsPanel.transform);
                var item = go.GetComponent<checkoutPanelPaymentMethodItemView>();   
                item.Initialize(paymentMethod, this);
                
                // Add ui separator
                Instantiate(original : CHECKOUT.Resources.UI_Seporator, parent : PaymentMethodsPanel.transform);
            }
            
            // Add defult add card button
            this.AddCreditCardButtonElement.SetActive(paymentMethods.Count == 0);
            
            ///////////////
            
            // checkoutPanelPaymentMethodItemView[] methods = this.gameObject.GetComponentsInChildren<checkoutPanelPaymentMethodItemView>();
            // foreach (var method in methods)
            //     method.Refresh();
        }

        //////////////////////////////////////////////////////////////////////////// Radio Buttons
        
        public void OnRadiobuttonSelected(checkoutPanelPaymentMethodItemView SelectedItem)
        {
            foreach (var item in PaymentMethodItemViews)
            {
                if (item == SelectedItem)
                    continue;
                
                item.Unselect();
            }
        }
        
        //////////////////////////////////////////////////////////////////////////// UI Events
        
        public void OnConfirmPurchaseClick()
        {
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

        public void ShowPurchaseButton()
        {
            GooglePayButton.SetActive(false);
            PurchaseButton.SetActive(true);
        }

        public void ShowGooglePlayButton()
        {
            GooglePayButton.SetActive(true);
            PurchaseButton.SetActive(false);
        }
    }
}
