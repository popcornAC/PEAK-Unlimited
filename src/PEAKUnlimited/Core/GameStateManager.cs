// <copyright file="GameStateManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PEAKUnlimited.Core
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Manages game state including player counts and campfire tracking.
    /// </summary>
    public class GameStateManager
    {
        private readonly List<Campfire> campfireList = new List<Campfire>();
        private readonly Dictionary<Campfire, List<GameObject>> marshmallows = new Dictionary<Campfire, List<GameObject>>();
        private int numberOfPlayers = 1;
        private bool isAfterAwake = false;
        private const int VanillaMaxPlayers = 4;
        private bool isClimbing = false;
        private int lastMaxPlayers = VanillaMaxPlayers;

        /// <summary>
        /// Gets the current number of players.
        /// </summary>
        public int NumberOfPlayers => this.numberOfPlayers;

        /// <summary>
        /// Gets a value indicating whether gets whether the game is after the initial awake phase.
        /// </summary>
        public bool IsAfterAwake => this.isAfterAwake;

        /// <summary>
        /// Gets the vanilla maximum players.
        /// </summary>
        public int VanillaMaxPlayersValue => VanillaMaxPlayers;

        /// <summary>
        /// Gets the list of campfires.
        /// </summary>
        public IReadOnlyList<Campfire> CampfireList => this.campfireList;

        /// <summary>
        /// Gets the marshmallows dictionary.
        /// </summary>
        public IReadOnlyDictionary<Campfire, List<GameObject>> Marshmallows => this.marshmallows;

        /// <summary>
        /// Increments the player count.
        /// </summary>
        public void PlayerJoined()
        {
            this.numberOfPlayers++;
        }

        /// <summary>
        /// Decrements the player count.
        /// </summary>
        public void PlayerLeft()
        {
            this.numberOfPlayers--;
            if (this.numberOfPlayers < 0)
            {
                this.numberOfPlayers = 0;
            }
        }

        /// <summary>
        /// Sets the after awake flag.
        /// </summary>
        public void SetAfterAwake()
        {
            this.isAfterAwake = true;
        }

        /// <summary>
        /// Adds a campfire to the tracking list.
        /// </summary>
        /// <param name="campfire">The campfire to add.</param>
        public void AddCampfire(Campfire campfire)
        {
            if (!this.campfireList.Contains(campfire))
            {
                this.campfireList.Add(campfire);
            }
        }

        /// <summary>
        /// Adds marshmallows to a campfire.
        /// </summary>
        /// <param name="campfire">The campfire.</param>
        /// <param name="marshmallowObjects">The marshmallow game objects.</param>
        public void AddMarshmallowsToCampfire(Campfire campfire, List<GameObject> marshmallowObjects)
        {
            if (!this.marshmallows.ContainsKey(campfire))
            {
                this.marshmallows[campfire] = new List<GameObject>();
            }

            this.marshmallows[campfire].AddRange(marshmallowObjects);
        }

        /// <summary>
        /// Removes a marshmallow from a campfire.
        /// </summary>
        /// <param name="campfire">The campfire.</param>
        /// <param name="marshmallow">The marshmallow to remove.</param>
        public void RemoveMarshmallowFromCampfire(Campfire campfire, GameObject marshmallow)
        {
            if (this.marshmallows.ContainsKey(campfire))
            {
                this.marshmallows[campfire].Remove(marshmallow);
            }
        }

        /// <summary>
        /// Gets the number of extra players beyond vanilla maximum.
        /// </summary>
        /// <returns>The number of extra players.</returns>
        public int GetExtraPlayersCount()
        {
            return GameLogic.CalculateExtraMarshmallows(this.numberOfPlayers, VanillaMaxPlayers, 0);
        }

        /// <summary>
        /// Checks if there are extra players beyond vanilla maximum.
        /// </summary>
        /// <returns>True if there are extra players.</returns>
        public bool HasExtraPlayers()
        {
            return this.numberOfPlayers > VanillaMaxPlayers;
        }

        /// <summary>
        /// Resets the game state for a new game.
        /// </summary>
        public void Reset()
        {
            this.numberOfPlayers = 1;
            this.isAfterAwake = false;
            this.campfireList.Clear();
            this.marshmallows.Clear();
        }

        /// <summary>
        /// Gets a value indicating whether the player is currently climbing.
        /// </summary>
        public bool IsCurrentlyClimbing
        {
            get
            {
                var mainMenu = UnityEngine.Object.FindFirstObjectByType<MainMenu>();
                return mainMenu == null || !mainMenu.gameObject.activeInHierarchy;
            }
        }

        /// <summary>
        /// Updates the configuration.
        /// </summary>
        /// <param name="config">The new configuration.</param>
        public void UpdateConfiguration(ConfigurationManager.PluginConfig config)
        {
            // If max players changed, reset player count to 1 (or keep as is)
            if (config.MaxPlayers != lastMaxPlayers)
            {
                this.numberOfPlayers = 1;
                lastMaxPlayers = config.MaxPlayers;
            }
            // You can add more config-based state updates here as needed
        }

        /// <summary>
        /// Adds a player to the count.
        /// </summary>
        public void AddPlayer()
        {
            this.PlayerJoined();
        }

        /// <summary>
        /// Removes a player from the count.
        /// </summary>
        public void RemovePlayer()
        {
            this.PlayerLeft();
        }

        /// <summary>
        /// Deactivates an end screen.
        /// </summary>
        /// <param name="endScreen">The end screen to deactivate.</param>
        public void DeactivateEndScreen(EndScreen endScreen)
        {
            // Deactivate the end screen UI
            if (endScreen != null)
            {
                endScreen.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Deletes an end screen.
        /// </summary>
        /// <param name="endScreen">The end screen to delete.</param>
        public void DeleteEndScreen(EndScreen endScreen)
        {
            // Destroy the end screen UI
            if (endScreen != null)
            {
                UnityEngine.Object.Destroy(endScreen.gameObject);
            }
        }
    }
}
