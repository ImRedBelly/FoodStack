using UnityEngine;
using UnityEngine.UI;

namespace Windows.RecipesPopup
{
    public class IngredientPanelView : MonoBehaviour
    {
        [SerializeField] private Image _ingredientImage;

        public void SetIngredientImage(Sprite ingredientImage)
        {
            _ingredientImage.sprite = ingredientImage;
        }
    }
}