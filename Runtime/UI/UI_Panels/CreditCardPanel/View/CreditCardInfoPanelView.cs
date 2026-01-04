using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Galleon.Checkout;
using Galleon.Checkout.Foundation;
using Galleon.Checkout.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//#if UNITY_ANDROID
using AdvancedInputFieldPlugin;
//#endif

namespace Galleon.Checkout.UI
{
    public class CreditCardInfoPanelView : View
    {
        // #if UNITY_ANDROID
        
        //////////////////////////////////////////////////////////////////////////// Members

        public List<AdvancedInputField> AdvancedInputFields;

        public AdvancedInputField       CreditCardNumberField;
        public AdvancedInputField       NameInputField;
        public AdvancedInputField       DateInputField;
        public AdvancedInputField       CVVInputField;

        public GameObject               CardNumberErrorTextBackground;
        public GameObject               NameErrorTextBackground;
        public GameObject               CVVNumberErrorTextBackground;
        public GameObject               DateErrorTextBackground;

        public TMP_Text                 CardNumberErrorText;
        public TMP_Text                 NameErrorText;
        public TMP_Text                 CVVNumberErrorText;
        public TMP_Text                 DateErrorText;

        string                          NameMissingInfoText = "* Please Enter Name";
        string                          CardNumberInfoText  = "* Please Enter Card Number";
        string                          CVVNumberInfoText   = "* Please Enter CVV";
        string                          DateInfoText        = "* Please Enter Date";

        public Image                    CardTypeIcon;
        public Sprite                   CardIcon_MasterCard;
        public Sprite                   CardIcon_Visa;
        public Sprite                   CardIcon_Amex;
        public Sprite                   CardIcon_Diners;
        public Sprite                   CardIcon_Discover;

        public CheckboxButton           cbx_SaveCardDetails;

        private CardFormat              CurrentCardFormat = default;
        private CardFormat              lastFormatUsed;

        bool                            IsValidCVV              = false;
        bool                            IsValidCreditCardNumber = false;
        bool                            IsValidDate             = false;
        
        public GameObject               TestCardButton;
        
        //////////////////////////////////////////////////////////////////////////// View Result

        public ViewResult Result = ViewResult.None;

        public void Awake()
        {
            if (NameErrorText)
            {
                NameErrorTextBackground.SetActive(false);
            }

            if (CVVNumberErrorText)
            {
                CVVNumberErrorTextBackground.SetActive(false);
            }

            if (DateErrorText)
            {
                DateErrorTextBackground.SetActive(false);
            }

            if (CardNumberErrorText)
            {
                CardNumberErrorTextBackground.SetActive(false);
            }

            /*  if (CVVInputField)
              {
                  CVVInputField.OnValueChanged.AddListener(OnCVVValueChanged);
              }

              if (DateInputField)
              {
                  DateInputField.OnValueChanged.AddListener(OnDateValueChanged);
              }

              if (NameInputField)
              {
                  NameInputField.OnValueChanged.AddListener(OnNameValueChanged);
              }
            */
        }

        private void OnEnable()
        {
            // Clear input fields
            if (NameInputField)        NameInputField       .Text = string.Empty;
            if (CreditCardNumberField) CreditCardNumberField.Text = string.Empty;
            if (DateInputField)        DateInputField       .Text = string.Empty;
            if (CVVInputField)         CVVInputField        .Text = string.Empty;

            // Reset validation flags
            IsValidCVV              = false;
            IsValidCreditCardNumber = false;
            IsValidDate             = false;

            // Hide Error Messages
            NameErrorTextBackground      .SetActive(false);
            CardNumberErrorTextBackground.SetActive(false);
            CVVNumberErrorTextBackground .SetActive(false);
            DateErrorTextBackground      .SetActive(false);

            // Remove card icon
            RemoveCardIcon();

            // Reset card format
            lastFormatUsed    = default;
            CurrentCardFormat = default;
            
            #if DEBUG
            TestCardButton.SetActive(false);
            #else
            TestCardButton.SetActive(false);
            #endif
        }

        public enum ViewResult
        {
            None,
            Confirm,
        }

        //////////////////////////////////////////////////////////////////////////// UI Events

