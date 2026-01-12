using Gameplay.Cards.Configs;
using Gameplay.Recipes.Types;
using Gameplay.Types;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay.Recipes.Configs
{
    [CreateAssetMenu(menuName = "Gameplay/Configs/Recipe", fileName = "RecipeConfig")]
    public class RecipeConfig : ScriptableObject
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public float CreateTime { get; private set; } = 2;
        [field: SerializeField] public Sprite InfoIcon { get; private set; }
        [field: SerializeField] public bool WithBurn { get; private set; } = true;
        [field: SerializeField] public int PriceUnlock { get; private set; } = 1;
        [field: SerializeField] public int Profit { get; private set; } = 10;
        [field: SerializeField] public RecipeType RecipeType { get; private set; }
        [field: FormerlySerializedAs("<CardPackType>k__BackingField")] [field: SerializeField] public RecipeCategoryType RecipeCategoryType { get; private set; }
        [field: Space]
        [field: SerializeField] public CardConfig[] Ingredients { get; private set; }
        [field: SerializeField] public CardConfig Result { get; private set; }

        private void OnValidate()
        {
            Name = name;
        }
    }
}