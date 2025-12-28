using Gameplay.OrderButton.Handlers;
using Gameplay.OrderButton.Interfaces;
using UnityEngine;

namespace Gameplay.OrderButton
{
    public class OrderButton : MonoBehaviour, IOrderButton
    {
        public Transform Transform => transform;
        public Transform ClientPoint => _clientPoint;
        public Collider2D Collider => _collider2D;

        [SerializeField] private OrderButtonHandlerView _viewHandler;
        [SerializeField] private Transform _clientPoint;
        [SerializeField] private Collider2D _collider2D;

        public void UpdateOrderSprite(Sprite sprite)
        {
            _viewHandler.UpdateOrderSprite(sprite);
        }

        public void ShowClient()
        {
            _viewHandler.ShowClient();
        }

        public void HideClient()
        {
            _viewHandler.HideClient();
        }
    }
}