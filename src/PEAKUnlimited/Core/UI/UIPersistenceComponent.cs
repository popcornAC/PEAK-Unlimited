// <copyright file="UIPersistenceComponent.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PEAKUnlimited.Core.UI
{
    using UnityEngine;

    /// <summary>
    /// Component to ensure UI panel stays active and visible.
    /// </summary>
    public class UIPersistenceComponent : MonoBehaviour
    {
        private bool shouldBeVisible = false;

        public void SetShouldBeVisible(bool visible)
        {
            this.shouldBeVisible = visible;
        }

        private void Update()
        {
            // If the panel should be visible but isn't, reactivate it
            if (this.shouldBeVisible && !this.gameObject.activeInHierarchy)
            {
                this.gameObject.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            // Component destroyed, no action needed
        }
    }
}
