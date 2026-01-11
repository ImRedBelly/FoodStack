using System.Collections.Generic;
using Gameplay.CardsPack.Configs;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(menuName = "Gameplay/Configs/CardPacksConfig", fileName = "CardPacksConfig")]
    public class CardPacksConfig : ScriptableObject
    {
        [SerializeField] private CardPackConfig[] _cardPackConfigs;

        public IReadOnlyCollection<CardPackConfig> CardPackConfigs => _cardPackConfigs;
    }
}