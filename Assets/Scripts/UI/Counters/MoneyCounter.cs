using TMPro;
using UniRx;
using UnityEngine;

namespace UI.Counters
{
    public class MoneyCounter : MonoBehaviour
    {
        [SerializeField] private TMP_Text _textMoney;

        private void Start()
        {
            SaveUtility.Money
                .Subscribe(x => _textMoney.SetText(x.ToString()))
                .AddTo(this);
        }
    }
}