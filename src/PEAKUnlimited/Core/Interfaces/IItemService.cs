using UnityEngine;

namespace PEAKUnlimited.Core.Interfaces
{
    /// <summary>
    /// Interface for item-related operations.
    /// </summary>
    public interface IItemService
    {
        /// <summary>
        /// Gets an item by its ID.
        /// </summary>
        /// <param name="itemId">The item ID.</param>
        /// <returns>The item.</returns>
        Item GetItem(int itemId);

        
    }
} 