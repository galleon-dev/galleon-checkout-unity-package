using UnityEngine;

namespace Galleon.Checkout
{
    public class TermsOfServiceElementView : MonoBehaviour
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public string TermsOfServiceUrl => CHECKOUT.Config.GetString("terms_of_service_url", "https://www.superplay.co/terms/");
        public string PrivacyPolicyUrl  => CHECKOUT.Config.GetString("privacy_policy_url",   "https://www.superplay.co/terms/");
        public string ReturnPolicyUrl   => CHECKOUT.Config.GetString("return_policy_url",    "https://www.superplay.co/terms/");
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Events
        
        public async void OnTermsOfServiceClick()
        {
            await OpenTermsOfServiceURL().Execute();
        }
        
        public async void OnPrivacyPolicyClick()
        {
            await OpenPrivacyPolicyURL().Execute();
        }
        
        public async void OnReturnPolicyClick()
        {
            await OpenReturnPolicyURL().Execute();
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Steps
        
        public Step OpenTermsOfServiceURL()
        =>
            new Step(name   : $"open_terms_of_service_url"
                    ,action : async (s) =>
                    {
                        Application.OpenURL(TermsOfServiceUrl);
                    });
        
        public Step OpenPrivacyPolicyURL()
        =>
            new Step(name   : $"open_privacy policy_url"
                    ,action : async (s) =>
                    {
                        Application.OpenURL(PrivacyPolicyUrl);
                    });
        
        public Step OpenReturnPolicyURL()
        =>
            new Step(name   : $"open_return_policy_url"
                    ,action : async (s) =>
                    {
                        Application.OpenURL(ReturnPolicyUrl);
                    });
    }
}
