using UnityEngine;

namespace Gameplay.Core.Interfaces
{
    public interface IDragObject
    {
        Transform Transform { get; }
        void OnDragEnd();
        void OnDragStart();
    }
}