using Gameplay.Cards.Types;
using UnityEngine;

namespace Gameplay.Cards.Configs
{
    [CreateAssetMenu(menuName = "Gameplay/Configs/Card", fileName = "CardConfig")]
    public class CardConfig : ScriptableObject
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public Sprite Sprite { get; private set; }
        [field: SerializeField] public CardType CardType { get; private set; }
    }
}