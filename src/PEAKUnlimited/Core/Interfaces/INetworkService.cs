using UnityEngine;

namespace PEAKUnlimited.Core.Interfaces
{
    /// <summary>
    /// Interface for network-related operations.
    /// </summary>
    public interface INetworkService
    {
        /// <summary>
        /// Gets whether the network is connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets whether this client is the master client.
        /// </summary>
        bool IsMasterClient { get; }

        /// <summary>
        /// Instantiates a networked object.
        /// </summary>
        /// <param name="prefabName">The prefab name.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <returns>The instantiated game object.</returns>
        GameObject Instantiate(string prefabName, Vector3 position, Quaternion rotation);

        /// <summary>
        /// Destroys a networked object.
        /// </summary>
        /// <param name="gameObject">The game object to destroy.</param>
        void Destroy(GameObject gameObject);
    }
} 