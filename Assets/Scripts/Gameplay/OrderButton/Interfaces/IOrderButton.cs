using UnityEngine;

namespace Gameplay.OrderButton.Interfaces
{
    public interface IOrderButton
    {
        Transform Transform { get; }
        Transform ClientPoint { get; }
        Collider2D ColliderClient { get; }
        Collider2D ColliderButton { get; }

        public void UpdateOrderSprite(Sprite sprite);
        public void ShowClient();
        public void HideClient();
    }
}