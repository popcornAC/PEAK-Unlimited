// <copyright file="UnityItemService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PEAKUnlimited.Core.Services
{
    using PEAKUnlimited.Core.Interfaces;
    using UnityEngine;
    using Zorro.Core;
    using System.Collections.Generic;

    /// <summary>
    /// Unity item service implementation.
    /// </summary>
    public class UnityItemService : IItemService
    {
        /// <summary>
        /// Gets an item by its ID.
        /// </summary>
        /// <param name="itemId">The item ID.</param>
        /// <returns>The item.</returns>
        public Item GetItem(int itemId)
        {
            return SingletonAsset<ItemDatabase>.Instance.itemLookup[(ushort)itemId];
        }

        /// <summary>
        /// Spawns extra backpacks at the specified position.
        /// </summary>
        /// <param name="position">The position to spawn backpacks at.</param>
        /// <param name="segment">The segment to spawn backpacks in.</param>
        public void SpawnExtraBackpacks(Vector3 position, Segment segment)
        {
            // Use the original logic for spawning extra backpacks
            var plugin = Plugin.currentInstance;
            var gameState = plugin.gameStateManager;
            var config = plugin.pluginConfig;
            int number = 0;
            if (config.CheatExtraBackpacks > 0 && config.CheatExtraBackpacks <= 10)
            {
                number = config.CheatExtraBackpacks - 1; // Minus one as there is already a backpack present
            }
            else
            {
                number = GameLogic.CalculateExtraBackpacks(gameState.NumberOfPlayers, gameState.VanillaMaxPlayersValue);
            }
            if (number > 0)
            {
                Item backpack = GetItem(6); // Backpack item ID
                for (int i = 0; i < number; i++)
                {
                    Vector3 spawnPos = Plugin.GetEvenlySpacedPointsAroundCampfire(number, 3.3f, 3.7f, position, segment)[i];
                    // If on the beach, spawn above ground
                    if (segment != null && segment.ToString().ToLower().Contains("wings"))
                    {
                        spawnPos += new Vector3(0, 10f, 0f);
                    }
                    var obj = Photon.Pun.PhotonNetwork.Instantiate("0_Items/" + backpack.name, spawnPos, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f));
                    obj.transform.parent = null;
                }
            }
        }

        /// <summary>
        /// Spawns extra marshmallows at the specified position.
        /// </summary>
        /// <param name="position">The position to spawn marshmallows at.</param>
        /// <param name="segment">The segment to spawn marshmallows in.</param>
        public void SpawnExtraMarshmallows(Vector3 position, Segment segment)
        {
            // Use the original logic for spawning extra marshmallows
            var plugin = Plugin.currentInstance;
            var gameState = plugin.gameStateManager;
            var config = plugin.pluginConfig;
            int amountToSpawn = 0;
            if (config.CheatExtraMarshmallows > 0)
            {
                amountToSpawn = config.CheatExtraMarshmallows - gameState.VanillaMaxPlayersValue;
                if (gameState.NumberOfPlayers < gameState.VanillaMaxPlayersValue)
                {
                    amountToSpawn = config.CheatExtraMarshmallows - gameState.NumberOfPlayers;
                }
            }
            else
            {
                amountToSpawn = gameState.GetExtraPlayersCount();
            }
            if (amountToSpawn > 0)
            {
                Item marshmallow = GetItem(46); // Marshmallow item ID
                var marshmallows = new List<GameObject>();
                foreach (Vector3 spawnPos in Plugin.GetEvenlySpacedPointsAroundCampfire(amountToSpawn, 2.5f, 3f, position, segment))
                {
                    var obj = Photon.Pun.PhotonNetwork.Instantiate("0_Items/" + marshmallow.name, spawnPos, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f));
                    marshmallows.Add(obj);
                }
                // Add to game state for tracking
                var campfire = UnityEngine.Object.FindFirstObjectByType<Campfire>();
                if (campfire != null)
                {
                    gameState.AddMarshmallowsToCampfire(campfire, marshmallows);
                }
            }
        }

        /// <summary>
        /// Spawns a late join marshmallow.
        /// </summary>
        public void SpawnLateJoinMarshmallow()
        {
            // For each campfire, spawn a marshmallow for the new player
            var plugin = Plugin.currentInstance;
            var gameState = plugin.gameStateManager;
            foreach (var campfire in gameState.CampfireList)
            {
                Item marshmallow = GetItem(46);
                Vector3 position = campfire.transform.position;
                var marshmallowObj = Photon.Pun.PhotonNetwork.Instantiate("0_Items/" + marshmallow.name, position, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f));
                gameState.AddMarshmallowsToCampfire(campfire, new List<GameObject> { marshmallowObj });
            }
        }

        /// <summary>
        /// Removes a late join marshmallow.
        /// </summary>
        public void RemoveLateJoinMarshmallow()
        {
            // For each campfire, remove a marshmallow for the player who left
            var plugin = Plugin.currentInstance;
            var gameState = plugin.gameStateManager;
            foreach (var campfire in gameState.CampfireList)
            {
                if (gameState.Marshmallows.TryGetValue(campfire, out var marshmallowList) && marshmallowList.Count > 0)
                {
                    var marshmallow = marshmallowList[0];
                    plugin.networkService.Destroy(marshmallow);
                    gameState.RemoveMarshmallowFromCampfire(campfire, marshmallow);
                }
            }
        }
    }
}
