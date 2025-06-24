using UnityEngine;

namespace Galleon.FooterText
{
    public class ManageFooterText : MonoBehaviour
    {
        public static ManageFooterText Instance { get; private set; }

        public GameObject ViewPayemtnMethodsPanel;

        private void Awake()
        {
            Instance = this;
        }

        public void Show(bool _Status)
        {
            Debug.Log("Show Footer: " + _Status);
            ViewPayemtnMethodsPanel.SetActive(_Status);
        }

    }
}