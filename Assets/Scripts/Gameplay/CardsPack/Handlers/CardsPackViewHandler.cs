using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gameplay.CardsPack.Handlers
{
    public class CardsPackViewHandler : MonoBehaviour
    {
        [SerializeField] private SortingGroup _sortingGroup;
        [SerializeField] private SpriteRenderer _viewCard;
        [SerializeField] private SpriteRenderer _shadowCard;
        
        public void Initialize(Sprite sprite, int sortingOrder)
        {
            _viewCard.sprite = sprite;

            SetSortingOrder(sortingOrder);
            SetStateShadow(false);
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
    }
}