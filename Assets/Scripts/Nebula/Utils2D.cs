using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Nebula
{
    public static class Utils2D
    {
        // This method will return a Vector2 increased/decreased by a factor specified to the
        // vector component specified.
        public static Vector2 SmoothVectorTo(Vector2 toChange, float factor, string component = "xy")
        {
            if (component == "x")
            {
                return new Vector2(toChange.x * factor, toChange.y);
            }
            else if (component == "y")
            {
                return new Vector2(toChange.x, toChange.y * factor);
            }
            else
            {
                return new Vector2(toChange.x * factor, toChange.y * factor);
            }
        }

        public static void DisplayInfo(Transform transform, string message)
        {
            // Create a canvas to put text onto at the transform passed in.
            GameObject canvasObject = new GameObject("Custom Canvas: " + message);
            canvasObject.transform.SetParent(transform);

            Canvas textCanvas = canvasObject.AddComponent<Canvas>();
            textCanvas.GetComponent<RectTransform>().localPosition = Vector3.zero;
            textCanvas.renderMode = RenderMode.WorldSpace;
            textCanvas.worldCamera = Camera.main;

            // Create text and place it in the canvas.
            GameObject textObject = new GameObject("Custom Text: " + message);
            textObject.transform.SetParent(canvasObject.transform);

            TextMeshPro text = textObject.AddComponent<TextMeshPro>();
            text.GetComponent<RectTransform>().localPosition = Vector3.zero;
            text.autoSizeTextContainer = true;
            text.text = message;
            text.fontSize = 4;
            text.sortingOrder = 1;

            // Destroy it after 1 physics call.
            GameObject.Destroy(canvasObject, Time.fixedDeltaTime);
        }
    }
}