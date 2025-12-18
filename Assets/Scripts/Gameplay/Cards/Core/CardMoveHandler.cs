using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay.Cards.Core
{
    [RequireComponent(typeof(ObservablePointerDownTrigger))]
    [RequireComponent(typeof(ObservablePointerUpTrigger))]
    public class CardMoveHandler : MonoBehaviour
    {
        private Action _dragStarted;
        private Action _dragEnded;

        private Camera _camera;
        private IDisposable _dragSubscription;
        private Vector3 _dragOffset;

        public void Initialize(Camera inCamera, CompositeDisposable disposables, Action dragStarted, Action dragEnded)
        {
            _camera = inCamera;
            _dragStarted = dragStarted;
            _dragEnded = dragEnded;

            GetComponent<ObservablePointerDownTrigger>()
                .OnPointerDownAsObservable()
                .Subscribe(OnPointerDown)
                .AddTo(disposables);

            GetComponent<ObservablePointerUpTrigger>()
                .OnPointerUpAsObservable()
                .Subscribe(_ => EndDrag())
                .AddTo(disposables);
        }


        private void OnPointerDown(PointerEventData eventData)
        {
            _dragSubscription = Observable.EveryUpdate()
                .Subscribe(_ => Drag())
                .AddTo(this);
            
            var worldPointerPos = ScreenToWorldOnPlane(eventData.position);
            _dragOffset = transform.position - worldPointerPos;

            _dragStarted?.Invoke();
        }

        private void Drag()
        {
            var worldPointerPos = ScreenToWorldOnPlane(Input.mousePosition);
            transform.position = worldPointerPos + _dragOffset;
        }

        private void EndDrag()
        {
            _dragSubscription?.Dispose();
            _dragSubscription = null;
            _dragEnded?.Invoke();
        }

        private Vector3 ScreenToWorldOnPlane(Vector2 screenPos)
        {
            var ray = _camera.ScreenPointToRay(screenPos);

            var plane = new Plane(Vector3.forward, new Vector3(0, 0, 0));
            plane.Raycast(ray, out var distance);

            return ray.GetPoint(distance);
        }
    }
}