using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GalleonGradientLoopLoaderUI
{
    public class GradientLoop : MonoBehaviour
    {

        [SerializeField] private RectTransform imageRectTransform; // Reference to the Image's RectTransform
        [SerializeField] private float speed = 100f; // Speed of the movement (pixels per second)
        [SerializeField] private float loopWidth; // Width at which the image loops (image width)

        private float startX;
        private float imageWidth;

        void Start()
        {
            if (imageRectTransform == null)
            {
                imageRectTransform = GetComponent<RectTransform>();
            }

            imageWidth = imageRectTransform.rect.width;

            loopWidth = 2 * imageWidth;

            startX = imageRectTransform.anchoredPosition.x;
        }

        void Update()
        {
            // Move the image to the right
            Vector2 position = imageRectTransform.anchoredPosition;
            position.x += speed * Time.deltaTime;

            // Check if the image has moved beyond the loop width
            if (position.x >= startX + loopWidth)
            {
                // Reset position to create a seamless loop
                position.x -= loopWidth;
            }

            imageRectTransform.anchoredPosition = position;
        }
    }
}