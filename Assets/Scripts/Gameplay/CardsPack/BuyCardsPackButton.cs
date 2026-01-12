using System;
using System.Collections.Generic;
using Core;
using Gameplay.CardsPack.Configs;
using Gameplay.CardsPack.Handlers;
using Gameplay.Types;
using Support;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Gameplay.CardsPack
{
    public class BuyCardsPackButton : DisposableBehaviour<BuyCardsPackButton.Model>
    {
        public IObservable<RecipeCategoryType> OnClick => _onClick;
        private readonly Subject<RecipeCategoryType> _onClick = new();

        public class Model
        {
            public readonly IReadOnlyCollection<CardPackConfig> CardPackConfigs;

            public Model(IReadOnlyCollection<CardPackConfig> cardPackConfigs)
            {
                CardPackConfigs = cardPackConfigs;
            }
        }

        [SerializeField] private RecipeCategoryType _recipeCategoryType;
        [SerializeField] private BuyCardsPackHandler _buyCardsPackHandler;
        [SerializeField] private TMP_Text _categoryTexts;
        [SerializeField] private ObservablePointerClickTrigger _clickTrigger;

        protected override void OnInit()
        {
            base.OnInit();

            _onClick.AddTo(Disposables);

            _clickTrigger
                .OnPointerClickAsObservable()
                .SafeSubscribe(_ => _onClick.OnNext(_recipeCategoryType))
                .AddTo(Disposables);


            foreach (var cardPackConfig in ActiveModel.CardPackConfigs)
            {
                if (cardPackConfig.RecipeCategoryType == _recipeCategoryType)
                {
                    _categoryTexts.SetText(cardPackConfig.Name);

                    break;
                }
            }
        }

        public void SetActiveCardPanel(bool state)
        {
            _buyCardsPackHandler.SetActiveCardPanel(state);
        }
    }
}