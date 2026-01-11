using TMPro;
using UniRx;
using UnityEngine;

namespace UI.Counters
{
    public class StarsCounter : MonoBehaviour
    {
        [SerializeField] private TMP_Text _textMoney;

        private void Start()
        {
            SaveUtility.Stars
                .Subscribe(x => _textMoney.SetText(x.ToString()))
                .AddTo(this);
        }
    }
}