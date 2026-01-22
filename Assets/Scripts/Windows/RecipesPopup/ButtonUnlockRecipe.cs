using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Windows.RecipesPopup
{
    public class ButtonUnlockRecipe : MonoBehaviour, IDisposable
    {
        public event Action ButtonUnlockRecipeClick;

        [SerializeField] private Button _buyButton;
        [SerializeField] private Image _buttonImage;
        [SerializeField] private TMP_Text _textBuy;
        [SerializeField] private GameObject _coinObject;
        [Space] [SerializeField] private Sprite _unlockedSprite;
        [SerializeField] private Sprite _buyingSprite;
        [SerializeField] private Sprite _inactiveSprite;
        [SerializeField] private Color _unlockedTextColor;
        [SerializeField] private Color _buyingTextColor;
        [SerializeField] private Color _inactiveTextColor;

        private void OnEnable()
        {
            _buyButton.onClick.AddListener(OnButtonUnlockRecipeClick);
        }

        private void OnDisable()
        {
            Dispose();
        }

        public void Dispose()
        {
            _buyButton.onClick.RemoveListener(OnButtonUnlockRecipeClick);
            ButtonUnlockRecipeClick = null;
        }

        public void SetState(bool isUnlocked, bool enoughMoney)
        {
            _buyButton.interactable = !isUnlocked;
            _buttonImage.sprite = isUnlocked ? _unlockedSprite : enoughMoney ? _buyingSprite : _inactiveSprite;
            _textBuy.color = isUnlocked ? _unlockedTextColor : enoughMoney ? _buyingTextColor : _inactiveTextColor;
            _coinObject.SetActive(!isUnlocked);
        }

        public void SetTextPrice(string price)
        {
            _textBuy.SetText(price);
        }

        private void OnButtonUnlockRecipeClick()
        {
            ButtonUnlockRecipeClick?.Invoke();
        }
    }
}