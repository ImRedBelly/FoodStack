using UnityEngine;

public static class SaveUtility
{
    public static int Level
    {
        get => PlayerPrefs.GetInt("Level", 0);
        set => PlayerPrefs.SetInt("Level", value);
    }
}