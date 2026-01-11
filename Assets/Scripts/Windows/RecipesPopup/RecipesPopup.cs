using System;
using System.Collections.Generic;
using Gameplay.Recipes.Configs;
using Services.WindowService;
using Support;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Windows.RecipesPopup
{
    public class RecipesPopup : WindowBase<RecipesPopup.Model>
    {
        public class Model
        {
            public readonly IReadOnlyCollection<RecipeConfig> RecipesConfig;
            public readonly Action OnClickResume;
            public readonly WindowsService WindowsService;


            public Model(
                IReadOnlyCollection<RecipeConfig> recipesConfig,
                Action onClickResume,
                WindowsService windowsService)
            {
                RecipesConfig = recipesConfig;
                OnClickResume = onClickResume;
                WindowsService = windowsService;
            }
        }

        [SerializeField] private Button _buttonResume;
        [Space] [SerializeField] private Transform _parent;
        [SerializeField] private RecipePanelView _recipePanelViewPrefab;
        [SerializeField] private IngredientPanelView _ingredientPanelViewPrefab;
        [SerializeField] private ButtonUnlockRecipe _buttonUnlockRecipePrefab;
        [Space] [SerializeField] private GameObject _plusPrefab;
        [SerializeField] private GameObject _equalPrefab;

        private readonly List<GameObject> _createdObjects = new List<GameObject>();
        private readonly Dictionary<ButtonUnlockRecipe, RecipeConfig> _buttonUnlockRecipes = new Dictionary<ButtonUnlockRecipe, RecipeConfig>();

        protected override void OnOpen()
        {
            _buttonResume
                .OnClickAsObservable()
                .SafeSubscribe(_ =>
                {
                    ActiveModel.OnClickResume?.Invoke();
                    ActiveModel.WindowsService.Close();
                })
                .AddTo(Disposables);

            CreateRecipes();
        }

        protected override void OnClose()
        {
            foreach (var createdObject in _createdObjects)
            {
                Destroy(createdObject);
            }

            foreach (var buttonUnlockRecipe in _buttonUnlockRecipes)
            {
                buttonUnlockRecipe.Key.Dispose();
                Destroy(buttonUnlockRecipe.Key);
            }

            _createdObjects.Clear();
            _buttonUnlockRecipes.Clear();
            
            base.OnClose();
        }

        private void CreateRecipes()
        {
            foreach (var recipeConfig in ActiveModel.RecipesConfig)
            {
                CreateRecipePanelView(recipeConfig);
            }
        }

        private void CreateRecipePanelView(RecipeConfig recipeConfig)
        {
            var recipePanelView = Instantiate(_recipePanelViewPrefab, _parent);
            recipePanelView.SetRecipeNameText(recipeConfig.Name);
            _createdObjects.Add(recipePanelView.gameObject);

            var resultPanelView = Instantiate(_ingredientPanelViewPrefab, recipePanelView.Parent);
            resultPanelView.SetIngredientImage(recipeConfig.Result.Sprite);
            _createdObjects.Add(resultPanelView.gameObject);

            var equal = Instantiate(_equalPrefab, recipePanelView.Parent);
            _createdObjects.Add(equal);

            for (int i = 0; i < recipeConfig.Ingredients.Length; i++)
            {
                var ingredientPanelView = Instantiate(_ingredientPanelViewPrefab, recipePanelView.Parent);
                ingredientPanelView.SetIngredientImage(recipeConfig.Ingredients[i].Sprite);
                _createdObjects.Add(ingredientPanelView.gameObject);

                if (i != recipeConfig.Ingredients.Length - 1)
                {
                    var plus = Instantiate(_plusPrefab, recipePanelView.Parent);
                    _createdObjects.Add(plus);
                }
            }


            var buttonUnlockRecipe = Instantiate(_buttonUnlockRecipePrefab, recipePanelView.ButtonUnlockRecipeParent);

            var isUnlocked = recipeConfig.PriceUnlock <= 0 || SaveUtility.IsRecipeUnlocked(recipeConfig.Name);
            var enoughMoney = recipeConfig.PriceUnlock <= SaveUtility.GetMoney();

            buttonUnlockRecipe.SetState(isUnlocked, enoughMoney);
            buttonUnlockRecipe.SetTextPrice(isUnlocked ? "Unlocked" : "Unlock: " + recipeConfig.PriceUnlock);
            buttonUnlockRecipe.ButtonUnlockRecipeClick += UnlockRecipe;
            _buttonUnlockRecipes.Add(buttonUnlockRecipe, recipeConfig);

            void UnlockRecipe()
            {
                var canUnlock = recipeConfig.PriceUnlock <= SaveUtility.GetMoney();
                if (canUnlock)
                {
                    SaveUtility.SpendMoney(recipeConfig.PriceUnlock);
                    SaveUtility.RecipeUnlock(recipeConfig.Name);

                    foreach (var button in _buttonUnlockRecipes)
                    {
                        isUnlocked = button.Value.PriceUnlock <= 0 || SaveUtility.IsRecipeUnlocked(button.Value.Name);
                        enoughMoney = button.Value.PriceUnlock <= SaveUtility.GetMoney();

                        button.Key.SetState(isUnlocked, enoughMoney);
                        button.Key.SetTextPrice(isUnlocked ? "Unlocked" : "Unlock: " + button.Value.PriceUnlock);
                    }
                }
            }
        }
    }
}