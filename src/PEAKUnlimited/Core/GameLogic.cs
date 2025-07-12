// <copyright file="GameLogic.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PEAKUnlimited.Core
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;


    public static class GameLogic
    {
        public static bool ValidatePlayerCount(int playerCount, int vanillaMaxPlayers)
        {
            return playerCount > 0 && playerCount <= 30;
        }

        public static bool ValidateMaxPlayers(int maxPlayers)
        {
            return maxPlayers > 0 && maxPlayers <= 30;
        }

        public static bool ValidateCheatMarshmallows(int count)
        {
            return count >= 0 && count <= 30;
        }

        public static bool ValidateCheatBackpacks(int count)
        {
            return count >= 0 && count <= 10;
        }

        public static T[] ExpandArrayForExtraPlayers<T>(T[] originalArray, int newCount)
        {
            var newArray = new T[newCount];
            for (int i = 0; i < originalArray.Length && i < newCount; i++)
            {
                newArray[i] = originalArray[i];
            }

            return newArray;
        }

        public static int CalculateExtraMarshmallows(int currentPlayers, int vanillaMaxPlayers, int cheatMarshmallows)
        {
            if (cheatMarshmallows > 0)
            {
                return cheatMarshmallows - vanillaMaxPlayers;
            }

            return Math.Max(0, currentPlayers - vanillaMaxPlayers);
        }

        public static int CalculateExtraBackpacks(int currentPlayers, int vanillaMaxPlayers)
        {
            return CalculateExtraBackpacks(currentPlayers, vanillaMaxPlayers, true);
        }

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

        // Delegate for ground placement logic, to allow mocking in tests
        public delegate Vector3 SetToGroundDelegate(Vector3 vector);

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
