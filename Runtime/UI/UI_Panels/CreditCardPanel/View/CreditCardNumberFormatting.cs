// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using UnityEngine;

#if UNITY_EDITOR
public class CreditCardNumberFormatting : MonoBehaviour {}
#elif ANDROID && !UNITY_EDITOR

using Galleon.Checkout.UI;
using System.Text;
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
    /// <summary>Class to format text as credit card number separated by spaces every 4 numbers</summary>
    public class CreditCardNumberFormatting : LiveDecorationFilter
    {
        
        public AdvancedInputField CreditCardAdvancedInputField;
        public CreditCardInfoPanelView CreditCardInfoPanelView;
        CreditCardInfoPanelView.CardFormat cardFormat;

        /// <summary>The maximum amount of separator characters to use</summary>
        private int MAX_SEPARATOR_CHARACTERS = 3;
        private int MaxInputFieldLimit = 16;
        /// <summary>The character used to separate groups of 4 numbers</summary>
        [SerializeField]
        private string separatorCharacter = " - ";

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
                int numberCount = 0;
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
                                MaxInputFieldLimit = cardFormat.MaxLength;
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
           // Debug.Log("DetermineProcessedCaret(), caretPosition: " + caretPosition + " processedText: " + processedText.Length + " Only Numbers: " + CreditCardAdvancedInputField.Text.Length);

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
           //Debug.Log("DetermineCaret(), processedCaretPosition: " + processedCaretPosition + " processedText: " + processedText.Length + " Only Numbers: " + CreditCardAdvancedInputField.Text.Length);

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