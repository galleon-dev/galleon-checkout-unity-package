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

#if UNITY_ANDROID
using AdvancedInputFieldPlugin;
#endif

namespace Galleon.Checkout.UI
{
    public class CreditCardInfoPanelView : View
    {
        #if UNITY_ANDROID
        
        //////////////////////////////////////////////////////////////////////////// Members

        public List<AdvancedInputField> AdvancedInputFields;

        public AdvancedInputField CreditCardNumberField;
        public AdvancedInputField NameInputField;
        public AdvancedInputField DateInputField;
        public AdvancedInputField CVVInputField;

        public TMP_Text CardNumberErrorText;
        public TMP_Text NameErrorText;
        public TMP_Text CVVNumberErrorText;
        public TMP_Text DateErrorText;

        string NameMissingInfoText = "* Please Enter Name";
        string CardNumberInfoText = "* Please Enter Card Number";
        string CVVNumberInfoText = "* Please Enter CVV";
        string DateInfoText = "* Please Enter Date";

        public Image CardTypeIcon;
        public Sprite CardIcon_MasterCard;
        public Sprite CardIcon_Visa;
        public Sprite CardIcon_Amex;
        public Sprite CardIcon_Diners;
        public Sprite CardIcon_Discover;

        public CheckboxButton cbx_SaveCardDetails;

        private CardFormat CurrentCardFormat = default;
        private CardFormat lastFormatUsed;

        bool IsValidCVV = false;
        bool IsValidCreditCardNumber = false;
        bool IsValidDate = false;
        int expectedCVVLength = 3;
        
        //////////////////////////////////////////////////////////////////////////// View Result

        public ViewResult Result = ViewResult.None;

        public void Awake()
        {
            if (NameErrorText)
            {
                NameErrorText.gameObject.SetActive(false);
            }

            if (CVVNumberErrorText)
            {
                CVVNumberErrorText.gameObject.SetActive(false);
            }

            if (DateErrorText)
            {
                DateErrorText.gameObject.SetActive(false);
            }

            if (CardNumberErrorText)
            {
                CardNumberErrorText.gameObject.SetActive(false);
            }

            if (CVVInputField)
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
        }



        public enum ViewResult
        {
            None,
            Confirm,
        }

        //////////////////////////////////////////////////////////////////////////// View Flow

        public bool IsCompleted = false;

        public Step View()
        =>
            new Step(name: $"view_credit_card_panel"
                    , action: async (s) =>
                    {
                        IsCompleted = false;

                        this.gameObject.SetActive(true);

                        while (!IsCompleted)
                            await Task.Yield();

                        this.gameObject.SetActive(false);
                    });


        // Related to Autofill Fix while refocusing inputfields
        /* 
         
          private void OnEnable()
           {
                   RefocusInputFields();
           }

           private void Start()
           {
               NativeKeyboardManager.ResetAutofill();
           }

           public void RefocusInputFields()
           {
             StartCoroutine(RefocusAdvancedInputFields());
           }

           IEnumerator RefocusAdvancedInputFields()
           {
               Debug.Log("<color=green>RefocusInputFields() for Autofill</color>");
               int AdvancedInputFieldsAmount = AdvancedInputFields.Count;

               for (int i = 0; i < AdvancedInputFieldsAmount; i++)
               {
                   // yield return new WaitForEndOfFrame();
                   AdvancedInputFields[i].SelectionRefresh(); // instead of ManualSelect();
                   // AdvancedInputFields[i].SetCaretToTextEnd();
                   //   yield return new WaitForEndOfFrame();
                   AdvancedInputFields[i].ManualDeselect(EndEditReason.KEYBOARD_DONE); //EventSystem.current.SetSelectedGameObject(null); // Deselection
               }
               //  yield return new WaitForSeconds(0.5f);
               yield return null;
           }

        */
        //////////////////////////////////////////////////////////////////////////// UI Events

