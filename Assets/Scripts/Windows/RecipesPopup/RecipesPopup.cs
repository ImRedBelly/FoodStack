using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.CardsPack.Configs;
using Gameplay.Recipes.Configs;
using Gameplay.Types;
using Services.WindowService;
using Support;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Windows.RecipesPopup
{
    [Serializable]
    public struct RecipeCategoryButton
    {
        public RecipeCategoryType Category;
        public Button Button;
        public TMP_Text[] CategoryTexts;
    }

    public class RecipesPopup : WindowBase<RecipesPopup.Model>
    {
        public class Model
        {
            public readonly IReadOnlyCollection<RecipeConfig> RecipesConfig;
            public readonly IReadOnlyCollection<CardPackConfig> CardPackConfigs;
            public readonly Action OnClickResume;
            public readonly WindowsService WindowsService;


            public Model(IReadOnlyCollection<RecipeConfig> recipesConfig,
                IReadOnlyCollection<CardPackConfig> cardPackConfigs,
                Action onClickResume,
                WindowsService windowsService)
            {
                RecipesConfig = recipesConfig;
                CardPackConfigs = cardPackConfigs;
                OnClickResume = onClickResume;
                WindowsService = windowsService;
            }
        }

        [SerializeField] private Button _buttonResume;
        [Space]
        [SerializeField] private Transform _parent;
        [SerializeField] private RecipePanelView _recipePanelViewPrefab;
        [SerializeField] private IngredientPanelView _ingredientPanelViewPrefab;
        [SerializeField] private ButtonUnlockRecipe _buttonUnlockRecipePrefab;
        [Space] 
        [SerializeField] private RecipeCategoryButton[] _recipeCategoryButtons;
        [Space] 
        [SerializeField] private GameObject _plusPrefab;
        [SerializeField] private GameObject _equalPrefab;

        private readonly List<GameObject> _createdObjects = new List<GameObject>();

        private readonly Dictionary<ButtonUnlockRecipe, RecipeConfig> _buttonUnlockRecipes =
            new Dictionary<ButtonUnlockRecipe, RecipeConfig>();

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

            foreach (var recipeCategoryButton in _recipeCategoryButtons)
            {
                recipeCategoryButton.Button.onClick.AddListener(ClickOpenCategory);

                foreach (var cardPackConfig in ActiveModel.CardPackConfigs)
                {
                    if (cardPackConfig.RecipeCategoryType == recipeCategoryButton.Category)
                    {
                        foreach (var categoryText in recipeCategoryButton.CategoryTexts)
                        {
                            categoryText.SetText(cardPackConfig.Name);
                        }

                        break;
                    }
                }

                void ClickOpenCategory() => OpenCategory(recipeCategoryButton.Category);
            }

            OpenCategory(RecipeCategoryType.Pantry);
        }

        protected override void OnClose()
        {
            ClearRecipes();
            foreach (var recipeCategoryButton in _recipeCategoryButtons)
            {
                recipeCategoryButton.Button.onClick.RemoveAllListeners();
            }

            base.OnClose();
        }

        private void ClearRecipes()
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
        }

        private void OpenCategory(RecipeCategoryType recipeCategoryType)
        {
            ClearRecipes();
            foreach (var recipeConfig in ActiveModel.RecipesConfig)
            {
                if (recipeConfig.RecipeCategoryType == recipeCategoryType)
                {
                    CreateRecipePanelView(recipeConfig);
                }
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
            var enoughMoney = recipeConfig.PriceUnlock <= SaveUtility.GetStars();

            buttonUnlockRecipe.SetState(isUnlocked, enoughMoney);
            buttonUnlockRecipe.SetTextPrice(isUnlocked ? "Unlocked" : "Unlock: " + GetPriceRecipe(recipeConfig));
            buttonUnlockRecipe.ButtonUnlockRecipeClick += UnlockRecipe;
            _buttonUnlockRecipes.Add(buttonUnlockRecipe, recipeConfig);

            void UnlockRecipe()
            {
                var canUnlock = GetPriceRecipe(recipeConfig) <= SaveUtility.GetStars();
                if (canUnlock)
                {
                    SaveUtility.SpendStars(recipeConfig.PriceUnlock);
                    SaveUtility.RecipeUnlock(recipeConfig.Name);

                    foreach (var config in ActiveModel.RecipesConfig)
                    {
                        if (recipeConfig.Ingredients.Contains(config.Result) &&
                            !SaveUtility.IsRecipeUnlocked(config.Name))
                        {
                            SaveUtility.SpendStars(config.PriceUnlock);
                            SaveUtility.RecipeUnlock(config.Name);
                        }
                    }

                    foreach (var button in _buttonUnlockRecipes)
                    {
                        isUnlocked = button.Value.PriceUnlock <= 0 || SaveUtility.IsRecipeUnlocked(button.Value.Name);
                        enoughMoney = button.Value.PriceUnlock <= SaveUtility.GetStars();

                        button.Key.SetState(isUnlocked, enoughMoney);
                        button.Key.SetTextPrice(isUnlocked ? "Unlocked" : "Unlock: " + GetPriceRecipe(button.Value));
                    }
                }
            }
        }

        private int GetPriceRecipe(RecipeConfig config)
        {
            int price = config.PriceUnlock;

            foreach (var recipeConfig in ActiveModel.RecipesConfig)
            {
                if (config.Ingredients.Contains(recipeConfig.Result) &&
                    !SaveUtility.IsRecipeUnlocked(recipeConfig.Name))
                {
                    price += recipeConfig.PriceUnlock;
                }
            }

            return price;
        }
    }
}