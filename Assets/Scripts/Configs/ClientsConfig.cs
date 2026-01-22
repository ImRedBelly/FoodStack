using System.Collections.Generic;
using Gameplay.Clients.Configs;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(menuName = "Gameplay/Configs/ClientsConfig", fileName = "ClientsConfig")]
    public class ClientsConfig : ScriptableObject
    {
        [SerializeField] private ClientConfig[] _clientsConfigs;

        public IReadOnlyCollection<ClientConfig> ClientsConfigs => _clientsConfigs;
    }
}