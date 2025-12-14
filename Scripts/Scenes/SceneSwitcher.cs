using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneSwitcher 
{  
    public static void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Empty u bodoh!");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"the '{sceneName}' is nonexistent!");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    public static void LoadScene(int sceneBuildIndex)
    {
        if (sceneBuildIndex < 0 || sceneBuildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"Scene build index {sceneBuildIndex} is out of range!");
            return;
        }

        SceneManager.LoadScene(sceneBuildIndex);
    }

 
    public static void LoadSceneAsync(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Empty u bodoh!");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"the '{sceneName}' is nonexistent!");
            return;
        }

        SceneManager.LoadSceneAsync(sceneName);
    }

    public static void LoadSceneAsync(int sceneBuildIndex)
    {
        if (sceneBuildIndex < 0 || sceneBuildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"Scene build index {sceneBuildIndex} is out of range!");
            return;
        }

        SceneManager.LoadSceneAsync(sceneBuildIndex);
    }
}