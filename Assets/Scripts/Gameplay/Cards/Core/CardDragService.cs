using System;
using Core;
using Gameplay.Cards.Interfaces;
using UniRx;
using UnityEngine;

namespace Gameplay.Cards.Core
{
    public sealed class CardDragService : DisposableClass
    {
        public IObservable<ICard> OnStartDrag => _onStartDrag;
        public IObservable<ICard> OnEndDrag => _onEndDrag;

        private readonly Subject<ICard> _onStartDrag = new();
        private readonly Subject<ICard> _onEndDrag = new();

        private readonly Camera _camera;
        private readonly LayerMask _cardLayer;

        private ICard _currentCard;
        private IDisposable _dragDisposable;

        private Vector3 _offset;

        public CardDragService(Camera camera, LayerMask cardLayer)
        {
            _camera = camera;
            _cardLayer = cardLayer;
        }

        protected override void OnInit()
        {
            base.OnInit();

            Observable.EveryUpdate()
                .Where(_ => Input.GetMouseButtonDown(0))
                .Subscribe(_ => TryBeginDrag())
                .AddTo(Disposables);

            Observable.EveryUpdate()
                .Where(_ => Input.GetMouseButtonUp(0))
                .Subscribe(_ => EndDrag())
                .AddTo(Disposables);

            _onStartDrag.AddTo(Disposables);
            _onEndDrag.AddTo(Disposables);
        }

        private void TryBeginDrag()
        {
            var worldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f, _cardLayer);

            if (!hit.collider)
                return;

            if (!hit.collider.TryGetComponent<ICard>(out var card))
                return;

            _currentCard = card;
            _currentCard.OnDragStart();

            _offset = card.Transform.position - worldPos;
            _offset.z = 0;

            _onStartDrag?.OnNext(_currentCard.Transform.GetComponent<Card>());
            _dragDisposable = Observable.EveryUpdate().Subscribe(_ => UpdateDrag());
        }

        private void UpdateDrag()
        {
            if (_currentCard == null)
                return;

            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            var plane = new Plane(Vector3.forward, Vector3.zero);

            plane.Raycast(ray, out var distance);
            var worldPos = ray.GetPoint(distance);
            worldPos.z = 0;

            _currentCard.Transform.position = worldPos + _offset;
        }


        private void EndDrag()
        {
            _dragDisposable?.Dispose();
            _dragDisposable = null;

            if (_currentCard != null)
            {
                _onEndDrag?.OnNext(_currentCard);
                _currentCard.OnDragEnd();
                _currentCard = null;
            }
        }
    }
}