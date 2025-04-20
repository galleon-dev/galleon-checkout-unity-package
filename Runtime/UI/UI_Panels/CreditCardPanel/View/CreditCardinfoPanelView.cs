using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Galleon.Checkout;
using Galleon.Checkout.UI;
using TMPro;
using UnityEngine;

public class CreditCardInfoPanelView : View
{
    //////////////////////////////////////////////////////////////////////////// Members
    
    public TMP_InputField CreditCardNumberField;

    //////////////////////////////////////////////////////////////////////////// View Result
        
    public      ViewResult Result = ViewResult.None;
    public enum ViewResult
    {
        None,
        Back,
        Confirm,
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
    
    //////////////////////////////////////////////////////////////////////////// UI Events
    
    public void OnOkClick()
    {
        this.IsCompleted = true;
        this.Result      = ViewResult.Confirm;
    }
    
    //////////////////////////////////////////////////////////////////////////// Input Field Text Formatting
    
    public void OnValueChanged(string text)
    {
        string formattedText = "";        
        string inputText     = text.Replace(" ", "");

        for (int i = 0; i < inputText.Length; i++)
        {
            if (i % 4 == 0 && i != 0) 
                formattedText += " ";
            
            formattedText += inputText[i];
        }
        
        if (formattedText == text)
            return;
        
        CreditCardNumberField.text = formattedText;
        CreditCardNumberField.MoveTextEnd(true);
    }
}
