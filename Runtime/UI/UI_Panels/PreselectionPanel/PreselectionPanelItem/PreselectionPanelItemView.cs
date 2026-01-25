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
    public class PreselectionPanelItemView : View
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
        public IBonusItemView  IBonusItemView;
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties

        public UserPaymentMethod     PaymentMethod         { get; set; }
        public PreselectionPanelView PreselectionPanelView { get; set; }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public void Initialize(UserPaymentMethod paymentMethod, PreselectionPanelView PreselectionPanelView)
        {
            // "log"
            this.gameObject.name += $"_{paymentMethod.Type}";
            
            // Definitions
            this.PaymentMethod         = paymentMethod;
            this.PreselectionPanelView = PreselectionPanelView;
            
            // Bonus
            if (this.bonusItemView != null)
                Destroy(this.bonusItemView.gameObject); // destroy placeholder
            if (CheckoutClient.Instance.Resources.CheckoutAssets?.BonusItemPrefab != null)
            {
                this.IBonusItemView = Instantiate(CheckoutClient.Instance.Resources.CheckoutAssets.BonusItemPrefab, BonusContainer.transform).GetComponent<IBonusItemView>();
                var bonusItem = CHECKOUT.Session.BonusData.FirstOrDefault() ?? new BonusItem() { BonusMainText = "Extra", BonusRewardText = "1000k" };
                this.IBonusItemView.Initialize(bonusItem.BonusMainText, bonusItem.BonusRewardText);
            }
            
            // Refresh
            Refresh();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Refresh

        public override void RefreshState()
        {
            if (PreselectionPanelView == null)
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
            if (this.PaymentMethod.IsSelected)
                PreselectionPanelView.SetPurchaseButtonSprite(this.PaymentMethod);
            
            // Set Seperator Color 
            if (this.PaymentMethod.IsSelected)
                SetSeperatorColor(SelectedOptionColor, true);
            else
                SetSeperatorColor(UnselectedOptionColor, false);
            
            // Bonus
            if (this.bonusItemView != null && IBonusItemView != null)
            {    
                if (this.PaymentMethod.IsSelected) IBonusItemView.Open();
                else                               IBonusItemView.Close();
                
                if (this.PaymentMethod.Type == "native")
                    (IBonusItemView as MonoBehaviour)?.gameObject.SetActive(false);
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
            this.PaymentMethod?.Select();
            this.PreselectionPanelView.OnRadiobuttonSelected(this);
            
            CHECKOUT.Session.PreselectedPaymentMethod = this.PaymentMethod;
            
            //Refresh();
            this.PreselectionPanelView.Refresh();
        }

        public void Unselect()
        {
            this.PaymentMethod?.Unselect();
            //Refresh();
            
            this.PreselectionPanelView.Refresh();
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

    }
}
