using UnityEngine;

namespace Gameplay.Cards.Configs
{
    [CreateAssetMenu(menuName = "Gameplay/Configs/Ingredient", fileName = "IngredientConfig")]
    public class IngredientConfig : ScriptableObject
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public Sprite Sprite { get; private set; }
    }
}