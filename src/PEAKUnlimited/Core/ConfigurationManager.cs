// <copyright file="ConfigurationManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PEAKUnlimited.Core
{
    using System;
    using BepInEx.Configuration;

    /// <summary>
    /// Manages plugin configuration and validation.
    /// </summary>
    public class ConfigurationManager
    {
        /// <summary>
        /// Configuration settings for the plugin.
        /// </summary>
        public class PluginConfig
        {
            /// <summary>
            /// Gets or sets the maximum number of players.
            /// </summary>
            public int MaxPlayers { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether gets or sets whether to add extra marshmallows.
            /// </summary>
            public bool ExtraMarshmallows { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether gets or sets whether to add extra backpacks.
            /// </summary>
            public bool ExtraBackpacks { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether gets or sets whether to add marshmallows for late-joining players.
            /// </summary>
            public bool LateJoinMarshmallows { get; set; }

            /// <summary>
            /// Gets or sets the number of cheat extra marshmallows.
            /// </summary>
            public int CheatExtraMarshmallows { get; set; }

            /// <summary>
            /// Gets or sets the number of cheat extra backpacks.
            /// </summary>
            public int CheatExtraBackpacks { get; set; }
        }

        public static PluginConfig Default => new PluginConfig
        {
            MaxPlayers = 20,
            ExtraMarshmallows = true,
            ExtraBackpacks = true,
            LateJoinMarshmallows = false,
            CheatExtraMarshmallows = 0,
            CheatExtraBackpacks = 0
        };

        /// <summary>
        /// Validates and processes configuration values.
        /// </summary>
        /// <param name="config">The raw configuration values.</param>
        /// <returns>Processed and validated configuration.</returns>
        public static PluginConfig ProcessConfiguration(PluginConfig config)
        {
            var processed = new PluginConfig
            {
                MaxPlayers = ValidateAndClampMaxPlayers(config.MaxPlayers),
                ExtraMarshmallows = config.ExtraMarshmallows,
                ExtraBackpacks = config.ExtraBackpacks,
                LateJoinMarshmallows = config.LateJoinMarshmallows,
                CheatExtraMarshmallows = ValidateAndClampCheatMarshmallows(config.CheatExtraMarshmallows),
                CheatExtraBackpacks = ValidateAndClampCheatBackpacks(config.CheatExtraBackpacks),
            };

            return processed;
        }

        /// <summary>
        /// Validates and clamps the maximum players setting.
        /// </summary>
        /// <param name="maxPlayers">The raw max players value.</param>
        /// <returns>The validated and clamped value.</returns>
        public static int ValidateAndClampMaxPlayers(int maxPlayers)
        {
            if (maxPlayers <= 0)
            {
                return 1;
            }

            if (maxPlayers > 30)
            {
                return 30;
            }

            return maxPlayers;
        }

        /// <summary>
        /// Validates and clamps the cheat marshmallows setting.
        /// </summary>
        /// <param name="cheatMarshmallows">The raw cheat marshmallows value.</param>
        /// <returns>The validated and clamped value.</returns>
        public static int ValidateAndClampCheatMarshmallows(int cheatMarshmallows)
        {
            if (cheatMarshmallows < 0)
            {
                return 0;
            }

            if (cheatMarshmallows > 30)
            {
                return 30;
            }

            return cheatMarshmallows;
        }

        /// <summary>
        /// Validates and clamps the cheat backpacks setting.
        /// </summary>
        /// <param name="cheatBackpacks">The raw cheat backpacks value.</param>
        /// <returns>The validated and clamped value.</returns>
        public static int ValidateAndClampCheatBackpacks(int cheatBackpacks)
        {
            if (cheatBackpacks < 0)
            {
                return 0;
            }

            if (cheatBackpacks > 10)
            {
                return 10;
            }

            return cheatBackpacks;
        }

        /// <summary>
        /// Creates a configuration from BepInEx config entries.
        /// </summary>
        /// <param name="configMaxPlayers">Max players config entry.</param>
        /// <param name="configExtraMarshmallows">Extra marshmallows config entry.</param>
        /// <param name="configExtraBackpacks">Extra backpacks config entry.</param>
        /// <param name="configLateMarshmallows">Late marshmallows config entry.</param>
        /// <param name="configCheatExtraMarshmallows">Cheat marshmallows config entry.</param>
        /// <param name="configCheatExtraBackpacks">Cheat backpacks config entry.</param>
        /// <returns>Processed configuration.</returns>
        public static PluginConfig CreateFromBepInExConfig(
            ConfigEntry<int> configMaxPlayers,
            ConfigEntry<bool> configExtraMarshmallows,
            ConfigEntry<bool> configExtraBackpacks,
            ConfigEntry<bool> configLateMarshmallows,
            ConfigEntry<int> configCheatExtraMarshmallows,
            ConfigEntry<int> configCheatExtraBackpacks)
        {
            var rawConfig = new PluginConfig
            {
                MaxPlayers = configMaxPlayers.Value,
                ExtraMarshmallows = configExtraMarshmallows.Value,
                ExtraBackpacks = configExtraBackpacks.Value,
                LateJoinMarshmallows = configLateMarshmallows.Value,
                CheatExtraMarshmallows = configCheatExtraMarshmallows.Value,
                CheatExtraBackpacks = configCheatExtraBackpacks.Value,
            };

            return ProcessConfiguration(rawConfig);
        }
    }
}
