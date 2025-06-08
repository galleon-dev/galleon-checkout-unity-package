using System.Collections;
using System.Collections.Generic;
using Galleon.Checkout;
using Galleon.Checkout.Samples;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galleon.SampleApp
{
    public class SampleAppController : MonoBehaviour
    {
        ////////////////////////////////////////////////////////////////////// Members
        
        public StoreView StoreView;
        
        ////////////////////////////////////////////////////////////////////// Lifecycle
        
        void Awake()
        {
            Debug.Log($"SampleAppController.Awake");
        }
        
        async void Start()
        {
            await CheckoutAPI.Initialize();
        }
        
        
        #if UNITY_EDITOR
        [MenuItem("Tools/Galleon/Open Test Scene")]
        #endif
        public static void OpenTestScene()
        {
            string scenePath = "Packages/com.galleon.checkout/Runtime/Samples/SampleApp/_SampleAppScene.unity";
         
            if (!string.IsNullOrEmpty(scenePath))
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
                }
                else
                    UnityEngine.SceneManagement.SceneManager.LoadScene(scenePath);
                #endif

            }
            else
            {
                Debug.LogError("Scene path is not provided or invalid.");
            }
        }
    }
}