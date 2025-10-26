using System;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using System.Net.Mail;
using System.Net;
using AdvancedInputFieldPlugin;

namespace Galleon.Checkout.UI
{
    public class SuccessPanelView : View
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// View Result

        public      ViewResult Result = ViewResult.None;
        public enum ViewResult
        {
            None,
            Back,
            Confirm,
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
      
        public GameObject         EmailInputFieldText;
        public GameObject         Gap;
        public GameObject         EmailInputFieldContainer;

        public AdvancedInputField EmailInputField;
        public TMP_Text           ErrorText;
        public TMP_Text           MainText;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public override void Initialize()
        {
            this.ErrorText.gameObject.SetActive(false);     
        }

        private void Update()
        {
            EmailInputField.Text = EmailInputField.Text;
        }

        private void OnEnable()
        {
            string Email = PlayerPrefs.GetString("Email");

            if (!string.IsNullOrEmpty(Email))
            {
                EmailInputField.Text = Email;
            }

            if (string.IsNullOrEmpty(EmailInputField.Text))
            {
                if (EmailInputFieldContainer)
                {
                    ShowEmail(true);
                }
            }
            else
            {
                if (EmailInputFieldContainer)
                {
                    ShowEmail(false);
                }
            }
        }

        public void ShowEmail(bool Status)
        {
            Status = true;
            EmailInputFieldText.SetActive(Status);
            Gap.SetActive(Status);
            EmailInputFieldContainer.SetActive(Status);
        }

        //////////////////////////////////////////////////////////////////////////// UI Events

        public void OnConfirmSuccessButtonClick()
        {
            Result = ViewResult.Confirm;
            CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(Result.ToString());
        }

        public async void OnConfirmEmailButtonClick()
        {
            await SendReceipt();
            OnConfirmSuccessButtonClick();
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Private API Methods
        
        private async Task SendReceipt()
        {
            var email = this.EmailInputField.Text;

            if (string.IsNullOrEmpty(email))
            {
                this.ErrorText.gameObject.SetActive(true);
                ErrorText.text = "Email address is required";
                return;
            }

            var message = new MailMessage
            {
                From       = new MailAddress("test@localhost"),
                Subject    = "Your Purchase Receipt",
                IsBodyHtml = true,
                Body       = $@"
                             <html>
                             <body>
                                 <h1>Thank you for your purchase!</h1>
                                 <p>Here is your receipt:</p>
                                 <p>product : {CheckoutClient.Instance.CurrentSession.SelectedProduct.DisplayName}</p>
                                 <p>price   : {CheckoutClient.Instance.CurrentSession.SelectedProduct.PriceText  }</p>
                                 <p>Time    : {DateTime.UtcNow.ToLongDateString()} - {DateTime.UtcNow.ToShortTimeString()} (UTC)</p>
                             </body>
                             </html>",
            };
            try
            {
                await SendEmail(to: email, subject: message.Subject, body: message.Body);

                this.ErrorText.gameObject.SetActive(false);
                MainText.text = "Done! Your receipt has been sent to your email. Check your inbox for the details.";
            }
            catch (System.Exception ex)
            {
                this.ErrorText.gameObject.SetActive(true);
                ErrorText.text = $"Failed to send email";
                Debug.LogError($"Failed to send email: {ex.Message}");
            }
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Private Helper Methods
        
        
        private async Task SendEmail(string to, string subject, string body)
        {
            Debug.Log("Sending Email");

            var message        = new MailMessage("levan@galleon.so", to);
            message.Subject    = subject;
            message.Body       = body;
            message.IsBodyHtml = true;

            using var smtp     = new SmtpClient("smtp.gmail.com", 587);
            smtp.EnableSsl     = true;
            smtp.Credentials   = new NetworkCredential("levan@galleon.so", "viil dbxh fvgo jcys");

            smtp.Send(message);
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Test
        
        public Step TEST_FillTestEmail() 
        =>
            new Step(name   : $"fill_test_email"
                    ,action : async (s) =>
                    {
                        this.EmailInputField.Text = "levan@galleon.so";
                    });
        
        public Step TEST_SendReceiptClicked() 
        =>
            new Step(name   : $"test_click_send_receipt"
                    ,action : async (s) =>
                    {
                        OnConfirmEmailButtonClick();
                    });
    }
}

