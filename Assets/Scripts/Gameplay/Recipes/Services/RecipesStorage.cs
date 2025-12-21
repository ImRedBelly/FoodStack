using Gameplay.Cards.Interfaces;
using Gameplay.Recipes.Configs;
using Gameplay.Tools.Interfaces;

namespace Gameplay.Recipes.Services
{
    public class RecipesStorage
    {
        private readonly RecipeConfig[] _configs;

        public RecipesStorage(RecipeConfig[] configs)
        {
            _configs = configs;
        }

        public void GetRecipe(IToolCard toolCard, IIngredientCard ingredientCard)
        {
          
        }
    }
}