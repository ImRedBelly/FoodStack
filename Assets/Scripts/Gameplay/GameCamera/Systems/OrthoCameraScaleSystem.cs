using Core;
using UniRx;
using UnityEngine;

namespace Gameplay.GameCamera.Systems
{
    public enum MatchMode
    {
        MatchWidth,
        MatchHeight
    }

    public class OrthoCameraScaleSystem : DisposableClass
    {
        private readonly Vector2 _referenceResolution = new(1080, 1920);
        private readonly float _referenceOrthoSize = 5f;
        private readonly MatchMode _match = MatchMode.MatchWidth;
        private readonly bool _autoUpdate = true;

        private readonly Camera _camera;


        public OrthoCameraScaleSystem(Camera camera)
        {
            _camera = camera;
        }

        protected override void OnInit()
        {
            base.OnInit();
            Observable.EveryUpdate()
                .Subscribe(_ => UpdateCameraScale())
                .AddTo(Disposables);
        }

        private void UpdateCameraScale()
        {
            if (!_autoUpdate) return;
            if (_camera == null) return;

            if (!_camera.orthographic)
            {
                Debug.LogWarning($"{nameof(OrthoCameraScaleSystem)}: Camera is not orthographic.");
                return;
            }

            float refAspect = _referenceResolution.x / _referenceResolution.y;
            float currentAspect = (float)Screen.width / Screen.height;


            float newSize = _referenceOrthoSize;

            switch (_match)
            {
                case MatchMode.MatchWidth:
                    newSize = _referenceOrthoSize * (refAspect / currentAspect);
                    break;

                case MatchMode.MatchHeight:
                    newSize = _referenceOrthoSize;
                    break;
            }

            newSize = Mathf.Max(0.01f, newSize);

            if (!Mathf.Approximately(_camera.orthographicSize, newSize))
                _camera.orthographicSize = newSize;
        }
    }
}