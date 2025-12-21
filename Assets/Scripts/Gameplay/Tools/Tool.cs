using Core;
using Gameplay.Tools.Configs;
using Gameplay.Tools.Interfaces;
using UnityEngine;

namespace Gameplay.Tools
{
    public class Tool : DisposableBehaviour<Tool.Model>, ITool
    {
        public class Model
        {
            public readonly ToolConfig Config;

            public Model(ToolConfig config)
            {
                Config = config;
            }
        }

        public Transform Transform => transform;
        public Collider2D Collider => _collider2D;

        [SerializeField] private Collider2D _collider2D;
    }
}