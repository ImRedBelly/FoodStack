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
            _viewOrder.enabled = sprite != null;
            _viewOrder.sprite = sprite;
        }

        public void ShowClient(bool immediately)
        {
            _clientPoint.DOKill();
            _clientPoint.DOScale(Vector3.one, immediately ? 0 : Constants.TimeAnimationClient)
                .SetLink(_clientPoint.gameObject, LinkBehaviour.KillOnDisable)
                .From(Vector3.zero)
                .SetEase(Ease.OutBack);
        }

        public void HideClient(bool immediately)
        {
            _clientPoint.DOKill();
            _clientPoint.DOScale(Vector3.zero, immediately ? 0 : Constants.TimeAnimationClient)
                .SetLink(_clientPoint.gameObject, LinkBehaviour.KillOnDisable)
                .From(Vector3.one)
                .SetEase(Ease.InBack);
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