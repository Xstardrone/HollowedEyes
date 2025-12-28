using UnityEngine;
using UnityEngine.UI;

public class EndScroll : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float scrollSpeed = 0.02f;
    void Start()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;
    }
    void Update()
    {
        if (scrollRect.verticalNormalizedPosition > 0f)
        {
            scrollRect.verticalNormalizedPosition -= scrollSpeed * Time.deltaTime;
        }
    }
}
