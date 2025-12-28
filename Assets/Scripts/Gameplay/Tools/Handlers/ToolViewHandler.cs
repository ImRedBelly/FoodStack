using UnityEngine;

namespace Gameplay.Tools.Handlers
{
    public class ToolViewHandler : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _viewCard;
        [SerializeField] private SpriteRenderer _viewSlider;
        [SerializeField] private SpriteRenderer _eligibleFrame;

        public void Initialize(Sprite sprite)
        {
            _viewCard.sprite = sprite;

            SetStateEligibleFrame(false);
            SetStateSlider(false);
        }

        public void SetStateEligibleFrame(bool active)
        {
            _eligibleFrame.gameObject.SetActive(active);
        }

        public void SetStateSlider(bool active)
        {
            _viewSlider.transform.parent.gameObject.SetActive(active);
        }

        public void SetProgress(float progress)
        {
            _viewSlider.size = new Vector2(Constants.MaxSliderValue * progress, _viewSlider.size.y);
        }
    }
}