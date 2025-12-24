using System.Collections.Generic;
using System.Linq;
using Core;
using Gameplay.Cards.Factory;
using Gameplay.Cards.Interfaces;
using Gameplay.Tools.Factory;
using Gameplay.Tools.Interfaces;
using Support;
using UniRx;

namespace Gameplay.Cards.Systems
{
    public class CardEligibleFrameStateSystem : DisposableClass
    {
        private readonly CardFactory _cardFactory;
        private readonly ToolFactory _toolFactory;
        private readonly CardDragSystem _dragSystem;
        private readonly CardStackSystem _cardStackSystem;

        private readonly List<IIngredientCard> _cards = new();
        private readonly List<IToolCard> _tools = new();

        public CardEligibleFrameStateSystem(CardFactory cardFactory, ToolFactory toolFactory, CardDragSystem dragSystem, CardStackSystem cardStackSystem)
        {
            _cardFactory = cardFactory;
            _toolFactory = toolFactory;
            _dragSystem = dragSystem;
            _cardStackSystem = cardStackSystem;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _cardFactory.OnCardCreated
                .SafeSubscribe(AddCard)
                .AddTo(Disposables);

            _cardFactory.OnCardRemoved
                .SafeSubscribe(RemoveCard)
                .AddTo(Disposables);

            _toolFactory.OnCardCreated
                .SafeSubscribe(AddTool)
                .AddTo(Disposables);

            _dragSystem.OnEndDrag
                .SafeSubscribe(EndDrag)
                .AddTo(Disposables);

            _dragSystem.OnStartDrag
                .SafeSubscribe(StartDrag)
                .AddTo(Disposables);
        }


        private void AddCard(IIngredientCard newIngredientCard)
        {
            if (!_cards.Contains(newIngredientCard))
            {
                _cards.Add(newIngredientCard);
            }
        }

        private void RemoveCard(IIngredientCard ingredientCard)
        {
            if (_cards.Contains(ingredientCard))
            {
                _cards.Remove(ingredientCard);
            }
        }

        private void AddTool(IToolCard newToolCard)
        {
            if (!_tools.Contains(newToolCard))
            {
                _tools.Add(newToolCard);
            }
        }

        private void StartDrag(IIngredientCard draggedIngredientCard)
        {
            foreach (var card in _cards)
                card.SetStateEligibleFrame(TryActivateEligibleFrame(draggedIngredientCard, card));

            foreach (var tool in _tools)
                tool.SetStateEligibleFrame(true);
        }


        private void EndDrag(IIngredientCard draggedIngredientCard)
        {
            foreach (var card in _cards)
                card.SetStateEligibleFrame(false);

            foreach (var tool in _tools)
                tool.SetStateEligibleFrame(false);
        }

        private bool TryActivateEligibleFrame(IIngredientCard draggedIngredientCard, IIngredientCard targetIngredientCard)
        {
            var draggedStack = _cardStackSystem.GetStack(draggedIngredientCard);
            var targetStack = _cardStackSystem.GetStack(targetIngredientCard);

            bool equalCards = draggedIngredientCard == targetIngredientCard;
            bool draggedStackContainsTarget = draggedStack.Cards.Contains(targetIngredientCard);
            bool isLastCardInStack = targetStack.Cards.Last() == targetIngredientCard;

            return !equalCards && !draggedStackContainsTarget && isLastCardInStack;
        }
    }
}