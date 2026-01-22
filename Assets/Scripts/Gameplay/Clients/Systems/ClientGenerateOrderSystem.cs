using System.Collections.Generic;
using System.Linq;
using Core;
using Gameplay.Clients.Configs;
using Gameplay.Recipes.Configs;
using Gameplay.Recipes.Types;
using UnityEngine;

namespace Gameplay.Clients.Systems
{
    public class ClientGenerateOrderSystem : DisposableClass
    {
        private readonly IReadOnlyCollection<ClientConfig> _clientConfigs;
        private readonly IEnumerable<RecipeConfig> _recipeConfigs;
        private int _clientIndex;
        private int _recipeIndex;

        public ClientGenerateOrderSystem(IReadOnlyCollection<ClientConfig> clientConfigs,
            IReadOnlyCollection<RecipeConfig> recipeConfigs)
        {
            _clientConfigs = clientConfigs;
            _recipeConfigs = recipeConfigs.Where(IsAvailable);
            
            _clientIndex = Random.Range(0, _clientConfigs.Count);
            _recipeIndex = Random.Range(0, _recipeConfigs.Count());
        }

        public (ClientConfig clientConfig, RecipeConfig recipeConfig) GenerateOrderData()
        {
            var clientConfig = _clientConfigs.ElementAt(_clientIndex % _clientConfigs.Count);
            var recipeConfig = _recipeConfigs.ElementAt(_recipeIndex % _recipeConfigs.Count());
            _clientIndex++;
            _recipeIndex++;
            return (clientConfig, recipeConfig);
        }

        private bool IsAvailable(RecipeConfig config)
        {
            return config.RecipeType == RecipeType.Final && (config.PriceUnlock <= 0 || SaveUtility.IsRecipeUnlocked(config.Name));
        }
    }
}