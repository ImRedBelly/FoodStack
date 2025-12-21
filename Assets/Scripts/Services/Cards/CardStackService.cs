using System.Collections.Generic;
using Gameplay.Cards.Interfaces;

namespace Services.Cards
{
    public class CardStack
    {
        public readonly List<ICard> Cards = new();
    }

    public class CardStackService
    {
        private readonly CardStackMoveService _cardStackMoveService;

        private readonly List<CardStack> _stacks = new();
        private readonly Dictionary<ICard, CardStack> _cardToStack = new();

        private CardStack _lastSourceStack;
        private List<ICard> _lastDetachedCards;

        public CardStackService(CardStackMoveService cardStackMoveService)
        {
            _cardStackMoveService = cardStackMoveService;
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
            _cardStackMoveService.UpdateWorldPositions(targetStack);
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
                _cardStackMoveService.UpdateWorldPositions(stack);

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

            _cardStackMoveService.UpdateWorldPositions(_lastSourceStack);

            _lastSourceStack = null;
            _lastDetachedCards = null;
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