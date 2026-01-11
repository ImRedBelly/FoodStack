using System;
using Gameplay.Cards.Configs;
using Gameplay.Clients.Configs;
using Gameplay.Recipes.Configs;
using UnityEngine;

namespace Gameplay.Level.Configs
{
    public enum DifficultyType
    {
        Default = 0,
        Hard = 1,
        SuperHard = 2
    }

    [Serializable]
    public struct LevelData
    {
        public CardConfig[] ToolCards;
        public CardConfig[] IngredientCards;
     //   public OrderQueue[] OrderQueue;
        public float LevelTime;
        public DifficultyType DifficultyType;
    }

    // [Serializable]
    // public struct OrderQueue
    // {
    //     public ClientConfig ClientConfig;
    //     public RecipeConfig RecipeConfig;
    // }

    [CreateAssetMenu(menuName = "Gameplay/Configs/Level", fileName = "Level ")]
    public class LevelConfig : ScriptableObject
    {
        [field: SerializeField] public LevelData LevelData { get; private set; }
    }
}