        public async void On_OkClick()
        {
            if (IsCorrectInputFields())
            {
                // Create payment method object
                var card                     = new CreditCardUserUserPaymentMethod();
                card.Data.type               = "credit_card";
                card.Data.credit_card_type   = card.Type;
                
                // Set card Data
                card.Type                    = CurrentCardFormat.Name;
                card.DisplayName             = $"{CreditCardNumberField.Text.Substring(CreditCardNumberField.Text.Length - 4)}";
                card.CardHolderName          = NameInputField.Text;
                card.CardNumber              = CreditCardNumberField.Text;
                card.CardCCV                 = CVVInputField.Text;
                card.CardMonth               = DateInputField.Text.Substring(0, 2);
                card.CardYear                = DateInputField.Text.Substring(2, 2);

                // Set additional data
                card.IsNewPaymentMethod      = true;
                card.ShouldSavePaymentMethod = cbx_SaveCardDetails.IsChecked;
                
                // Add payment method
                await CHECKOUT.PaymentMethods.AddNewUserPaymentMethod(card).Execute();
                
                // Finish viewing this screen
                this.Result = ViewResult.Confirm;
                CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(this.Result.ToString());
            }
        }

        bool IsCorrectInputFields()
        {
            bool InputFieldsCorrect = true;

            // Check if the credit card number starts with Amex prefix (34 or 37)
            bool isAmex =  CreditCardNumberField.Text.Replace(" ", "").StartsWith("34") 
                        || CreditCardNumberField.Text.Replace(" ", "").StartsWith("37");
            int expectedCVVLength = isAmex ? 4 : 3;
            
            string Info = "* Please Enter Information";

            if (string.IsNullOrEmpty(NameInputField.Text))
            {
                NameErrorText.text = NameMissingInfoText;
                NameErrorTextBackground.SetActive(true);
                InputFieldsCorrect = false;
            }
            else if (string.IsNullOrEmpty(CreditCardNumberField.Text))
            {
                CardNumberErrorText.text = CardNumberInfoText;
                CardNumberErrorTextBackground.SetActive(true);
                InputFieldsCorrect = false;
            }
            else if (string.IsNullOrEmpty(CVVInputField.Text))
            {
                CVVNumberErrorText.text = CVVNumberInfoText;
                CVVNumberErrorTextBackground.SetActive(true);
                InputFieldsCorrect = false;
            }
            else if (CVVInputField.Text.Length != expectedCVVLength)
            {
                CVVNumberErrorText.text = $"* Enter a {expectedCVVLength}-Digit CVV";
                CVVNumberErrorTextBackground.SetActive(true);
                InputFieldsCorrect = false;
            }
            else if (string.IsNullOrEmpty(DateInputField.Text))
            {
                DateErrorText.text = DateInfoText;
                DateErrorTextBackground.SetActive(true);
                InputFieldsCorrect = false;
            }
            else if (!ValidateDateExpiry(DateInputField.Text, out string dateError))
            {
                DateErrorText.text = dateError;
                DateErrorTextBackground.SetActive(true);
                InputFieldsCorrect = false;
            }

            if (!IsValidCVV || !IsValidCreditCardNumber || !IsValidDate)
            {
                Debug.Log("Invalid Entered Information");
                InputFieldsCorrect = false;
            }

            return InputFieldsCorrect;
        }

        IEnumerator CheckIfFocusOnCVV()
        {
            // Wait a short time before checking input fields
            yield return new WaitForSeconds(0.1f);
            
            // Check if name, card number and date are filled but CVV is empty
            if (string.IsNullOrEmpty(NameInputField.Text) ||  string.IsNullOrEmpty(CreditCardNumberField.Text) ||
                string.IsNullOrEmpty(DateInputField.Text) || !string.IsNullOrEmpty(CVVInputField.Text))
            {
                // Cannot focus CVV yet as other fields are not complete
            }
            else
            {
                // Get card format based on entered number
                CardFormat CF = GetFormatForDigits(CreditCardNumberField.Text);
                if (MaxLength == 0)
                {
                    MaxLength = CF.MaxLength;
                }

                // Check if date is complete and card number matches required length
                if (DateInputField.Text.Length == 4 
                && (CreditCardNumberField.Text.Length == MaxLength || (CreditCardNumberField.Text.Length == 16) && CF.Name.ToLower() == "visa"))
                {
                    // Deselect any currently selected input field
                    int AdvancedInputFieldsAmount = AdvancedInputFields.Count;
                    for (int i = 0; i < AdvancedInputFieldsAmount; i++)
                    {
                        if (AdvancedInputFields[i].Selected)
                        {
                            AdvancedInputFields[i].ManualDeselect(EndEditReason.PROGRAMMATIC_DESELECT);
                            yield return new WaitForEndOfFrame();
                        }
                    }

                    // Auto-focus the CVV input field
                    CVVInputField.ManualSelect();
                }
            }
        }


