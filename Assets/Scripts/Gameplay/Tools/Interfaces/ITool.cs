using UnityEngine;

namespace Gameplay.Tools.Interfaces
{
    public interface ITool
    {
        Transform Transform { get; }
        Collider2D Collider { get; }
    }
}