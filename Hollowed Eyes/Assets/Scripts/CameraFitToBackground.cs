using UnityEngine;

public class CameraFitToBackground : MonoBehaviour
{
    [SerializeField] private SpriteRenderer background;
    
    private Camera cam;
    private float targetAspect;
    private bool debugLogged = false;
    private Material blackMaterial;
    private Texture2D blackTexture;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        
        // Create black texture for drawing bars
        blackTexture = new Texture2D(1, 1);
        blackTexture.SetPixel(0, 0, Color.black);
        blackTexture.Apply();
        
        // Force solid black background
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        
        // Ensure camera culling mask is correct
        cam.cullingMask = -1; // Render everything
        
        // Debug.Log($"Camera Awake - BG: {background?.name}, ClearFlags: {cam.clearFlags}, BGColor: {cam.backgroundColor}");
    }
    
    // Use OnGUI to draw black bars - this is the most reliable method
    void OnGUI()
    {
        if (blackTexture == null) return;
        
        Rect camRect = cam.rect;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        
        GUI.depth = -1000; // Draw on top of everything
        
        // If pillarbox (vertical bars on sides)
        if (camRect.width < 1.0f)
        {
            float barWidth = camRect.x * screenWidth;
            
            // Left bar
            if (barWidth > 0)
            {
                GUI.DrawTexture(new Rect(0, 0, barWidth, screenHeight), blackTexture);
            }
            
            // Right bar
            float rightBarStart = (camRect.x + camRect.width) * screenWidth;
            float rightBarWidth = screenWidth - rightBarStart;
            if (rightBarWidth > 0)
            {
                GUI.DrawTexture(new Rect(rightBarStart, 0, rightBarWidth, screenHeight), blackTexture);
            }
            
            if (!debugLogged)
            {
                // Debug.Log($"OnGUI Pillarbox - LeftBar: {barWidth}, RightBarStart: {rightBarStart}, RightBarWidth: {rightBarWidth}");
            }
        }
        // If letterbox (horizontal bars on top/bottom)
        else if (camRect.height < 1.0f)
        {
            float barHeight = camRect.y * screenHeight;
            
            // Bottom bar (GUI Y is from top)
            float bottomBarY = (camRect.y + camRect.height) * screenHeight;
            float bottomBarHeight = screenHeight - bottomBarY;
            if (bottomBarHeight > 0)
            {
                GUI.DrawTexture(new Rect(0, 0, screenWidth, screenHeight - bottomBarY), blackTexture);
            }
            
            // Top bar
            if (barHeight > 0)
            {
                GUI.DrawTexture(new Rect(0, screenHeight - barHeight, screenWidth, barHeight), blackTexture);
            }
            
            if (!debugLogged)
            {
                // Debug.Log($"OnGUI Letterbox - TopBarHeight: {barHeight}, BottomBarY: {bottomBarY}");
            }
        }
        
        debugLogged = true;
    }

    void Start()
    {
        // Recalculate everything on scene load to clear previous scene data
        if (background != null)
        {
            float bgWidth = background.bounds.size.x;
            float bgHeight = background.bounds.size.y;
            targetAspect = bgWidth / bgHeight;
        }
        
        FitCamera();
    }
    
    void OnEnable()
    {
        // Recalculate when enabled to handle scene changes
        if (background != null)
        {
            float bgWidth = background.bounds.size.x;
            float bgHeight = background.bounds.size.y;
            targetAspect = bgWidth / bgHeight;
        }
    }
    
    void LateUpdate()
    {
        FitCamera();
    }
    
    void FitCamera()
    {
        if (background == null)
        {
            // Debug.LogWarning("Background is null in FitCamera!");
            return;
        }
        
        float bgHeight = background.bounds.size.y;
        cam.orthographicSize = bgHeight / 2f;
        
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;
        
        if (scaleHeight < 1.0f)
        {
            Rect rect = cam.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            cam.rect = rect;
            // Debug.Log($"Letterbox mode - Rect: {rect}, WindowAspect: {windowAspect}, TargetAspect: {targetAspect}");
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            cam.rect = rect;
            // Debug.Log($"Pillarbox mode - Rect: {rect}, WindowAspect: {windowAspect}, TargetAspect: {targetAspect}");
        }
        
        transform.position = new Vector3(
            background.transform.position.x,
            background.transform.position.y,
            transform.position.z
        );
    }
}