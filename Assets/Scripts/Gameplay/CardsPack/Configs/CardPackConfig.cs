using Gameplay.CardsPack.Types;
using UnityEngine;

namespace Gameplay.CardsPack.Configs
{
    [CreateAssetMenu(menuName = "Gameplay/Configs/CardPack", fileName = "CardPackConfig")]
    public class CardPackConfig : ScriptableObject
    {
        [field: SerializeField] public CardPackType CardPackType { get; private set; }
        [field: SerializeField] public string Name { get; private set; }
    }
}