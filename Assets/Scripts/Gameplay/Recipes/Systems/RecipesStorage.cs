using System.Collections.Generic;
using System.Linq;
using Gameplay.Cards.Systems;
using Gameplay.Recipes.Configs;

namespace Gameplay.Recipes.Systems
{
    public class RecipesStorage
    {
        public IReadOnlyCollection<RecipeConfig> RecipeConfigs { get; private set; }

        public RecipesStorage(IReadOnlyCollection<RecipeConfig> configs)
        {
            RecipeConfigs = configs;
        }

        public IReadOnlyCollection<RecipeConfig> GetOpenRecipes()
        {
            return RecipeConfigs.Where(x => x.PriceUnlock <= 0 || SaveUtility.IsRecipeUnlocked(x.Name)).ToList();
        }
    }
}