using Gameplay.OrderButton.Handlers;
using Gameplay.OrderButton.Interfaces;
using UnityEngine;

namespace Gameplay.OrderButton
{
    public class OrderButton : MonoBehaviour, IOrderButton
    {
        public Transform Transform => transform;
        public Transform ClientPoint => _clientPoint;
        public Collider2D ColliderClient => _collider2D;
        public Collider2D ColliderButton => _collider2D;

        [SerializeField] private OrderButtonHandlerView _viewHandler;
        [SerializeField] private Transform _clientPoint;
        [SerializeField] private Collider2D _collider2D;

        public void UpdateOrderSprite(Sprite sprite)
        {
            _viewHandler.UpdateOrderSprite(sprite);
        }

        public void ShowClient(bool immediately)
        {
            _viewHandler.ShowClient(immediately);
        }

        public void HideClient(bool immediately)
        {
            _viewHandler.HideClient(immediately);
        }

        public void SetStateSlider(bool active)
        {
            _viewHandler.SetStateSlider(active);
        }

        public void SetProgress(float progress)
        {
            _viewHandler.SetProgress(progress);
        }
    }
}