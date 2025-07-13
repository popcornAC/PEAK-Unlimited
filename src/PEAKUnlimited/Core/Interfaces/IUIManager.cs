// <copyright file="IUIManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PEAKUnlimited.Core.Interfaces
{
    using UnityEngine;

    /// <summary>
    /// Interface for UI management operations.
    /// </summary>
    public interface IUIManager
    {
        /// <summary>
        /// Gets a value indicating whether gets whether the UI is currently visible.
        /// </summary>
        bool IsUIVisible { get; }

        /// <summary>
        /// Shows the configuration UI.
        /// </summary>
        void ShowConfigurationUI();

        /// <summary>
        /// Hides the configuration UI.
        /// </summary>
        void HideConfigurationUI();

        /// <summary>
        /// Toggles the configuration UI visibility.
        /// </summary>
        void ToggleConfigurationUI();

        /// <summary>
        /// Updates the UI manager (called every frame).
        /// </summary>
        void Update();
    }
}
