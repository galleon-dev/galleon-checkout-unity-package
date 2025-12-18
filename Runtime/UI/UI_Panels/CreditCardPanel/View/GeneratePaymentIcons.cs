using System;
using System.Collections.Generic;
using Galleon.Checkout.UI;
using UnityEngine;
using UnityEngine.UI;

public class GeneratePaymentIcons : MonoBehaviour
{
    [Serializable]
    public class PaymentIcon
    {
        public Sprite sprite;
        public bool   Show = true; // enabled by default
    }

    [Header("Payment Icons")]
    public List<PaymentIcon> paymentIcons = new List<PaymentIcon>();

    [Header("UI Settings")]
    public Transform parentContainer;
    public GameObject imagePrefab;

    void Start()
    {
        imagePrefab.SetActive(false);
        Generate();
    }

    public void Generate()
    {
        var cardPanel = this.GetComponentInParent<CreditCardInfoPanelView>();
    
        // create images for each enabled icon
        for (int i = 0; i < paymentIcons.Count; i++)
        {
            var icon = paymentIcons[i];
            
            if (!icon.Show || icon.sprite == null)
                continue;

            GameObject imgObj = Instantiate(imagePrefab, parentContainer);
            imgObj.GetComponent<Image>().sprite = icon.sprite;
            imgObj.SetActive(true);
            
            
            #if DEBUG
    
            var button = imgObj.GetComponent<Button>();
            if (button != null)
            {
                var cardType = i switch
                               {
                                    0 => "master_card",
                                    1 => "visa", 
                                    2 => "paypal",
                                    3 => "g_pay",
                                    4 => "discover",
                                    5 => "amex",
                                    _ => "diners"
                               };
                
                
                button.onClick.AddListener(() => cardPanel.On_PaymentIconClicked(cardType));
            }
            #endif // DEBUG
        }
    }
}