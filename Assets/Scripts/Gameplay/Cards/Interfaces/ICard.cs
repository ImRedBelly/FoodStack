using Gameplay.Cards.Configs;
using Gameplay.Cards.Types;
using Gameplay.Core.Interfaces;
using UnityEngine;

namespace Gameplay.Cards.Interfaces
{
    public interface ICard : IDragObject
    {
        CardType CardType { get; }
        Transform Container { get; }
        Collider2D Collider { get; }
        CardConfig CardConfig { get; }

        void UpdateSortingOrder();
        void SetStateEligibleFrame(bool state);
        void SetStateFlame(bool state);
        void SetStateSlider(bool state);
        void SetProgress(float progress);
    }
}