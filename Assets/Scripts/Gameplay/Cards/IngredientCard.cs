using Core;
using Gameplay.Cards.Configs;
using Gameplay.Cards.Handlers;
using Gameplay.Cards.Interfaces;
using UnityEngine;

namespace Gameplay.Cards
{
    public class IngredientCard : DisposableBehaviour<IngredientCard.Model>, IIngredientCard
    {
        public class Model
        {
            public readonly IngredientConfig IngredientConfig;

            public Model(IngredientConfig ingredientConfig)
            {
                IngredientConfig = ingredientConfig;
            }
        }

        public Transform Transform => transform;
        public Transform Container => _container;
        public Collider2D Collider => _collider2D;
        public IngredientConfig IngredientConfig => ActiveModel.IngredientConfig;

        [SerializeField] private Transform _container;
        [SerializeField] private Collider2D _collider2D;
        [SerializeField] private CardViewHandler _viewHandler;

        protected override void OnInit()
        {
            base.OnInit();
            _viewHandler.Initialize(ActiveModel.IngredientConfig.Sprite, Constants.DefaultSortingOrder);
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

        public void UpdateSortingOrder()
        {
            _viewHandler.SetSortingOrder((int)((transform.position.y * -10) + 50));
        }
    }
}