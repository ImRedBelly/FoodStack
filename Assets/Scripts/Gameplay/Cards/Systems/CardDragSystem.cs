using System;
using System.Collections.Generic;
using Core;
using Gameplay.Cards.Factory;
using Gameplay.Cards.Interfaces;
using Gameplay.Core.Interfaces;
using Support;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gameplay.Cards.Systems
{
    public sealed class CardDragSystem : DisposableClass
    {
        public IObservable<IDragObject> OnStartDrag => _onStartDrag;
        public IObservable<IDragObject> OnEndDrag => _onEndDrag;

        private readonly Subject<IDragObject> _onStartDrag = new();
        private readonly Subject<IDragObject> _onEndDrag = new();

        private readonly Camera _camera;
        private readonly LayerMask _cardLayer;
        private readonly CardFactory _cardFactory;
        private readonly CardStackSystem _cardStackSystem;
        private readonly CardStackMoveSystem _cardStackMoveSystem;

        private IDragObject _currentDragObject;

        private Vector3 _offsetDrag;
        private Vector3 _offsetClick;
        private IDisposable _dragDisposable;

        private readonly GraphicRaycaster _graphicRaycaster;
        private readonly EventSystem _eventSystem;

        private readonly List<RaycastResult> _uiRaycastResults = new();


        public CardDragSystem(Camera camera, GraphicRaycaster graphicRaycaster,
            LayerMask cardLayer, CardFactory cardFactory, CardStackSystem cardStackSystem,
            CardStackMoveSystem cardStackMoveSystem)
        {
            _camera = camera;
            _graphicRaycaster = graphicRaycaster;
            _eventSystem = EventSystem.current;
            _cardLayer = cardLayer;
            _cardFactory = cardFactory;
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

            _cardFactory.OnCardRemoved
                .SafeSubscribe(RemoveCard)
                .AddTo(Disposables);


            _onStartDrag.AddTo(Disposables);
            _onEndDrag.AddTo(Disposables);
        }

        private void TryBeginDrag()
        {
            if (IsPointerOverUI())
                return;

            var worldPos = _camera.ScreenToWorldPoint(Input.mousePosition);

            var allCards = Physics2D.RaycastAll(worldPos, Vector2.zero, 0f, _cardLayer);

            float minPositionY = float.MaxValue;
            foreach (var hit in allCards)
            {
                if (hit.collider.TryGetComponent<IDragObject>(out var dragObject))
                {
                    if (hit.collider.transform.position.y < minPositionY)
                    {
                        minPositionY = hit.collider.transform.position.y;
                        _currentDragObject = dragObject;
                    }
                }
            }

            if (_currentDragObject == null) return;

            _currentDragObject.OnDragStart();

            _offsetClick = _currentDragObject.Transform.position - worldPos;
            _offsetClick.z = 0;

            _onStartDrag?.OnNext(_currentDragObject);
            _dragDisposable = Observable.EveryUpdate().Subscribe(_ => UpdateDrag());
        }

        private void UpdateDrag()
        {
            if (_currentDragObject == null)
                return;

            UpdateCardPositions(Constants.DragSpeed, Constants.CardDragOffset);
        }

        private void EndDrag()
        {
            _dragDisposable?.Dispose();
            _dragDisposable = null;

            if (_currentDragObject != null)
            {
                UpdateCardPositions(Constants.MaxDragSpeed, Vector3.zero);

                _onEndDrag?.OnNext(_currentDragObject);
                _currentDragObject.OnDragEnd();
                _currentDragObject = null;
            }
        }

        private void RemoveCard(ICard card)
        {
            if (_currentDragObject == card)
            {
                _currentDragObject = null;
            }
        }

        private void UpdateCardPositions(float lerpSpeed, Vector3 offset)
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            var plane = new Plane(Vector3.forward, Vector3.zero);

            plane.Raycast(ray, out var distance);
            var worldPos = ray.GetPoint(distance);
            worldPos.z = 0;

            var targetPosition = worldPos + _offsetClick + offset;

            if (_currentDragObject is ICard card)
            {
                var stack = _cardStackSystem.GetStack(card);
                _cardStackMoveSystem.UpdateWorldPositions(stack, targetPosition, lerpSpeed);
            }
            else
            {
                MoveObject(_currentDragObject, targetPosition, lerpSpeed);
            }
        }

        private void MoveObject(IDragObject dragObject, Vector3 targetPosition, float lerpSpeed)
        {
            var transform = dragObject.Transform;

            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                Time.deltaTime * lerpSpeed
            );
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