        public void OnCreditCardValueEndEdit(AdvancedInputField _AdvancedInputFieldText)
        {
            FormatCreditCardInput(_AdvancedInputFieldText.Text);
        }


        public void OnValueChanged(string text)
        {
            FormatCreditCardInput(text);
        }

        public void SetCardIcon(CardFormat card)
        {
            this.CardTypeIcon.gameObject.SetActive(true);
            if (card.Name == "MasterCard")
            {
                this.CardTypeIcon.sprite = CardIcon_MasterCard;
            }
            else if (card.Name == "Visa")
            {
                this.CardTypeIcon.sprite = CardIcon_Visa;
            }
            else if (card.Name == "Amex")
            {
                this.CardTypeIcon.sprite = CardIcon_Amex;
            }
            else if (card.Name == "Diners")
            {
                this.CardTypeIcon.sprite = CardIcon_Diners;
            }
            else if (card.Name == "Discover")
            {
                this.CardTypeIcon.sprite = CardIcon_Discover;
            }
            else
            {
                this.CardTypeIcon.gameObject.SetActive(false); // Hide icon if no match
            }
        }

        public void RemoveCardIcon()
        {
            if (this.CardTypeIcon)
            {
                this.CardTypeIcon.gameObject.SetActive(false);
            }
        }

        public void OnDateValueEndEdit(AdvancedInputField _AdvancedInputFieldText)
        {
            OnDateValueChanged(_AdvancedInputFieldText.Text);
        }

        void OnDateValueChanged(string rawInput)
        {
            //Debug.Log("<color=green>OnDateValueChanged. rawInput: " + rawInput + "</color>");

            if (rawInput.Length > 4)
            {
                rawInput = rawInput.Remove(rawInput.Length - 1);
            }

            if (rawInput.Length == 4)
            {
                if (!ValidateDateExpiry(rawInput, out string err))
                {
                    DateErrorText.text = err;
                    DateErrorTextBackground.SetActive(true);

                    IsValidDate = false;
                }
                else
                {
                    DateErrorTextBackground.SetActive(false);
                    IsValidDate = true;

                    StartCoroutine(CheckIfFocusOnCVV());
                }
            }
            else
            {
                DateErrorTextBackground.SetActive(false);
                IsValidDate = true;
            }
        }

        public static bool ValidateDateExpiry(string formatted, out string error)
        {
            error = null;

            // Expect format "MM/YY"
            if (formatted.Length != 4)
            {
                error = "Invalid format. Use MM/YY";
                return false;
            }

            string monthStr = formatted.Substring(0, 2);
            string yearStr  = formatted.Substring(2, 2);

            if (!int.TryParse(monthStr, out int month) ||
                !int.TryParse(yearStr,  out int yy))
            {
                error = "Month/year must be numeric";
                return false;
            }

            if (month < 1 || month > 12)
            {
                error = "Invalid date (MM/YY)";
                return false;
            }

            // Interpret e.g. "24" as 2024 (assumes 2000–2099 range)
            int fullYear = 2000 + yy;
            var now      = DateTime.Now;

            // Cards expire at end of month — valid if expiry >= end-of-month of current:
            var expiryEnd = new DateTime(fullYear, month, DateTime.DaysInMonth(fullYear, month), 23, 59, 59);

            if (expiryEnd < now)
            {
                error = "Card has expired";
                return false;
            }

            return true;
        }

        public void OnCVVValueEndEdit(AdvancedInputField _AdvancedInputFieldText)
        {
            OnCVVValueChanged(_AdvancedInputFieldText.Text);
        }

