using Core;
using Gameplay.Clients.Configs;
using Gameplay.Clients.Handlers;
using UnityEngine;

namespace Gameplay.Clients
{
    public class ClientCard : DisposableBehaviour<ClientCard.Model>
    {
        public class Model
        {
            public readonly ClientConfig ClientConfig;

            public Model(ClientConfig clientConfig)
            {
                ClientConfig = clientConfig;
            }
        }

        [SerializeField] private ClientViewHandler _viewHandler;

        protected override void OnInit()
        {
            base.OnInit();
            _viewHandler.Initialize(ActiveModel.ClientConfig.Sprite);
        }
    }
}