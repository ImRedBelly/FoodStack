using Core;
using Gameplay.Cards.Configs;
using Gameplay.Cards.Handlers;
using Gameplay.Cards.Interfaces;
using UnityEngine;

namespace Gameplay.Cards
{
    public class Card : DisposableBehaviour<Card.Model>, ICard
    {
        public class Model
        {
            public readonly CardConfig Config;

            public Model(CardConfig config)
            {
                Config = config;
            }
        }

        public Transform Transform => transform;
        public Transform Container => _container;
        public Collider2D Collider => _collider2D;
        public CardConfig CardConfig => ActiveModel.Config;

        [SerializeField] private Transform _container;
        [SerializeField] private Collider2D _collider2D;
        [SerializeField] private CardViewHandler _viewHandler;

        protected override void OnInit()
        {
            base.OnInit();
            _viewHandler.Initialize(ActiveModel.Config.Sprite, Constants.DefaultSortingOrder);
        }

        public void Dispose()
        {
            Disposables?.Dispose();
        }

        public virtual void OnDragStart()
        {
            _viewHandler.SetSortingOrder(Constants.DragSortingOrder);
            _viewHandler.SetStateShadow(true);
        }

        public bool CanDrag()
        {
            return ActiveModel.Config.CanDrag;
        }

        public virtual void OnDragEnd()
        {
            _viewHandler.SetSortingOrder(Constants.DefaultSortingOrder);
            _viewHandler.SetStateShadow(false);
        }

        public void SetStateEligibleFrame(bool state)
        {
            _viewHandler.SetStateEligibleFrame(state);
        }

        public void SetStateFlame(bool state)
        {
            _viewHandler.SetStateFlame(state);
        }

        public void SetStateSlider(bool state)
        {
            _viewHandler.SetStateSlider(state);
        }

        public void SetProgress(float progress)
        {
            _viewHandler.SetProgress(progress);
        }

        public void UpdateSortingOrder()
        {
            _viewHandler.SetSortingOrder((int)((transform.position.y * -10) + 50));
        }
    }
}