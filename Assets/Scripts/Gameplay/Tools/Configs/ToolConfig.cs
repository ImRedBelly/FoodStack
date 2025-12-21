using Gameplay.Recipes.Configs;
using UnityEngine;

namespace Gameplay.Tools.Configs
{
    [CreateAssetMenu(menuName = "Gameplay/Configs/Tool", fileName = "ToolConfig")]
    public class ToolConfig : ScriptableObject
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public Sprite Sprite { get; private set; }
        [field: Space] 
        [field: SerializeField] public RecipeConfig[] Recipes { get; private set; }
    }
}