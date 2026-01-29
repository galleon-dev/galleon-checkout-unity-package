// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using UnityEngine;

using Galleon.Checkout.UI;
using System.Text;

namespace AdvancedInputFieldPlugin
{
    /// Class to format text as credit card number separated by spaces every 4 numbers
    public class CreditCardNumberFormatting : LiveDecorationFilter
    {
        
        public AdvancedInputField          CreditCardAdvancedInputField;
        public CreditCardInfoPanelView     CreditCardInfoPanelView;
        CreditCardInfoPanelView.CardFormat cardFormat;

        /// The maximum amount of separator characters to use
        private int MAX_SEPARATOR_CHARACTERS = 3;
        private int MaxInputFieldLimit = 16;
        
        /// The character used to separate groups of 4 numbers
        [SerializeField]
        private string separatorCharacter = " - ";

        /// The StringBuilder
        private StringBuilder stringBuilder;

        /// The StringBuilder
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
            // Debug.Log("ProcessText, caretPosition: " + caretPosition);
            int length = text.Length;

            if (length > MaxInputFieldLimit)
            {
                CreditCardAdvancedInputField.Text = CreditCardAdvancedInputField.Text.Remove(length - 1);
                return StringBuilder.ToString();
            }
            else
            {
                StringBuilder.Length = 0; //Clears the contents of the StringBuilder
                int numberCount    = 0;
                int separatorCount = 0;


                if (length == 0)
                {
                    if (CreditCardInfoPanelView)
                    {
                        CreditCardInfoPanelView.RemoveCardIcon();
                    }
                    return string.Empty;
                }

                for (int i = 0; i < length; i++)
                {
                    char c = text[i];

                    if (IsNumber(c))
                    {
                        if (i >= 0 && i <= 4)
                        {
                            // Get Card Type
                            if (CreditCardInfoPanelView)
                            {
                                cardFormat = CreditCardInfoPanelView.GetFormatForDigits(text);
                                CreditCardInfoPanelView.SetCardIcon(cardFormat);
                            }

                            // Set Limit
                            if (CreditCardAdvancedInputField)
                            {
                                MaxInputFieldLimit       = cardFormat.MaxLength;
                                MAX_SEPARATOR_CHARACTERS = cardFormat.GroupSizes.Length; //cardFormat.MaxLength;
                                // Debug.Log("SET LIMIT: " + cardFormat.MaxLength);
                            }
                        }

                        // DYNAMIC SEPARATOR APPROACH
                        if (numberCount == cardFormat.GroupSizes[separatorCount] && separatorCount < MAX_SEPARATOR_CHARACTERS)
                        {
                            numberCount = 0;
                            StringBuilder.Append(separatorCharacter);
                            separatorCount++;
                        }

                        numberCount++;

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
            // Focus / refocus formatting pass - DO NOT override caret
            if (caretPosition == 0 && text.Length > 0 && processedText.Length > 0)
                return processedText.Length;

            int numberCount = 0;

            for (int i = 0; i < processedText.Length; i++)
            {
                if (IsNumber(processedText[i]))
                {
                    if (numberCount == caretPosition)
                        return i;

                    numberCount++;
                }
            }

            return processedText.Length;
        }

        public override int DetermineCaret(string text, string processedText, int processedCaretPosition)
        {
            if (processedCaretPosition <= 0)
                return 0;

            int numberCount = 0;

            for (int i = 0; i < processedCaretPosition; i++)
            {
                if (IsNumber(processedText[i]))
                    numberCount++;
            }

            return Mathf.Clamp(numberCount, 0, text.Length);
        }

        private bool IsNumber(char c)
        {
            return c >= '0' && c <= '9';
        }
    }
}