using Gameplay.Cards.Interfaces;
using UnityEngine;

namespace Gameplay.Cards.Systems
{
    public class CardStackMoveSystem
    {
        public void UpdateWorldPositions(CardStack stacks)
        {
            UpdateWorldPositions(stacks, stacks.Cards[0].Container.position, Constants.MaxDragSpeed);
        }

        public void UpdateWorldPositions(CardStack stacks, Vector3 basePos, float lerpSpeed)
        {
            for (int i = 0; i < stacks.Cards.Count; i++)
            {
                float indexFactor = 1f / (i + 1f);

                var targetPosition = basePos - Vector3.up * (i * 0.2f);
                var transform = stacks.Cards[i].Transform;

                transform.position = Vector3.Lerp(
                    transform.position,
                    targetPosition,
                    Time.deltaTime * lerpSpeed * indexFactor
                );
            }
        }

        public void UpdateWorldPositions(CardStack stacks, IIngredientCard card, Vector3 basePos, float lerpSpeed)
        {
            for (int i = 0; i < stacks.Cards.Count; i++)
            {
                if (stacks.Cards[i] == card)
                {
                    basePos += Vector3.up * (i * 0.2f);
                    break;
                }
            }

            for (int i = 0; i < stacks.Cards.Count; i++)
            {
                float indexFactor = 1f / (i + 1f);

                var targetPosition = basePos - Vector3.up * (i * 0.2f);
                var transform = stacks.Cards[i].Transform;

                transform.position = Vector3.Lerp(
                    transform.position,
                    targetPosition,
                    Time.deltaTime * lerpSpeed * indexFactor
                );
            }
        }
    }
}