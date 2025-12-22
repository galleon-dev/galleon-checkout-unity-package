using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Galleon.Checkout;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

    public class SelectPaymetnMethodPanelDropdownOption : MonoBehaviour
{
    //////////////////////////////////////////////////////////////// Members
    
    [Header("UI")]
    public TMP_Text Label;
    public Image    Icon;
    
    [Header("values")]
    public string   Text;
    public Sprite   Sprite;

    //////////////////////////////////////////////////////////////// Lifecycle

    public void Setup(string pmID)
    {
        try
        {
            // Get UPM
            var pm          = CHECKOUT.PaymentMethods.UserPaymentMethods.FirstOrDefault(upm => upm.DisplayName == pmID);
            
            // Get details
            var displayName = pm?.DisplayName;
            var sprite      = pm?.GetIconSprite();
            
            // Set values
            Label.text  = displayName;
            Icon.sprite = sprite;
    
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}

