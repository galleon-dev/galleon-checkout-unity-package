using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AdvancedInputFieldPlugin;
using Galleon.Checkout;
using Galleon.Checkout.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Galleon.Checkout.UI
{
    public class CreditCardInfoPanelView : View
    {
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

        public Image CardTypeIcon;
        public Sprite CardIcon_MasterCard;
        public Sprite CardIcon_Visa;
        public Sprite CardIcon_Amex;
        public Sprite CardIcon_Diners;
        public Sprite CardIcon_Discover;

        public CheckboxButton cbx_SaveCardDetails;

        private CardFormat CurrentCardFormat = default;
        private CardFormat lastFormatUsed;
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
                CVVInputField.CharacterLimit = 3;
            }

            if (DateInputField)
            {
                DateInputField.CharacterLimit = 5; // "MM/YY"
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

        //////////////////////////////////////////////////////////////////////////// Lifecycle

        /*  public override void Initialize()
          {
              CardNumberErrorText.gameObject.SetActive(false);
          }
        */

        bool RefocusOnce = true;
        private void OnEnable()
        {
            if (RefocusOnce)
            {
                RefocusInputFields();
                RefocusOnce = false;
            }
        }

        // Related to Autofill Fix while refocusing inputfields
        public void RefocusInputFields()
        {
            StartCoroutine(RefocusAdvancedInputFields());
        }

        IEnumerator RefocusAdvancedInputFields()
        {
            int AdvancedInputFieldsAmount = AdvancedInputFields.Count;

            NativeKeyboardManager.Keyboard.SetIgnoreHeight(true);

            for (int i = 0; i < AdvancedInputFieldsAmount; i++)
            {
                yield return new WaitForEndOfFrame();
                AdvancedInputFields[i].SelectionRefresh(); // instead of ManualSelect();
                AdvancedInputFields[i].SetCaretToTextEnd();

                yield return new WaitForEndOfFrame();
                EventSystem.current.SetSelectedGameObject(null); // Deselection
            }
            yield return new WaitForSeconds(0.5f);

            NativeKeyboardManager.Keyboard.SetIgnoreHeight(false);

            yield return null;
        }

        //////////////////////////////////////////////////////////////////////////// UI Events

        public void On_OkClick()
        {
            if (IsCorrectInputFields())
            {
                PaymentMethod card = new PaymentMethod();

                card.Type = CurrentCardFormat.Name;
                card.DisplayName = $"{card.Type} - **** - {CreditCardNumberField.Text.Substring(CreditCardNumberField.Text.Length - 4)}";

                CheckoutClient.Instance.CurrentSession.User.AddPaymentMethod(card);
                CheckoutClient.Instance.CurrentSession.User.SelectPaymentMethod(card);


                this.Result = ViewResult.Confirm;
                CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(this.Result.ToString());
            }
        }

        bool IsCorrectInputFields()
        {
            bool InputFieldsCorrect = true;

            string Info = "Please enter information.";

            if (string.IsNullOrEmpty(NameInputField.Text))
            {
                NameErrorText.text = Info;
                NameErrorText.gameObject.SetActive(true);
                InputFieldsCorrect = false;
            }
            else if (string.IsNullOrEmpty(CreditCardNumberField.Text))
            {
                CardNumberErrorText.text = Info;
                CardNumberErrorText.gameObject.SetActive(true);
                InputFieldsCorrect = false;
            }
            else if (string.IsNullOrEmpty(CVVInputField.Text))
            {
                CVVNumberErrorText.text = Info;
                CVVNumberErrorText.gameObject.SetActive(true);
                InputFieldsCorrect = false;
            }
            else if (CVVInputField.Text.Length != expectedLength)
            {
                CVVNumberErrorText.text = $"Enter a {expectedLength}-digit CVV";
                CVVNumberErrorText.gameObject.SetActive(true);
            }
            else if (string.IsNullOrEmpty(DateInputField.Text))
            {
                DateErrorText.text = Info;
                DateErrorText.gameObject.SetActive(true);
                InputFieldsCorrect = false;
            }

            return InputFieldsCorrect;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Input Field Text Formatting

        private Coroutine caretCoroutine;
        private bool isUpdating;


        public void OnValueChanged(string text)
        {
            FormatInput(text);
        }

        ////////

        void SetCardIcon(CardFormat card)
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


        string lastDigitInput = "";
        void OnDateValueChanged(string rawInput)
        {

            Debug.Log("<color=green>OnDateValueChanged. rawInput: " + rawInput + "</color>");

            bool isBackspace = rawInput.Length <= lastDigitInput.Length;

            // Strip non-digits
            string digits = "";
            foreach (char c in rawInput)
            {
                if (char.IsDigit(c))
                    digits += c;
            }
            if (digits.Length > 4)
                digits = digits.Substring(0, 4);


            Debug.Log("Digits: " + digits);
            // Apply formatting
            // string formatted = digits.Length <= 2 ? digits : digits.Substring(0, 2) + "/" + digits.Substring(2);

            // Build formatted text
            string formatted;
            if (digits.Length == 0)
            {
                formatted = "";
            }
            else if (digits.Length == 1)
            {
                formatted = digits; // "1"
            }
            else if (digits.Length == 2)
            {
                formatted = digits + "/"; // "12/"
            }
            else
            {
                formatted = digits.Insert(2, "/"); // "12/3" or "12/34"
            }

            // Prevent slash from reappearing when backspacing from 3 -> 2
            if (isBackspace && digits.Length == 2)
            {
                formatted = digits;
            }

            // Only update if text changed
            if (DateInputField.Text != formatted)
            {
                DateInputField.SetText(formatted); // SetTextAndPreserveSelection
                DateInputField.SetCaretToTextEnd();
            }

            if (formatted.Length == 5)
            {
                if (!ValidateDateExpiry(formatted, out string err))
                {
                    DateErrorText.text = err;
                    DateErrorText.gameObject.SetActive(true);
                }
                else
                {
                    DateErrorText.gameObject.SetActive(false);
                }
            }
            else
            {
                DateErrorText.gameObject.SetActive(false);
            }

            lastDigitInput = digits;
        }

        public static bool ValidateDateExpiry(string formatted, out string error)
        {
            error = null;

            // Expect format "MM/YY"
            if (formatted.Length != 5 || formatted[2] != '/')
            {
                error = "Invalid format. Use MM/YY";
                return false;
            }

            string monthStr = formatted.Substring(0, 2);
            string yearStr = formatted.Substring(3, 2);

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



        // o Luhn check for CVV — only length matters which can be different based on card whether (3 or 4 numbers)
        int expectedLength = 3;
        public void OnCVVValueChanged()
        {
            Debug.Log("<color=green>OnCVVValueChanged. rawInput: " + CVVInputField.Text + "</color>");

            string rawInput = CVVInputField.Text;

            if (rawInput.Length != 0)
            {
                //   CVVNumberErrorText.gameObject.SetActive(false);

                // Strip non-digits
                string digits = new string(rawInput.Where(char.IsDigit).ToArray());

                // Determine expected length (4 for Amex, else 3)
                bool isAmex = CreditCardNumberField.Text.Replace(" ", "").StartsWith("34") ||
                              CreditCardNumberField.Text.Replace(" ", "").StartsWith("37");
                expectedLength = isAmex ? 4 : 3;

                CVVInputField.CharacterLimit = expectedLength;

                // Trim to max length & set
                if (digits.Length > expectedLength)
                {
                    digits = digits.Substring(0, expectedLength);
                }

                CVVInputField.Text = digits;

                // Validate
                if (digits.Length == expectedLength)
                {
                    CVVNumberErrorText.gameObject.SetActive(false);
                }
                else
                {
                    CVVNumberErrorText.text = $"Enter a {expectedLength}-digit CVV";
                    CVVNumberErrorText.gameObject.SetActive(true);
                }
            }
            else
            {
                CVVNumberErrorText.gameObject.SetActive(false);
            }
        }

        public void OnNameValueChanged(string inputName)
        {
            if (!string.IsNullOrEmpty(inputName))
            {
                NameErrorText.gameObject.SetActive(false);
            }
            // Validator selected for name entering from inspector menu 
        }

        // OnTMPValueChanged FOR TESTING
        /*
        public void OnTMPValueChanged(string text)
        {
            Debug.Log("OnTMPValueChanged: " + text);
            string formatted = FormatCardNumber(text);
            if (CreditCardNumberField.Text != formatted)
            {
                int caretPosition = formatted.Length;
                CreditCardNumberFieldTMP.text = formatted;
                CreditCardNumberFieldTMP.caretPosition = caretPosition;
            }

            return;
        }
        */
        void FormatInput(string rawInput)
        {
            //   if (isUpdating) return;
            //   isUpdating = true;

            Debug.Log("<color=green>CreditCardNumberField. rawInput: " + rawInput + "</color>");

            // SET FORMATTING
            string digits = Regex.Replace(rawInput, @"\D", "");
            string formatted = FormatCardNumber(digits);
            if (CreditCardNumberField.Text != formatted)
            {
                Debug.Log("Formatted: " + formatted);
                CreditCardNumberField.SetText(formatted);
                CreditCardNumberField.SetCaretToTextEnd();
            }

            // SET CARD ICON
            var format = GetFormatForDigits(digits);
            if (format.Name != lastFormatUsed.Name)
            {
                SetCardIcon(format);
                lastFormatUsed = format;

                CreditCardNumberField.CharacterLimit = format.InputFieldLimit;
            }

            // LUHN VALIDATION (only when input is complete)
            // LUHN only checks if the number is structurally valid. If the final result is not divisible by 10, the number is invalid.
            bool isValid = IsValidLuhn(digits); // digits.Length == format.MaxLength && 

            Debug.Log("LUHN isValid: " + isValid);
            Debug.Log("digits.Length == format.MaxLength: " + (digits.Length == format.MaxLength));

            // I have excluded (digits.Length == format.MaxLength) in conditions rechecking as for Visa card this varies between 13-19 digits, where in our logic we defined only 19. 

            if (!isValid)
            {
                CardNumberErrorText.gameObject.SetActive(true);
            }
            else
            {
                CardNumberErrorText.gameObject.SetActive(false);
            }

            this.CurrentCardFormat = format;
        }


        public static string FormatCardNumber(string digits)
        {
            // Group digits into chunks of 4, then join with " - "
            string formatted = Regex.Replace(digits, ".{1,4}", "$0").Trim();
            string[] groups = Regex.Matches(formatted, ".{1,4}")
                                   .Select(m => m.Value)
                                   .ToArray();

            string FormattedNumber = string.Join(" - ", groups);
            return FormattedNumber;
        }

        private IEnumerator DelayedSetCaret(int position)
        {
            yield return null; // Wait for end of frame
            CreditCardNumberField.CaretPosition = position;
        }

        ////////

        CardFormat GetFormatForDigits(string digits)
        {
            foreach (var (matcher, format) in cardFormats)
            {
                if (matcher(digits))
                    return format;
            }
            return cardFormats[^1].format; // fallback
        }

        ////////

        private struct CardFormat
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

        ////////

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
    }
}

