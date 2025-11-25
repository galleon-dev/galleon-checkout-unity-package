using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneratePaymentIcons : MonoBehaviour
{
    [Serializable]
    public class PaymentIcon
    {
        public Sprite sprite;
        public bool Show = true; // enabled by default
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
        // create images for each enabled icon
        foreach (var icon in paymentIcons)
        {
            if (!icon.Show || icon.sprite == null)
                continue;

            GameObject imgObj = Instantiate(imagePrefab, parentContainer);
            imgObj.GetComponent<Image>().sprite = icon.sprite;
            imgObj.SetActive(true);
        }
    }
}
