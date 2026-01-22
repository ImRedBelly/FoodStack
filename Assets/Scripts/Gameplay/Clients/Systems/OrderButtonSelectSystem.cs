using System.Collections.Generic;
using System.Linq;
using Core;
using Gameplay.OrderButton.Factory;
using Gameplay.OrderButton.Interfaces;
using Support;
using UniRx;

namespace Gameplay.Clients.Systems
{
    public class OrderButtonSelectSystem : DisposableClass
    {
        private readonly OrderButtonFactory _orderButtonFactory;

        private readonly Dictionary<IOrderButton, bool> _orderButtonStates = new();

        public OrderButtonSelectSystem(OrderButtonFactory orderButtonFactory)
        {
            _orderButtonFactory = orderButtonFactory;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _orderButtonFactory.OnOrderButtonCreated
                .SafeSubscribe(AddOrderButton)
                .AddTo(Disposables);
        }

        private void AddOrderButton(IOrderButton orderButton)
        {
            _orderButtonStates[orderButton] = false;
        }

        public IOrderButton GetOrderButton()
        {
            var kv = _orderButtonStates.FirstOrDefault(x => x.Value == false);
            if (kv.Key != null)
            {
                _orderButtonStates[kv.Key] = true;
                return kv.Key;
            }

            return null;
        }

        public void ReturnOrderButton(IOrderButton orderButton)
        {
            _orderButtonStates[orderButton] = false;
        }
    }
}