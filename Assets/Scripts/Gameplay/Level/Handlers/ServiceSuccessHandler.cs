using DG.Tweening;
using UnityEngine;

namespace Gameplay.Level.Handlers
{
    public class ServiceSuccessHandler : MonoBehaviour
    {
        [SerializeField] private Transform _view;

        public void ShowSuccess()
        {
            DOTween.Sequence()
                .Append(_view.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack))
                .AppendInterval(0.5f)
                .Append(_view.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack))
                .SetLink(gameObject, LinkBehaviour.KillOnDisable);
        }

        public void Init()
        {
            _view.localScale = Vector3.zero;
        }
    }
}