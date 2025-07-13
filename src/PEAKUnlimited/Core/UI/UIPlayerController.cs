// <copyright file="UIPlayerController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PEAKUnlimited.Core.UI
{
    using UnityEngine;

    /// <summary>
    /// Handles pausing and resuming player movement when UI is shown/hidden.
    /// </summary>
    public class UIPlayerController
    {
        /// <summary>
        /// Pauses player movement and frees mouse cursor.
        /// </summary>
        public static void PausePlayerMovement()
        {
            // Free the mouse cursor
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;

            // Disable character controller if present
            var characterController = UnityEngine.Object.FindFirstObjectByType<CharacterController>();
            if (characterController != null)
            {
                characterController.enabled = false;
            }
        }

        /// <summary>
        /// Resumes player movement and locks mouse cursor.
        /// </summary>
        public static void ResumePlayerMovement()
        {
            // Lock the mouse cursor back
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;

            // Re-enable character controller if present
            var characterController = UnityEngine.Object.FindFirstObjectByType<CharacterController>();
            if (characterController != null)
            {
                characterController.enabled = true;
            }
        }
    }
}
