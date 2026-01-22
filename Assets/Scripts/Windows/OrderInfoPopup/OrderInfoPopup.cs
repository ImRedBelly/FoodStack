using System;
using System.Collections.Generic;
using System.Linq;
using Windows.RecipesPopup;
using Cysharp.Threading.Tasks;
using Gameplay.Recipes.Configs;
using Services.WindowService;
using Support;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Windows.OrderInfoPopup
{
    public class OrderInfoPopup : WindowBase<OrderInfoPopup.Model>
    {
        public class Model
        {
            public readonly Action OnClickResume;
            public readonly RecipeConfig RecipeConfig;
            public readonly IReadOnlyCollection<RecipeConfig> RecipesCollection;
            public readonly WindowsService WindowsService;


            public Model(
                Action onClickResume,
                RecipeConfig recipeConfig,
                IReadOnlyCollection<RecipeConfig> recipesCollection,
                WindowsService windowsService)
            {
                OnClickResume = onClickResume;
                RecipeConfig = recipeConfig;
                RecipesCollection = recipesCollection;
                WindowsService = windowsService;
            }
        }

        [SerializeField] private Button _buttonResume;
        [Space] [SerializeField] private Transform _parent;
        [SerializeField] private RecipePanelView _recipePanelViewPrefab;
        [SerializeField] private IngredientPanelView _ingredientPanelViewPrefab;

        [SerializeField] private GameObject _plusPrefab;
        [SerializeField] private GameObject _equalPrefab;

        private readonly List<GameObject> _createdObjects = new List<GameObject>();

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

            _createdObjects.Clear();
            base.OnClose();
        }

        private async void CreateRecipes()
        {
            CreateMainRecipe();
            CreateSecondaryRecipe();

            await UniTask.Yield();
            foreach (var componentsInChild in GetComponentsInChildren<ContentSizeFitter>())
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(componentsInChild.GetComponent<RectTransform>());
            }
        }

        private void CreateMainRecipe()
        {
            CreateRecipePanelView(ActiveModel.RecipeConfig);
        }

        private void CreateSecondaryRecipe()
        {
            foreach (var recipeConfig in ActiveModel.RecipesCollection)
            {
                if (ActiveModel.RecipeConfig.Ingredients.Contains(recipeConfig.Result))
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
        }
    }
}