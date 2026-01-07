using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Level.Handlers
{
    public class DaySliderHandler : MonoBehaviour
    {
        [SerializeField] private Image _sliderBlueImage, _sliderRedImage;

        public void SetProgress(float progress)
        {
            _sliderBlueImage.fillAmount = progress;
            _sliderRedImage.fillAmount = progress;
        }

        public void SetErrorStateView(bool errorState)
        {
            _sliderBlueImage.enabled = !errorState;
            _sliderRedImage.enabled = errorState;
        }
    }
}