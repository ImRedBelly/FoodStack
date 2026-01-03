using System.Collections.Generic;
using Gameplay.Cards.Systems;
using Gameplay.Recipes.Configs;

namespace Gameplay.Recipes.Services
{
    public class RecipesStorage
    {
        public IReadOnlyCollection<RecipeConfig> RecipeConfigs { get; private set; }

        public RecipesStorage(RecipeConfig[] configs)
        {
            RecipeConfigs = configs;
        }

        public void GetRecipe(CardStack cardStack)
        {
        }
    }
}