        public async void On_OkClick()
        {
            if (IsCorrectInputFields())
            {
                var card = new CreditCardUserUserPaymentMethod();

                card.Type        = CurrentCardFormat.Name;
                card.DisplayName = $"{card.Type} - **** - {CreditCardNumberField.Text.Substring(CreditCardNumberField.Text.Length - 4)}";

                card.CardHolderName = NameInputField.Text;
                card.CardNumber     = CreditCardNumberField.Text;
                card.CardCCV        = CVVInputField.Text;
                card.CardMonth      = DateInputField.Text.Substring(0, 2);
                card.CardYear       = DateInputField.Text.Substring(2, 2);
                
                await card.RunVaultingSteps().Execute();
                
                CheckoutClient.Instance.CurrentSession.User.AddPaymentMethod(card);
                CheckoutClient.Instance.CurrentSession.User.SelectPaymentMethod(card);

                this.Result = ViewResult.Confirm;
                CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(this.Result.ToString());
            }
        }

        bool IsCorrectInputFields()
        {
            bool InputFieldsCorrect = true;

            string Info = "* Please Enter Information";

            if (string.IsNullOrEmpty(NameInputField.Text))
            {
                NameErrorText.text = NameMissingInfoText;
                NameErrorText.gameObject.SetActive(true);
                InputFieldsCorrect = false;
            }
            else if (string.IsNullOrEmpty(CreditCardNumberField.Text))
            {
                CardNumberErrorText.text = CardNumberInfoText;
                CardNumberErrorText.gameObject.SetActive(true);
                InputFieldsCorrect = false;
            }
            else if (string.IsNullOrEmpty(CVVInputField.Text))
            {
                CVVNumberErrorText.text = CVVNumberInfoText;
                CVVNumberErrorText.gameObject.SetActive(true);
                InputFieldsCorrect = false;
            }
            else if (CVVInputField.Text.Length != expectedCVVLength)
            {
                CVVNumberErrorText.text = $"* Enter a {expectedCVVLength}-Digit CVV";
                CVVNumberErrorText.gameObject.SetActive(true);
                InputFieldsCorrect = false;
            }
            else if (string.IsNullOrEmpty(DateInputField.Text))
            {
                DateErrorText.text = DateInfoText;
                DateErrorText.gameObject.SetActive(true);
                InputFieldsCorrect = false;
            }

            if (!IsValidCVV && !IsValidCreditCardNumber && !IsValidDate)
            {
                Debug.Log("Invalid Entered Information");
                InputFieldsCorrect = false;
            }

            return InputFieldsCorrect;
        }

