using Gameplay.Cards.Configs;
using Gameplay.Cards.Types;
using UnityEngine;

namespace Gameplay.Cards.Interfaces
{
    public interface ICard
    {
        CardType CardType { get; }
        Transform Transform { get; }
        Transform Container { get; }
        Collider2D Collider { get; }
        CardConfig CardConfig { get; }

        void OnDragEnd();
        void OnDragStart();

        void UpdateSortingOrder();
        void SetStateEligibleFrame(bool state);
        void SetStateFlame(bool state);
        void SetStateSlider(bool state);
        void SetProgress(float progress);
    }
}