using UnityEngine;

/// <summary>
/// Raises UI elements when the mobile keyboard appears to prevent them from being hidden.
/// Attach this component to any UI element that needs to be raised when the keyboard is active.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UiKeyboardRaiser : MonoBehaviour
{
    [Tooltip("Multiplier for how much of the keyboard height to use for raising the UI (0.5 = half)")]
    [Range(0, 1)][HideInInspector]
    public float raiseAmountMultiplier = 0.5f;
    
    private RectTransform rectTransform;
    private Vector2       originalPosition;
    private bool          wasKeyboardVisible;
    
    private void Awake()
    {
        rectTransform    = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
    }
    
    private void OnEnable()
    {
        originalPosition = rectTransform.anchoredPosition;
    }
    
    private void OnDisable()
    {
        // Reset position when disabled
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }
    
    private void Update()
    {
        #if UNITY_ANDROID || UNITY_IOS
        bool isKeyboardVisible = TouchScreenKeyboard.visible;
        
        // Only process when keyboard visibility changes
        if (isKeyboardVisible != wasKeyboardVisible)
        {
            wasKeyboardVisible = isKeyboardVisible;
            
            if (isKeyboardVisible)
            {
                // Keyboard appeared - raise the UI
                float keyboardHeight = TouchScreenKeyboard.area.height;
                RaiseUI(keyboardHeight);
            }
            else
            {
                // Keyboard disappeared - reset position
                ResetPosition();
            }
        }
        #endif
    }
    
    /// <summary>
    /// Raises the UI element based on keyboard height
    /// </summary>
    /// <param name="keyboardHeight">Height of the keyboard in pixels</param>
    private void RaiseUI(float keyboardHeight)
    {
        if (keyboardHeight <= 0) 
            return;
        
        // Get canvas scale factor to convert screen pixels to UI coordinates
        Canvas canvas                   = GetComponentInParent<Canvas>();
        float  scaleFactor              = canvas != null ? canvas.scaleFactor : 1f;
        
        // Calculate how much to move the UI
        float adjustAmount              = keyboardHeight / scaleFactor * raiseAmountMultiplier;
        
        // Move UI up
        Vector2 newPosition             = originalPosition;
        newPosition.y                  += adjustAmount;
        rectTransform.anchoredPosition  =  newPosition;
    }
    
    /// <summary>
    /// Resets UI position to original
    /// </summary>
    private void ResetPosition()
    {
        rectTransform.anchoredPosition = originalPosition;
    }
}
