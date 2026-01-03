using Core;
using Gameplay.Cards.Interfaces;
using Support;
using UniRx;
using UnityEngine;

namespace Gameplay.Cards.Systems
{
    public class SetupStackPositionInToolSystem : DisposableClass
    {
        private readonly CardCollisionSystem _cardCollisionSystem;
        private readonly CardStackSystem _cardStackSystem;
        private readonly CardStackMoveSystem _cardStackMoveSystem;

        public SetupStackPositionInToolSystem(
            CardCollisionSystem cardCollisionSystem,
            CardStackSystem cardStackSystem,
            CardStackMoveSystem cardStackMoveSystem)
        {
            _cardCollisionSystem = cardCollisionSystem;
            _cardStackSystem = cardStackSystem;
            _cardStackMoveSystem = cardStackMoveSystem;
        }

        protected override void OnInit()
        {
            base.OnInit();
            
            // _cardCollisionSystem.OnCardCollisionWithTool
            //     .SafeSubscribe(DetectTool)
            //     .AddTo(Disposables);
        }

        private void DetectTool((ICard tool, ICard card) data)
        {
            var stack = _cardStackSystem.GetStack(data.card);
            if (stack == null) return;

            _cardStackMoveSystem
                .UpdateWorldPositions(stack, data.tool.Transform.position - Vector3.up * 0.2f, Constants.MaxDragSpeed);
        }
    }
}