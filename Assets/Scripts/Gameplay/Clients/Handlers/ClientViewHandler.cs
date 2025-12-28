using UnityEngine;

namespace Gameplay.Clients.Handlers
{
    public class ClientViewHandler : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _viewCard;

        public void Initialize(Sprite sprite)
        {
            _viewCard.sprite = sprite;
        }
    }
}