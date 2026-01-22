using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Gameplay.Cards.Configs;
using Gameplay.Cards.Factory;
using Gameplay.Cards.Interfaces;
using Gameplay.Cards.Systems;
using Gameplay.Cards.Types;
using Gameplay.Level.Systems;
using Gameplay.Recipes.Configs;
using Support;
using UniRx;
using UnityEngine;

namespace Gameplay.Recipes.Systems
{
    public struct CreateTaskData
    {
        public readonly RecipeConfig Recipe;
        public readonly IDisposable Disposable;

        public CreateTaskData(RecipeConfig recipe, IDisposable disposable)
        {
            Recipe = recipe;
            Disposable = disposable;
        }
    }

    public class CreateDishService : DisposableClass
    {
        private readonly CardPlacementSystem _cardPlacementSystem;
        private readonly CardStackSystem _cardStackSystem;
        private readonly CardFactory _cardFactory;
        private readonly RecipesStorage _recipesStorage;
        private readonly PauseGameSystem _pauseGameSystem;

        private readonly Dictionary<ICard, CardStack> _toolToStack = new();
        private readonly Dictionary<ICard, CreateTaskData> _activeCreateTasks = new();

        private bool _pauseState;

        public CreateDishService(
            CardPlacementSystem cardPlacementSystem,
            CardStackSystem cardStackSystem,
            CardFactory cardFactory,
            RecipesStorage recipesStorage,
            PauseGameSystem pauseGameSystem)
        {
            _cardPlacementSystem = cardPlacementSystem;
            _cardStackSystem = cardStackSystem;
            _cardFactory = cardFactory;
            _recipesStorage = recipesStorage;
            _pauseGameSystem = pauseGameSystem;
        }

        protected override void OnInit()
        {
            base.OnInit();

            // _cardCollisionSystem.OnCardDropWithoutMerge
            //     .SafeSubscribe(DropWithoutMerge)
            //     .AddTo(Disposables);
            //
            // _cardCollisionSystem.OnCardCollisionWithCard
            //     .SafeSubscribe(DetectTool)
            //     .AddTo(Disposables);

            _cardStackSystem.OnUpdateStacks
                .SafeSubscribe(UpdateStacks)
                .AddTo(Disposables);

            _pauseGameSystem.OnPauseGame
                .SafeSubscribe(PauseGame)
                .AddTo(Disposables);
        }

        // private void DropWithoutMerge(ICard card)
        // {
        //     var stack = _cardStackSystem.GetStack(card);
        //     if (stack == null) return;
        //     var tool = stack.Cards.First();
        //
        //     _toolToStack[tool] = stack;
        //     TryCreateDish();
        // }
        //
        // private void DetectTool((ICard card1, ICard card2) data)
        // {
        //     var stack = _cardStackSystem.GetStack(data.card1);
        //     if (stack == null) return;
        //     var tool = stack.Cards.First();
        //
        //     _toolToStack[tool] = stack;
        //     TryCreateDish();
        // }

        private void UpdateStacks(List<CardStack> stacks)
        {
            foreach (var stack in stacks)
            {
                if (stack.Cards.Count <= 1)
                {
                    DisableStackFlame(stack);
                    continue;
                }

                var tool = stack.Cards.First();
                _toolToStack[tool] = stack;
            }

            TryCreateDish();
        }

        private void PauseGame(bool pauseState)
        {
            _pauseState = pauseState;
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
                    DisableStackFlame(stack);
                    CancelCreateTask(tool);
                    continue;
                }

                if (_activeCreateTasks.TryGetValue(tool, out var data) && data.Recipe == matchedRecipe)
                    continue;

                CreateDishTask(tool, matchedRecipe, stack);
            }
        }

        private void CreateDishTask(ICard toolCard, RecipeConfig recipeConfig, CardStack stack)
        {
            CancelCreateTask(toolCard);
            DisableStackFlame(stack);

            var firstCard = stack.Cards.First();
            var lastCard = stack.Cards.Last();

            firstCard.SetStateSlider(true);
            lastCard.SetStateFlame(recipeConfig.WithBurn);

            float elapsedTime = 0f;

            var disposable = Observable
                .EveryUpdate()
                .Where(_ => !_pauseState)
                .TakeWhile(_ => elapsedTime < recipeConfig.CreateTime)
                .Subscribe(
                    _ =>
                    {
                        elapsedTime += Time.deltaTime;
                        float progress = Mathf.Clamp01(elapsedTime / recipeConfig.CreateTime);
                        firstCard.SetProgress(progress);
                    },
                    () =>
                    {
                        firstCard.SetStateSlider(false);
                        lastCard.SetStateFlame(false);

                        _activeCreateTasks.Remove(toolCard);
                        var newDishCard = _cardFactory.CreateCard(recipeConfig.Result, Vector3.zero);
                        _cardPlacementSystem.CardDropWithoutMerge(newDishCard);

                        List<ICard> removeCards = new();
                        foreach (var card in _toolToStack[toolCard].Cards)
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

                        _toolToStack.Remove(toolCard);
                    });

            Disposables.Add(disposable);

            _activeCreateTasks[toolCard] = new CreateTaskData(recipeConfig, disposable);
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
                Disposables.Remove(data.Disposable);
                data.Disposable.Dispose();

                _activeCreateTasks.Remove(toolCard);

                var stack = _cardStackSystem.GetStack(toolCard);
                if (stack != null)
                {
                    stack.Cards.First().SetStateSlider(false);
                    stack.Cards.Last().SetStateFlame(false);
                }
            }
        }

        private static void DisableStackFlame(CardStack stack)
        {
            foreach (var card in stack.Cards)
            {
                card.SetStateFlame(false);
            }
        }
    }
}