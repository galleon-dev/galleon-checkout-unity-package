using System.Threading.Tasks;
using UnityEngine;

namespace Galleon.Checkout
{
    public class BonusItemView : MonoBehaviour
    {
        public GameObject closedImage;
        public GameObject openImage;
        
        public bool IsOpen;
        
        [ContextMenu("open")]
        public async Task Open()
        {
            IsOpen = true;
            closedImage.SetActive(false);
            openImage  .SetActive(true);
        }
        
        [ContextMenu("close")]
        public async Task Close()
        {
            IsOpen = false;
            closedImage.SetActive(true);
            openImage  .SetActive(false);
        }
    }
}