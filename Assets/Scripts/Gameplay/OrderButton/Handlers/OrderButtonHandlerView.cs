using DG.Tweening;
using UnityEngine;

namespace Gameplay.OrderButton.Handlers
{
    public class OrderButtonHandlerView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _viewOrder;
        [SerializeField] private SpriteRenderer _viewSlider;
        [SerializeField] private Transform _clientPoint;

        public void UpdateOrderSprite(Sprite sprite)
        {
            _viewOrder.sprite = sprite;
        }

        public void ShowClient()
        {
            _clientPoint.DOKill();
            _clientPoint.DOScale(Vector3.one, 0.5f).SetEase(Ease.InBack);
        }

        public void HideClient()
        {
            _clientPoint.DOKill();
            _clientPoint.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
        }
        public void SetStateSlider(bool active)
        {
            _viewSlider.transform.parent.gameObject.SetActive(active);
        }

        public void SetProgress(float progress)
        {
            _viewSlider.size = new Vector2(Constants.MaxSliderValue * progress, _viewSlider.size.y);
        }
    }
}