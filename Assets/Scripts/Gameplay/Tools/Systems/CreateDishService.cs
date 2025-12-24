using System.Collections.Generic;
using Core;
using DG.Tweening;
using Gameplay.Cards.Configs;
using Gameplay.Cards.Factory;
using Gameplay.Cards.Interfaces;
using Gameplay.Cards.Systems;
using Gameplay.Recipes.Configs;
using Gameplay.Tools.Interfaces;
using Support;
using UniRx;
using UnityEngine;

namespace Gameplay.Tools.Systems
{
    public class CreateDishService : DisposableClass
    {
        private readonly CardCollisionSystem _cardCollisionSystem;
        private readonly CardStackSystem _cardStackSystem;
        private readonly CardFactory _cardFactory;

        private readonly Dictionary<IToolCard, CardStack> _toolToStack = new();
        private readonly Dictionary<IToolCard, Tween> _activeCreateTasks = new();

        public CreateDishService(
            CardCollisionSystem cardCollisionSystem,
            CardStackSystem cardStackSystem, 
            CardFactory cardFactory)
        {
            _cardCollisionSystem = cardCollisionSystem;
            _cardStackSystem = cardStackSystem;
            _cardFactory = cardFactory;
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

        private void DetectTool((IToolCard tool, IIngredientCard card) data)
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

                foreach (var recipeConfig in tool.RecipeConfigs)
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

                if (_activeCreateTasks.ContainsKey(tool))
                    continue;

                CreateDishTask(tool, matchedRecipe, stack.Cards);
            }
        }

        private void CreateDishTask(IToolCard toolCard, RecipeConfig recipeConfig, List<IIngredientCard> cards)
        {
            CancelCreateTask(toolCard);

            toolCard.SetStateSlider(true);

            var tween = DOVirtual
                .Float(0, 1, recipeConfig.CreateTime, toolCard.SetProgress)
                .OnComplete(() =>
                {
                    _toolToStack.Remove(toolCard);
                    toolCard.SetStateSlider(false);
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
                    _activeCreateTasks.Remove(toolCard);
                });

            _activeCreateTasks[toolCard] = tween;
        }

        private static bool CanCreateDish(IngredientConfig[] ingredients, IReadOnlyList<IIngredientCard> cards)
        {
            if (ingredients == null || cards == null) return false;
            if (ingredients.Length != cards.Count) return false;

            Dictionary<IngredientConfig, int> cachedIngredients = new();
            
            foreach (var ingredient in ingredients)
            {
                if (cachedIngredients.TryGetValue(ingredient, out var count))
                    cachedIngredients[ingredient] = count + 1;
                else
                    cachedIngredients[ingredient] = 1;
            }

            foreach (var card in cards)
            {
                var config = card.IngredientConfig;
                if (!cachedIngredients.TryGetValue(config, out var count))
                    return false;

                if (count == 1)
                    cachedIngredients.Remove(config);
                else
                    cachedIngredients[config] = count - 1;
            }

            return true;
        }

        private void CancelCreateTask(IToolCard toolCard)
        {
            if (_activeCreateTasks.TryGetValue(toolCard, out var tween))
            {
                tween.Kill();
                _activeCreateTasks.Remove(toolCard);
            }
        }
    }
}