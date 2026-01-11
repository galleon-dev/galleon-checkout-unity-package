using System.Collections;
using System.Collections.Generic;
using Galleon.Checkout.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{
    public class SettingsPanelToggleItem : View
    {
        //// Members
        
        [Header("UI")]
        public Image    Icon;
        public TMP_Text Label;
        
        public GameObject ToggleOffGO;
        public GameObject ToggleOnGO;
        
        //// Properties
        
        public UserPaymentMethod UserPaymentMethod { get; set; }
        public SettingsPanelView SettingsPanelView { get; set; }
        
        //// Lifecycle
        
        public void Initialize(UserPaymentMethod userPaymentMethod, SettingsPanelView settingsPanelView)
        {
            this.UserPaymentMethod = userPaymentMethod;
            this.SettingsPanelView = settingsPanelView;
            Refresh();
        }
        
        //// Refresh
        
        public override void RefreshState()
        {    
            this.Label.text  = UserPaymentMethod?.DisplayName;
            this.Icon.sprite = this.UserPaymentMethod?.GetIconSprite();
            
            this.ToggleOffGO.SetActive(!CHECKOUT.Globals.IsNativeStoreEnabled);
            this.ToggleOnGO.SetActive( CHECKOUT.Globals.IsNativeStoreEnabled);
        }

        //// UI Events

        public void On_ToggleClicked()
        {
            CHECKOUT.Globals.IsNativeStoreEnabled = !CHECKOUT.Globals.IsNativeStoreEnabled;
            RefreshState();
        }
    }
}