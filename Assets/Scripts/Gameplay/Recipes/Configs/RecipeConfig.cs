using Gameplay.Cards.Configs;
using UnityEngine;

namespace Gameplay.Recipes.Configs
{
    [CreateAssetMenu(menuName = "Gameplay/Configs/Recipe", fileName = "RecipeConfig")]
    public class RecipeConfig : ScriptableObject
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public float CreateTime { get; private set; } = 2;
        [field: SerializeField] public Sprite InfoIcon { get; private set; }
        [field: Space] 
        [field: SerializeField] public IngredientConfig[] Ingredients { get; private set; }
        [field: SerializeField] public IngredientConfig Result { get; private set; }

        private void OnValidate()
        {
            Name = name;
        }
    }
}