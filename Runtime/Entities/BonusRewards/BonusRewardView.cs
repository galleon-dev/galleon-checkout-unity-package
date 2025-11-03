using System.Threading.Tasks;
using UnityEngine;

namespace Galleon.Checkout
{
    public class BonusRewardView : MonoBehaviour
    {
        public GameObject closedImage;
        public GameObject openImage;
        
        public bool IsOpen;
        
        public async Task Open()
        {
            IsOpen = true;
            closedImage.SetActive(false);
            openImage.SetActive(true);
        }
        
        public async Task Close()
        {
            IsOpen = false;
            closedImage.SetActive(true);
            openImage.SetActive(false);
        }
    }
}