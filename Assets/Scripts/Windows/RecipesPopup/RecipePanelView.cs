using TMPro;
using UnityEngine;

namespace Windows.RecipesPopup
{
    public class RecipePanelView : MonoBehaviour
    {
        [field: SerializeField] public Transform Parent { get; private set; }
        [SerializeField] private TMP_Text _recipeNameText;

        public void SetRecipeNameText(string recipeName)
        {
            _recipeNameText.SetText(recipeName);
        }
    }
}