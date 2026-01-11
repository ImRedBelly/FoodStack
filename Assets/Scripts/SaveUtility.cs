using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;

public static class SaveUtility
{
    private const string LevelKey = "Level";
    private const string StarsKey = "Stars";
    private const string RecipeUnlockKey = "RecipeUnlock";

    private static List<string> _recipeUnlockedNames = new();

    public static IntReactiveProperty Stars;

    public static void Init(CompositeDisposable disposables)
    {
        Stars = new IntReactiveProperty(GetStars());
        Stars.AddTo(disposables);
    }

    public static int Level
    {
        get => PlayerPrefs.GetInt(LevelKey, 0);
        set => PlayerPrefs.SetInt(LevelKey, value);
    }

    public static int GetStars()
    {
        return PlayerPrefs.GetInt(StarsKey, 25);
    }

    public static void AppendStars(int value)
    {
        var futureMoney = Math.Clamp(GetStars() + value, 0, int.MaxValue);
        PlayerPrefs.GetInt(StarsKey, futureMoney);
    }

    public static void SpendStars(int value)
    {
        var futureMoney = Math.Clamp(GetStars() - value, 0, int.MaxValue);
        PlayerPrefs.SetInt(StarsKey, futureMoney);
        Stars.Value = futureMoney;
    }


    public static bool IsRecipeUnlocked(string recipeName)
    {
        var savedList = JsonConvert.DeserializeObject<List<string>>(PlayerPrefs.GetString(RecipeUnlockKey));
        _recipeUnlockedNames = savedList ?? new List<string>();
        return _recipeUnlockedNames.Contains(recipeName);
    }

    public static void RecipeUnlock(string recipeName)
    {
        if (!IsRecipeUnlocked(recipeName))
        {
            _recipeUnlockedNames.Add(recipeName);
            PlayerPrefs.SetString(RecipeUnlockKey, JsonConvert.SerializeObject(_recipeUnlockedNames));
        }
    }
}