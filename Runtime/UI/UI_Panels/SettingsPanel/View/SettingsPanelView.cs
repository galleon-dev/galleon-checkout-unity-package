using System.Threading.Tasks;
using Galleon.Checkout;
using UnityEngine;

public class SettingsPanelView : MonoBehaviour
{
    public      ViewResult Result = ViewResult.None;
    public enum ViewResult
    {
        None,
        Back,
        Close,
    }
    
    //////////////////////////////////////////////////////////////////////////// View Flow

    public bool IsCompleted = false;

    public Step View()
    =>
        new Step(name   : $"view_settings_panel"
                ,action : async (s) =>
                {
                    IsCompleted = false;
                    
                    this.gameObject.SetActive(true);
                    
                    while (!IsCompleted)
                        await Task.Yield();
                    
                    this.gameObject.SetActive(false);
                });
    
    //////////////////////////////////////////////////////////////////////////// UI Events
    
    public void OnCloseClicked()
    {
        this.IsCompleted = true;
        this.Result      = ViewResult.Close;
    }
}
