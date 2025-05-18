using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvancedInputFieldPlugin;
using Galleon.Checkout;
using Galleon.Checkout.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{
    public class CreditCardInfoPanelView : View
    {
        //////////////////////////////////////////////////////////////////////////// Members
        
        public AdvancedInputField CreditCardNumberField;
        public AdvancedInputField NameInputField;
        public AdvancedInputField DateInputField;
        public AdvancedInputField CCVField;
        
        public TMP_Text CardNumberErrorText;
        
        public Image  CardTypeIcon;
        public Sprite CardIcon_MasterCard;
        public Sprite CardIcon_Visa;
        public Sprite CardIcon_Amex;
        public Sprite CardIcon_Diners;
        public Sprite CardIcon_Discover;
        
        public CheckboxButton cbx_SaveCardDetails;
        
        private CardFormat CurrentCardFormat = default;

        //////////////////////////////////////////////////////////////////////////// View Result
            
        public      ViewResult Result = ViewResult.None;
        public enum ViewResult
        {
            None,
            Confirm,
        }
        
        //////////////////////////////////////////////////////////////////////////// View Flow

        public bool IsCompleted = false;

        public Step View()
        =>
            new Step(name   : $"view_credit_card_panel"
                    ,action : async (s) =>
                    {
                        IsCompleted = false;
                        
                        this.gameObject.SetActive(true);
                        
                        while (!IsCompleted)
                            await Task.Yield();
                        
                        this.gameObject.SetActive(false);
                    });
        
        //////////////////////////////////////////////////////////////////////////// Lifecycle

        public override void Initialize()
        {
            CardNumberErrorText.gameObject.SetActive(false);
        }

        //////////////////////////////////////////////////////////////////////////// UI Events
        
        public void On_OkClick()
        {
            PaymentMethod card = new PaymentMethod();
            
            card.Type = CurrentCardFormat.Name;
            card.DisplayName = $"{card.Type} - **** - {CreditCardNumberField.Text.Substring(CreditCardNumberField.Text.Length - 4)}";
            
            CheckoutClient.Instance.CurrentSession.User.AddPaymentMethod(card);
            CheckoutClient.Instance.CurrentSession.User.SelectPaymentMethod(card);
            
            
            this.Result = ViewResult.Confirm;
            CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(this.Result.ToString());
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Input Field Text Formatting
        
        
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
        
        void FormatInput(string rawInput)
        {
            if (isUpdating) return;
            isUpdating = true;

            string digits = "";
            int originalCaret = CreditCardNumberField.CaretPosition;
            int caretInDigits = 0;

            for (int i = 0; i < rawInput.Length; i++)
            {
                if (char.IsDigit(rawInput[i]))
                {
                    if (i < originalCaret) caretInDigits++;
                    digits += rawInput[i];
                }
            }

            var format = GetFormatForDigits(digits);
            if (digits.Length > format.MaxLength)
                digits = digits.Substring(0, format.MaxLength);

            ////////////
            SetCardIcon(format);
            ////////////
            
            string formatted = "";
            int digitIndex = 0;
            int newCaretPos = 0;

            for (int group = 0; group < format.GroupSizes.Length && digitIndex < digits.Length; group++)
            {
                int groupSize = format.GroupSizes[group];
                for (int i = 0; i < groupSize && digitIndex < digits.Length; i++)
                {
                    if (digitIndex < caretInDigits)
                        newCaretPos++;

                    formatted += digits[digitIndex++];
                }

                if (digitIndex < digits.Length)
                {
                    formatted += " ";
                    if (digitIndex < caretInDigits)
                        newCaretPos++;
                }
            }

            CreditCardNumberField.Text          = formatted;
            CreditCardNumberField.CaretPosition = newCaretPos;
            
            // Luhn check
            bool valid = IsValidLuhn(digits);
            if (CreditCardNumberField != null)
            {
              //inputBackground.color = valid ? Color.green : Color.red;
            }

            isUpdating = false;
            

            if (digits.Length == format.MaxLength)
            {
                CardNumberErrorText.gameObject.SetActive(!valid);
                
                string mark = valid ? "V" : "X";
                Debug.Log($"[{mark}] {formatted}");
            }
            else
            {
                CardNumberErrorText.gameObject.SetActive(false);
            }
            
            /////////////////////////////////
            
            this.CurrentCardFormat = format;
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
            public int    MaxLength;
            public int[]  GroupSizes;

            public CardFormat(string name, int maxLength, int[] groupSizes)
            {
                Name       = name;
                MaxLength  = maxLength;
                GroupSizes = groupSizes;
            }
        }

        private static readonly List<(Func<string, bool> matcher, CardFormat format)> cardFormats = new()
        {
            // Amex: 15 digits → 4-6-5
            (d => d.StartsWith("34") 
               || d.StartsWith("37"), 
                new CardFormat("Amex", 15, new[] { 4, 6, 5 })),

            // Visa: starts with 4, up to 19 digits → 4-4-4-4-3
            (d => d.StartsWith("4"), 
                new CardFormat("Visa", 19, new[] { 4, 4, 4, 4, 3 })),

            // MasterCard: 51–55, 2221–2720 → 16 digits → 4-4-4-4
            (d => (d.Length >= 2 && int.TryParse(d.Substring(0, 2), out var p2) && p2 >= 51   && p2 <= 55) 
               || (d.Length >= 4 && int.TryParse(d.Substring(0, 4), out var p4) && p4 >= 2221 && p4 <= 2720),
                new CardFormat("MasterCard", 16, new[] { 4, 4, 4, 4 })),

            // Discover: 6011, 65, 644–649 → 16 digits
            (d => d.StartsWith("6011") 
               || d.StartsWith("65") 
               || (d.Length >= 3 && int.TryParse(d.Substring(0, 3), out var p3) && p3 >= 644 && p3 <= 649),
                new CardFormat("Discover", 16, new[] { 4, 4, 4, 4 })),

         // // JCB: 3528–3589 → 16–19 digits
         // (d => d.Length >= 4 && int.TryParse(d.Substring(0, 4), out var pJcb) && pJcb >= 3528 && pJcb <= 3589,
         //     new CardFormat("JCB", 19, new[] { 4, 4, 4, 4, 3 })),

            // Diners Club: starts with 36, 38, 39 → 14 digits → 4-6-4
            (d => d.StartsWith("36") || d.StartsWith("38") || d.StartsWith("39"),
                new CardFormat("Diners", 14, new[] { 4, 6, 4 })),

            // Default fallback
            (_ => true, new CardFormat("Unknown", 16, new[] { 4, 4, 4, 4 }))
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