        IEnumerator CheckIfFocusOnCVV()
        {
            yield return new WaitForSeconds(0.1f);
            if (!string.IsNullOrEmpty(NameInputField.Text) && !string.IsNullOrEmpty(CreditCardNumberField.Text) && !string.IsNullOrEmpty(DateInputField.Text) && string.IsNullOrEmpty(CVVInputField.Text))
            {
                CardFormat CF = GetFormatForDigits(CreditCardNumberField.Text);
                if (MaxLength == 0)
                {
                    MaxLength = CF.MaxLength;
                }

                if (DateInputField.Text.Length == 4 && (CreditCardNumberField.Text.Length == MaxLength || (CreditCardNumberField.Text.Length == 16) && CF.Name.ToLower() == "visa"))
                {
                    // Debug.Log("SELECT CVV");
                    int AdvancedInputFieldsAmount = AdvancedInputFields.Count;
                    for (int i = 0; i < AdvancedInputFieldsAmount; i++)
                    {
                        if (AdvancedInputFields[i].Selected)
                        {
                            // Debug.Log("Deselected: " + AdvancedInputFields[i].name);
                            AdvancedInputFields[i].ManualDeselect(EndEditReason.PROGRAMMATIC_DESELECT);
                            yield return new WaitForEndOfFrame();
                        }
                    }
                    CVVInputField.ManualSelect();
                }
            }
            else
            {
                // Debug.Log("Some of the InputFields are not empty, therefore we don't focus on CVV");
            }
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

        void OnDateValueChanged(string rawInput)
        {
            Debug.Log("<color=green>OnDateValueChanged. rawInput: " + rawInput + "</color>");

            if (rawInput.Length > 4)
            {
                rawInput = rawInput.Remove(rawInput.Length - 1);
            }

            if (rawInput.Length == 4)
            {
                if (!ValidateDateExpiry(rawInput, out string err))
                {
                    DateErrorText.text = err;
                    DateErrorText.gameObject.SetActive(true);

                    IsValidDate = false;
                }
                else
                {
                    DateErrorText.gameObject.SetActive(false);
                    IsValidDate = true;

                    StartCoroutine(CheckIfFocusOnCVV());
                }
            }
            else
            {
                DateErrorText.gameObject.SetActive(false);
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
            string yearStr = formatted.Substring(2, 2);

            if (!int.TryParse(monthStr, out int month) ||
                !int.TryParse(yearStr, out int yy))
            {
                error = "Month/year must be numeric";
                return false;
            }

            if (month < 1 || month > 12)
            {
                error = "Invalid month (01–12 only)";
                return false;
            }

            // Interpret e.g. "24" as 2024 (assumes 2000–2099 range)
            int fullYear = 2000 + yy;
            var now = DateTime.Now;

            // Cards expire at end of month — valid if expiry >= end-of-month of current:
            var expiryEnd = new DateTime(fullYear, month, DateTime.DaysInMonth(fullYear, month), 23, 59, 59);

            if (expiryEnd < now)
            {
                error = "Card has expired";
                return false;
            }

            return true;
        }

        public void OnCVVValueChanged(string digits)
        {
            Debug.Log("<color=green>OnCVVValueChanged. rawInput: " + digits + "</color>");

            bool isAmex = CreditCardNumberField.Text.Replace(" ", "").StartsWith("34") ||
            CreditCardNumberField.Text.Replace(" ", "").StartsWith("37");
            int expectedCVVLength = isAmex ? 4 : 3;

            if (expectedCVVLength == 4)
            {
                CVVInputField.PlaceHolderText = "0000";
            }
            else
            {
                if (digits.Length == 4)
                {
                    CVVInputField.Text = digits.Remove(digits.Length - 1);
                }
                CVVInputField.PlaceHolderText = "000";
            }

            if (digits.Length > expectedCVVLength)
            {
                digits = digits.Remove(digits.Length - 1);
            }

            if (digits.Length != 0)
            {
                if (digits.Length == expectedCVVLength)
                {
                    CVVNumberErrorText.gameObject.SetActive(false);
                    IsValidCVV = true;
                }
                else
                {
                    CVVNumberErrorText.text = $"Enter a {expectedCVVLength}-digit CVV";
                    CVVNumberErrorText.gameObject.SetActive(true);
                    IsValidCVV = false;
                }
            }
            else
            {
                CVVNumberErrorText.gameObject.SetActive(false);
                IsValidCVV = true;
            }
        }

        public void OnNameValueChanged(string inputName)
        {
            if (!string.IsNullOrEmpty(inputName))
            {
                NameErrorText.gameObject.SetActive(false);
            }

            StartCoroutine(CheckIfFocusOnCVV());
        }

        int MaxLength;
        void FormatCreditCardInput(string rawInput)
        {
            Debug.Log("FormatCreditCardInput: " + rawInput);
            MaxLength = GetFormatForDigits(rawInput).MaxLength;
            if (rawInput.Length > MaxLength)
            {
                rawInput = rawInput.Remove(rawInput.Length - 1);
                Debug.Log("Updated RawInput: " + rawInput);
            }

            CheckLuhnOnEndEdit(rawInput);

            OnCVVValueChanged(CVVInputField.Text);

            StartCoroutine(CheckIfFocusOnCVV());
        }

        public void CheckLuhnOnEndEdit(string digits)
        {
            // LUHN VALIDATION (only when input is complete)
            // LUHN only checks if the number is structurally valid. If the final result is not divisible by 10, the number is invalid.

            IsValidCreditCardNumber = IsValidLuhn(digits); // digits.Length == format.MaxLength && 

            Debug.Log("LUHN isValid: " + IsValidCreditCardNumber);

            if (!IsValidCreditCardNumber)
            {
                CardNumberErrorText.gameObject.SetActive(true);
                CardNumberErrorText.text = "* Invalid Number";
            }
            else
            {
                CardNumberErrorText.gameObject.SetActive(false);
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
            public int MaxLength;
            public int[] GroupSizes;
            public int InputFieldLimit;
            public CardFormat(string name, int maxLength, int[] groupSizes, int inputFieldLimit)
            {
                Name = name;
                MaxLength = maxLength;
                GroupSizes = groupSizes;
                InputFieldLimit = inputFieldLimit;
            }
        }

        private static readonly List<(Func<string, bool> matcher, CardFormat format)> cardFormats = new()
        {
            // Amex: 15 digits → 4-6-5
            (d => d.StartsWith("34")
               || d.StartsWith("37"),
                new CardFormat("Amex", 15, new[] { 4, 6, 5 },
                    21)),

            // Visa: starts with 4, up to 19 digits → 4-4-4-4-3
            (d => d.StartsWith("4"),
                new CardFormat("Visa", 19, new[] { 4, 4, 4, 4, 3 },
                    31)),

            // MasterCard: 51–55, 2221–2720 → 16 digits → 4-4-4-4
            (d => (d.Length >= 2 && int.TryParse(d.Substring(0, 2), out var p2) && p2 >= 51 && p2 <= 55)
               || (d.Length >= 4 && int.TryParse(d.Substring(0, 4), out var p4) && p4 >= 2221 && p4 <= 2720),
                new CardFormat("MasterCard", 16, new[] { 4, 4, 4, 4 },
                    25)),

            // Discover: 6011, 65, 644–649 → 16 digits
            (d => d.StartsWith("6011")
               || d.StartsWith("65")
               || (d.Length >= 3 && int.TryParse(d.Substring(0, 3), out var p3) && p3 >= 644 && p3 <= 649),
                new CardFormat("Discover", 16, new[] { 4, 4, 4, 4 },
                    28)),

            // // JCB: 3528–3589 → 16–19 digits
            // (d => d.Length >= 4 && int.TryParse(d.Substring(0, 4), out var pJcb) && pJcb >= 3528 && pJcb <= 3589,
            //     new CardFormat("JCB", 19, new[] { 4, 4, 4, 4, 3 })),

            // Diners Club: starts with 36, 38, 39 → 14 digits → 4-6-4
            (d => d.StartsWith("36") || d.StartsWith("38") || d.StartsWith("39"),
                new CardFormat("Diners", 14, new[] { 4, 6, 4 },
                    20)),

            // Default fallback
            (_ => true, new CardFormat("Unknown", 16, new[] { 4, 4, 4, 4 },
                25))

        };

        bool IsValidLuhn(string digits)
        {
            int sum = 0;
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
                alt = !alt;
            }
            return digits.Length >= 12 && sum % 10 == 0; // avoid false positive on short input
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Test Scenarios
        
        public TestScenario scenario_2_part_1 => new TestScenario(expressions : new[]
                                                                              {
                                                                                  $"{nameof(test_fill_card_data)}()",
                                                                                  $"{nameof(test_confirm)}()"
                                                                              });
        
        public Step test_fill_card_data() => new Step(action : async (s) =>
                                                             {
                                                                 NameInputField.Text        = "jhon doe";
                                                                 CreditCardNumberField.Text = "4242 4242 4242 4242";
                                                                 DateInputField.Text        = "09/26";
                                                                 CVVInputField.Text         = "111";
                                                                 
                                                                 await Task.Delay(500);
                                                                 EntityNode.CurrentTestScenario = "scenario_2_part_2";
                                                             });
        
        public Step test_confirm() => new Step(action : async (s) => On_OkClick() );
    
        #endif // ANDROID
    
    }

}
