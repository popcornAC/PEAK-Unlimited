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
            // Test backpack spawn calculations with randomness
            int vanillaMaxPlayers = 4;
            
            // Test cases that should have deterministic results (when backpackNumber % 4 == 0)
            int currentPlayers = 8; // 4 extra players = 1.0 backpacks = always 1
            int expectedBackpacks = 1;
            int actualBackpacks = GameLogic.CalculateExtraBackpacks(currentPlayers, vanillaMaxPlayers, false);
            Assert.AreEqual(expectedBackpacks, actualBackpacks,
                $"Should spawn {expectedBackpacks} extra backpacks for {currentPlayers} players (deterministic case)");

            // Test cases with randomness
            currentPlayers = 9; // 5 extra players = 1.25 backpacks = 1 or 2 (with randomness)
            actualBackpacks = GameLogic.CalculateExtraBackpacks(currentPlayers, vanillaMaxPlayers, false);
            Assert.IsTrue(actualBackpacks >= 1 && actualBackpacks <= 2,
                $"Should spawn 1 or 2 extra backpacks for {currentPlayers} players (random case)");

            currentPlayers = 12; // 8 extra players = 2.0 backpacks = always 2
            expectedBackpacks = 2;
            actualBackpacks = GameLogic.CalculateExtraBackpacks(currentPlayers, vanillaMaxPlayers, false);
            Assert.AreEqual(expectedBackpacks, actualBackpacks,
                $"Should spawn {expectedBackpacks} extra backpacks for {currentPlayers} players (deterministic case)");
        }



        [TestMethod]
        public void TestBackpackRandomnessLogic()
        {
            // Test the randomness logic specifically
            int vanillaMaxPlayers = 4;
            
            // Test edge cases where randomness should apply
            int currentPlayers = 5; // 1 extra player = 0.25 backpacks
            int actualBackpacks = GameLogic.CalculateExtraBackpacks(currentPlayers, vanillaMaxPlayers, false);
            Assert.IsTrue(actualBackpacks >= 0 && actualBackpacks <= 1,
                "5 players should have 0 or 1 backpacks due to randomness");

            currentPlayers = 9; // 5 extra players = 1.25 backpacks
            actualBackpacks = GameLogic.CalculateExtraBackpacks(currentPlayers, vanillaMaxPlayers, false);
            Assert.IsTrue(actualBackpacks >= 1 && actualBackpacks <= 2,
                "9 players should have 1 or 2 backpacks due to randomness");

            currentPlayers = 13; // 9 extra players = 2.25 backpacks
            actualBackpacks = GameLogic.CalculateExtraBackpacks(currentPlayers, vanillaMaxPlayers, false);
            Assert.IsTrue(actualBackpacks >= 2 && actualBackpacks <= 3,
                "13 players should have 2 or 3 backpacks due to randomness");
        }
    }
}
