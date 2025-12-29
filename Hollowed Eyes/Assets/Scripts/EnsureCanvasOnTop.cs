using UnityEngine;

// Add this to your UI Canvas to ensure it renders on top of black bars
public class EnsureCanvasOnTop : MonoBehaviour
{
    void Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            // Force overlay mode
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // Very high to render on top
            Debug.Log($"Canvas {gameObject.name} set to Overlay with sort order 1000");
        }
    }
}
