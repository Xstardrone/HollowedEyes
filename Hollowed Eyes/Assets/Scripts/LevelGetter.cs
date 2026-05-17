using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class LevelGetter : MonoBehaviour
{
    public static LevelGetter Instance;
    public int CurrentLevel { get; private set; }
    public string CurrentLevelName { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        ParseCurrentScene(SceneManager.GetActiveScene());
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ParseCurrentScene(scene);
    }
    
    void ParseCurrentScene(Scene scene)
    {
        CurrentLevelName = scene.name;
        Match match = Regex.Match(scene.name, @"\d+");
        CurrentLevel = match.Success ? int.Parse(match.Value) : 0;
    }
}
