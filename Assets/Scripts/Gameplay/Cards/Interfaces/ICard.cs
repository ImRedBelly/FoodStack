using UnityEngine;

namespace Gameplay.Cards.Interfaces
{
    public interface ICard
    {
        Transform Transform { get; }
        Transform Container { get; }
        Collider2D Collider { get; }
        
        void OnDragStart();
        void OnDragEnd();
        void SetStateEligibleFrame(bool state);
    }
}