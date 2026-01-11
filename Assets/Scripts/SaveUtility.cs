using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;

public static class SaveUtility
{
    private const string LevelKey = "Level";
    private const string MoneyKey = "Money";
    private const string RecipeUnlockKey = "RecipeUnlock";

    private static List<string> _recipeUnlockedNames = new();

    public static IntReactiveProperty Money;

    public static void Init(CompositeDisposable disposables)
    {
        Money = new IntReactiveProperty(GetMoney());
        Money.AddTo(disposables);
    }

    public static int Level
    {
        get => PlayerPrefs.GetInt(LevelKey, 0);
        set => PlayerPrefs.SetInt(LevelKey, value);
    }

    public static int GetMoney()
    {
        return PlayerPrefs.GetInt(MoneyKey, 25);
    }

    public static void AppendMoney(int value)
    {
        var futureMoney = Math.Clamp(GetMoney() + value, 0, int.MaxValue);
        PlayerPrefs.GetInt(MoneyKey, futureMoney);
    }

    public static void SpendMoney(int value)
    {
        var futureMoney = Math.Clamp(GetMoney() - value, 0, int.MaxValue);
        PlayerPrefs.SetInt(MoneyKey, futureMoney);
        Money.Value = futureMoney;
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