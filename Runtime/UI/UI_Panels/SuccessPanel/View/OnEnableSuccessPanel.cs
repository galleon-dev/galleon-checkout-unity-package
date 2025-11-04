using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Galleon.Checkout.UI
{
    public class OnEnableSuccessPanel : MonoBehaviour
    {
        public GameObject LeftSuccessPanel;

        void OnEnable()
        {
            LeftSuccessPanel.SetActive(true);
        }

        void OnDisable()
        {
            LeftSuccessPanel.SetActive(false);
        }
    }
}