using UnityEngine;
using UnityEngine.UI;

namespace GalleonGradientPingPongLoaderUI
{
    public class SpriteGradientLoop : MonoBehaviour
    {
        public Material GradientMaterial;
        Color LeftColor = new Color(0.92f, 0.92f, 0.92f, 1f); //new Color(0.7f, 0.7f, 0.7f, 1f);
        Color RightColor = new Color(1, 1, 1, 1);

        public float LerpSpeed = 2f;
        private float LerpTime;

        void Start()
        {
            GradientMaterial.SetColor("_Color", LeftColor);
            GradientMaterial.SetColor("_Color2", RightColor);
        }

        private void OnDisable()
        {
            LerpTime = 0f;
        }

        void Update()
        {
            // Lerp time goes from 0 to 1 and back
            LerpTime += Time.deltaTime * LerpSpeed;
            float t = (Mathf.Sin(LerpTime) + 1.0f) / 2.0f; // Creates a ping-pong effect

            Color currentLeft = Color.Lerp(LeftColor, RightColor, t);
            Color currentRight = Color.Lerp(RightColor, LeftColor, t);

            GradientMaterial.SetColor("_Color", currentLeft);
            GradientMaterial.SetColor("_Color2", currentRight);
        }
    }
}