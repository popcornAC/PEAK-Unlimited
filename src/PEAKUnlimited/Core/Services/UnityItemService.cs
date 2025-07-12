using PEAKUnlimited.Core.Interfaces;
using UnityEngine;
using Zorro.Core;

namespace PEAKUnlimited.Core.Services
{
    /// <summary>
    /// Unity item service implementation.
    /// </summary>
    public class UnityItemService : IItemService
    {
        /// <summary>
        /// Gets an item by its ID.
        /// </summary>
        /// <param name="itemId">The item ID.</param>
        /// <returns>The item.</returns>
        public Item GetItem(int itemId)
        {
            return SingletonAsset<ItemDatabase>.Instance.itemLookup[(ushort)itemId];
        }

        
    }
} 