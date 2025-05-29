using System.Threading.Tasks;
using AdvancedInputFieldPlugin;
using Galleon.Checkout;
using Galleon.Checkout.UI;
using TMPro;
using UnityEngine;

public class SettingsPanelView : View
{
    //////////////////////////////////////////////////////////////////////////// Members
    
    [Header("Email")]
    public TMP_Text           EmailLabel;
    public AdvancedInputField EmailInputField;
    
    [Header("Payment Methods")]
    public GameObject     SettingsPanelPaymentMethodItemPrefab;
    public GameObject     PaymentMethodsHolder;
    public bool           IsEditingEmail = false;
    
    //////////////////////////////////////////////////////////////////////////// View Result
    
    public      ViewResult Result = ViewResult.None;
    public enum ViewResult
    {
        None,
        Back,
        Close,
        DeletePaymentMethod,
    }
    
    //////////////////////////////////////////////////////////////////////////// Initialization
            
    public override void Initialize()
    {
        // Remove children (if any)
        foreach (Transform child in PaymentMethodsHolder.transform)
        {
            Debug.Log($"-Removing Item {child.gameObject.name}");
            Destroy(child.gameObject);
        }
        
        // Add children
        var paymentMethods = CheckoutClient.Instance.CurrentUser.PaymentMethods;
        foreach (var paymentMethod in paymentMethods)
        {
            var go   = Instantiate(original : SettingsPanelPaymentMethodItemPrefab, parent : PaymentMethodsHolder.transform);
            var item = go.GetComponent<SettingsPanelPaymentMethodItem>();
            item.Initialize(paymentMethod, this);
            
            // Add ui separator
            Instantiate(original : CHECKOUT.Resources.UI_Seporator, parent : PaymentMethodsHolder.transform);
        }
    }
        
    //////////////////////////////////////////////////////////////////////////// View Flow

    public bool IsCompleted = false;

    public Step View()
    =>
        new Step(name   : $"view_settings_panel"
                ,action : async (s) =>
                {
                    IsCompleted = false;
                    
                    this.gameObject.SetActive(true);
                    
                    while (!IsCompleted)
                        await Task.Yield();
                    
                    this.gameObject.SetActive(false);
                });
    
    //////////////////////////////////////////////////////////////////////////// Refresh
    
    public override void RefreshState()
    {
        // Remove children (if any)
        foreach (Transform child in PaymentMethodsHolder.transform)
        {
            //Debug.Log($"-Removing Item {child.gameObject.name}");
            Destroy(child.gameObject);
        }
        
        // Add children
        var paymentMethods = CheckoutClient.Instance.CurrentUser.PaymentMethods;
        foreach (var paymentMethod in paymentMethods)
        {
            var go   = Instantiate(original : SettingsPanelPaymentMethodItemPrefab, parent : PaymentMethodsHolder.transform);
            var item = go.GetComponent<SettingsPanelPaymentMethodItem>();
            item.Initialize(paymentMethod, this);
            
            // Add ui separator
            Instantiate(original : CHECKOUT.Resources.UI_Seporator, parent : PaymentMethodsHolder.transform);
        }
    }
    
    //////////////////////////////////////////////////////////////////////////// UI Events
    
    public void On_EditEmailClicked()
    {
        if (!IsEditingEmail)
        {
            EmailLabel     .gameObject.SetActive(false);
            EmailInputField.gameObject.SetActive(true);
            IsEditingEmail = true;
        }
        else
        {
            EmailLabel     .gameObject.SetActive(true);
            EmailInputField.gameObject.SetActive(false);
            IsEditingEmail = false;
        }
    }
    
    public void On_FinishedEditingEmail(string str, EndEditReason reason)
    {
        Debug.Log($"str = {str}");
        Debug.Log($"reason = {reason}");
        
        EmailLabel     .gameObject.SetActive(true);
        EmailInputField.gameObject.SetActive(false);
        IsEditingEmail = false;
        
        this.EmailLabel.text = str;
    }
    
    //////////////////////////////////////////////////////////////////////////// Events
    
    public void DeletePaymentMethod(PaymentMethod paymentMethod)
    {
        if (IsEditingEmail)
            return;
        
        CheckoutClient.Instance.CurrentSession.LastDialogRequest     = "delete_payment_method";
        CheckoutClient.Instance.CurrentSession.PaymentMethodToDelete = paymentMethod;
        
        this.Result = ViewResult.DeletePaymentMethod;
        CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(this.Result.ToString());
    }
}
