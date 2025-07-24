using System;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using System.Net.Mail;
using System.Net;

namespace Galleon.Checkout.UI
{
    public class SuccessPanelView : View
    {
        //////////////////////////////////////////////////////////////////////////// View Result
        
        public      ViewResult Result = ViewResult.None;
        public enum ViewResult
        {
            None,
            Back,
            Confirm,
        }
        
        //////////////////////////////////////////////////////////////////////////// Members
        
       // public TMP_InputField EmailInputField;
        public AdvancedInputFieldPlugin.AdvancedInputField EmailInputField;
        public TMP_Text       ErrorText;
        public TMP_Text       MainText;
        
        //////////////////////////////////////////////////////////////////////////// Lifecycle

        public override void Initialize()
        {
            this.ErrorText.gameObject.SetActive(false);
        }

        //////////////////////////////////////////////////////////////////////////// View Flow

        public bool IsCompleted = false;
        
        public Step View()
        =>
            new Step(name   : $"view_success_panel"
                    ,action : async (s) =>
                    {
                        IsCompleted = false;
                        
                        this.gameObject.SetActive(true);
                        
                      //await s.CaptureReport();
                      //// while (!IsCompleted)
                      ////     await Task.Yield();
                      //await Task.Delay(1000);
                        
                      //this.gameObject.SetActive(false);
                    });
        
        //////////////////////////////////////////////////////////////////////////// UI Events
        
        public void OnConfirmSuccessButtonClick()
        {
            IsCompleted = true;
            Result      = ViewResult.Confirm;
        }
        
        public async void OnConfirmEmailButtonClick()
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
                From = new MailAddress("test@localhost"),
                Subject = "Your Purchase Receipt",
                Body = $@"
                    <html>
                    <body>
                        <h1>Thank you for your purchase!</h1>
                        <p>Here is your receipt:</p>
                        <p>product : {CheckoutClient.Instance.CurrentSession.SelectedProduct.DisplayName}</p>
                        <p>price   : {CheckoutClient.Instance.CurrentSession.SelectedProduct.PriceText  }</p>
                        <p>Time    : {DateTime.UtcNow.ToLongDateString()} - {DateTime.UtcNow.ToShortTimeString()} (UTC)</p>
                    </body>
                    </html>",
                IsBodyHtml = true
            };
            try
            {
                await SendEmail(to: email, subject:message.Subject, body:message.Body);
                
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
        
        public async Task SendEmail(string to, string subject, string body)
        {
            Debug.Log("Sending Email");
            
            var message        = new MailMessage("levan@galleon.so", to);
            message.Subject    = subject;
            message.Body       = body;
            message.IsBodyHtml = true;

            using var smtp   = new SmtpClient("smtp.gmail.com", 587);
            smtp.EnableSsl   = true;
            smtp.Credentials = new NetworkCredential("levan@galleon.so", "viil dbxh fvgo jcys");

            smtp.Send(message);
        }
    }
}
