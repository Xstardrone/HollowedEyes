using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class CameraFitToBackground : MonoBehaviour
{
    [System.Serializable]
    private class ResolutionProfile
    {
        [Tooltip("Apply this profile when Screen.width >= minWidth and Screen.height >= minHeight")]
        public int minWidth = 1920;
        public int minHeight = 1080;

        [Tooltip("UI reference resolution for CanvasScaler components")]
        public Vector2 uiReferenceResolution = new Vector2(1920, 1080);

        [Tooltip("Extra camera zoom per profile. 1.0 keeps default behavior")]
        [Range(0.7f, 1.3f)] public float cameraZoomMultiplier = 1f;
    }

    [SerializeField] private SpriteRenderer background;
    [SerializeField] private bool keepUpdatedAtRuntime = true;

    [Header("Fullscreen First")]
    [SerializeField] private bool forceFullscreenOnSupportedPlatforms = true;

    [Tooltip("Try to fill the entire screen first, then fall back to contain when crop is too high")]
    [SerializeField] private bool preferFullscreenFill = true;

    [Tooltip("Maximum crop fraction allowed on either axis before fallback to contain mode")]
    [SerializeField, Range(0f, 0.45f)] private float maxAllowedCropPerAxis = 0.2f;

    [Header("UI Scaling")]
    [SerializeField] private bool updateCanvasScalers = true;
    [SerializeField, Range(0f, 1f)] private float canvasMatchWidthOrHeight = 0.5f;

    [SerializeField] private ResolutionProfile[] resolutionProfiles =
    {
        new ResolutionProfile { minWidth = 3840, minHeight = 2160, uiReferenceResolution = new Vector2(3840, 2160), cameraZoomMultiplier = 1f },
        new ResolutionProfile { minWidth = 2560, minHeight = 1440, uiReferenceResolution = new Vector2(2560, 1440), cameraZoomMultiplier = 1f },
        new ResolutionProfile { minWidth = 1920, minHeight = 1080, uiReferenceResolution = new Vector2(1920, 1080), cameraZoomMultiplier = 1f },
        new ResolutionProfile { minWidth = 1366, minHeight = 768, uiReferenceResolution = new Vector2(1366, 768), cameraZoomMultiplier = 1f },
        new ResolutionProfile { minWidth = 1280, minHeight = 720, uiReferenceResolution = new Vector2(1280, 720), cameraZoomMultiplier = 1f }
    };

    private Camera cam;
    private int lastScreenWidth = -1;
    private int lastScreenHeight = -1;

    void Awake()
    {
        cam = GetComponent<Camera>();

        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;

        ApplyFullscreenPreference();
    }

    void Start()
    {
        if (background == null)
        {
            Debug.LogError("CameraFitToBackground: Background not assigned.");
            return;
        }

        FitAndApply();
    }

    void LateUpdate()
    {
        if (!keepUpdatedAtRuntime)
            return;

        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            FitAndApply();
        }
    }

    void ApplyFullscreenPreference()
    {
#if !UNITY_WEBGL
        if (!forceFullscreenOnSupportedPlatforms)
            return;

        if (!Screen.fullScreen)
            Screen.fullScreen = true;

        if (Screen.fullScreenMode != FullScreenMode.FullScreenWindow)
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
#endif
    }

    void FitAndApply()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        if (background == null)
            return;

        ResolutionProfile profile = GetBestProfile(Screen.width, Screen.height);

        Bounds bgBounds = background.bounds;
        float bgWidth = Mathf.Max(0.001f, bgBounds.size.x);
        float bgHeight = Mathf.Max(0.001f, bgBounds.size.y);
        float screenAspect = (float)Screen.width / Mathf.Max(1, Screen.height);

        float containSize = Mathf.Max(bgHeight * 0.5f, bgWidth / (2f * screenAspect));
        float fillSize = Mathf.Min(bgHeight * 0.5f, bgWidth / (2f * screenAspect));

        float visibleWidthAtFill = 2f * fillSize * screenAspect;
        float visibleHeightAtFill = 2f * fillSize;

        float cropX = Mathf.Clamp01((bgWidth - visibleWidthAtFill) / bgWidth);
        float cropY = Mathf.Clamp01((bgHeight - visibleHeightAtFill) / bgHeight);

        bool fillAcceptable = cropX <= maxAllowedCropPerAxis && cropY <= maxAllowedCropPerAxis;
        float targetSize = (preferFullscreenFill && fillAcceptable) ? fillSize : containSize;

        if (profile != null)
            targetSize *= profile.cameraZoomMultiplier;

        cam.rect = new Rect(0f, 0f, 1f, 1f);
        cam.orthographicSize = Mathf.Max(0.01f, targetSize);

        UpdateCanvasScaler(profile);

        transform.position = new Vector3(
            bgBounds.center.x,
            bgBounds.center.y,
            transform.position.z
        );
    }

    ResolutionProfile GetBestProfile(int width, int height)
    {
        ResolutionProfile best = null;
        int bestArea = -1;

        for (int i = 0; i < resolutionProfiles.Length; i++)
        {
            ResolutionProfile profile = resolutionProfiles[i];
            if (profile == null)
                continue;

            if (width >= profile.minWidth && height >= profile.minHeight)
            {
                int area = profile.minWidth * profile.minHeight;
                if (area > bestArea)
                {
                    best = profile;
                    bestArea = area;
                }
            }
        }

        if (best != null)
            return best;

        if (resolutionProfiles.Length > 0)
            return resolutionProfiles[resolutionProfiles.Length - 1];

        return null;
    }

    void UpdateCanvasScaler(ResolutionProfile profile)
    {
        if (!updateCanvasScalers || profile == null)
            return;

        CanvasScaler[] scalers = FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < scalers.Length; i++)
        {
            CanvasScaler scaler = scalers[i];
            if (scaler == null)
                continue;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = profile.uiReferenceResolution;
            scaler.matchWidthOrHeight = canvasMatchWidthOrHeight;
        }
    }
}
