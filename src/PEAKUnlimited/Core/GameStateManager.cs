using System.Collections.Generic;
using UnityEngine;

namespace PEAKUnlimited.Core
{
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

        /// <summary>
        /// Gets the current number of players.
        /// </summary>
        public int NumberOfPlayers => numberOfPlayers;

        /// <summary>
        /// Gets whether the game is after the initial awake phase.
        /// </summary>
        public bool IsAfterAwake => isAfterAwake;

        /// <summary>
        /// Gets the vanilla maximum players.
        /// </summary>
        public int VanillaMaxPlayersValue => VanillaMaxPlayers;

        /// <summary>
        /// Gets the list of campfires.
        /// </summary>
        public IReadOnlyList<Campfire> CampfireList => campfireList;

        /// <summary>
        /// Gets the marshmallows dictionary.
        /// </summary>
        public IReadOnlyDictionary<Campfire, List<GameObject>> Marshmallows => marshmallows;

        /// <summary>
        /// Increments the player count.
        /// </summary>
        public void PlayerJoined()
        {
            numberOfPlayers++;
        }

        /// <summary>
        /// Decrements the player count.
        /// </summary>
        public void PlayerLeft()
        {
            numberOfPlayers--;
            if (numberOfPlayers < 0)
            {
                numberOfPlayers = 0;
            }
        }

        /// <summary>
        /// Sets the after awake flag.
        /// </summary>
        public void SetAfterAwake()
        {
            isAfterAwake = true;
        }

        /// <summary>
        /// Adds a campfire to the tracking list.
        /// </summary>
        /// <param name="campfire">The campfire to add.</param>
        public void AddCampfire(Campfire campfire)
        {
            if (!campfireList.Contains(campfire))
            {
                campfireList.Add(campfire);
            }
        }

        /// <summary>
        /// Adds marshmallows to a campfire.
        /// </summary>
        /// <param name="campfire">The campfire.</param>
        /// <param name="marshmallowObjects">The marshmallow game objects.</param>
        public void AddMarshmallowsToCampfire(Campfire campfire, List<GameObject> marshmallowObjects)
        {
            if (!marshmallows.ContainsKey(campfire))
            {
                marshmallows[campfire] = new List<GameObject>();
            }
            
            marshmallows[campfire].AddRange(marshmallowObjects);
        }

        /// <summary>
        /// Removes a marshmallow from a campfire.
        /// </summary>
        /// <param name="campfire">The campfire.</param>
        /// <param name="marshmallow">The marshmallow to remove.</param>
        public void RemoveMarshmallowFromCampfire(Campfire campfire, GameObject marshmallow)
        {
            if (marshmallows.ContainsKey(campfire))
            {
                marshmallows[campfire].Remove(marshmallow);
            }
        }

        /// <summary>
        /// Gets the number of extra players beyond vanilla maximum.
        /// </summary>
        /// <returns>The number of extra players.</returns>
        public int GetExtraPlayersCount()
        {
            return GameLogic.CalculateExtraMarshmallows(numberOfPlayers, VanillaMaxPlayers, 0);
        }

        /// <summary>
        /// Checks if there are extra players beyond vanilla maximum.
        /// </summary>
        /// <returns>True if there are extra players.</returns>
        public bool HasExtraPlayers()
        {
            return numberOfPlayers > VanillaMaxPlayers;
        }

        /// <summary>
        /// Resets the game state for a new game.
        /// </summary>
        public void Reset()
        {
            numberOfPlayers = 1;
            isAfterAwake = false;
            campfireList.Clear();
            marshmallows.Clear();
        }
    }
} 