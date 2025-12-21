using System.Collections.Generic;
using Core;
using Gameplay.Cards.Interfaces;
using Gameplay.Cards.Services;
using Gameplay.Tools.Factory;
using Gameplay.Tools.Interfaces;
using Support;
using UniRx;
using UnityEngine;

namespace Gameplay.Tools.Services
{
    public class ToolCardsDetectService : DisposableClass
    {
        private readonly ToolFactory _toolFactory;
        private readonly CardDragService _cardDragService;

        private readonly List<ITool> _tools = new();

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

        private void EndDrag(ICard card)
        {
            foreach (var tool in _tools)
            {
                if (IsOverlapping(tool, card))
                {
                    Debug.LogError("Detect Tool");
                }
            }
        }

        private void AddTool(ITool newTool)
        {
            if (!_tools.Contains(newTool))
            {
                _tools.Add(newTool);
            }
        }


        private bool IsOverlapping(ITool tool, ICard card)
        {
            return tool.Collider.bounds.Intersects(card.Collider.bounds);
        }
    }
}