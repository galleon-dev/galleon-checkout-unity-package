using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Galleon.Checkout;
using UnityEngine;
using UnityEngine.Networking;

public class CheckoutPopupMobile : MonoBehaviour
{
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
    
    [Header("Panels")]
    public  ParentPanel  ParentPanel;
    public  GameObject   CheckoutPanel;
    public  GameObject   CreditCardPanel;
    public  GameObject   SuccessPanel;
    public  GameObject   ErrorPanel;
    
    [Header("A/B Tests")]
    public  GameObject   CheckoutPanelA;
    public  GameObject   CheckoutPanelB;
     
    private bool         isPending = false;
    
    public static string ABTest = "a"; 
    
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

    private async void OnEnable()
    {
        if (ABTest == "a")
        {
            CheckoutPanel = CheckoutPanelA;
            CheckoutPanelA.gameObject.SetActive(true);
            CheckoutPanelB.gameObject.SetActive(false);
          //ABTest = "b";
        }
        else
        {
            CheckoutPanel = CheckoutPanelB;
            CheckoutPanelB.gameObject.SetActive(true);
            CheckoutPanelA.gameObject.SetActive(false);
            ABTest = "a";
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Events

    [ContextMenu("Test")]
    public async void OnPurchaseClick()
    {
        if (isPending)
            return;
        
        isPending = true;
        
        // fake request 
        // UnityWebRequest requet = UnityWebRequest.Get("http://localhost:5007/purchase");
        // var asyncOp = requet.SendWebRequest();
        // while (!asyncOp.isDone)
        //     await Task.Yield();
        // bool isSuccess = asyncOp.webRequest.error == null && asyncOp.webRequest.downloadHandler.text == "true";
        // if (!isSuccess)
        // {
        //     CheckoutPanel.gameObject.SetActive(false);
        //     ErrorPanel   .gameObject.SetActive(true);
        //     
        //     if (asyncOp.webRequest.error != null)
        //         Debug.LogError(asyncOp.webRequest.error);
        // }
        // else
        // {        
        //     CheckoutPanel  .gameObject.SetActive(false);
        //     CreditCardPanel.gameObject.SetActive(true);   
        // }
        
        bool isSuccess = true;
        isPending      = false;
        
        SetCreditCardPanelActive();
    }

    public void OnCreditCardConfirmClick()
    {
        CreditCardPanel.gameObject.SetActive(false);
        SuccessPanel   .gameObject.SetActive(true);
    }

    public void OnSuccessConfirmClick()
    {
        Close();
    }

    public void OnErrorConfirmClick()
    {
        ErrorPanel   .gameObject.SetActive(false);
        CheckoutPanel.gameObject.SetActive(true);
    }

    public void OnShadeClick()
    {
        Close();
    }

    public void OnBackClick()
    {
        Close();
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Actions

    public void SetCheckoutPanelActive()
    {
        DisableAllPanels();
        CheckoutPanel.gameObject.SetActive(true);
    }

    public void SetCreditCardPanelActive()
    {
        DisableAllPanels();
        CreditCardPanel.gameObject.SetActive(true);
    }

    public void SetSuccessPanelActive()
    {
        DisableAllPanels();
        SuccessPanel.gameObject.SetActive(true);
    }

    private void DisableAllPanels()
    {
        CheckoutPanel  .gameObject.SetActive(false);
        CheckoutPanelA .gameObject.SetActive(false);
        CheckoutPanelB .gameObject.SetActive(false);
        CreditCardPanel.gameObject.SetActive(false);
        SuccessPanel   .gameObject.SetActive(false);
        ErrorPanel     .gameObject.SetActive(false);
    }

    public void Close()
    {
        Destroy(this.gameObject);
    }

    void Update()
    {
        // if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
        // {
        //     if (Input.GetKeyDown(KeyCode.Escape))
        //     {
        //         // Handle back button press
        //         OnBackClick();
        //     }
        // }
    }
}
