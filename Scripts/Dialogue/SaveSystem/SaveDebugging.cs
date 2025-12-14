using System.IO;
using UnityEngine;

public class SaveDebugging : MonoBehaviour
{
    private Player player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            Save();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Load();
        }

    }


    // SAVE + LOAD TEMPLATE (In other scripts)

    //---for save---
    //1. get data to save (player position)
    //2. Create a SaveObject to store the data
    //3. convert to json
    //4. write to File

    private void Save()
    {
        Vector2 playerposition = player.GetPosition();

        SaveObject saveObject = new SaveObject {
            playerPosition = player.GetPosition()
        };

    string json = JsonUtility.ToJson(saveObject);
        SaveSystem.Save(json);
        Debug.Log("saved");
        Debug.Log("Player new save Position is " + saveObject.playerPosition);
    }

    //---for load---
    //1. Check if file exists
    //2. Read from file
    //3. convert from json
    //4. set data

    private void Load()
    {
        string saveString = SaveSystem.LoadMostRecent();

        if (saveString != null)
        {
            Debug.Log("Loaded: " + saveString);

            SaveObject saveObject = JsonUtility.FromJson<SaveObject>(saveString);

            player.SetPosition(saveObject.playerPosition);
            Debug.Log("Player new load Position is " + saveObject.playerPosition);
        }
        else
        {
            Debug.Log("no saves found");
        }
    }

    private class SaveObject
    {
        public Vector2 playerPosition;
    }
}
