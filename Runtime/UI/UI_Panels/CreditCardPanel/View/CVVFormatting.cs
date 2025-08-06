// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#if ANDROID


using Galleon.Checkout.UI;
using System.Text;
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
    /// <summary>Class to format text as date (000 or 0000)</summary>
    public class CVVFormatting : LiveDecorationFilter
    {
        public AdvancedInputField CVVAdvancedInputField;
        public CreditCardInfoPanelView CreditCardInfoPanelView;

        /// <summary>The maximum amount of number characters</summary>
        private int MAX_NUMBERS = 4;

        /// <summary>The character used to separate numbers</summary>

        /// <summary>The StringBuilder</summary>
        private StringBuilder stringBuilder;

        /// <summary>The StringBuilder</summary>
        public StringBuilder StringBuilder
        {
            get
            {
                if (stringBuilder == null)
                {
                    stringBuilder = new StringBuilder();
                }

                return stringBuilder;
            }
        }

        public override string ProcessText(string text, int caretPosition)
        {
           // Debug.Log("CVV ProcessText: " + text);
            StringBuilder.Length = 0; //Clears the contents of the StringBuilder
            int numberCount = 0;

            int length = text.Length;
            if (length == 0)
            {
                return string.Empty;
            }

            // Determine expected length (4 for Amex, else 3)
            bool isAmex = CreditCardInfoPanelView.CreditCardNumberField.Text.Replace(" ", "").StartsWith("34") ||
            CreditCardInfoPanelView.CreditCardNumberField.Text.Replace(" ", "").StartsWith("37");
            int expectedCVVLength = isAmex ? 4 : 3;

            MAX_NUMBERS = expectedCVVLength;
            if (MAX_NUMBERS == 4)
            {
                CVVAdvancedInputField.PlaceHolderText = "0000";
            }
            else
            {
                CVVAdvancedInputField.PlaceHolderText = "000";
            }

            if (length > MAX_NUMBERS)
            {
                CVVAdvancedInputField.Text = CVVAdvancedInputField.Text.Remove(length - 1);
                return StringBuilder.ToString();
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    char c = text[i];

                    if (IsNumber(c))
                    {
                        numberCount++;
                        if (numberCount > MAX_NUMBERS)
                        {
                            Debug.LogWarning("There are more than 4 number characters. Please set the character limit to 4 to support the date format (00/00)");
                            return null;
                        }

                        StringBuilder.Append(c);
                    }
                    else
                    {
                        Debug.LogWarning("Unexpected character: " + c);
                        return string.Empty;
                    }
                }
                return StringBuilder.ToString();
            }
        }

        public override int DetermineProcessedCaret(string text, int caretPosition, string processedText)
        {
            // Debug.Log("DetermineProcessedCaret");

            if (caretPosition == 0)
            {
                return 0;
            }

            int length = processedText.Length;
            if (length == 0)
            {
                return 0;
            }

            int numberCount = 0;
            for (int i = 0; i < length; i++)
            {
                char c = processedText[i];

                if (IsNumber(c))
                {
                    numberCount++;

                    if (numberCount == caretPosition + 1)
                    {
                        return i;
                    }
                }
            }

            return length;
        }

        public override int DetermineCaret(string text, string processedText, int processedCaretPosition)
        {
            if (processedCaretPosition == 0)
            {
                return 0;
            }

            int length = processedText.Length;
            if (length == 0)
            {
                return 0;
            }

            if (processedCaretPosition == length)
            {
                return text.Length;
            }

            int numberCount = 0;
            for (int i = 0; i < processedCaretPosition; i++)
            {
                char c = processedText[i];

                if (IsNumber(c))
                {
                    numberCount++;
                }
            }

            return numberCount;
        }

        private bool IsNumber(char c)
        {
            return (c >= '0' && c <= '9');
        }
    }
}


#endif