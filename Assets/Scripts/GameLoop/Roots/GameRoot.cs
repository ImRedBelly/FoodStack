using Core;
using Gameplay.Cards;
using Gameplay.Cards.Core;
using Support;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace GameLoop.Roots
{
    public class GameRoot : DisposableBehaviour<LobbyRoot.Model>
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private LayerMask _cardLayer;

        [SerializeField] private Button _quitButton;

        protected override void OnInit()
        {
            base.OnInit();

            var playConfirmWindow =
                ActiveModel.WindowResolver.GetPlayConfirmWindowModel(ActiveModel.OnGameAction, false);

            _quitButton
                .OnClickAsObservable()
                .SafeSubscribe(_ => ActiveModel.WindowsService.Open(playConfirmWindow, false))
                .AddTo(Disposables);

            new CardDragService(_camera, _cardLayer)
                .Init()
                .AddTo(Disposables);

            foreach (var card in FindObjectsOfType<BaseCard>())
            {
                card.Init(new BaseCard.Model());
            }
        }
    }
}