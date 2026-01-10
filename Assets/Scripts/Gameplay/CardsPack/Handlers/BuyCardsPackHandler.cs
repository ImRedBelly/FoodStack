using UnityEngine;

namespace Gameplay.CardsPack.Handlers
{
    public class BuyCardsPackHandler : MonoBehaviour
    {
        [SerializeField] private GameObject _activeCardPanel;
        [SerializeField] private GameObject _inactiveCardPanel;

        public void SetActiveCardPanel(bool state)
        {
            _activeCardPanel.SetActive(state);
            _inactiveCardPanel.SetActive(!state);
        }
    }
}