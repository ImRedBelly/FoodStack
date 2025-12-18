using Core;
using Gameplay.Cards.Core;
using Gameplay.Cards.Interfaces;
using UnityEngine;

namespace Gameplay.Cards
{
    public abstract class BaseCard : DisposableBehaviour<BaseCard.Model>, IDraggableCard
    {
        public class Model
        {
            public Model()
            {
            }
        }

        public Transform Transform => transform;

        [SerializeField] private Transform _container;
        [SerializeField] private CardViewHandler _viewHandler;

        private readonly int _defaultSortingOrder = 1;
        private readonly int _dragSortingOrder = 100;

        protected override void OnInit()
        {
            base.OnInit();
            _viewHandler.Initialize(_defaultSortingOrder);
        }


        public virtual void OnDragStart()
        {
            _viewHandler.SetSortingOrder(_dragSortingOrder);
            _viewHandler.SetStateShadow(true);
            _viewHandler.SetStateEligibleFrame(true);
        }

        public virtual void OnDragEnd()
        {
            _viewHandler.SetSortingOrder(_defaultSortingOrder);
            _viewHandler.SetStateShadow(false);
            _viewHandler.SetStateEligibleFrame(false);
        }
    }
}