using UnityEngine;

namespace Galleon.FooterText
{
    public class ManageFooterText : MonoBehaviour
    {
        public static ManageFooterText Instance { get; private set; }

        public GameObject ViewPaymentMethodsPanel;
        public GameObject TermsOfServicePanel;
        private void Awake()
        {
            Instance = this;
        }

        public void Show(bool _Status)
        {
            Debug.Log("Show View Payment Methods Panel: " + _Status);
            ViewPaymentMethodsPanel.SetActive(_Status);
        }

        public void ShowTermsOfService(bool _Status)
        {
            Debug.Log("Show Terms Of Service Panel: " + _Status);
            TermsOfServicePanel.SetActive(_Status);
        }

    }
}