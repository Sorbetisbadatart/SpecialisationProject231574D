using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchScene(string sceneName)
    {
        SceneSwitcher.LoadScene(sceneName);
    }

    public void SwitchScene(int sceneID)
    {
        SceneSwitcher.LoadScene(sceneID);
    }
}
