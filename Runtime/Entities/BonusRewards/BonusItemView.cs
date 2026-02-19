using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Galleon.Checkout
{
    public interface IBonusItemView
    {
        public void Initialize(string mainText, string rewardText);
        public void Open();
        public void Close();
    }
    
    public class BonusItemView : MonoBehaviour, IBonusItemView
    {
        //////////////////////////////////////////////////////////////////////// Members
        
        public GameObject closedImage;
        public GameObject openImage;
        
        public bool       IsOpen;
       
        public string     MainText;
        public string     RewardText;
        
        public TMP_Text   MainTextLabel;
        public TMP_Text   rewardTextLabel;
        
        //////////////////////////////////////////////////////////////////////// Lifecycle

        public void Initialize(string mainText, string rewardText)
        {
            this.MainText   = mainText;
            this.RewardText = rewardText;
            
            if (MainTextLabel != null && !string.IsNullOrEmpty(MainTextLabel.text))
                MainTextLabel.text = MainText;
            if (rewardTextLabel != null && !string.IsNullOrEmpty(rewardTextLabel.text))
                rewardTextLabel.text = RewardText;
        }
        
        //////////////////////////////////////////////////////////////////////// Methods
        
        [ContextMenu("open")]
        public void Open()
        {
            IsOpen = true;
            closedImage.SetActive(false);
            openImage  .SetActive(true);
        }
        
        [ContextMenu("close")]
        public void Close()
        {
            IsOpen = false;
            closedImage.SetActive(true);
            openImage  .SetActive(false);
        }
    }
}