using Gameplay.Cards.Configs;
using UnityEngine;

namespace Gameplay.Cards.Interfaces
{
    public interface ICard
    {
        Transform Transform { get; }
        Transform Container { get; }
        Collider2D Collider { get; }
        CardConfig CardConfig { get; }

        void OnDragEnd();
        void OnDragStart();
        bool CanDrag();

        void UpdateSortingOrder();
        void SetStateEligibleFrame(bool state);
        void SetStateFlame(bool state);
        void SetStateSlider(bool state);
        void SetProgress(float progress);
    }
}