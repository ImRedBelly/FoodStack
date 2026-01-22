using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gameplay.CardsPack.Handlers
{
    public class CardsPackViewHandler : MonoBehaviour
    {
        [SerializeField] private SortingGroup _sortingGroup;
        [SerializeField] private SpriteRenderer _shadowCard;
        [SerializeField] private TMP_Text _textNamePack;
        [SerializeField] private TMP_Text _textCountCards;

        public void Initialize()
        {
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

        public void UpdateNameText(string namePack)
        {
            _textNamePack.SetText(namePack);
        }

        public void UpdateCountText(string countCards)
        {
            _textCountCards.SetText(countCards);
        }
    }
}