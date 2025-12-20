using System.Collections.Generic;
using Gameplay.Cards.Interfaces;
using UnityEngine;

namespace Services.Cards
{
    public class CardStack
    {
        public readonly List<ICard> Cards = new();

        public void UpdateWorldPositions() => 
            UpdateWorldPositions(Cards[0].Transform.position);

        public void UpdateWorldPositions(Vector3 basePos)
        {
            for (int i = 0; i < Cards.Count; i++)
            {
                Cards[i].Transform.position = basePos - Vector3.up * (i * 0.2f);
            }
        }
    }
}