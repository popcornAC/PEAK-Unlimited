// <copyright file="PluginTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PEAKUnlimited.Tests
{
    using System.Collections.Generic;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PEAKUnlimited.Core;
    using UnityEngine;
    using UnityEngine.UI;

    [TestClass]
    public class PluginTests
    {
        [TestMethod]
        public void TestReflectionFieldAccess()
        {
            // Test that the oldPip field can be accessed via reflection
            var oldPipField = typeof(EndScreen).GetField("oldPip", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(oldPipField, "oldPip field should be accessible via reflection");
            Assert.AreEqual(typeof(Image[]), oldPipField.FieldType, "oldPip field should be of type Image[]");
        }

        [TestMethod]
        public void TestGetEvenlySpacedPointsAroundCampfire()
        {
            // Test the marshmallow positioning logic
            Vector3 campfirePosition = new Vector3(0, 0, 0);
            int numPoints = 6;
            float innerRadius = 2.5f;
            float outerRadius = 3.0f;

            var points = Plugin.GetEvenlySpacedPointsAroundCampfire(numPoints, innerRadius, outerRadius, campfirePosition, default(Segment));

            Assert.AreEqual(numPoints, points.Count, "Should return exactly the requested number of points");

            // Test that points are within expected radius
            foreach (var point in points)
            {
                float distance = Vector3.Distance(campfirePosition, point);
                Assert.IsTrue(
                    distance >= innerRadius && distance <= outerRadius,
                    $"Point {point} should be within radius {innerRadius}-{outerRadius}, but was {distance}");
            }
        }

        [TestMethod]
        public void TestPlayerCountValidation()
        {
            // Test player count validation logic
            int vanillaMaxPlayers = 4;

            // Test valid player counts
            Assert.IsTrue(GameLogic.ValidatePlayerCount(5, vanillaMaxPlayers), "5 players should be valid");
            Assert.IsTrue(GameLogic.ValidatePlayerCount(20, vanillaMaxPlayers), "20 players should be valid");

            // Test invalid player counts
            Assert.IsFalse(GameLogic.ValidatePlayerCount(0, vanillaMaxPlayers), "0 players should be invalid");
            Assert.IsFalse(GameLogic.ValidatePlayerCount(-1, vanillaMaxPlayers), "Negative players should be invalid");
            Assert.IsFalse(GameLogic.ValidatePlayerCount(31, vanillaMaxPlayers), "31+ players should be invalid");
        }

        [TestMethod]
        public void TestConfigurationValidation()
        {
            // Test configuration value validation
            Assert.IsTrue(GameLogic.ValidateMaxPlayers(20), "20 max players should be valid");
            Assert.IsTrue(GameLogic.ValidateMaxPlayers(1), "1 max player should be valid");
            Assert.IsFalse(GameLogic.ValidateMaxPlayers(0), "0 max players should be invalid");
            Assert.IsFalse(GameLogic.ValidateMaxPlayers(31), "31+ max players should be invalid");

            Assert.IsTrue(GameLogic.ValidateCheatMarshmallows(15), "15 cheat marshmallows should be valid");
            Assert.IsTrue(GameLogic.ValidateCheatMarshmallows(0), "0 cheat marshmallows should be valid");
            Assert.IsFalse(GameLogic.ValidateCheatMarshmallows(31), "31+ cheat marshmallows should be invalid");

            Assert.IsTrue(GameLogic.ValidateCheatBackpacks(5), "5 cheat backpacks should be valid");
            Assert.IsTrue(GameLogic.ValidateCheatBackpacks(0), "0 cheat backpacks should be valid");
            Assert.IsFalse(GameLogic.ValidateCheatBackpacks(11), "11+ cheat backpacks should be invalid");
        }

        [TestMethod]
        public void TestEndScreenArrayExpansion()
        {
            // Test that end screen arrays are properly expanded
            int originalCount = 4;
            int newCount = 8;

            var originalArray = new Image[originalCount];
            var expandedArray = GameLogic.ExpandArrayForExtraPlayers(originalArray, newCount);

            Assert.AreEqual(newCount, expandedArray.Length, "Array should be expanded to new count");

            // Test that original elements are preserved
            for (int i = 0; i < originalCount; i++)
            {
                Assert.AreEqual(originalArray[i], expandedArray[i], $"Original element {i} should be preserved");
            }
        }

        [TestMethod]
        public void TestMarshmallowSpawnLogic()
        {
            // Test marshmallow spawn calculations
            int vanillaMaxPlayers = 4;
            int currentPlayers = 6;

            int expectedMarshmallows = currentPlayers - vanillaMaxPlayers; // Should be 2
            int actualMarshmallows = GameLogic.CalculateExtraMarshmallows(currentPlayers, vanillaMaxPlayers, 0);

            Assert.AreEqual(expectedMarshmallows, actualMarshmallows,
                $"Should spawn {expectedMarshmallows} extra marshmallows for {currentPlayers} players");
        }

        [TestMethod]
        public void TestBackpackSpawnLogic()
        {
            // Test backpack spawn calculations
            int vanillaMaxPlayers = 4;
            int currentPlayers = 8;

            // 25% chance per extra player, so 4 extra players = 1 backpack
            int expectedBackpacks = (currentPlayers - vanillaMaxPlayers) / 4; // Should be 1
            int actualBackpacks = GameLogic.CalculateExtraBackpacks(currentPlayers, vanillaMaxPlayers);

            Assert.AreEqual(expectedBackpacks, actualBackpacks,
                $"Should spawn {expectedBackpacks} extra backpacks for {currentPlayers} players");
        }
    }

    // Mock EndScreen class for testing
    public class EndScreen
    {
        private Image[] oldPip = new Image[4];
        public Image[] Scouts = new Image[4];
        public Image[] ScoutsAtPeak = new Image[4];
        public EndScreenScoutWindow[] ScoutWindows = new EndScreenScoutWindow[4];
        public Transform[] ScoutLines = new Transform[4];
    }

    // Mock classes for testing
    public class EndScreenScoutWindow
    {
    }
}
