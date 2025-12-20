using System.Collections.Generic;
using Gameplay.Cards.Interfaces;
using UnityEngine;

namespace Services.Cards
{
    public class CardStack
    {
        public readonly List<ICard> Cards = new();

        private const float BaseSpeed = 2500f;

        public void UpdateWorldPositions() => UpdateWorldPositions(Cards[0].Transform.position, BaseSpeed);

        public void UpdateWorldPositions(Vector3 basePos, float lerpSpeed)
        {
            for (int i = 0; i < Cards.Count; i++)
            {
                float indexFactor = 1f / (i + 1f);

                var targetPosition = basePos - Vector3.up * (i * 0.2f);
                var transform = Cards[i].Transform;

                transform.position = Vector3.Lerp(
                    transform.position,
                    targetPosition,
                    Time.deltaTime * lerpSpeed * indexFactor
                );
            }
        }
    }
}