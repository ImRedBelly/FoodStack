using TMPro;
using UnityEngine;

namespace Gameplay.Level.Handlers
{
    public class LevelTargetHandler : MonoBehaviour
    {
        [SerializeField] private TMP_Text _levelTargetText;

        public void SetLevelTarget(string levelTarget)
        {
            _levelTargetText.text = levelTarget;
        }
    }
}