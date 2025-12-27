using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomCamera : MonoBehaviour
{
    CinemachineCamera cc;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        cc = GetComponent<CinemachineCamera>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        cc.Follow = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
