using System;
using Core;
using Gameplay.Cards.Interfaces;
using UniRx;
using UnityEngine;

namespace Gameplay.Cards.Core
{
    public sealed class CardDragService : DisposableClass
    {
        private readonly Camera _camera;
        private readonly LayerMask _cardLayer;

        private IDraggableCard _currentCard;
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
        }


        private void TryBeginDrag()
        {
            var worldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f, _cardLayer);

            if (!hit.collider)
                return;

            if (!hit.collider.TryGetComponent<IDraggableCard>(out var card))
                return;
            
            _currentCard = card;
            _currentCard.OnDragStart();

            _offset = card.Transform.position - worldPos;
            _offset.z = 0;
     
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
            
            _currentCard?.OnDragEnd();
            _currentCard = null;
        }
    }
}