using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Allows clicking on UI elements that are rendered to a RenderTexture and displayed via a RawImage.
    /// Replaces the standard GraphicRaycaster on Canvases set to Screen Space - Camera (Targeting a Texture).
    /// </summary>
    public class RenderTextureRaycaster : GraphicRaycaster
    {
        [Header("Render Texture Settings")]
        [Tooltip("The RawImage on the main screen (Overlay) that displays the Render Texture.")]
        public RawImage screenRawImage;

        [Tooltip("The Camera that renders this Canvas into the Render Texture.")]
        public Camera renderTextureCamera;

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (screenRawImage == null || renderTextureCamera == null)
            {
                // Fallback to default behavior if not set up
                // This might fail if the camera is not the screen camera, but it's safe to try.
                base.Raycast(eventData, resultAppendList);
                return;
            }

            // 1. Convert Screen Click (Mouse Position) to Local Point within the RawImage
            // This determines where on the "virtual screen" (the texture) the user clicked.
            Vector2 localPoint;
            // Note: For Screen Space - Overlay RawImages, use null for the camera.
            // If the RawImage itself is in World Space, we'd need that canvas's camera.
            // We'll assume Overlay for now as it's the standard for CRT effects.
            Camera worldCamera = null;
            if (screenRawImage.canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                worldCamera = screenRawImage.canvas.worldCamera;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                screenRawImage.rectTransform, 
                eventData.position, 
                worldCamera, 
                out localPoint))
            {
                // Click was outside the displayed Texture area
                return;
            }

            // 2. Convert Local Point to UV Coordinates (0 to 1)
            Rect rect = screenRawImage.rectTransform.rect;
            // Normalize the point. localPoint is relative to the pivot.
            float u = (localPoint.x - rect.x) / rect.width;
            float v = (localPoint.y - rect.y) / rect.height;

            // Check bounds (ignore clicks outside the image)
            if (u < 0 || u > 1 || v < 0 || v > 1)
                return;

            // 3. Map UV to the Resolution of the Render Texture Camera
            // This gives us the "Screen Position" as seen by the internal camera.
            Vector2 internalScreenPos = new Vector2(
                u * renderTextureCamera.pixelWidth,
                v * renderTextureCamera.pixelHeight
            );

            // 4. Temporarily swap the event position for the Raycast
            // We trick the base implementation into thinking the mouse is at the mapped position.
            Vector2 originalPos = eventData.position;
            eventData.position = internalScreenPos;

            // 5. Perform the Raycast
            // GraphicRaycaster uses eventCamera (which should be renderTextureCamera) to raycast against UI elements.
            base.Raycast(eventData, resultAppendList);

            // 6. Restore original position to avoid confusing other systems
            eventData.position = originalPos;
        }
    }
}
