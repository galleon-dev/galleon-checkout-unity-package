using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galleon.Checkout.Foundation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{   
    public class checkoutPanelPaymentMethodItemView : View
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members

        [Header("UI")]
        public Image           Icon;
        public TMP_Text        Label;
        public Image           CheckedImage;
        public Image           UncheckedImage;
        public List<Image>     Separators;
        public List<Image>     ExtraSeparators;
        public Color           SelectedOptionColor   = new Color(0.2862745f, 0.7411765f, 0.9529412f, 1);
        public Color           UnselectedOptionColor = new Color(0.8679245f, 0.8679245f, 0.8679245f, 1);

        [Header("Bonus")]
        public GameObject      BonusContainer;
        public BonusItemView   bonusItemView;
        
        
        private IBonusItemView Ibonus;
        IBonusItemView GetBonusItemView() => Ibonus ?? bonusItemView;
        MonoBehaviour GetBonusItemMonoBehaviour() => (Ibonus ?? bonusItemView) as MonoBehaviour;
        
        
        [Header("Dropdown")]
        public GameObject      DropdownArrow;
        public TMP_Dropdown    DropdownButton;
        private List<string>   _dropdownPaymentMethodIds = new List<string>();

        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties

        public UserPaymentMethod PaymentMethod     { get; set; }
        public CheckoutPanelView CheckoutPanelView { get; set; }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public void Initialize(UserPaymentMethod paymentMethod, CheckoutPanelView CheckoutPanelView)
        {
            // "log"
            this.gameObject.name += $"_{paymentMethod.Type}";
            
            // Definitions
            this.PaymentMethod     = paymentMethod;
            this.CheckoutPanelView = CheckoutPanelView;
            
            var def = paymentMethod.GetPaymentMethodDefinition();
            InitializeBonus(def?.BonusItem);
            
            // Refresh
            Refresh();
        }
 
        private void InitializeBonus(BonusItem bonusData)
        {
            if (bonusData == null
            ||  CHECKOUT.Globals.IsPreselectionEnabled
            ||  !CHECKOUT.Globals.IsBonusEnabled)
            {
                bonusItemView? .gameObject.SetActive(false);
                BonusContainer?.gameObject.SetActive(false);
                return;
            }
        
            // acquire custom prefab
            var customPrefab = CHECKOUT.Resources.CheckoutAssets.BonusItemPrefab;
            if (customPrefab == null) return;
            
            // turn placeholder off
            var placeHolderPrefab = this.bonusItemView?.gameObject;
            if (placeHolderPrefab != null) placeHolderPrefab.SetActive(false);
            
            // instantiate custom prefab
            var bonusGO          = Instantiate(original : customPrefab, parent: BonusContainer.transform);
            this.bonusItemView   = bonusGO.GetComponent<BonusItemView>();
            this.bonusItemView?.Initialize(bonusData.BonusMainText, bonusData.BonusRewardText);
            
            this.Ibonus = bonusGO.GetComponent<IBonusItemView>();
            Ibonus?.Initialize(bonusData.BonusMainText, bonusData.BonusRewardText);
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Refresh

        public override void RefreshState()
        {
            if (CheckoutPanelView == null)
            {
                this.Icon.sprite = CHECKOUT.Sprites.AddCreditCardIconSprite;
                this.Label.text  = "Add Credit Card";
                return;
            }

            this.Label.text    = PaymentMethod.DisplayName;
            this.CheckedImage  .gameObject.SetActive( this.PaymentMethod.IsSelected);
            this.UncheckedImage.gameObject.SetActive(!this.PaymentMethod.IsSelected);
            
            // Debug.Log("this.PaymentMethod.Type: " + this.PaymentMethod.Type + "  this.PaymentMethod.IsSelected" + this.PaymentMethod.IsSelected);
            
            // Set icon Sprite
            this.Icon.sprite = this.PaymentMethod.GetIconSprite();
            
            // Set button Sprite
            // if (this.PaymentMethod.IsSelected)
            //     CheckoutPanelView.SetPurchaseButtonSprite(this.PaymentMethod.GetButtonSprite());
            
            // Set Seperator Color 
            if (this.PaymentMethod.IsSelected)
                SetSeperatorColor(SelectedOptionColor, true);
            else
                SetSeperatorColor(UnselectedOptionColor, false);
            
            // Bonus
            if (this.GetBonusItemView() != null)
            {    
                if (this.PaymentMethod.IsSelected) GetBonusItemView().Open();
                else                               GetBonusItemView().Close();
                
                if (this.PaymentMethod.Type == "native")
                    GetBonusItemMonoBehaviour()?.gameObject.SetActive(false);
            }
            
            // Dropdown
            bool shouldShowDropdown =  this.PaymentMethod.Type == "credit_card"
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
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Events

        public void On_Click()
        {
            Select();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Actions

        public void Select()
        {
            this.CheckoutPanelView.SelectUserPaymentMethod(this.PaymentMethod);
            
            // this.PaymentMethod?.Select();
            // this.CheckoutPanelView.OnRadiobuttonSelected(this);
            // 
            // this.CheckoutPanelView.SoftRefreshState();
            //Refresh();
        }

        public void Unselect()
        {
            this.PaymentMethod?.Unselect();
            
            // this.CheckoutPanelView.SoftRefreshState();
            //Refresh();
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Dropdown Events 
        
        public void On_DropdownValueChanged(int newValue)
        {
            if (newValue < 0 || newValue >= _dropdownPaymentMethodIds.Count)
                throw new System.Exception("Invalid dropdown index");

            var paymentMethodId = _dropdownPaymentMethodIds[newValue];
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
            _dropdownPaymentMethodIds.Clear();

            // Definitions
            var myType       = this.PaymentMethod.Type;
            var OfSameType   = CHECKOUT.PaymentMethods.UserPaymentMethods.Where(x => x.Type == myType).Except(new [] {this.PaymentMethod} ).ToList();
            var upms         = (new List<UserPaymentMethod>() { this.PaymentMethod }).Concat(OfSameType).OrderByDescending(x => x.LastSuccessfulUseTime).ToList( );

            // Add options (display name visually, but track ID internally)
            foreach (var pm in upms)
            {
                DropdownButton.options.Add(new TMP_Dropdown.OptionData(pm.DisplayName, pm.GetIconSprite()));
                _dropdownPaymentMethodIds.Add(pm.ID);
            }
        }
        
        public void UpdateItemPaymentMethod(UserPaymentMethod upm)
        {
            this.PaymentMethod = upm;
            Select();
            this.Refresh();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// helper Methods
        
        private void SetSeperatorColor(Color _Color, bool _Status)
        {
            for (int i = 0; i < Separators.Count; i++)
            {
                Separators[i].color = _Color;
                Separators[i].gameObject.SetActive(_Status);
            }
        }

        // For Dropdown Menu
        public void SelectDropdownPaymentMethod(UserPaymentMethod paymentMethod, CheckoutPanelView CheckoutPanelView)
        {
            Debug.Log("SetPaymentMethod: " + paymentMethod.Type + "  CheckoutPanelView: " + CheckoutPanelView);
         
            this.PaymentMethod     = paymentMethod;
            this.CheckoutPanelView = CheckoutPanelView;
            this.PaymentMethod?.Select();
        }
    }
}

