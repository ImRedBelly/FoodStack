using System.Collections.Generic;
using Gameplay.Recipes.Configs;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(menuName = "Gameplay/Configs/RecipesConfig", fileName = "RecipesConfig")]
    public class RecipesConfig : ScriptableObject
    {
        [SerializeField] private RecipeConfig[] _recipeConfigs;

        public IReadOnlyCollection<RecipeConfig> RecipeConfigs => _recipeConfigs;
    }
}