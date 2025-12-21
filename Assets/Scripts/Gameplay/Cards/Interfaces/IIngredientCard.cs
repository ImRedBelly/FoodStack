using Gameplay.Cards.Configs;
using UnityEngine;

namespace Gameplay.Cards.Interfaces
{
    public interface IIngredientCard
    {
        Transform Transform { get; }
        Transform Container { get; }
        Collider2D Collider { get; }
        IngredientConfig IngredientConfig { get; }
        
        void OnDragStart();
        void OnDragEnd();
        void SetStateEligibleFrame(bool state);
        void UpdateSortingOrder();
    }
}