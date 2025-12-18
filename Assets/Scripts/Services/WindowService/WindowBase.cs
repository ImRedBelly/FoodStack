using System;
using Support;
using UniRx;
using UnityEngine;

namespace Services.WindowService
{
    public abstract class WindowBase<T> : WindowBase
    {
        public sealed override Type ModelType => typeof(T);
        protected T ActiveModel { get; private set; }

        protected readonly CompositeDisposable Disposables = new();

        private readonly Subject<Unit> _onCloseSubject = new();
        private readonly Subject<Unit> _onOpenSubject = new();

        public sealed override void Open(object model)
        {
            gameObject.SetActive(true);
            ActiveModel = (T)model;

            OnOpen();
            
            _onOpenSubject.SafeSubscribe(_ => {}).AddTo(this);
        }

        public sealed override void Close()
        {
            Disposables.Clear();
    
            OnClose();
            
            ActiveModel = default;
            gameObject.SetActive(false);
        }

        protected virtual void OnOpen()
        {
            _onOpenSubject.OnNext(Unit.Default);
        }

        protected virtual void OnClose()
        {
            _onCloseSubject.OnNext(Unit.Default);
        }
    }

    public abstract class WindowBase : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;

        public abstract Type ModelType { get; }

        public void SetOrder(int order)
        {
            _canvas.sortingOrder = order;
        }

        public abstract void Open(object model);
        public abstract void Close();
    }
}