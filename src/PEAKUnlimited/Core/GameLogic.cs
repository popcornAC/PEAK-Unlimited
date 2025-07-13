// <copyright file="GameLogic.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PEAKUnlimited.Core
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Contains game logic for player validation and calculations.
    /// </summary>
    public static class GameLogic
    {
        /// <summary>
        /// Validates the player count against the vanilla maximum.
        /// </summary>
        /// <param name="playerCount">The current player count.</param>
        /// <param name="vanillaMaxPlayers">The vanilla maximum players.</param>
        /// <returns>True if the player count is valid.</returns>
        public static bool ValidatePlayerCount(int playerCount, int vanillaMaxPlayers)
        {
            return playerCount > 0 && playerCount <= 30;
        }

        /// <summary>
        /// Validates the maximum players setting.
        /// </summary>
        /// <param name="maxPlayers">The maximum players value.</param>
        /// <returns>True if the maximum players is valid.</returns>
        public static bool ValidateMaxPlayers(int maxPlayers)
        {
            return maxPlayers > 0 && maxPlayers <= 30;
        }

        /// <summary>
        /// Validates the cheat marshmallows count.
        /// </summary>
        /// <param name="count">The cheat marshmallows count.</param>
        /// <returns>True if the count is valid.</returns>
        public static bool ValidateCheatMarshmallows(int count)
        {
            return count >= 0 && count <= 30;
        }

        /// <summary>
        /// Validates the cheat backpacks count.
        /// </summary>
        /// <param name="count">The cheat backpacks count.</param>
        /// <returns>True if the count is valid.</returns>
        public static bool ValidateCheatBackpacks(int count)
        {
            return count >= 0 && count <= 10;
        }

        /// <summary>
        /// Expands an array to accommodate extra players.
        /// </summary>
        /// <typeparam name="T">The type of array elements.</typeparam>
        /// <param name="originalArray">The original array.</param>
        /// <param name="newCount">The new count.</param>
        /// <returns>The expanded array.</returns>
        public static T[] ExpandArrayForExtraPlayers<T>(T[] originalArray, int newCount)
        {
            var newArray = new T[newCount];
            for (int i = 0; i < originalArray.Length && i < newCount; i++)
            {
                newArray[i] = originalArray[i];
            }

            return newArray;
        }

        /// <summary>
        /// Calculates the number of extra marshmallows needed.
        /// </summary>
        /// <param name="currentPlayers">The current number of players.</param>
        /// <param name="vanillaMaxPlayers">The vanilla maximum players.</param>
        /// <param name="cheatMarshmallows">The cheat marshmallows setting.</param>
        /// <returns>The number of extra marshmallows.</returns>
        public static int CalculateExtraMarshmallows(int currentPlayers, int vanillaMaxPlayers, int cheatMarshmallows)
        {
            if (cheatMarshmallows > 0)
            {
                return cheatMarshmallows - vanillaMaxPlayers;
            }

            return Math.Max(0, currentPlayers - vanillaMaxPlayers);
        }

        /// <summary>
        /// Calculates the number of extra backpacks needed.
        /// </summary>
        /// <param name="currentPlayers">The current number of players.</param>
        /// <param name="vanillaMaxPlayers">The vanilla maximum players.</param>
        /// <returns>The number of extra backpacks.</returns>
        public static int CalculateExtraBackpacks(int currentPlayers, int vanillaMaxPlayers)
        {
            return CalculateExtraBackpacks(currentPlayers, vanillaMaxPlayers, true);
        }

        /// <summary>
        /// Calculates the number of extra backpacks needed with optional randomness.
        /// </summary>
        /// <param name="currentPlayers">The current number of players.</param>
        /// <param name="vanillaMaxPlayers">The vanilla maximum players.</param>
        /// <param name="useRandom">Whether to use random calculation.</param>
        /// <returns>The number of extra backpacks.</returns>
        public static int CalculateExtraBackpacks(int currentPlayers, int vanillaMaxPlayers, bool useRandom)
        {
            int extraPlayers = currentPlayers - vanillaMaxPlayers;
            if (extraPlayers <= 0)
            {
                return 0;
            }

            double backpackNumber = extraPlayers * 0.25;

            if (backpackNumber % 4 == 0)
            {
                return (int)backpackNumber;
            }
            else
            {
                int baseNumber = (int)backpackNumber;
                if (useRandom)
                {
                    var random = new System.Random();
                    if (random.NextDouble() <= backpackNumber - baseNumber)
                    {
                        return baseNumber + 1;
                    }
                }
                else
                {
                    if ((backpackNumber - baseNumber) >= 0.5)
                    {
                        return baseNumber + 1;
                    }
                }

                return baseNumber;
            }
        }

        /// <summary>
        /// Delegate for ground placement logic, to allow mocking in tests.
        /// </summary>
        /// <param name="vector">The position vector.</param>
        /// <returns>The ground position.</returns>
        public delegate Vector3 SetToGroundDelegate(Vector3 vector);

        /// <summary>
        /// Gets evenly spaced points around a campfire.
        /// </summary>
        /// <param name="numPoints">The number of points to generate.</param>
        /// <param name="innerRadius">The inner radius.</param>
        /// <param name="outerRadius">The outer radius.</param>
        /// <param name="campfirePosition">The campfire position.</param>
        /// <param name="setToGround">The ground placement delegate.</param>
        /// <returns>List of evenly spaced points.</returns>
        public static List<Vector3> GetEvenlySpacedPointsAroundCampfire(
            int numPoints,
            float innerRadius,
            float outerRadius,
            Vector3 campfirePosition,
            SetToGroundDelegate setToGround)
        {
            List<Vector3> points = new List<Vector3>();
            for (int i = 0; i < numPoints; i++)
            {
                float radius = (i % 2 == 0) ? innerRadius : outerRadius;
                float angle = i * Mathf.PI * 2f / numPoints;
                float x = radius * Mathf.Cos(angle);
                float z = radius * Mathf.Sin(angle);
                points.Add(setToGround(new Vector3(x, 0f, z) + campfirePosition));
            }

            return points;
        }
    }
}
