using UnityEngine;

public class CameraFitToBackground : MonoBehaviour
{
    [SerializeField] private SpriteRenderer background;
    
    private Camera cam;
    private float targetAspect;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        
        // Calculate target aspect ratio from background
        float bgWidth = background.bounds.size.x;
        float bgHeight = background.bounds.size.y;
        targetAspect = bgWidth / bgHeight;
        
        FitCamera();
    }
    
    void LateUpdate()
    {
        FitCamera();
    }
    
    void FitCamera()
    {
        if (background == null) return;
        
        float bgHeight = background.bounds.size.y;
        cam.orthographicSize = bgHeight / 2f;
        
        // Get current window aspect
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;
        
        // Adjust camera rect to maintain aspect ratio
        if (scaleHeight < 1.0f)
        {
            // Letterbox (black bars top/bottom)
            Rect rect = cam.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            cam.rect = rect;
        }
        else
        {
            // Pillarbox (black bars left/right)
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            cam.rect = rect;
        }
        
        // Center camera on background
        transform.position = new Vector3(
            background.transform.position.x,
            background.transform.position.y,
            transform.position.z
        );
    }
}