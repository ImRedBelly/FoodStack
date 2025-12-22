using System;
using Gameplay.Recipes.Configs;
using UnityEngine;

namespace Gameplay.Tools.Interfaces
{
    public interface IToolCard : IDisposable
    {
        Transform Transform { get; }
        Collider2D Collider { get; }
        RecipeConfig[] RecipeConfigs { get; }
        void SetStateEligibleFrame(bool state);
        void SetStateSlider(bool state);
        void SetProgress(float progress);
    }
}