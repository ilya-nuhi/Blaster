using UnityEngine;
using System.IO;

public class LevelData
{
    public int level_number;
    public int grid_width;
    public int grid_height;
    public int move_count;
    public string[] grid;
}

public class PersistentPath
{
    public LevelData LoadLevel(string levelFileName)
    {
        // TextAsset textAsset = Resources.Load<TextAsset>("Levels/" + levelFileName);
        // if (textAsset != null)
        // {
        //     return JsonUtility.FromJson<LevelData>(textAsset.text);
        // }
        string filePath = Path.Combine(Application.persistentDataPath + "/Levels/" + levelFileName);
        
        if (File.Exists(filePath))
        {
            string jsonText = File.ReadAllText(filePath);
            return JsonUtility.FromJson<LevelData>(jsonText);
        }
        else
        {
            Debug.LogError("Cannot find level file: " + levelFileName);
            return null;
        }
    }
}
