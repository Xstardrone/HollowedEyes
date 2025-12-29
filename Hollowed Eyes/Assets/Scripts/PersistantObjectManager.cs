using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class PersistentObjectManager : MonoBehaviour
{
    [System.Serializable]
    public class TagSceneRule
    {
        [Tooltip("Tag of objects to control")]
        public string tag;

        [Tooltip("Scenes where these tagged objects should be disabled")]
        public List<string> disableInScenes = new List<string>();
    }

    [Header("Tag → Scene Disable Rules")]
    public List<TagSceneRule> rules = new List<TagSceneRule>();

    List<GameObject> taggedObjects = new List<GameObject>();

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyRules(scene.name);
    }

    private void ApplyRules(string sceneName)
    {
        foreach (var rule in rules)
        {
            if (string.IsNullOrEmpty(rule.tag)) continue;

            taggedObjects = new List<GameObject>();

            foreach (GameObject g in FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (g.CompareTag(rule.tag))
                {
                    taggedObjects.Add(g);
                    Debug.Log("Found object: " + g.name);
                }
            }

            bool shouldDisable = rule.disableInScenes.Contains(sceneName);

            foreach (GameObject obj in taggedObjects)
            {
                if (sceneName == "Main Menu")
                {
                    Destroy(obj);
                } else
                {
                    obj.SetActive(!shouldDisable);
                }
            }
        }
    }
}
