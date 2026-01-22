using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gameplay.Cards.Handlers
{
    public class CardViewHandler : MonoBehaviour
    {
        [SerializeField] private SortingGroup _sortingGroup;
        [SerializeField] private SpriteRenderer _viewCard;
        [SerializeField] private SpriteRenderer _shadowCard;
        [SerializeField] private SpriteRenderer _viewSlider;
        [SerializeField] private SpriteRenderer _eligibleFrame;
        [SerializeField] private GameObject _flameObject;

        public void Initialize(Sprite sprite, int sortingOrder)
        {
            _viewCard.sprite = sprite;

            SetSortingOrder(sortingOrder);
            SetStateShadow(false);
            SetStateEligibleFrame(false);
            SetStateFlame(false);
            SetStateSlider(false);
        }

        public void SetSortingOrder(int sortingOrder)
        {
            _sortingGroup.sortingOrder = sortingOrder;
        }

        public void SetStateShadow(bool active)
        {
            _shadowCard.gameObject.SetActive(active);

            _shadowCard.transform
                .DOLocalMove(Constants.CardDragOffset * -1, 0.1f)
                .From(Vector3.zero)
                .SetLink(_shadowCard.gameObject, LinkBehaviour.KillOnDisable);
        }

        public void SetStateEligibleFrame(bool active)
        {
            _eligibleFrame.gameObject.SetActive(active);
        }

        public void SetStateFlame(bool active)
        {
            _flameObject.SetActive(active);
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