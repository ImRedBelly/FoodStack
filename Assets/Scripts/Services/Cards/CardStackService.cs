using System.Collections.Generic;
using Gameplay.Cards.Interfaces;

namespace Services.Cards
{
    public class CardStackService
    {
        private readonly List<CardStack> _stacks = new();
        private readonly Dictionary<ICard, CardStack> _cardToStack = new();

        private CardStack _lastSourceStack;
        private List<ICard> _lastDetachedCards;

        
        public CardStack GetStack(ICard card)
        {
            _cardToStack.TryGetValue(card, out CardStack stack);
            return stack;
        }

        public CardStack CreateStack(ICard card)
        {
            CardStack stack = new CardStack();
            stack.Cards.Add(card);

            _stacks.Add(stack);
            _cardToStack[card] = stack;

            return stack;
        }
        
        
        public void MergeStacks(ICard draggedRoot, CardStack target)
        {
            if (!_cardToStack.TryGetValue(draggedRoot, out var source))
                return;

            if (source == target)
                return;

            foreach (var card in source.Cards)
            {
                target.Cards.Add(card);
                _cardToStack[card] = target;
            }

            source.Cards.Clear();
            _stacks.Remove(source);

            target.UpdateWorldPositions();
        }
        
        public void DetachSubStack(ICard card)
        {
            if (!_cardToStack.TryGetValue(card, out var stack))
                return;

            int index = stack.Cards.IndexOf(card);
            if (index < 0)
                return;

            _lastSourceStack = stack;
            _lastDetachedCards = stack.Cards.GetRange(index, stack.Cards.Count - index);

            stack.Cards.RemoveRange(index, stack.Cards.Count - index);

            if (stack.Cards.Count == 0)
                _stacks.Remove(stack);
            else
                stack.UpdateWorldPositions();

            var newStack = new CardStack();
            foreach (var c in _lastDetachedCards)
            {
                newStack.Cards.Add(c);
                _cardToStack[c] = newStack;
            }

            _stacks.Add(newStack);
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

            _lastSourceStack.UpdateWorldPositions();

            _lastSourceStack = null;
            _lastDetachedCards = null;
        }

    }
}