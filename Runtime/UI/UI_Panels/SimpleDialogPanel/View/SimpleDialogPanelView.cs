using Galleon.Checkout.Foundation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galleon.Checkout.UI
{
    public class SimpleDialogPanelView : View
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        public TMP_Text MainText;
        public TMP_Text ConfirmButtonText;
        public TMP_Text DeclineButtonText;
        
        //// Members
        
        [Header("Remove Payment Method - UI")]
        public GameObject RemovePaymentMethodGameObject;
        public Image      RemovePaymentMethodIcon;
        public TMP_Text   RemovePaymentMethodLabel;
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Result
        
        public      DialogResult Result = DialogResult.None;
        public enum DialogResult
        {
            None,
            Confirm,
            Decline,
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Refresh

        public override void RefreshState()
        {
            var paymentMethod                  = CheckoutClient.Instance.CurrentSession.userPaymentMethodToDelete;
            this.RemovePaymentMethodLabel.text = paymentMethod.DisplayName;
            
            this.RemovePaymentMethodIcon.sprite = paymentMethod.GetIconSprite();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// UI Events
        
        public void On_ConfirmClicked()
        {
            Debug.Log("On Confirm Clicked");
            
            Debug.Log($"Removing Payment Method : {CheckoutClient.Instance.CurrentSession.userPaymentMethodToDelete.DisplayName}");
            CheckoutClient.Instance.CurrentSession.User.RemovePaymentMethod(CheckoutClient.Instance.CurrentSession.userPaymentMethodToDelete);
            
            this.Result = DialogResult.Confirm;
            CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(this.Result.ToString());
        }
        
        public void On_DeclineClicked()
        {
            Debug.Log("On Decline Clicked");
            this.Result = DialogResult.Decline;
            CheckoutClient.Instance.CheckoutScreenMobile.OnPageFinishedWithResult(this.Result.ToString());
        }
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Test Scenarios
        
        public TestScenario scenario_2_part_2 => new TestScenario(expressions : new[]
                                                                                {
                                                                                    $"{nameof(test_confirm)}()", 
                                                                                    $"{nameof(test_part3)}()"     
                                                                                });
        
        public Step test_confirm() => new Step(action : async (s) => { On_ConfirmClicked(); });
        public Step test_part3()   => new Step(action : async (s) => { EntityNode.CurrentTestScenario = "scenario_2_part_3"; });
    }
}
