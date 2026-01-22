using Gameplay.Level.Configs;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(menuName = "Gameplay/Configs/LevelsConfig", fileName = "LevelsConfig")]
    public class LevelsConfig : ScriptableObject
    {
        [SerializeField] private LevelConfig[] _levelConfigs;
        [SerializeField] private int _levelRepeatData;

        public LevelData GetLevelData(int index)
        {
            return _levelConfigs[index % _levelConfigs.Length].LevelData;
        }
    }
}