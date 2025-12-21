using Core;
using Gameplay.Recipes.Configs;
using Gameplay.Tools.Configs;
using Gameplay.Tools.Handlers;
using Gameplay.Tools.Interfaces;
using UnityEngine;

namespace Gameplay.Tools
{
    public class ToolCard : DisposableBehaviour<ToolCard.Model>, IToolCard
    {
        public class Model
        {
            public readonly ToolConfig ToolConfig;

            public Model(ToolConfig toolConfig)
            {
                ToolConfig = toolConfig;
            }
        }

        public Transform Transform => transform;
        public Collider2D Collider => _collider2D;
        public RecipeConfig[] RecipeConfigs => ActiveModel.ToolConfig.Recipes;

        [SerializeField] private Collider2D _collider2D;
        [SerializeField] private ToolViewHandler _toolViewHandler;

        protected override void OnInit()
        {
            base.OnInit();

            _toolViewHandler.Initialize(ActiveModel.ToolConfig.Sprite, Constants.DefaultSortingOrder);
        }

        public void SetStateEligibleFrame(bool state)
        {
            _toolViewHandler.SetStateEligibleFrame(state);
        }


        public void SetStateSlider(bool state)
        {
            _toolViewHandler.SetStateSlider(state);
        }

        public void SetProgress(float progress)
        {
            _toolViewHandler.SetProgress(progress);
        }
    }
}