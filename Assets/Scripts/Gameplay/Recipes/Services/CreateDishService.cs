using System.Collections.Generic;
using System.Linq;
using Core;
using DG.Tweening;
using Gameplay.Cards.Configs;
using Gameplay.Cards.Factory;
using Gameplay.Cards.Interfaces;
using Gameplay.Cards.Systems;
using Gameplay.Cards.Types;
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

            _cardCollisionSystem.OnCardDropWithoutMerge
                .SafeSubscribe(DropWithoutMerge)
                .AddTo(Disposables);

            _cardCollisionSystem.OnCardCollisionWithCard
                .SafeSubscribe(DetectTool)
                .AddTo(Disposables);

            _cardStackSystem.OnUpdateStacks
                .SafeSubscribe(UpdateStacks)
                .AddTo(Disposables);
        }

        private void DropWithoutMerge(ICard card)
        {
            var stack = _cardStackSystem.GetStack(card);
            if (stack == null) return;
            var tool = stack.Cards.First();

            _toolToStack[tool] = stack;
            TryCreateDish();
        }

        private void DetectTool((ICard card1, ICard card2) data)
        {
            var stack = _cardStackSystem.GetStack(data.card1);
            if (stack == null) return;
            var tool = stack.Cards.First();

            _toolToStack[tool] = stack;
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

            var firstCard = cards.First();
            var lastCard = cards.Last();
            firstCard.SetStateSlider(true);
            lastCard.SetStateFlame(recipeConfig.WithBurn);

            var tween = DOVirtual
                .Float(0, 1, recipeConfig.CreateTime, firstCard.SetProgress)
                .OnComplete(() =>
                {
                    _toolToStack.Remove(toolCard);

                    firstCard.SetStateSlider(false);
                    lastCard.SetStateFlame(false);

                    _activeCreateTasks.Remove(toolCard);
                    _cardFactory.CreateCard(recipeConfig.Result, Vector3.zero);

                    List<ICard> removeCards = new List<ICard>();
                    foreach (var card in cards)
                    {
                        if (card.CardType == CardType.Consumable)
                        {
                            removeCards.Add(card);
                            _cardFactory.RemoveCard(card);
                        }
                    }

                    foreach (var card in removeCards)
                    {
                        _cardStackSystem.RemoveCardFromStack(card);
                    }
                })
                .OnKill(() =>
                {
                    firstCard.SetStateSlider(false);
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