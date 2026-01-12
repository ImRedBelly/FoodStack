using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Gameplay.Cards.Configs;
using Gameplay.Cards.Factory;
using Gameplay.CardsPack.Configs;
using Support;
using UniRx;
using UnityEngine;

namespace Gameplay.CardsPack.Systems
{
    public class OpenCardPackSystem : DisposableClass
    {
        private readonly CardPackFactory _cardPackFactory;
        private readonly CardFactory _cardFactory;

        private Dictionary<CardPack, int> _cardPacks = new();
        private Dictionary<CardConfig, int> _generatedCardConfigs = new();

        private readonly int _value = 3;

        public OpenCardPackSystem(CardPackFactory cardPackFactory, CardFactory cardFactory)
        {
            _cardPackFactory = cardPackFactory;
            _cardFactory = cardFactory;
        }

        protected override void OnInit()
        {
            base.OnInit();

            _cardPackFactory.OnCardPackCreated
                .SafeSubscribe(AddCardPack)
                .AddTo(Disposables);
        }

        private void AddCardPack(CardPack cardPack)
        {
            cardPack.OnClick
                .SafeSubscribe(OpenCardPack)
                .AddTo(Disposables);

            _cardPacks.Add(cardPack, 5);
            cardPack.UpdateCountText(_cardPacks[cardPack].ToString());
        }

        private void OpenCardPack(CardPack cardPack)
        {
            if (_cardPacks.TryGetValue(cardPack, out int count))
            {
                _cardPacks[cardPack]--;
                _cardFactory.CreateCard(CalculateCreateCard(cardPack.CardPackConfig), Vector3.zero);

                if (_cardPacks[cardPack] == 0)
                {
                    _cardPackFactory.RemoveCardPack(cardPack);
                    _cardPacks.Remove(cardPack);
                }
                else
                {
                    cardPack.UpdateCountText(_cardPacks[cardPack].ToString());
                }
            }
        }

        // private CardConfig CalculateCreateCard(CardPackConfig cardPackConfig)
        // {
        //     if (cardPackConfig.CardPackGenerateData == null || cardPackConfig.CardPackGenerateData.Count == 0)
        //         throw new InvalidOperationException("CardPackGenerateData пустой.");
        //
        //     var data = cardPackConfig.CardPackGenerateData
        //         .Where(d => d.Percent > 0f && d.CardConfig != null)
        //         .ToList();
        //
        //     if (data.Count == 0)
        //         throw new InvalidOperationException("Нет элементов с Percent > 0 и валидным CardConfig.");
        //
        //     var counts = new List<(CardConfig cfg, int count)>(data.Count);
        //     foreach (var d in data)
        //     {
        //         _generatedCardConfigs.TryGetValue(d.CardConfig, out var c);
        //         counts.Add((d.CardConfig, c));
        //     }
        //
        //     counts.Sort((a, b) => a.count.CompareTo(b.count));
        //
        //     if (counts.Count >= 2)
        //     {
        //         int minCount = counts[0].count;
        //         int secondMinCount = counts[1].count;
        //
        //         if (secondMinCount - minCount >= _value)
        //         {
        //             var minGroup = counts
        //                 .Where(t => t.count == minCount)
        //                 .Select(t => t.cfg)
        //                 .Distinct()
        //                 .ToList();
        //
        //             var forced = minGroup[UnityEngine.Random.Range(0, minGroup.Count)];
        //
        //             if (!_generatedCardConfigs.TryAdd(forced, 1))
        //                 _generatedCardConfigs[forced]++;
        //
        //             return forced;
        //         }
        //     }
        //
        //     float sum = data.Sum(d => d.Percent);
        //     if (sum <= 0f)
        //         throw new InvalidOperationException("Сумма весов (Percent) должна быть > 0.");
        //
        //     float r = UnityEngine.Random.Range(0f, sum);
        //     float cumulative = 0f;
        //
        //     CardConfig selected = null;
        //     for (int i = 0; i < data.Count; i++)
        //     {
        //         cumulative += data[i].Percent;
        //         if (r < cumulative)
        //         {
        //             selected = data[i].CardConfig;
        //             break;
        //         }
        //     }
        //
        //     if (selected == null)
        //         selected = data[^1].CardConfig;
        //
        //     if (!_generatedCardConfigs.TryAdd(selected, 1))
        //         _generatedCardConfigs[selected]++;
        //
        //     return selected;
        // }
        
        private CardConfig CalculateCreateCard(CardPackConfig cardPackConfig)
        {
            if (cardPackConfig.CardPackGenerateData == null || cardPackConfig.CardPackGenerateData.Count == 0)
                throw new InvalidOperationException("CardPackGenerateData пустой.");

            
            var data = cardPackConfig.CardPackGenerateData
                .Where(x => x.Percent > 0f && x.CardConfig != null)
                .ToList();

            if (data.Count == 0)
                throw new InvalidOperationException("Нет элементов с Percent > 0 и валидным CardConfig.");

            float sum = data.Sum(x => x.Percent);

            if (sum <= 0f)
                throw new InvalidOperationException("Сумма весов (Percent) должна быть > 0.");

            float r = UnityEngine.Random.Range(0f, sum);

            float cumulative = 0f;
            CardConfig selected = null;

            for (int i = 0; i < data.Count; i++)
            {
                cumulative += data[i].Percent;
                if (r < cumulative)
                {
                    selected = data[i].CardConfig;
                    break;
                }
            }

            if (selected == null)
                selected = data[^1].CardConfig;

            if (!_generatedCardConfigs.TryAdd(selected, 1))
                _generatedCardConfigs[selected]++;

            return selected;
        }
    }
}