using System;
using Gameplay.Cards.Configs;
using Gameplay.Clients.Configs;
using Gameplay.Recipes.Configs;
using Gameplay.Tools.Configs;
using UnityEngine;

namespace Gameplay.Level.Configs
{
    [Serializable]
    public struct LevelData
    {
        public ToolConfig[] ToolCards;
        public IngredientConfig[] IngredientCards;
        public OrderQueue[] OrderQueue;
        public float LevelTime;
    }

    [Serializable]
    public struct OrderQueue
    {
        public ClientConfig ClientConfig;
        public RecipeConfig RecipeConfig;
    }

    [CreateAssetMenu(menuName = "Gameplay/Configs/Level", fileName = "Level ")]
    public class LevelConfig : ScriptableObject
    {
        [field: SerializeField] public LevelData LevelData { get; private set; }
    }
}