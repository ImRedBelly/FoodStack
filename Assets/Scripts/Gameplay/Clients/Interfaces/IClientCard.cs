using UnityEngine;

namespace Gameplay.Clients.Interfaces
{
    public interface IClientCard
    {
        Transform Transform { get; }
        Collider2D Collider { get; }
    }
}