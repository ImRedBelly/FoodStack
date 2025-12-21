using System.Collections.Generic;
using System.Linq;
using Core;
using DG.Tweening;
using Gameplay.Cards.Configs;
using Gameplay.Cards.Factory;
using Gameplay.Cards.Interfaces;
using Gameplay.Cards.Services;
using Gameplay.Recipes.Configs;
using Gameplay.Tools.Interfaces;
using Support;
using UniRx;
using UnityEngine;

namespace Gameplay.Tools.Services
{
    public class CreateDishService : DisposableClass
    {
        private readonly ToolCardsDetectService _toolCardsDetectService;
        private readonly CardStackService _cardStackService;
        private readonly CardFactory _cardFactory;

        private readonly Dictionary<IToolCard, CardStack> _toolToStack = new();

        public CreateDishService(
            ToolCardsDetectService toolCardsDetectService,
            CardStackService cardStackService, CardFactory cardFactory)
        {
            _toolCardsDetectService = toolCardsDetectService;
            _cardStackService = cardStackService;
            _cardFactory = cardFactory;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _toolCardsDetectService.OnDetectTool
                .SafeSubscribe(DetectTool)
                .AddTo(Disposables);

            _cardStackService.OnUpdateStacks
                .SafeSubscribe(UpdateStacks)
                .AddTo(Disposables);
        }

        private void DetectTool((IToolCard tool, IIngredientCard card) data)
        {
            var stack = _cardStackService.GetStack(data.card);
            if (stack == null) return;

            _toolToStack[data.tool] = stack;
            TryCreateDish();
        }

        private void UpdateStacks(Unit unit)
        {
            TryCreateDish();
        }

        private void TryCreateDish()
        {
            foreach (var kv in _toolToStack)
            {
                foreach (var recipeConfig in kv.Key.RecipeConfigs)
                {
                    if (!CanCreateDish(recipeConfig.Ingredients, kv.Value.Cards)) continue;
                    CreateDishTask(kv.Key, recipeConfig);
                    break;
                }
            }
        }

        private void CreateDishTask(IToolCard toolCard, RecipeConfig recipeConfig)
        {
            toolCard.SetStateSlider(true);
            DOVirtual
                .Float(0, 1, recipeConfig.CreateTime, toolCard.SetProgress)
                .OnComplete(() =>
                {
                    toolCard.SetStateSlider(false);
                    _cardFactory.CreateIngredient(recipeConfig.Result, Vector3.zero);
                    Debug.LogError("CreateDish: " + recipeConfig.Name);
                });
        }

        private static bool CanCreateDish(IngredientConfig[] ingredients, IReadOnlyList<IIngredientCard> cards)
        {
            if (ingredients == null || cards == null) return false;
            if (ingredients.Length != cards.Count) return false;

            return !ingredients.Where((t, i) => t != cards[i].IngredientConfig).Any();
        }
    }
}