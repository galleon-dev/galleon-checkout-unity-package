using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CreditCardFormView : MonoBehaviour
{
    public TMP_InputField CreditCardNumberField;
    
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
