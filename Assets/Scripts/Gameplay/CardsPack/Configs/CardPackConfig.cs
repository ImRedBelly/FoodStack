using System;
using System.Collections.Generic;
using Gameplay.Cards.Configs;
using Gameplay.Types;
using UnityEngine;

namespace Gameplay.CardsPack.Configs
{
    [Serializable]
    public struct CardPackGenerateData
    {
        public CardConfig CardConfig;
        public float Percent;
    }

    [CreateAssetMenu(menuName = "Gameplay/Configs/CardPack", fileName = "CardPackConfig")]
    public class CardPackConfig : ScriptableObject
    {
        public IReadOnlyCollection<CardPackGenerateData> CardPackGenerateData => _cardPackGenerateData;
        [field: SerializeField] public RecipeCategoryType RecipeCategoryType { get; private set; }
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public int Price { get; private set; }
        [field: SerializeField] public int CountCards { get; private set; }
        [SerializeField] public CardPackGenerateData[] _cardPackGenerateData;
    }
}