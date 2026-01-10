using System;
using System.Collections.Generic;
using Core;
using Gameplay.OrderButton.Interfaces;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gameplay.OrderButton.Services
{
    public class OrderButtonClickSystem : DisposableClass
    {
        public IObservable<IOrderButton> OnClickOrderButton => _onClickOrderButton;

        private readonly Subject<IOrderButton> _onClickOrderButton = new();

        private readonly List<RaycastResult> _uiRaycastResults = new();

        private readonly Camera _camera;
        private readonly LayerMask _buttonLayer;
        private readonly GraphicRaycaster _graphicRaycaster;
        private readonly EventSystem _eventSystem;


        public OrderButtonClickSystem(Camera camera, LayerMask buttonLayer, GraphicRaycaster graphicRaycaster)
        {
            _camera = camera;
            _buttonLayer = buttonLayer;
            _graphicRaycaster = graphicRaycaster;
            _eventSystem = EventSystem.current;
        }

        protected override void OnInit()
        {
            base.OnInit();
            _onClickOrderButton.AddTo(Disposables);

            Observable.EveryUpdate()
                .Where(_ => Input.GetMouseButtonDown(0))
                .Subscribe(_ => TryBeginDrag())
                .AddTo(Disposables);
        }

        private void TryBeginDrag()
        {
            if (IsPointerOverUI())
                return;

            var worldPos = _camera.ScreenToWorldPoint(Input.mousePosition);

            var allCards = Physics2D.RaycastAll(worldPos, Vector2.zero, 0f, _buttonLayer);

            float minPositionY = float.MaxValue;
            foreach (var hit in allCards)
            {
                if (hit.collider.TryGetComponent<IOrderButton>(out var orderButton))
                {
                    if (hit.collider.transform.position.y < minPositionY)
                    {
                        minPositionY = hit.collider.transform.position.y;
                         _onClickOrderButton?.OnNext(orderButton);
                    }
                }
            }
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