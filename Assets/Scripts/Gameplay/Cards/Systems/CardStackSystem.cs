using System;
using System.Collections.Generic;
using Gameplay.Cards.Interfaces;
using UniRx;

namespace Gameplay.Cards.Systems
{
    public class CardStack
    {
        public readonly List<ICard> Cards = new();
    }

    public class CardStackSystem
    {
        public IObservable<Unit> OnUpdateStacks => _onUpdateStacks;
        private readonly Subject<Unit> _onUpdateStacks = new();


        private readonly CardStackMoveSystem _cardStackMoveSystem;

        private readonly List<CardStack> _stacks = new();
        private readonly Dictionary<ICard, CardStack> _cardToStack = new();

        private CardStack _lastSourceStack;
        private List<ICard> _lastDetachedCards;

        public CardStackSystem(CardStackMoveSystem cardStackMoveSystem)
        {
            _cardStackMoveSystem = cardStackMoveSystem;
        }


        public CardStack GetStack(ICard card)
        {
            return _cardToStack.TryGetValue(card, out CardStack stack) ? stack : CreateStack(card);
        }

        public void MergeStacks(ICard draggedCard, CardStack targetStack)
        {
            var sourceStack = GetStack(draggedCard);

            if (sourceStack == targetStack)
                return;

            foreach (var card in sourceStack.Cards)
            {
                targetStack.Cards.Add(card);
                _cardToStack[card] = targetStack;
            }

            sourceStack.Cards.Clear();
            _stacks.Remove(sourceStack);
            _cardStackMoveSystem.UpdateWorldPositions(targetStack);
            _onUpdateStacks?.OnNext(Unit.Default);
        }

        public void DetachSubStack(ICard card)
        {
            var stack = GetStack(card);

            int index = stack.Cards.IndexOf(card);
            if (index < 0)
                return;

            _lastSourceStack = stack;
            _lastDetachedCards = stack.Cards.GetRange(index, stack.Cards.Count - index);

            stack.Cards.RemoveRange(index, stack.Cards.Count - index);

            if (stack.Cards.Count == 0)
                _stacks.Remove(stack);
            else
                _cardStackMoveSystem.UpdateWorldPositions(stack);

            var newStack = new CardStack();
            foreach (var c in _lastDetachedCards)
            {
                newStack.Cards.Add(c);
                _cardToStack[c] = newStack;
            }

            _stacks.Add(newStack);

            _onUpdateStacks?.OnNext(Unit.Default);
        }

        public void RestoreDetachedStack(ICard root)
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

            _cardStackMoveSystem.UpdateWorldPositions(_lastSourceStack);

            _lastSourceStack = null;
            _lastDetachedCards = null;

            _onUpdateStacks?.OnNext(Unit.Default);
        }

        private CardStack CreateStack(ICard card)
        {
            CardStack stack = new CardStack();
            stack.Cards.Add(card);

            _stacks.Add(stack);
            _cardToStack[card] = stack;

            return stack;
        }
    }
}