        public void OnCVVValueChanged(string digits)
        {
            // Check if the credit card number starts with Amex prefix (34 or 37)
            bool isAmex =  CreditCardNumberField.Text.Replace(" ", "").StartsWith("34") 
                        || CreditCardNumberField.Text.Replace(" ", "").StartsWith("37");
            
            // Set expected CVV length based on card type (4 for Amex, 3 for others)
            int expectedCVVLength = isAmex ? 4 : 3;

            // Update placeholder text based on expected CVV length
            if (expectedCVVLength == 4)
            {
                CVVInputField.PlaceHolderText = "0000";
            }
            else
            {
                // If current input is 4 digits but expecting 3, remove last digit
                if (digits.Length == 4)
                {
                    CVVInputField.Text = digits.Remove(digits.Length - 1);
                }
                CVVInputField.PlaceHolderText = "000";
            }

            // Truncate input if longer than expected length
            if (digits.Length > expectedCVVLength)
            {
                digits = digits.Remove(digits.Length - 1);
            }

            // Validate CVV input
            if (digits.Length != 0)
            {
                if (digits.Length == expectedCVVLength)
                {
                    // Valid CVV entered
                    CVVNumberErrorTextBackground.SetActive(false);
                    IsValidCVV = true;
                }
                else
                {
                    // Show error for incomplete CVV
                    CVVNumberErrorText.text = $"Enter a {expectedCVVLength}-digit CVV";
                    CVVNumberErrorTextBackground.SetActive(true);
                    IsValidCVV = false;
                }
            }
            else
            {
                // Empty CVV input is considered valid
                CVVNumberErrorTextBackground.SetActive(false);
                IsValidCVV = true;
            }
        }

        public void OnNameValueEndEdit(AdvancedInputField _AdvancedInputFieldText)
        {
            OnNameValueChanged(_AdvancedInputFieldText.Text);
        }

        public void OnNameValueChanged(string inputName)
        {
            if (!string.IsNullOrEmpty(inputName))
            {
                NameErrorTextBackground.SetActive(false);
            }

            StartCoroutine(CheckIfFocusOnCVV());
        }

        int MaxLength;

        void FormatCreditCardInput(string rawInput)
        {
            // Debug.Log("FormatCreditCardInput: " + rawInput);
            MaxLength = GetFormatForDigits(rawInput).MaxLength;
            if (rawInput.Length > MaxLength)
            {
                rawInput = rawInput.Remove(rawInput.Length - 1);
                // Debug.Log("Updated RawInput: " + rawInput);
            }

            if (string.IsNullOrEmpty(CreditCardNumberField.Text))
            {
                CardNumberErrorTextBackground.SetActive(false);
            }
            else
            {
                CheckLuhnOnEndEdit(rawInput);
            }

            OnCVVValueChanged(CVVInputField.Text);

            StartCoroutine(CheckIfFocusOnCVV());
        }

        public void CheckLuhnOnEndEdit(string digits)
        {
            // LUHN VALIDATION (only when input is complete)
            // LUHN only checks if the number is structurally valid. If the final result is not divisible by 10, the number is invalid.

            IsValidCreditCardNumber = IsValidLuhn(digits); // digits.Length == format.MaxLength && 

            // Debug.Log("LUHN isValid: " + IsValidCreditCardNumber);

            if (!IsValidCreditCardNumber)
            {
                CardNumberErrorTextBackground.SetActive(true);
                CardNumberErrorText.text = "* Invalid Number";
            }
            else
            {
                CardNumberErrorTextBackground.SetActive(false);
            }
        }

        public CardFormat GetFormatForDigits(string digits)
        {
            foreach (var (matcher, format) in cardFormats)
            {
                if (matcher(digits))
                    return format;
            }

            this.CurrentCardFormat = cardFormats[^1].format;

            return cardFormats[^1].format; // fallback
        }

        public struct CardFormat
        {
            public string Name;
            public int    MaxLength;
            public int[]  GroupSizes;
            public int    InputFieldLimit;
            public CardFormat(string name, int maxLength, int[] groupSizes, int inputFieldLimit)
            {
                Name            = name;
                MaxLength       = maxLength;
                GroupSizes      = groupSizes;
                InputFieldLimit = inputFieldLimit;
            }
        }

