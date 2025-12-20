using UnityEngine;

namespace Gameplay.Cards.Configs
{
    [CreateAssetMenu(menuName = "Gameplay/Configs/Recipe", fileName = "RecipeConfig")]
    public class RecipeConfig : ScriptableObject
    {
        [field: SerializeField] public IngredientConfig[] Ingredients { get; private set; }
        [field: SerializeField] public IngredientConfig Result { get; private set; }
    }
}