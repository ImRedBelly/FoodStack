using Core;
using Gameplay.Clients.Configs;
using Gameplay.Clients.Handlers;
using Gameplay.Clients.Interfaces;
using UnityEngine;

namespace Gameplay.Clients
{
    public class ClientCard : DisposableBehaviour<ClientCard.Model>, IClientCard
    {
        public class Model
        {
            public readonly ClientConfig ClientConfig;

            public Model(ClientConfig clientConfig)
            {
                ClientConfig = clientConfig;
            }
        }


        public Transform Transform => transform;
        public Collider2D Collider => _collider2D;

        [SerializeField] private ClientViewHandler _viewHandler;
        [SerializeField] private Collider2D _collider2D;

        protected override void OnInit()
        {
            base.OnInit();
            _viewHandler.Initialize(ActiveModel.ClientConfig.Sprite);
        }
    }
}