using System.Collections;
using System.Collections.Generic;
using Galleon.Checkout.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{
    public class SettingsPanelPaymentMethodItem : View
    {
        //// Members
        
        [Header("UI")]
        public Image    Icon;
        public TMP_Text Label;
        
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
        }

        //// UI Events

        public void On_Delete_Clicked()
        {
            Debug.Log((this.UserPaymentMethod?.DisplayName??"NULL") + "_delete clicked");
            this.SettingsPanelView.DeletePaymentMethod(this.UserPaymentMethod);
        }
    }
}