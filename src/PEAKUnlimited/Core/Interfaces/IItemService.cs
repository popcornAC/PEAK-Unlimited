// <copyright file="IItemService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PEAKUnlimited.Core.Interfaces
{
    using UnityEngine;

    /// <summary>
    /// Interface for item-related operations.
    /// </summary>
    public interface IItemService
    {
        /// <summary>
        /// Gets an item by its ID.
        /// </summary>
        /// <param name="itemId">The ID of the item to get.</param>
        /// <returns>The item with the specified ID.</returns>
        Item GetItem(int itemId);

        /// <summary>
        /// Spawns extra backpacks at the specified position.
        /// </summary>
        /// <param name="position">The position to spawn backpacks at.</param>
        /// <param name="segment">The segment to spawn backpacks in.</param>
        void SpawnExtraBackpacks(Vector3 position, Segment segment);

        /// <summary>
        /// Spawns extra marshmallows at the specified position.
        /// </summary>
        /// <param name="position">The position to spawn marshmallows at.</param>
        /// <param name="segment">The segment to spawn marshmallows in.</param>
        void SpawnExtraMarshmallows(Vector3 position, Segment segment);

        /// <summary>
        /// Spawns a late join marshmallow.
        /// </summary>
        void SpawnLateJoinMarshmallow();

        /// <summary>
        /// Removes a late join marshmallow.
        /// </summary>
        void RemoveLateJoinMarshmallow();
    }
}
