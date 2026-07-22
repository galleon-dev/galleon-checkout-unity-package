using System;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using System.Net.Mail;
using System.Net;
using AdvancedInputFieldPlugin;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Galleon.Checkout.UI
{
    public class SuccessPanelView : View
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// View Result

        public ViewResult Result = ViewResult.None;
        public enum ViewResult
        {
            None,
            Back,
            Confirm,
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members

        public GameObject       EmailInputFieldText;
        public List<GameObject> Gaps;
        public GameObject       EmailInputFieldContainer;
        public GameObject       EmailButtonGO;
        public GameObject       SuccessLabel;
        public GameObject       SuccessLabelForExistingEmail;

        public AdvancedInputField EmailInputField;
        public TMP_Text           ErrorText;
        public TMP_Text           MainText;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public override void Initialize()
        {
            this.ErrorText.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            EmailInputField.Text = "";
        }

        public override async void RefreshState()
        {
            // Analytics: Receipt Email Screen Viewed
            CheckoutAPI.InvokeAnalyticsEvent("receipt_email_screen_viewed", new()
            {
                { "checkout_session_id",    CHECKOUT.Session?.SessionID                 ?? "" },
                { "purchase_amount",        CHECKOUT.Session?.SelectedProduct?.Amount   ?? 0m },
                { "currency",               CHECKOUT.Session?.SelectedProduct?.Currency ?? "" }
            });

            // If the payment method used for this session was PayPal, we already have the
            // buyer's email from PayPal, so we skip asking for it and auto-confirm (same
            // path as when the email is already known).
            bool isPayPal = CHECKOUT.PaymentMethods.SelectedUserPaymentMethod?.Type == "paypal";
            // Web checkout completes on a hosted page; treat it like PayPal / a known email:
            // skip the email prompt, just show the success checkmark, then auto-close.
            bool isWebCheckout = CHECKOUT.PaymentMethods.SelectedUserPaymentMethod?.Type == "web_checkout";
            bool hasEmail = !CHECKOUT.User.Email.IsNullOrEmpty();

            if (!hasEmail && !isPayPal && !isWebCheckout)
            {
                if (EmailInputFieldContainer)
                    ShowEmail();

                await new Step(name: $"capture_email", tags: new[] { "report" }).Execute();
            }
            else
            {
                if (hasEmail)
                    EmailInputField.Text = CHECKOUT.User.Email;

                if (EmailInputFieldContainer)
                    HideEmail();

                await Task.Delay(1200);

                await new Step(name: $"capture_email", tags: new[] { "report" }).Execute();

                Result = ViewResult.Confirm;
                CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(Result.ToString());
            }
        }
        
        public void EndPageWithResult()
        {
            Result = ViewResult.Confirm;
            CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(Result.ToString());
        }
        
        //////////////////////////////////////////////////////////////////////////// UI Events

        public async void OnConfirmEmailButtonClick()
        {
            bool valid      = ValidateEmail(EmailInputField.Text);
            ErrorText.text  = valid ? "" : "Invalid email format";
            ErrorText.gameObject.SetActive(!valid);

            if (valid)
            {
                // Analytics: Receipt Email Form Sent
                CheckoutAPI.InvokeAnalyticsEvent("receipt_email_form_sent", new Dictionary<string, object>
                {
                    { "checkout_session_id", CHECKOUT.Session?.SessionID                 ?? "" },
                    { "purchase_amount",     CHECKOUT.Session?.SelectedProduct?.Amount   ?? 0m },
                    { "currency",            CHECKOUT.Session?.SelectedProduct?.Currency ?? "" }
                });

                CHECKOUT.Session.OnSessionFinishedStep.AddChildStep(SaveEmail());

                if (!CHECKOUT.IsTest)
                    CHECKOUT.Session.OnSessionFinishedStep.AddChildStep(SendReceipt());

                EndPageWithResult();
            }
        }
        

        public async void On_FinishedEditingEmail(string str, EndEditReason reason)
        {
            bool valid      = ValidateEmail(EmailInputField.Text);

            if (!string.IsNullOrEmpty(EmailInputField.Text))
            {
                ErrorText.text = valid ? "" : "Invalid email format";
            }
            else
            {
                ErrorText.text = "";
            }

            ErrorText.gameObject.SetActive(!valid);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Email storage

        public Step SaveEmail()
        =>
            new Step(name   : $"save_email"
                    ,action : async (s) =>
                    {
                        CHECKOUT.User.UserInfo.email = this.EmailInputField.Text;
                        await CHECKOUT.Actions.UpdateEmail().Execute();

                    });

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Private API Methods

        public Step SendReceipt()
        =>
            new Step(name: $"send_receipt"
                    , action: async (s) =>
                    {
                        return; 
                        
                        var email = this.EmailInputField.Text;

                        if (string.IsNullOrEmpty(email))
                        {
                            this.ErrorText.gameObject.SetActive(true);
                            ErrorText.text = "Email address is required";
                            return;
                        }

                        var message = new MailMessage
                        {
                            From = new MailAddress("test@localhost"),
                            Subject = "Your Purchase Receipt",
                            IsBodyHtml = true,
                            Body = $@"
                                         <html>
                                         <body>
                                             <h1>Thank you for your purchase!</h1>
                                             <p>Here is your receipt:</p>
                                             <p>product : {CheckoutClient.Instance.CurrentSession.SelectedProduct.DisplayName}</p>
                                             <p>Time    : {DateTime.UtcNow.ToLongDateString()} - {DateTime.UtcNow.ToShortTimeString()} (UTC)</p>
                                         </body>
                                         </html>",
                        };

                        Debug.Log(email + " " + message.Subject + " " + message.Body);

                        try
                        {
                            await SendEmail(to: email, subject: message.Subject, body: message.Body).Execute();

                            this.ErrorText.gameObject.SetActive(false);
                            MainText.text = "Done! Your receipt has been sent to your email. \nCheck your inbox for the details.";
                        }
                        catch (System.Exception ex)
                        {
                            this.ErrorText.gameObject.SetActive(true);
                            ErrorText.text = $"Failed to send email";
                            Debug.LogError($"Failed to send email: {ex.Message}");
                        }
                    });


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI helper methods

        private void ShowEmail()
        {
            EmailInputFieldText.SetActive(true);
            EnableGaps(true);
            EmailInputFieldContainer.SetActive(true);

            EmailButtonGO.SetActive(true);
            SuccessLabel.SetActive(true);
            SuccessLabelForExistingEmail.SetActive(false);
        }

        private void HideEmail()
        {
            EmailInputFieldText.SetActive(false);
            EnableGaps(false);
            EmailInputFieldContainer.SetActive(false);

            EmailButtonGO.SetActive(false);
            SuccessLabel.SetActive(false);
            SuccessLabelForExistingEmail.SetActive(true);
        }

        void EnableGaps(bool Status)
        {
            int GapsAmount = Gaps.Count;

            for (int i = 0; i < GapsAmount; i++)
            {
                Gaps[i].SetActive(Status);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Private Helper Methods

        public Step SendEmail(string to, string subject, string body)
        =>
            new Step(name: $"send_email"
                    , action: async (s) =>
                    {

                        var message         = new MailMessage("levan@galleon.so", to);
                        message.Subject     = subject;
                        message.Body        = body;
                        message.IsBodyHtml  = true;

                        using var smtp      = new SmtpClient("smtp.gmail.com", 587);
                        smtp.EnableSsl      = true;
                        smtp.Credentials    = new NetworkCredential("levan@galleon.so", "viil dbxh fvgo jcys");

                        smtp.Send(message);
                    });

        
        private bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            /// Basic email regex pattern explained:
            /// ^       - Start of string
            /// [^@\s]+ - One or more characters that are not @ or whitespace
            /// @       - Literal @ symbol
            /// [^@\s]+ - One or more characters that are not @ or whitespace (domain name)
            /// \.      - Literal dot
            /// [^@\s]+ - One or more characters that are not @ or whitespace (TLD)
            /// $       - End of string
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Test

        public Step TEST_SuccesPanelWaitAndDoNothing()
        =>
            new Step(name: $"success_panel_wait_and_do_nothing"
                    , action: async (s) =>
                    {
                    });

        public Step TEST_FillTestEmail()
        =>
            new Step(name: $"fill_test_email"
                    , action: async (s) =>
                    {
                        this.EmailInputField.Select();
                        this.EmailInputField.Text = "levan@galleon.so";
                        await Task.Delay(500);
                        await new Step(name: $"set_test_credit_card", tags: new[] { "report" }).Execute();
                    });

        public Step TEST_SendReceiptClicked()
        =>
            new Step(name: $"test_click_send_receipt"
                    , action: async (s) =>
                    {
                        OnConfirmEmailButtonClick();
                    });
    }
}