        private static readonly List<(Func<string, bool> matcher, CardFormat format)> cardFormats 
        = new()
        {
            // Amex: 15 digits → 4-6-5
            (d => d.StartsWith("34") 
               || d.StartsWith("37"),
                   new CardFormat(name            : "Amex"
                                 ,maxLength       : 15
                                 ,groupSizes      : new[] { 4, 6, 5 }
                                 ,inputFieldLimit : 21)),

            // Visa: starts with 4, up to 19 digits → 4-4-4-4-3
            (d => d.StartsWith("4"),
                  new CardFormat(name            : "Visa"
                                ,maxLength       : 19
                                ,groupSizes      : new[] { 4, 4, 4, 4, 3 }
                                ,inputFieldLimit : 31)),

            // MasterCard: 51–55, 2221–2720 → 16 digits → 4-4-4-4
            (d => (d.Length >= 2 && int.TryParse(d.Substring(0, 2), out var p2) && p2 >= 51   && p2 <= 55)
               || (d.Length >= 4 && int.TryParse(d.Substring(0, 4), out var p4) && p4 >= 2221 && p4 <= 2720),
                  new CardFormat(name            : "MasterCard"
                                ,maxLength       : 16
                                ,groupSizes      : new[] { 4, 4, 4, 4 }
                                ,inputFieldLimit : 25)),

            // Discover: 6011, 65, 644–649 → 16 digits
            (d => d.StartsWith("6011")  
               || d.StartsWith("65") 
               || (d.Length >= 3 && int.TryParse(d.Substring(0, 3), out var p3) && p3 >= 644 && p3 <= 649),
                  new CardFormat(name            : "Discover"
                                ,maxLength       : 16
                                ,groupSizes      : new[] { 4, 4, 4, 4 }
                                ,inputFieldLimit : 28)),

            // JCB: 3528–3589 → 16–19 digits
            // (d => d.Length >= 4 && int.TryParse(d.Substring(0, 4), out var pJcb) && pJcb >= 3528 && pJcb <= 3589,
            //        new CardFormat(name            : "JCB"
            //                     ,maxLength       : 19
            //                     ,groupSizes      : new[] { 4, 4, 4, 4, 3 }
            //                     ,inputFieldLimit : 31)),

            // Diners Club International: starts with 300-305, 36, 38, 39 → 14 or 16 digits → 4-6-4 or 4-4-4-4
            (d =>  d.StartsWith("300") 
               ||  d.StartsWith("301") 
               ||  d.StartsWith("302") 
               ||  d.StartsWith("303")
               ||  d.StartsWith("304")
               ||  d.StartsWith("305")
               ||  d.StartsWith("36")  
               ||  d.StartsWith("38")  
               ||  d.StartsWith("39"),
                   new CardFormat(name            : "Diners"
                                 ,maxLength       : 16
                                 ,groupSizes      : new[] { 4, 4, 4, 4 }
                                 ,inputFieldLimit : 25)),

            // Default US & Canada : starts with 54,55 → 16 digits → 4-4-4-4  
            (d => d.StartsWith("54") 
               || d.StartsWith("55"),
                  new CardFormat(name            : "DinersUS"
                                ,maxLength       : 16
                                ,groupSizes      : new[] { 4, 4, 4, 4 }
                                ,inputFieldLimit : 25)),
            
            // Default fallback
            (_ => true, new CardFormat(name            : "Unknown"
                                      ,maxLength       : 16
                                      ,groupSizes      : new[] { 4, 4, 4, 4 }
                                      ,inputFieldLimit : 25))

        };

