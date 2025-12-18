using UnityEngine;

namespace Gameplay.Cards.Core
{
    public class CardViewHandler : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _viewCard;
        [SerializeField] private SpriteRenderer _shadowCard;
        [SerializeField] private SpriteRenderer _eligibleFrame;

        public void Initialize(int sortingOrder)
        {
            SetSortingOrder(sortingOrder);
            SetStateShadow(false);
            SetStateEligibleFrame(false);
        }

        public void SetSortingOrder(int sortingOrder)
        {
            _viewCard.sortingOrder = sortingOrder;
            _shadowCard.sortingOrder = sortingOrder - 1;
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