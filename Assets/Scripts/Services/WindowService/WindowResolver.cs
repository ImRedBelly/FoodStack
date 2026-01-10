using System;
using System.Collections.Generic;
using Windows;
using Windows.OrderInfoPopup;
using Windows.RecipesPopup;
using Gameplay.Recipes.Configs;

namespace Services.WindowService
{
    public class WindowResolver
    {
        private readonly WindowsService _windowsService;

        public WindowResolver(WindowsService windowsService)
        {
            _windowsService = windowsService;
        }

        public PausePopup.Model GetPausePopupModel(Action onClickResume, Action onClickReload, Action onClickUpgrade)
        {
            return new(onClickResume, onClickReload, onClickUpgrade, _windowsService);
        }

        public RecipesPopup.Model GetRecipesPopupModel(IReadOnlyCollection<RecipeConfig> recipesConfig,
            Action onClickResume)
        {
            return new(recipesConfig, onClickResume, _windowsService);
        }

        public OrderInfoPopup.Model GeOrderInfoPopupModel(RecipeConfig recipeConfig,
            IReadOnlyCollection<RecipeConfig> recipesCollection, Action onClickResume)
        {
            return new(onClickResume, recipeConfig, recipesCollection, _windowsService);
        }
    }
}