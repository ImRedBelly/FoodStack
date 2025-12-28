using System;
using Core;
using Gameplay.OrderButton.Interfaces;
using UniRx;
using UnityEngine;

namespace Gameplay.OrderButton.Factory
{
    public class OrderButtonFactory : DisposableClass
    {
        public IObservable<IOrderButton> OnOrderButtonCreated => _onOrderButtonCreated;

        private readonly Subject<IOrderButton> _onOrderButtonCreated = new();

        private readonly OrderButton _orderButtonPrefab;

        public OrderButtonFactory(OrderButton orderButtonPrefab)
        {
            _orderButtonPrefab = orderButtonPrefab;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _onOrderButtonCreated.AddTo(Disposables);
        }

        public void CreateOrderButton(int index)
        {
            var orderButton = UnityEngine.Object.Instantiate(_orderButtonPrefab, new Vector3(index == 0 ? -1 : 1, 2.9f, 0), Quaternion.identity);
            _onOrderButtonCreated?.OnNext(orderButton);
        }
    }
}