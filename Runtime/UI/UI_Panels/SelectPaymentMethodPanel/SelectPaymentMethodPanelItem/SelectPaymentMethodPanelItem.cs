using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{
    public class SelectPaymentMethodPanelItem : View
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members

        [Header("UI")]
        public Image                        Icon;
        public TMP_Text                     Label;

        [Header("Bonus")]
        public GameObject                   BonusContainer;
        public BonusItemView                bonusItemView;

        [Header("Dropdown")]
        public GameObject                   DropdownArrow;
        public TMP_Dropdown                 DropdownButton;

        private List<UserPaymentMethod>     upms;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties
        
        public PaymentMethodDefinition      PaymentMethodDefinition      { get; set; }
        public UserPaymentMethod            UserPaymentMethod            { get; set; }
        public SelectPaymentMethodPanelView SelectPaymentMethodPanelView { get; set; }

        public override bool AutoRefresh => false;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        public void Initialize(PaymentMethodDefinition      paymentMethodDefinition, 
                               SelectPaymentMethodPanelView SelectPaymentMethodPanelView)
        {
            this.PaymentMethodDefinition      = paymentMethodDefinition;
            this.SelectPaymentMethodPanelView = SelectPaymentMethodPanelView;
            
            var bonusData = (PaymentMethodDefinition != null) ? PaymentMethodDefinition?.BonusItem 
                          : (UserPaymentMethod       != null) ? UserPaymentMethod?.GetPaymentMethodDefinition()?.BonusItem 
                          : null;
            
            InitializeBonus(bonusData);
            
            
            Refresh();
        }
        
        public void Initialize(UserPaymentMethod            userPaymentMethod, 
                               SelectPaymentMethodPanelView SelectPaymentMethodPanelView)
        {
            if (userPaymentMethod.Type == "native")
                Debug.Log("Native Payment Method Added");
            
            this.UserPaymentMethod            = userPaymentMethod;
            this.SelectPaymentMethodPanelView = SelectPaymentMethodPanelView;
            
            if (this.UserPaymentMethod != null
            &&  this.UserPaymentMethod.Type == "native")
                this.bonusItemView?.gameObject.SetActive(false);
            
            if (userPaymentMethod.GetPaymentMethodDefinition()?.BonusItem != null)
                InitializeBonus(userPaymentMethod.GetPaymentMethodDefinition().BonusItem);
            
            Refresh();
        }
        
        private void InitializeBonus(BonusItem bonusData)
        {
            if (bonusData == null)
                this.bonusItemView?.gameObject.SetActive(false);
            
            var customPrefab = CHECKOUT.Resources.CheckoutAssets.BonusItemPrefab;
            
            if (customPrefab == null)
                return;
            
            // turn placeholder off
            var placeHolderPrefab = this.bonusItemView?.gameObject;
            if (placeHolderPrefab != null) placeHolderPrefab.SetActive(false);
            
            // instantiate custom prefab
            var bonusGO          = Instantiate(original : customPrefab, parent: BonusContainer.transform);
            this.bonusItemView   = bonusGO.GetComponent<BonusItemView>();
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Refresh
        
        public override void RefreshState()
        {   
            if (PaymentMethodDefinition != null)
                this.Label.text = PaymentMethodDefinition.DisplayName;
            else if (UserPaymentMethod != null)
                this.Label.text = UserPaymentMethod.DisplayName;

            //////////////////////////////////////////////// Default
            if (this.PaymentMethodDefinition      == null
            &&  this.UserPaymentMethod            == null)
            {
                this.Label.text  = "Add Credit or Debit Card";
                this.Icon.sprite = CHECKOUT.Sprites.AddCreditCardIconSprite;
            }
            //////////////////////////////////////////////// Payment Method Definitions
            else if (this.PaymentMethodDefinition != null)
            {
                this.Icon.sprite = this.PaymentMethodDefinition.GetIconSprite();

                if (this.PaymentMethodDefinition.Type == PaymentMethodDefinition.PAYMENT_METHOD_TYPE_CREDIT_CARD)
                    this.Label.text  = "Add Credit or Debit Card";
                
                // Dropdown
                DropdownButton.gameObject.SetActive(this.PaymentMethodDefinition.ShouldShowDropdown);
                DropdownArrow .gameObject.SetActive(this.PaymentMethodDefinition.ShouldShowDropdown);
            }
            //////////////////////////////////////////////// UserPaymentMethods
            else if (this.UserPaymentMethod != null)
            {
                this.Label.text  = this.UserPaymentMethod.DisplayName;
                this.Icon.sprite = this.UserPaymentMethod.GetIconSprite();
            }
            
            // Bonus
            
            bonusItemView? .gameObject.SetActive(!CHECKOUT.Globals.IsPreselectionEnabled);
            BonusContainer?.gameObject.SetActive(!CHECKOUT.Globals.IsPreselectionEnabled);
            
            if (this.bonusItemView != null)
            {    
                bonusItemView.Close();
             
                if (this.UserPaymentMethod != null && this.UserPaymentMethod.Type == "native")
                    bonusItemView.gameObject.SetActive(false);
            }
            
            if (!CHECKOUT.Globals.IsBonusEnabled)
                bonusItemView.gameObject.SetActive(false);
            
            // Dropdown
            bool shouldShowDropdown =  this.UserPaymentMethod != null
                                    && this.UserPaymentMethod.Type == "credit_card"
                                    && CHECKOUT.PaymentMethods.UserPaymentMethods.Count(x => x.Type == "credit_card") > 1;

            if (shouldShowDropdown)
            {                
                this.DropdownButton.gameObject.SetActive(true);
                this.DropdownArrow .gameObject.SetActive(true);
                PopulateDropdownItems();
            }
            else
            {
                this.DropdownButton.gameObject.SetActive(false);
                this.DropdownArrow .gameObject.SetActive(false);
            }
            
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Dropdown Events 
        
        public void On_DropdownValueChanged(int newValue)
        {
            var paymentMethodId = upms[newValue].ID;
            var paymentMethod   = CHECKOUT.PaymentMethods.UserPaymentMethods.FirstOrDefault(x => x.ID == paymentMethodId);

            if (paymentMethod == null)
                throw new System.Exception("No Payment Methods Found");

            UpdateItemPaymentMethod(paymentMethod);
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Dropdown Methods
        
        public async void PopulateDropdownItems()
        {
            // Clear
            DropdownButton.ClearOptions();


            // Definitions
            var myType      = this.UserPaymentMethod.Type;
            var otherUpms   = CHECKOUT.PaymentMethods.UserPaymentMethods.Where(x => x.Type == myType).Except(new []{this.UserPaymentMethod}).ToList();
            upms            = (new List<UserPaymentMethod>() { this.UserPaymentMethod }).Concat(otherUpms).ToList( );

            // Add options (using DisplayName for visual display)
            foreach (var pm in upms)
                DropdownButton.options.Add(new TMP_Dropdown.OptionData(pm.DisplayName, pm.GetIconSprite()));

            // for (int i = 0; i < dropdownItems.Count(); i++)
            // {
            //     var pm = pms[i];
            //     var id = pm.DisplayName;
            //     dropdownItems[i].Setup(id);
            // }
        }
        
        public void UpdateItemPaymentMethod(UserPaymentMethod upm)
        {
            this.PaymentMethodDefinition = null;
            this.UserPaymentMethod       = upm;
            this.Refresh();
        }

        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Events
        
        public void On_Click()
        {
            if (this.PaymentMethodDefinition == null
            &&  this.UserPaymentMethod       == null)
            {
                this.SelectPaymentMethodPanelView.On_NewCardClicked();
            }
            else
            {
                this.SelectPaymentMethodPanelView.On_Select(this);
            }
        }
    }
}
