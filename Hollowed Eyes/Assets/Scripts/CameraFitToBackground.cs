using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFitToBackground : MonoBehaviour
{
    [SerializeField] private SpriteRenderer background;

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();

        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
    }

    void Start()
    {
        if (background == null)
        {
            Debug.LogError("CameraFitToBackground: Background not assigned.");
            return;
        }

        FitCameraOnce();
    }

    void FitCameraOnce()
    {
        Bounds bgBounds = background.bounds;

        // Fit height exactly
        cam.orthographicSize = bgBounds.size.y * 0.5f;

        // Center camera on background
        transform.position = new Vector3(
            bgBounds.center.x,
            bgBounds.center.y,
            transform.position.z
        );
    }
}
