using System.Linq;
using System.Threading.Tasks;
using AdvancedInputFieldPlugin;
using Galleon.Checkout;
using Galleon.Checkout.Foundation;
using Galleon.Checkout.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelView : View
{
    //////////////////////////////////////////////////////////////////////////// Members
    
    [Header("Email")]
    public GameObject         EmailInputfieldBorder;
    public AdvancedInputField EmailInputField;

    [Header("Payment Methods")]
    public GameObject         SettingsPanelPaymentMethodItemPrefab;
    public GameObject         PaymentMethodsHolder;
    public bool               IsEditingEmail = false;
    
    //////////////////////////////////////////////////////////////////////////// View Result
    
    public ViewResult         Result = ViewResult.None;
    
    public LayoutElement      ScrollRectLayoutElement;
    public ScrollRect         ScrollRect;
    private int               ScrollRectMaxSize   = 6;
    private float             PaymentPrefabHeight = 175f;
    private float             SeparatorHeight     = 2f;

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

        string Email = PlayerPrefs.GetString("Email");

        if (!string.IsNullOrEmpty(Email))
        {
            if (EmailInputField)
            {
                EmailInputField.Text = Email;
            }
        }

        RefreshState();
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
        var paymentMethods = CHECKOUT.PaymentMethods.UserPaymentMethodsToRemove;
        foreach (var paymentMethod in paymentMethods)
        {
            var go   = Instantiate(original : SettingsPanelPaymentMethodItemPrefab, parent : PaymentMethodsHolder.transform);
            var item = go.GetComponent<SettingsPanelPaymentMethodItem>();
            item.Initialize(paymentMethod, this);
            
            // Add ui separator
            Instantiate(original : CHECKOUT.Resources.UI_Seporator, parent : PaymentMethodsHolder.transform);
        }
        
        // Email
        if (!CHECKOUT.User.Email.IsNullOrEmpty())
            this.EmailInputField.Text = CHECKOUT.User.Email;

        UpdateScrollRectMaxSize();
    }
    
    //////////////////////////////////////////////////////////////////////////// UI Helper methods
    
    public void UpdateScrollRectMaxSize()
    {
        int PaymentMethodsAmount = CHECKOUT.PaymentMethods.UserPaymentMethods.Count;

        // Debug.Log("<color=green>UpdateScrollRectMaxSize(): </color>" + PaymentMethodsAmount);

        if (PaymentMethodsAmount <= 1)
        {
            if (ScrollRect)
            {
                ScrollRect.vertical = false;
            }
        } 
        else
        {
            if (ScrollRect)
            {
                ScrollRect.vertical = true;
            }
        }

        if (PaymentMethodsAmount == 0)
        {
            if (ScrollRectLayoutElement)
            {
                ScrollRectLayoutElement.preferredHeight = 0;
            }
        }
        else if (PaymentMethodsAmount <= ScrollRectMaxSize)
        {
            if (ScrollRectLayoutElement)
            {
                ScrollRectLayoutElement.preferredHeight = PaymentMethodsAmount * (PaymentPrefabHeight + SeparatorHeight) + 2;
            }
        }
        else
        {
            if (ScrollRectLayoutElement)
            {
                ScrollRectLayoutElement.preferredHeight = ScrollRectMaxSize * (PaymentPrefabHeight + SeparatorHeight) + 2;
            }
        }
    }

    //////////////////////////////////////////////////////////////////////////// UI Events

    public void On_EditEmailClicked()
    {
        if (!IsEditingEmail)
        {
            EmailInputfieldBorder.SetActive(true);
            EmailInputField.interactable = true;
            IsEditingEmail = true;
        }
        else
        {
            EmailInputfieldBorder.SetActive(false);       
            EmailInputField.interactable = false;
            IsEditingEmail = false;
        }
    }
    
    public async void On_FinishedEditingEmail(string str, EndEditReason reason)
    {
        Debug.Log($"str = {str}");
        Debug.Log($"reason = {reason}");

        EmailInputfieldBorder.SetActive(false);
        EmailInputField.interactable = false;
        IsEditingEmail               = false;
        
        this.EmailInputField.Text            = str;
        CHECKOUT.Session.User.UserInfo.email = str;
        await CHECKOUT.Actions.SetEmail().Execute();
        
        // if(SuccessPanelEmailInputField)
        // {
        //     SuccessPanelEmailInputField.Text = EmailInputField.Text;
        // 
        //     if(string.IsNullOrEmpty(SuccessPanelEmailInputField.Text))
        //     {
        //         SuccessPanelView.ShowEmail(false);
        //     } else
        //     {
        //         SuccessPanelView.ShowEmail(true);
        //     }
        //     PlayerPrefs.SetString("Email", SuccessPanelEmailInputField.Text);
        //     PlayerPrefs.Save();
        // } 
    }
    
    public void On_Done()
    {
        this.Result = ViewResult.Back;
        CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(this.Result.ToString());
    }
    
    //////////////////////////////////////////////////////////////////////////// Events
    
    public void DeletePaymentMethod(UserPaymentMethod userPaymentMethod)
    {
        if (IsEditingEmail)
            return;
        
        CheckoutClient.Instance.CurrentSession.LastDialogRequest         = "delete_payment_method";
        CheckoutClient.Instance.CurrentSession.userPaymentMethodToDelete = userPaymentMethod;
        
        this.Result = ViewResult.DeletePaymentMethod;
        CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(this.Result.ToString());
    }
    
    
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Test Scenarios
        
        public Step test_delete_last_payment_method() => new Step(action : async (s) => GetComponentsInChildren<SettingsPanelPaymentMethodItem>().Last().On_Delete_Clicked() );
        public Step test_go_back()                    => new Step(action : async (s) => On_Done() );
}

