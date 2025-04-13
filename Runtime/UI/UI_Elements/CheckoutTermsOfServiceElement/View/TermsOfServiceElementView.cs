using UnityEngine;

namespace Galleon.Checkout
{
    public class TermsOfServiceElementView : MonoBehaviour
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public string TermsOfServiceUrl = "https://marble-globe-2b8.notion.site/TERMS-OF-SERVICE-1ce0aaf8c2e080dbb6d8f16cd1fe6e66";
        public string PrivacyPolicyUrl  = "https://marble-globe-2b8.notion.site/PRIVACY-POLICY-1ce0aaf8c2e0800c911ed43e37c2ca6d";
        public string ReturnPolicyUrl   = "https://marble-globe-2b8.notion.site/TERMS-OF-SERVICE-1ce0aaf8c2e080dbb6d8f16cd1fe6e66";
        
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
