using System;
using Gameplay.Cards.Configs;
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

        public float LevelTime;
        public int LevelTarget;
        public int StartMoney;
        
        public DifficultyType DifficultyType;
    }
    
    [CreateAssetMenu(menuName = "Gameplay/Configs/Level", fileName = "Level ")]
    public class LevelConfig : ScriptableObject
    {
        [field: SerializeField] public LevelData LevelData { get; private set; }
    }
}