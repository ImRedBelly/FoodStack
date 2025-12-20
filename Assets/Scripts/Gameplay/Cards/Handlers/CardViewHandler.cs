using UnityEngine;
using UnityEngine.Rendering;

namespace Gameplay.Cards.Handlers
{
    public class CardViewHandler : MonoBehaviour
    {
        [SerializeField] private SortingGroup _sortingGroup;
        [SerializeField] private SpriteRenderer _viewCard;
        [SerializeField] private SpriteRenderer _shadowCard;
        [SerializeField] private SpriteRenderer _eligibleFrame;

        public void Initialize(Sprite sprite, int sortingOrder)
        {
            _viewCard.sprite = sprite;

            SetSortingOrder(sortingOrder);
            SetStateShadow(false);
            SetStateEligibleFrame(false);
        }

        public void SetSortingOrder(int sortingOrder)
        {
            _sortingGroup.sortingOrder = sortingOrder;
        }

        public void SetStateShadow(bool active)
        {
            _shadowCard.gameObject.SetActive(active);
        }

        public void SetStateEligibleFrame(bool active)
        {
            _eligibleFrame.gameObject.SetActive(active);
        }
    }
}