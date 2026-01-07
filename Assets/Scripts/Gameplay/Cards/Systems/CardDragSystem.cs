using System;
using System.Collections.Generic;
using Core;
using Gameplay.Cards.Interfaces;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gameplay.Cards.Systems
{
    public sealed class CardDragSystem : DisposableClass
    {
        public IObservable<ICard> OnStartDrag => _onStartDrag;
        public IObservable<ICard> OnEndDrag => _onEndDrag;

        private readonly Subject<ICard> _onStartDrag = new();
        private readonly Subject<ICard> _onEndDrag = new();

        private readonly Camera _camera;
        private readonly LayerMask _cardLayer;
        private readonly CardStackSystem _cardStackSystem;
        private readonly CardStackMoveSystem _cardStackMoveSystem;

        private ICard _currentIngredientCard;

        private Vector3 _offsetDrag;
        private Vector3 _offsetClick;
        private IDisposable _dragDisposable;

        private readonly GraphicRaycaster _graphicRaycaster;
        private readonly EventSystem _eventSystem;

        private readonly List<RaycastResult> _uiRaycastResults = new();


        public CardDragSystem(Camera camera, GraphicRaycaster graphicRaycaster,
            LayerMask cardLayer, CardStackSystem cardStackSystem,
            CardStackMoveSystem cardStackMoveSystem)
        {
            _camera = camera;
            _graphicRaycaster = graphicRaycaster;
            _eventSystem = EventSystem.current;
            _cardLayer = cardLayer;
            _cardStackSystem = cardStackSystem;
            _cardStackMoveSystem = cardStackMoveSystem;
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

            var allCards = Physics2D.RaycastAll(worldPos, Vector2.zero, 0f, _cardLayer);

            float minPositionY = float.MaxValue;
            foreach (var hit in allCards)
            {
                if (hit.collider.TryGetComponent<ICard>(out var card))
                {
                    if (hit.collider.transform.position.y < minPositionY)
                    {
                        minPositionY = hit.collider.transform.position.y;
                        _currentIngredientCard = card;
                    }
                }
            }

            if (_currentIngredientCard == null) return;

            _currentIngredientCard.OnDragStart();

            _offsetClick = _currentIngredientCard.Transform.position - worldPos;
            _offsetClick.z = 0;

            _onStartDrag?.OnNext(_currentIngredientCard);
            _dragDisposable = Observable.EveryUpdate().Subscribe(_ => UpdateDrag());
        }

        private void UpdateDrag()
        {
            if (_currentIngredientCard == null)
                return;

            UpdateCardPositions(Constants.DragSpeed, Constants.CardDragOffset);
        }

        private void EndDrag()
        {
            _dragDisposable?.Dispose();
            _dragDisposable = null;

            if (_currentIngredientCard != null)
            {
                UpdateCardPositions(Constants.MaxDragSpeed, Vector3.zero);

                _onEndDrag?.OnNext(_currentIngredientCard);
                _currentIngredientCard.OnDragEnd();
                _currentIngredientCard = null;
            }
        }

        private void UpdateCardPositions(float lerpSpeed, Vector3 offset)
        {
            if (IsPointerOverUI())
                return;

            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            var plane = new Plane(Vector3.forward, Vector3.zero);

            plane.Raycast(ray, out var distance);
            var worldPos = ray.GetPoint(distance);
            worldPos.z = 0;

            var basePos = worldPos + _offsetClick + offset;

            var stack = _cardStackSystem.GetStack(_currentIngredientCard);
            _cardStackMoveSystem.UpdateWorldPositions(stack, basePos, lerpSpeed);
        }

        private bool IsPointerOverUI()
        {
            var eventData = new PointerEventData(_eventSystem)
            {
                position = Input.mousePosition
            };

            _uiRaycastResults.Clear();
            _graphicRaycaster.Raycast(eventData, _uiRaycastResults);

            return _uiRaycastResults.Count > 0;
        }
    }
}