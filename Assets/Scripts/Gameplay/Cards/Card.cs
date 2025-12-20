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
            public readonly IngredientConfig Config;

            public Model(IngredientConfig config)
            {
                Config = config;
            }
        }

        public Transform Transform => transform;
        public Collider2D Collider => _collider2D;

        [SerializeField] private Collider2D _collider2D;
        [SerializeField] private CardViewHandler _viewHandler;

        private readonly int _defaultSortingOrder = 1;
        private readonly int _dragSortingOrder = 100;

        protected override void OnInit()
        {
            base.OnInit();
            _viewHandler.Initialize(ActiveModel.Config.Sprite, _defaultSortingOrder);
        }


        public virtual void OnDragStart()
        {
            _viewHandler.SetSortingOrder(_dragSortingOrder);
            _viewHandler.SetStateShadow(true);
            transform.localScale = Vector3.one * 1.1f;
        }

        public virtual void OnDragEnd()
        {
            _viewHandler.SetSortingOrder(_defaultSortingOrder);
            _viewHandler.SetStateShadow(false);
            transform.localScale = Vector3.one;
        }

        public void SetStateEligibleFrame(bool state)
        {
            _viewHandler.SetStateEligibleFrame(state);
        }

        public void UpdateSortingOrder()
        {
            _viewHandler.SetSortingOrder((int)((transform.position.y * -10) + 50));
        }
    }
}