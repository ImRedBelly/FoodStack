using UnityEngine;

namespace Gameplay.Cards
{
    public class CardExample : BaseCard
    {
        public override void OnDragStart()
        {
            base.OnDragStart();
            transform.localScale = Vector3.one * 1.1f;
        }

        public override void OnDragEnd()
        {
            base.OnDragEnd();
            transform.localScale = Vector3.one;
        }
    }
}