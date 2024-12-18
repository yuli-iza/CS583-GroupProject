// Data container class for saving player progress and preferences
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]   // Allows Unity to serialize this class for saving to disk
public class SaveData
{
    public float[] levelHighscores = new float[10];    // Highscores for each level
    public float overallHighscore = 0f;               // Sum of all level highscores
    public float[] levelCompletionTimes = new float[10]; // Time taken to complete each level
    public float totalCompletionTime = 0f;            // Total time for all completed levels
    public int equippedSkinIndex = 0;                 // Selected skin index
    public bool isFirstTimeShown = false;             // Flag for first-time popup
}