        bool IsValidLuhn(string digits)
        {
            int  sum = 0;
            bool alt = false;
            for (int i = digits.Length - 1; i >= 0; i--)
            {
                int n = digits[i] - '0';
                if (alt)
                {
                    n *= 2;
                    if (n > 9) n -= 9;
                }
                sum += n;
                alt =  !alt;
            }
            return digits.Length >= 12 && sum % 10 == 0; // avoid false positive on short input
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Test UI Events
        
        public void On_TestFakeCardButtonClicked()
        {
            NameInputField.Text        = "veronica visa";
            CreditCardNumberField.Text = "4242424242424242";
            DateInputField.Text        = "0929";
            CVVInputField.Text         = "111";
            
            OnValueChanged(CreditCardNumberField.Text);
            OnDateValueChanged(DateInputField.Text);
            OnCVVValueChanged(CVVInputField.Text);
            
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Test Events
        
        public async void On_PaymentIconClicked(string cardType)
        {
            #if DEBUG
            
            switch (cardType.ToLower())
            {
                case "master_card" :
                {
                    NameInputField.Text        = new [] { "Ronald McMasterCard", "Marcus MasterCard", "Maria MasterCard", "Michael MasterCard", "Melissa MasterCard" }.RandomItem();
                    CreditCardNumberField.Text = "5555555555554444";
                    DateInputField.Text        = "0929";
                    CVVInputField.Text         = "123";
                     
                    break;
                }
                case "visa" :
                {
                    NameInputField.Text        = new [] { "Vincent Visa", "Victoria Visa", "Vanessa Visa", "Victor Visa", "Vladimir Visa" }.RandomItem();
                    CreditCardNumberField.Text = "4242424242424242";
                    DateInputField.Text        = "0929";
                    CVVInputField.Text         = "111";
                     
                    break;
                }
                case "amex" :
                {
                    NameInputField.Text        = new [] { "Alexander Amex", "Alice Amex", "Alex Amex", "Amanda Amex"}.RandomItem();
                    CreditCardNumberField.Text = "378282246310005";
                    DateInputField.Text        = "0929";
                    CVVInputField.Text         = "1234";
                
                    break;
                }
                case "discover" :
                {
                    NameInputField.Text        = new [] { "Douglas Discover", "Dominic Discover", "Dorothy Discover", "Doris Discover", "Daisy Discover" }.RandomItem();
                    CreditCardNumberField.Text = "6011111111111117";
                    DateInputField.Text        = "0929";
                    CVVInputField.Text         = "123";
                
                    break;
                }
                case "diners" :
                {
                    NameInputField.Text        = new [] { "Daenerys Diners", "Dante Diners", "Duncan Diners", "Dean Diners", "Donna Diners", "Diana Diners" }.RandomItem();
                    CreditCardNumberField.Text = "3056930009020004";
                    DateInputField.Text        = "0929";
                    CVVInputField.Text         = "123";
                
                    break;
                }
                default :
                {
                    NameInputField.Text        = "John Doe";
                    CreditCardNumberField.Text = "1234567890123456";
                    DateInputField.Text        = "0929";
                    CVVInputField.Text         = "123";

                    break;
                }
            }
            
            OnDateValueChanged(DateInputField.Text);
            OnCVVValueChanged(CVVInputField.Text);
            OnValueChanged(CreditCardNumberField.Text);
         
            await Task.Yield();
            
            FormatCreditCardInput(CreditCardNumberField.Text);
         
            
            cbx_SaveCardDetails.IsChecked = true;
            
            #endif // DEBUG
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Test Scenarios
        
        public Step test_fill_card_data() => new Step(name   : "credit_card_info_panel_fill_test_card"
                                                     ,action : async (s) =>
                                                               {
                                                                   NameInputField.Text        = "veronica visa";
                                                                   CreditCardNumberField.Text = "4242424242424242";
                                                                   DateInputField.Text        = "0929";
                                                                   CVVInputField.Text         = "111";
                                                                 
                                                                   OnValueChanged(CreditCardNumberField.Text);
                                                                   OnDateValueChanged(DateInputField.Text);
                                                                   OnCVVValueChanged(CVVInputField.Text);
                                                                 
                                                                   // IsValidCreditCardNumber = true;
                                                                   // IsValidCVV              = true;
                                                                   // IsValidDate             = true;
                                                                 
                                                                   cbx_SaveCardDetails.IsChecked = true;
                                                                 
                                                                   CreditCardNumberField.Select();
                                                                 
                                                                   await Task.Delay(500);
                                                                   await new Step(name: $"set_test_credit_card", tags: new [] {"report"} ).Execute();
                                                                 
                                                               });
        
        public Step test_confirm() => new Step(name   : "credit_card_info_panel_confirm"
                                              ,action : async (s) => On_OkClick() );
    
        //    #endif // ANDROID
    
    }

}