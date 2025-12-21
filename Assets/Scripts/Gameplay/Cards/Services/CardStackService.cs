using System;
using System.Collections.Generic;
using Gameplay.Cards.Interfaces;
using UniRx;

namespace Gameplay.Cards.Services
{
    public class CardStack
    {
        public readonly List<IIngredientCard> Cards = new();
    }

    public class CardStackService
    {
        public IObservable<Unit> OnUpdateStacks => _onUpdateStacks;
        private readonly Subject<Unit> _onUpdateStacks = new();


        private readonly CardStackMoveService _cardStackMoveService;

        private readonly List<CardStack> _stacks = new();
        private readonly Dictionary<IIngredientCard, CardStack> _cardToStack = new();

        private CardStack _lastSourceStack;
        private List<IIngredientCard> _lastDetachedCards;

        public CardStackService(CardStackMoveService cardStackMoveService)
        {
            _cardStackMoveService = cardStackMoveService;
        }


        public CardStack GetStack(IIngredientCard ingredientCard)
        {
            return _cardToStack.TryGetValue(ingredientCard, out CardStack stack) ? stack : CreateStack(ingredientCard);
        }

        public void MergeStacks(IIngredientCard draggedIngredientCard, CardStack targetStack)
        {
            var sourceStack = GetStack(draggedIngredientCard);

            if (sourceStack == targetStack)
                return;

            foreach (var card in sourceStack.Cards)
            {
                targetStack.Cards.Add(card);
                _cardToStack[card] = targetStack;
            }

            sourceStack.Cards.Clear();
            _stacks.Remove(sourceStack);
            _cardStackMoveService.UpdateWorldPositions(targetStack);
            _onUpdateStacks?.OnNext(Unit.Default);
        }

        public void DetachSubStack(IIngredientCard ingredientCard)
        {
            var stack = GetStack(ingredientCard);

            int index = stack.Cards.IndexOf(ingredientCard);
            if (index < 0)
                return;

            _lastSourceStack = stack;
            _lastDetachedCards = stack.Cards.GetRange(index, stack.Cards.Count - index);

            stack.Cards.RemoveRange(index, stack.Cards.Count - index);

            if (stack.Cards.Count == 0)
                _stacks.Remove(stack);
            else
                _cardStackMoveService.UpdateWorldPositions(stack);

            var newStack = new CardStack();
            foreach (var c in _lastDetachedCards)
            {
                newStack.Cards.Add(c);
                _cardToStack[c] = newStack;
            }

            _stacks.Add(newStack);
            _onUpdateStacks?.OnNext(Unit.Default);
        }

        public void RestoreDetachedStack(IIngredientCard root)
        {
            if (_lastSourceStack == null || _lastDetachedCards == null)
                return;

            var current = GetStack(root);
            if (current == null)
                return;

            foreach (var card in _lastDetachedCards)
            {
                current.Cards.Remove(card);
                _lastSourceStack.Cards.Add(card);
                _cardToStack[card] = _lastSourceStack;
            }

            if (current.Cards.Count == 0)
                _stacks.Remove(current);

            _cardStackMoveService.UpdateWorldPositions(_lastSourceStack);

            _lastSourceStack = null;
            _lastDetachedCards = null;

            _onUpdateStacks?.OnNext(Unit.Default);
        }

        private CardStack CreateStack(IIngredientCard ingredientCard)
        {
            CardStack stack = new CardStack();
            stack.Cards.Add(ingredientCard);

            _stacks.Add(stack);
            _cardToStack[ingredientCard] = stack;

            return stack;
        }
    }
}