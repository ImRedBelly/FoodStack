using System.Collections.Generic;
using System.Linq;
using Core;
using DG.Tweening;
using Gameplay.Cards.Configs;
using Gameplay.Cards.Factory;
using Gameplay.Cards.Interfaces;
using Gameplay.Cards.Systems;
using Gameplay.Recipes.Configs;
using Support;
using UniRx;
using UnityEngine;

namespace Gameplay.Recipes.Services
{
    public struct TweenData
    {
        public readonly RecipeConfig Recipe;
        public readonly Tween Tween;

        public TweenData(RecipeConfig recipeConfig, Tweener tween)
        {
            Recipe = recipeConfig;
            Tween = tween;
        }
    }

    public class CreateDishService : DisposableClass
    {
        private readonly CardCollisionSystem _cardCollisionSystem;
        private readonly CardStackSystem _cardStackSystem;
        private readonly CardFactory _cardFactory;
        private readonly RecipesStorage _recipesStorage;

        private readonly Dictionary<ICard, CardStack> _toolToStack = new();
        private readonly Dictionary<ICard, TweenData> _activeCreateTasks = new();

        public CreateDishService(
            CardCollisionSystem cardCollisionSystem,
            CardStackSystem cardStackSystem,
            CardFactory cardFactory,
            RecipesStorage recipesStorage)
        {
            _cardCollisionSystem = cardCollisionSystem;
            _cardStackSystem = cardStackSystem;
            _cardFactory = cardFactory;
            _recipesStorage = recipesStorage;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _cardCollisionSystem.OnCardCollisionWithTool
                .SafeSubscribe(DetectTool)
                .AddTo(Disposables);

            _cardStackSystem.OnUpdateStacks
                .SafeSubscribe(UpdateStacks)
                .AddTo(Disposables);
        }

        private void DetectTool((ICard tool, ICard card) data)
        {
            var stack = _cardStackSystem.GetStack(data.card);
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
                var tool = kv.Key;
                var stack = kv.Value;

                RecipeConfig matchedRecipe = null;

                foreach (var recipeConfig in _recipesStorage.RecipeConfigs)
                {
                    if (CanCreateDish(recipeConfig.Ingredients, stack.Cards))
                    {
                        matchedRecipe = recipeConfig;
                        break;
                    }
                }

                if (matchedRecipe == null)
                {
                    CancelCreateTask(tool);
                    continue;
                }

                if (_activeCreateTasks.TryGetValue(tool, out var data) && data.Recipe == matchedRecipe)
                    continue;

                CreateDishTask(tool, matchedRecipe, stack.Cards);
            }
        }

        private void CreateDishTask(ICard toolCard, RecipeConfig recipeConfig, List<ICard> cards)
        {
            CancelCreateTask(toolCard);

            var lastCard = cards.Last();
            toolCard.SetStateSlider(true);
            lastCard.SetStateFlame(true);

            var tween = DOVirtual
                .Float(0, 1, recipeConfig.CreateTime, toolCard.SetProgress)
                .OnComplete(() =>
                {
                    _toolToStack.Remove(toolCard);

                    toolCard.SetStateSlider(false);
                    lastCard.SetStateFlame(false);

                    _activeCreateTasks.Remove(toolCard);
                    _cardFactory.CreateIngredient(recipeConfig.Result, Vector3.zero);

                    foreach (var card in cards)
                    {
                        _cardFactory.RemoveIngredient(card);
                    }
                })
                .OnKill(() =>
                {
                    toolCard.SetStateSlider(false);
                    lastCard.SetStateFlame(false);
                    _activeCreateTasks.Remove(toolCard);
                });

            _activeCreateTasks[toolCard] = new TweenData(recipeConfig, tween);
        }

        private static bool CanCreateDish(CardConfig[] ingredients, IReadOnlyList<ICard> cards)
        {
            if (ingredients == null || cards == null) return false;
            if (ingredients.Length != cards.Count) return false;

            Dictionary<CardConfig, int> cachedIngredients = new();

            foreach (var ingredient in ingredients)
            {
                if (cachedIngredients.TryGetValue(ingredient, out var count))
                    cachedIngredients[ingredient] = count + 1;
                else
                    cachedIngredients[ingredient] = 1;
            }

            foreach (var card in cards)
            {
                var config = card.CardConfig;
                if (!cachedIngredients.TryGetValue(config, out var count))
                    return false;

                if (count == 1)
                    cachedIngredients.Remove(config);
                else
                    cachedIngredients[config] = count - 1;
            }

            return true;
        }

        private void CancelCreateTask(ICard toolCard)
        {
            if (_activeCreateTasks.TryGetValue(toolCard, out var data))
            {
                data.Tween.Kill();
                _activeCreateTasks.Remove(toolCard);
            }
        }
    }
}