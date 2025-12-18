using UnityEngine;

namespace Support
{
    public class SafeArea : MonoBehaviour
    {
        private RectTransform safeAreaTransform;

        private void Awake()
        {
            safeAreaTransform = GetComponent<RectTransform>();
        }

        public void Fit()
        {
            var safeArea = Screen.safeArea;

            var anchorMin = safeArea.position / new Vector2(Screen.width, Screen.height);
            var anchorMax = (safeArea.position + safeArea.size) / new Vector2(Screen.width, Screen.height);

            safeAreaTransform.anchorMin = anchorMin;
            safeAreaTransform.anchorMax = anchorMax;
        }
    }
}