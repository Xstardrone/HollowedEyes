using UnityEngine;

public class RopeRendering : MonoBehaviour
{

    LineRenderer ropeRenderer;
    GameObject player;
    GameObject swingNode;

    void Awake()
    {
        ropeRenderer = GetComponent<LineRenderer>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        ropeRenderer.enabled = false;

    }

    void OnEnable()
    {
        ropeRenderer.enabled = true;
        player = GameObject.FindGameObjectWithTag("Player");
        swingNode = player.GetComponent<PlayerMaskController>().swingNode;
    }

    // Update is called once per frame
    void Update()
    {
        ropeRenderer.enabled = true;
        ropeRenderer.positionCount = 2;
        ropeRenderer.SetPosition(0, swingNode.transform.position);
        ropeRenderer.SetPosition(1, player.transform.position);
    }

    void OnDisable()
    {
        ropeRenderer.enabled = false;
    }
}
