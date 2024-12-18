// Static utility class that handles saving and loading game data to/from disk
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    // Define paths for save data storage
    public static readonly string SAVE_FOLDER = Application.persistentDataPath + "/saves/";   // Directory for save files
    public static readonly string FILE_EXT = ".json";                                         // File extension for saves

    // Saves data to a JSON file
    public static void Save(string fileName, string dataToSave)
    {
        // Create saves directory if it doesn't exist
        if (!Directory.Exists(SAVE_FOLDER))
        {
            Directory.CreateDirectory(SAVE_FOLDER);
        }

        // Write data to file with specified name
        File.WriteAllText(SAVE_FOLDER + fileName + FILE_EXT, dataToSave);
    }

    // Loads data from a JSON file
    public static string Load(string fileName)
    {
        string fileLoc = SAVE_FOLDER + fileName + FILE_EXT;

        // Return file contents if it exists, null if it doesn't
        if (File.Exists(fileLoc))
        {
            string loadedData = File.ReadAllText(fileLoc);
            return loadedData;
        }
        else
        {
            return null;
        }
    }

    // Deletes save file to reset progress
    public static void ClearSave(string fileName)
    {
        string fileLoc = SAVE_FOLDER + fileName + FILE_EXT;
        if (File.Exists(fileLoc))
        {
            File.Delete(fileLoc);
            Debug.Log("Save data cleared.");
        }
    }
}