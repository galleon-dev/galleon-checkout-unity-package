using System.Linq;
using System.Text.RegularExpressions;
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
    public GameObject           EmailInputfieldBorder;
    public AdvancedInputField   EmailInputField;
    public TextMeshProUGUI      emailErrorText;
    public GameObject           EmailEditButton;

    [Header("Payment Methods")]
    public GameObject           SettingsPanelPaymentMethodItemPrefab;
    public GameObject           SettingsPanelTogglePrefab;
    public GameObject           PaymentMethodsHolder;
    public bool                 IsEditingEmail = false;
    public GameObject           InformationalLabel;
    public GameObject           Gap;
    
    private int                 ScrollRectMaxSize = 6;
    private float               PaymentPrefabHeight = 200f;
    private float               SeparatorHeight = 2f;

    [Header("misc")]
    public TMP_Text             BackButton;
    public LayoutElement        ScrollRectLayoutElement;
    public ScrollRect           ScrollRect;
    
    //////////////////////////////////////////////////////////////////////////// View Result

    public ViewResult Result = ViewResult.None;

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
        if (InformationalLabel)
        {
            InformationalLabel.SetActive(false);
        }

        string Email = PlayerPrefs.GetString("Email");

        EmailInputField.Text = Email;
        EmailInputfieldBorder.SetActive(false);
        
        
        RefreshState();
    }

    //////////////////////////////////////////////////////////////////////////// Refresh

    public override void RefreshState()
    {
        // Remove children (if any)
        foreach (Transform child in PaymentMethodsHolder.transform)
        {
            //Debug.Log($"-Removing Item {child.gameObject.name}");
            Destroy(child.gameObject);
        }

        int rowCount = 0;
        
        if (CHECKOUT.PaymentMethods.UserPaymentMethodsToRemove.Count > 0
        ||  CHECKOUT.Globals.IsNativeStoreToggleEnabled)
        {
            // Add ui separator
            Instantiate(original: CHECKOUT.Resources.UI_Seporator, parent: PaymentMethodsHolder.transform);
            Instantiate(original: CHECKOUT.Resources.UI_Seporator, parent: PaymentMethodsHolder.transform);
        }
        
        // Add children
        var paymentMethods = CHECKOUT.PaymentMethods.UserPaymentMethodsToRemove;
        foreach (var paymentMethod in paymentMethods)
        {
            // Item
            var go   = Instantiate(original: SettingsPanelPaymentMethodItemPrefab, parent: PaymentMethodsHolder.transform);
            var item = go.GetComponent<SettingsPanelPaymentMethodItem>();
            item.Initialize(paymentMethod, this);

            // Add ui separator
            Instantiate(original: CHECKOUT.Resources.UI_Seporator, parent: PaymentMethodsHolder.transform);
            
            rowCount++;
        }
        
        // Native store toggle Prefab
        Debug.Log($"IsNativeStoreToggleEnabled: {CHECKOUT.Globals.IsNativeStoreToggleEnabled}");
        if (CHECKOUT.Globals.IsNativeStoreToggleEnabled)
        {
            // Item
            var go   = Instantiate(original: SettingsPanelTogglePrefab, parent: PaymentMethodsHolder.transform);
            var item = go.GetComponent<SettingsPanelToggleItem>();
            var nativeStore = CHECKOUT.PaymentMethods.UserPaymentMethods.FirstOrDefault(x => x.Data.type == "native");
            item.Initialize(nativeStore, this);

            // Add ui separator
            Instantiate(original: CHECKOUT.Resources.UI_Seporator, parent: PaymentMethodsHolder.transform);
            
            rowCount++;
        }

        // Set UI
        InformationalLabel.SetActive(rowCount == 0);
        Gap.SetActive(rowCount == 0);
        ScrollRectLayoutElement.gameObject.SetActive(rowCount != 0);
        

        // Email
        if (!string.IsNullOrEmpty(CHECKOUT.User.Email))
            this.EmailInputField.Text = CHECKOUT.User.Email;

        // Back Button
        this.BackButton.gameObject.SetActive(CHECKOUT.Globals.SettingsBackButton != "");
        this.BackButton.text = CHECKOUT.Globals.SettingsBackButton;
        
        UpdateScrollRectMaxSize();
    }

    //////////////////////////////////////////////////////////////////////////// UI Helper methods

    public void UpdateScrollRectMaxSize()
    {
        int PaymentMethodsAmount = CHECKOUT.PaymentMethods.UserPaymentMethodsToRemove.Count; // CHECKOUT.PaymentMethods.UserPaymentMethods.Count;

        if (CHECKOUT.Globals.IsNativeStoreToggleEnabled)
            PaymentMethodsAmount++;
        
        // Debug.Log("<color=green>UpdateScrollRectMaxSize(): </color>" + PaymentMethodsAmount);

        if (PaymentMethodsAmount <= 1)
        {
            ScrollRect.vertical = false;
        }
        else
        {
            ScrollRect.vertical = true;
        }

        if (PaymentMethodsAmount == 0)
        {
            ScrollRectLayoutElement.preferredHeight = 0;
        }
        else if (PaymentMethodsAmount <= ScrollRectMaxSize)
        {
            ScrollRectLayoutElement.preferredHeight = PaymentMethodsAmount * (PaymentPrefabHeight + SeparatorHeight) + 10;
        }
        else
        {
            ScrollRectLayoutElement.preferredHeight = ScrollRectMaxSize * (PaymentPrefabHeight + SeparatorHeight) + 10;
        }
    }

    //////////////////////////////////////////////////////////////////////////// UI Events

    public void On_EditEmailInputFieldClicked()
    {
        EmailInputfieldBorder.SetActive(true); 
        EmailEditButton.SetActive(false);
        IsEditingEmail = true;
    }

    public void On_EditEmailButtonClicked()
    {
        EmailInputField.Select();
    }

    public void On_BackClicked()
    {
        this.Result = ViewResult.Back;
        CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(this.Result.ToString());
    }

    public async void On_FinishedEditingEmail(string str, EndEditReason reason)
    { 
        await validateAndSubmitEmail(str);
    }

    //////////////////////////////////////////////////////////////////////////// Email Methods

    private async Task validateAndSubmitEmail(string str)
    {
     
        EmailInputfieldBorder.SetActive(false);
        EmailEditButton.SetActive(true);
        IsEditingEmail = false;

        bool valid = ValidateEmail(EmailInputField.Text);

        if (EmailInputField.Text == "")
        {
            // its ok to delete email.
            valid = true;
        }
        if (!string.IsNullOrEmpty(EmailInputField.Text))
        {
            emailErrorText.text = valid ? "" : "Invalid email format";
        } 
        else
        {
            emailErrorText.text = "";
        }
       
        if (valid)
        {
            this.EmailInputField.Text            = str;
            CHECKOUT.Session.User.UserInfo.email = str;
            await CHECKOUT.Actions.UpdateEmail().Execute();
        }   
    }
    
    private bool ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Basic email regex
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
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

    public Step test_delete_last_payment_method() => new Step(action: async (s) => GetComponentsInChildren<SettingsPanelPaymentMethodItem>().Last().On_Delete_Clicked());
    public Step test_go_back() => new Step(action: async (s) => On_Done());
}

