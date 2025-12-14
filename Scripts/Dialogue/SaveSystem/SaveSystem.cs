using System.IO;
using UnityEngine;

public static class SaveSystem
{
    public static readonly string SAVE_FOLDER = Application.dataPath + "/Saves/";
    public static void Init()
    {
        //test if save folder exists
        if (!Directory.Exists(SAVE_FOLDER))
        {
            //create save folder
            Directory.CreateDirectory(SAVE_FOLDER);
        }
    }

    public static void Save(string saveString)
    {
        int saveNumber = 1;
        while (File.Exists(SAVE_FOLDER + "save_" + saveNumber + ".txt"))
        {
            saveNumber++;
        }
        File.WriteAllText(SAVE_FOLDER + "save_" + saveNumber + ".txt", saveString);
        Debug.Log(SAVE_FOLDER + "save_" + saveNumber + ".txt");
    }

    //public static string Load()
    //{
    //    if (File.Exists(SAVE_FOLDER + "/save.txt"))
    //    {
    //        string saveString = File.ReadAllText(SAVE_FOLDER + "/save.txt");
    //        return saveString;
    //    }
    //    else
    //    {
    //        return null;
    //    }
    //}

    public static string LoadMostRecent()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(SAVE_FOLDER);
        FileInfo[] saveFiles = directoryInfo.GetFiles("*.txt");
        FileInfo mostRecentFile = null;
        foreach (FileInfo fileInfo in saveFiles)
        {
            if (mostRecentFile == null)
            {
                mostRecentFile = fileInfo;
            }
            else if (fileInfo.LastWriteTime > mostRecentFile.LastWriteTime)
            {
                mostRecentFile = fileInfo;
            }
        }

        if (mostRecentFile != null)
        {
            string saveString = File.ReadAllText(mostRecentFile.FullName);
            Debug.Log(saveString);
            return saveString;
        }
        else
        {
            return null;
        }

    }
}

/*
 SAVE + LOAD TEMPLATE (In other scripts)

---for save---
1. get data to save (player position)
2. Create a SaveObject to store the data
3. convert to json
4. write to File

private void Save(){
    DataToSave playerposition = player.GetPosition();

    SaveObject saveObject = SaveObject{
        playerPosition = playerPosition
    }:

    string json = JsonUtility.ToJson(saveObject);
    File.WriteAllText();
    Debug.log("saved");
}

---for load---
1. Check if file exists
2. Read from file
3. convert from json
4. set data

private void Load(){
    if (File.Exists(Application.dataPath + "save.txt")){
        string SaveString = File.ReadAllText(Application.dataPath + "/save.txt");
        Debug.log("Loaded: " + saveString);
        
        SaveObject saveObject = JsonUltility.FromJson<SaveObject>(saveString);

        player.SetPosition(saveObject.playerPosition);
    }
    else{
        Debug.log("no saves found");
    }
}
 */
