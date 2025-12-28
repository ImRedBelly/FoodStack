using System;
using Gameplay.Cards.Configs;
using Gameplay.Clients.Configs;
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
        public IngredientConfig IngredientConfig;
    }

    [CreateAssetMenu(menuName = "Gameplay/Configs/Level", fileName = "Level ")]
    public class LevelConfig : ScriptableObject
    {
        [SerializeField] private LevelData[] _levelData;
        [SerializeField] private int _levelRepeatData;

        public LevelData GetLevelData(int index)
        {
            return _levelData[index % _levelData.Length];
        }
    }
}