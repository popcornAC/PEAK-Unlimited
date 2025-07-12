// <copyright file="PluginTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PEAKUnlimited.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PEAKUnlimited.Core;

    [TestClass]
    public class PluginTests
    {
        

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
}
