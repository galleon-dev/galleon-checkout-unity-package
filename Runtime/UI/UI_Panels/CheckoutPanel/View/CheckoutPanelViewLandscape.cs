using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;


namespace Galleon.Checkout.UI
{
    public class CheckoutPanelViewLandscape : View
    {

        [Header("Shop Item")]
        public TextMeshProUGUI ProductTitleText;
        public TextMeshProUGUI PriceText;
        public TextMeshProUGUI TaxText;
       
        [Header("Taxes")]
        public List<GameObject> TaxesPanels;
        public GameObject TaxesContainer;
        public GameObject TaxPrefab;
        public TextMeshProUGUI SubtotalPriceText;
        public TextMeshProUGUI TotalPriceText;
        bool IsUSAorCanadaUser = true;

        public override void Initialize()
        {
            Debug.Log("CheckoutPanelViewLandscape --> Initialize()");
            RefreshState();
        }

        public override void RefreshState()
        {
            if (CheckoutClient.Instance.CurrentSession == null) return;

            this.ProductTitleText.text = Checkout.CheckoutClient.Instance.CurrentSession.SelectedProduct.DisplayName;
            this.PriceText.text = Checkout.CheckoutClient.Instance.CurrentSession.SelectedProduct.PriceText;

            if (TaxesContainer != null)
            {
                GenerateTaxes();
            }
        }

        void GenerateTaxes()
        {
            Debug.Log("GenerateTaxes()");

            foreach (Transform child in TaxesContainer.transform)
            {
                Destroy(child.gameObject);
            }

            var taxes = CheckoutClient.Instance.TaxController.taxes;

            //   #if UNITY_EDITOR

            // These are Taxes added only for testing. Should be commented out later on
            taxes.Clear();
            taxes.Add("VAT", new Shared.TaxItem { tax_amount = 9.90m, inclusive = false });
            taxes.Add("IRS", new Shared.TaxItem { tax_amount = 5.50m, inclusive = false });
            taxes.Add("CUSTOMS", new Shared.TaxItem { tax_amount = 25.15m, inclusive = false });
            taxes.Add("Delivery Fee", new Shared.TaxItem { tax_amount = 6.00m, inclusive = false });
            //#endif

            if (Checkout.CheckoutClient.Instance != null)
            {
                float SubTotal = 0f;
                if (float.TryParse(Checkout.CheckoutClient.Instance.CurrentSession.SelectedProduct.PriceText.Replace("$", ""), NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                {
                    SubTotal = result;
                }

                Debug.Log("SubTotal Parsed: " + SubTotal);

                // CultureInfo.InvariantCulture is important from parsing perspective from string to float as on mobile devices it can appear ",", instead "." in float values
                SubtotalPriceText.text = $"${SubTotal.ToString(CultureInfo.InvariantCulture)}";

                decimal TaxesAmount = 0;

                Debug.Log("Taxes Amount: " + taxes.Count);

                // If Location is USA or Canada generate taxes
                if (IsUSAorCanadaUser)
                {
                    foreach (var tax in taxes)
                    {
                        CreateTaxPrefab(tax.Key, tax.Value.tax_amount.ToString(CultureInfo.InvariantCulture));
                        TaxesAmount += tax.Value.tax_amount;
                    }
                    ShowTaxesPanels(true);

                    TaxText.gameObject.SetActive(false);

                    TotalPriceText.text = $"${(SubTotal + (float)TaxesAmount).ToString(CultureInfo.InvariantCulture)}";
                }
                else
                {
                    foreach (var tax in taxes)
                    {
                        TaxesAmount += tax.Value.tax_amount;
                    }
                    ShowTaxesPanels(false);

                    TaxText.gameObject.SetActive(true);

                    TaxText.text = $"${TaxesAmount.ToString(CultureInfo.InvariantCulture)}";

                    this.PriceText.text = $"${SubTotal.ToString(CultureInfo.InvariantCulture)}";
                }
            }
        }

        void CreateTaxPrefab(string taxName, string taxAmount)
        {
            var taxPrefab = Instantiate(original: TaxPrefab, parent: TaxesContainer.transform);
            taxPrefab.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = taxName;
            taxPrefab.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = $"${taxAmount}";
        }

        void ShowTaxesPanels(bool status)
        {
            int TaxesPanelsAmount = TaxesPanels.Count;
            for (int i = 0; i < TaxesPanelsAmount; i++)
            {
                TaxesPanels[i].SetActive(status);
            }
        }

        public void SetUSAorCanadaUser(bool _IsUSAorCanadaUser)
        {
            IsUSAorCanadaUser = _IsUSAorCanadaUser;
        }
    }
}