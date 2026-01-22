using UnityEngine;

namespace Support.GraphicsGroup
{
    public class GraphicsSwitchState : GraphicsSwitchElement
    {
        [SerializeField] private bool _state;

        public override void Execute()
        {
            gameObject.SetActive(_state);
        }
    }
}