using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Galleon.Checkout
{
    public class RuntimeExplorer : MonoBehaviour
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members
        
        // UI
        public  UIDocument  doc;
        private EventSystem primaryEventSystem;

        // Shake detection
        private GameObject  shakeDetector;
        public  bool        enableShakeDetection = false;
        public  float       shakeThreshold = 3.0f;
        public  float       shakeCooldown = 0.5f;
        private float       lastShakeTime = -999f;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeShakeDetection()
        {
            var explorer = FindObjectOfType<RuntimeExplorer>(includeInactive:true);
            if (explorer != null)
            {
                explorer.shakeDetector = new GameObject("ShakeDetector");
                explorer.shakeDetector.transform.SetParent(explorer.transform.parent);
                var detector = explorer.shakeDetector.AddComponent<ShakeDetector>();
                DontDestroyOnLoad(detector.gameObject);
                detector.Initialize(explorer, explorer.shakeThreshold, explorer.shakeCooldown);
            }
        }

        private void OnEnable()
        {
            try
            {
                // Stop clicks from passing through
                this.primaryEventSystem    = EventSystem.current;
                primaryEventSystem.enabled = false;

                doc.enabled = true;
                
                // Initialize UI
                var root = doc.rootVisualElement;
                root.style.backgroundColor = Color.gray;
                root.Clear();

                // Scroll view
                var scrollView            = new ScrollView(); root.Add(scrollView);
                scrollView.mode           = ScrollViewMode.VerticalAndHorizontal;
                scrollView.style.flexGrow = 1;
                
                try
                {
                    scrollView.Add(new Label("Runtime Explorer"));   
                    scrollView.Add(new Divider());   
                    
                    var explorer = new Explorer();
                    explorer.Mode = Explorer.DisplayMode.Mobile;
                    explorer.RefreshMode();
                    scrollView.Add(explorer);   
                }
                catch (Exception ex) { Debug.LogException(ex); }
                
            
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void OnDisable()
        {
            try
            {
                primaryEventSystem.enabled = true;
                this.doc.rootVisualElement?.Clear();

                doc.enabled = false;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void OnDestroy()
        {
            if (shakeDetector != null)
            {
                Destroy(shakeDetector);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Shake Detection
        
        private class ShakeDetector : MonoBehaviour
        {
            private       RuntimeExplorer target;
            private       float           threshold;
            private       float           cooldown;
            private       float           lastShakeTime           = -999f;
            private       float           logTimer                = 0f;
            private const float           LOG_INTERVAL            = 1f; // Log every second
            private       int             shakeCount              = 0;
            private const int             REQUIRED_SHAKES         = 3;
            private const float           SHAKE_SEQUENCE_TIMEOUT  = 2f; // Reset count if no shake within 2 seconds
            private       float           lastShakeInSequenceTime = -999f;

            public void Initialize(RuntimeExplorer target, float threshold, float cooldown)
            {
                this.target    = target;
                this.threshold = threshold;
                this.cooldown  = cooldown;
                Debug.Log($"[ShakeDetector] Initialized - Threshold: {threshold}, Cooldown: {cooldown}, Required shakes: {REQUIRED_SHAKES}");
            }

            private void Update()
            {
                if (target == null)
                    return;

                if (!target.enableShakeDetection)
                    return;
                
                // Calculate acceleration magnitude
                Vector3 acceleration          = Input.acceleration;
                float   accelerationMagnitude = acceleration.magnitude;

                // Periodic logging of current acceleration
                logTimer += Time.deltaTime;
                if (logTimer >= LOG_INTERVAL)
                {
                    logTimer = 0f;
                }

                // Reset shake count if sequence timeout exceeded
                if (shakeCount > 0 && Time.time - lastShakeInSequenceTime > SHAKE_SEQUENCE_TIMEOUT)
                {
                    shakeCount = 0;
                }

                // Check if enough time has passed since last shake
                if (Time.time - lastShakeTime < cooldown)
                {
                    return;
                }

                // Detect shake
                if (accelerationMagnitude > threshold)
                {
                    lastShakeTime           = Time.time;
                    lastShakeInSequenceTime = Time.time;
                    shakeCount++;

                    if (shakeCount >= REQUIRED_SHAKES)
                    {
                        bool newState = !target.gameObject.activeSelf;
                        target.gameObject.SetActive(newState);
                        shakeCount = 0; // Reset for next sequence
                    }
                }
            }
        }
    }
}