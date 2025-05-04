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
        
        public TMP_InputField EmailInputField;
        public TMP_Text       ErrorText;
        
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
            var email = this.EmailInputField.text;
            
            if (string.IsNullOrEmpty(email))
            {
                ErrorText.text = "Email address is required";
                return;
            }

            try
            {
                using (var client = new SmtpClient("localhost", 25))
                {
                  //client.DeliveryMethod = SmtpDeliveryMethod.Network;
                  //client.UseDefaultCredentials = true;
                    
                    var message = new MailMessage
                    {
                        From = new MailAddress("test@localhost"),
                        Subject = "Your Purchase Receipt",
                        Body = $@"
                            <html>
                            <body>
                                <h1>Thank you for your purchase!</h1>
                                <p>Here is your receipt:</p>
                                <p>Order details will be included here...</p>
                            </body>
                            </html>",
                        IsBodyHtml = true
                    };
                    
                    message.To.Add(email);
                    
                    await client.SendMailAsync(message);
                    ErrorText.text = "Receipt email sent successfully";
                }
            }
            catch (System.Exception ex)
            {
                ErrorText.text = $"Failed to send email: {ex.Message}";
                Debug.LogError($"Failed to send email: {ex.Message}");
            }
        }
    }
}
