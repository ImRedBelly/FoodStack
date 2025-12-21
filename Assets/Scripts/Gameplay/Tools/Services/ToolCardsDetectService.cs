using System;
using System.Collections.Generic;
using Core;
using Gameplay.Cards.Interfaces;
using Gameplay.Cards.Services;
using Gameplay.Tools.Factory;
using Gameplay.Tools.Interfaces;
using Support;
using UniRx;

namespace Gameplay.Tools.Services
{
    public class ToolCardsDetectService : DisposableClass
    {
        public IObservable<(IToolCard, IIngredientCard)> OnDetectTool => _onDetectTool;

        private readonly Subject<(IToolCard, IIngredientCard)> _onDetectTool = new();

        private readonly ToolFactory _toolFactory;
        private readonly CardDragService _cardDragService;

        private readonly List<IToolCard> _tools = new();

        public ToolCardsDetectService(ToolFactory toolFactory, CardDragService cardDragService)
        {
            _toolFactory = toolFactory;
            _cardDragService = cardDragService;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _toolFactory.OnCardCreated
                .SafeSubscribe(AddTool)
                .AddTo(Disposables);

            _cardDragService.OnEndDrag
                .SafeSubscribe(EndDrag)
                .AddTo(Disposables);
        }

        private void EndDrag(IIngredientCard ingredientCard)
        {
            foreach (var tool in _tools)
            {
                if (!IsOverlapping(tool, ingredientCard)) continue;

                _onDetectTool?.OnNext((tool, ingredientCard));
                break;
            }
        }

        private void AddTool(IToolCard newToolCard)
        {
            if (!_tools.Contains(newToolCard))
            {
                _tools.Add(newToolCard);
            }
        }


        private bool IsOverlapping(IToolCard toolCard, IIngredientCard ingredientCard)
        {
            return toolCard.Collider.bounds.Intersects(ingredientCard.Collider.bounds);
        }
    }
}