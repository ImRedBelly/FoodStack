using Core;
using Gameplay.Cards.Interfaces;
using Gameplay.Cards.Services;
using Gameplay.Tools.Interfaces;
using Support;
using UniRx;
using UnityEngine;

namespace Gameplay.Tools.Services
{
    public class SetupStackPositionInToolService : DisposableClass
    {
        private readonly ToolCardsDetectService _toolCardsDetectService;
        private readonly CardStackService _cardStackService;
        private readonly CardStackMoveService _cardStackMoveService;

        public SetupStackPositionInToolService(
            ToolCardsDetectService toolCardsDetectService,
            CardStackService cardStackService,
            CardStackMoveService cardStackMoveService)
        {
            _toolCardsDetectService = toolCardsDetectService;
            _cardStackService = cardStackService;
            _cardStackMoveService = cardStackMoveService;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _toolCardsDetectService.OnDetectTool
                .SafeSubscribe(DetectTool)
                .AddTo(Disposables);
        }

        private void DetectTool((IToolCard tool, IIngredientCard card) data)
        {
            var stack = _cardStackService.GetStack(data.card);
            if (stack == null) return;

            _cardStackMoveService
                .UpdateWorldPositions(stack, data.tool.Transform.position - Vector3.up * 0.2f, Constants.MaxDragSpeed);
        }
    }
}