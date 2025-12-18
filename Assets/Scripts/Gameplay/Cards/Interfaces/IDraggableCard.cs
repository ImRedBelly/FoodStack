using UnityEngine;

namespace Gameplay.Cards.Interfaces
{
    public interface IDraggableCard
    {
        Transform Transform { get; }
        void OnDragStart();
        void OnDragEnd();
    }
}