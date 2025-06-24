using Galleon.FooterText;
using UnityEngine;

namespace Galleon.Footer
{
    public class CheckFooter : MonoBehaviour
    {
        private void OnEnable()
        {
            if (ManageFooterText.Instance)
            {
                ManageFooterText.Instance.Show(true);
            }
        }

        private void OnDisable()
        {
            if (ManageFooterText.Instance)
            {
                ManageFooterText.Instance.Show(false);
            }
        }
    }
}