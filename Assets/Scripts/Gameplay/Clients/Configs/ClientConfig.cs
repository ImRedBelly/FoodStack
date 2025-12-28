using UnityEngine;

namespace Gameplay.Clients.Configs
{
    [CreateAssetMenu(menuName = "Gameplay/Configs/Client", fileName = "ClientConfig")]
    public class ClientConfig : ScriptableObject
    {
        [field: SerializeField] public Sprite Sprite { get; private set; }
        [field: SerializeField] public string Name { get; private set; }
    }
}