// <copyright file="UIKeyboardHandler.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PEAKUnlimited.Core.UI
{
    using UnityEngine;

    /// <summary>
    /// Handles keyboard shortcuts for the configuration UI.
    /// </summary>
    public class UIKeyboardHandler
    {
        private readonly System.Action onSave;
        private readonly System.Action onReset;
        private readonly System.Action onClose;
        private readonly bool isUIVisible;

        /// <summary>
        /// Initializes a new instance of the <see cref="UIKeyboardHandler"/> class.
        /// </summary>
        /// <param name="onSave"></param>
        /// <param name="onReset"></param>
        /// <param name="onClose"></param>
        /// <param name="isUIVisible"></param>
        public UIKeyboardHandler(System.Action onSave, System.Action onReset, System.Action onClose, bool isUIVisible)
        {
            this.onSave = onSave;
            this.onReset = onReset;
            this.onClose = onClose;
            this.isUIVisible = isUIVisible;
        }

        /// <summary>
        /// Handles keyboard shortcuts for the UI.
        /// </summary>
        public void HandleKeyboardShortcuts()
        {
            if (!this.isUIVisible)
            {
                return;
            }

            // Save configuration with S key
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.S))
            {
                this.onSave?.Invoke();
            }

            // Reset configuration with R key
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.R))
            {
                this.onReset?.Invoke();
            }
        }
    }
}
