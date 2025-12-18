using System;
using UniRx;

namespace Core
{
    public class DisposableClass
    {
        protected CompositeDisposable Disposables;

        public IDisposable Init()
        {
            Disposables = new CompositeDisposable();

            OnInit();

            return Disposables;
        }

        protected virtual void OnInit()
        {
        }
    }
}