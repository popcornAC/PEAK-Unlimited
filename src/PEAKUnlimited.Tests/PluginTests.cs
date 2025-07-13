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

            // Test cases with randomness (simulate randomProvider returning 0.1 and 0.9)
            currentPlayers = 9; // 5 extra players = 1.25 backpacks = 1 or 2 (with randomness)
            // Simulate randomProvider returning 0.1 (should NOT increment)
            actualBackpacks = GameLogic.CalculateExtraBackpacks(currentPlayers, vanillaMaxPlayers, true, () => 0.1);
            Assert.AreEqual(1, actualBackpacks, "Should spawn 1 extra backpack for 9 players when randomProvider returns 0.1");
            // Simulate randomProvider returning 0.9 (should increment)
            actualBackpacks = GameLogic.CalculateExtraBackpacks(currentPlayers, vanillaMaxPlayers, true, () => 0.9);
            Assert.AreEqual(2, actualBackpacks, "Should spawn 2 extra backpacks for 9 players when randomProvider returns 0.9");

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
            int actualBackpacks = GameLogic.CalculateExtraBackpacks(currentPlayers, vanillaMaxPlayers, true, () => 0.1);
            Assert.AreEqual(0, actualBackpacks, "Should spawn 0 extra backpacks for 5 players when randomProvider returns 0.1");
            actualBackpacks = GameLogic.CalculateExtraBackpacks(currentPlayers, vanillaMaxPlayers, true, () => 0.9);
            Assert.AreEqual(1, actualBackpacks, "Should spawn 1 extra backpack for 5 players when randomProvider returns 0.9");

            currentPlayers = 9; // 5 extra players = 1.25 backpacks
            actualBackpacks = GameLogic.CalculateExtraBackpacks(currentPlayers, vanillaMaxPlayers, true, () => 0.1);
            Assert.AreEqual(1, actualBackpacks, "Should spawn 1 extra backpack for 9 players when randomProvider returns 0.1");
            actualBackpacks = GameLogic.CalculateExtraBackpacks(currentPlayers, vanillaMaxPlayers, true, () => 0.9);
            Assert.AreEqual(2, actualBackpacks, "Should spawn 2 extra backpacks for 9 players when randomProvider returns 0.9");

            currentPlayers = 13; // 9 extra players = 2.25 backpacks
            actualBackpacks = GameLogic.CalculateExtraBackpacks(currentPlayers, vanillaMaxPlayers, true, () => 0.1);
            Assert.AreEqual(2, actualBackpacks, "Should spawn 2 extra backpacks for 13 players when randomProvider returns 0.1");
            actualBackpacks = GameLogic.CalculateExtraBackpacks(currentPlayers, vanillaMaxPlayers, true, () => 0.9);
            Assert.AreEqual(3, actualBackpacks, "Should spawn 3 extra backpacks for 13 players when randomProvider returns 0.9");
        }
    }
}
