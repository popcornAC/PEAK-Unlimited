using PEAKUnlimited.Core.Interfaces;
using Photon.Pun;
using UnityEngine;

namespace PEAKUnlimited.Core.Services
{
    /// <summary>
    /// Photon network service implementation.
    /// </summary>
    public class PhotonNetworkService : INetworkService
    {
        /// <summary>
        /// Gets whether the network is connected.
        /// </summary>
        public bool IsConnected => PhotonNetwork.IsConnected;

        /// <summary>
        /// Gets whether this client is the master client.
        /// </summary>
        public bool IsMasterClient => PhotonNetwork.IsMasterClient;

        /// <summary>
        /// Instantiates a networked object.
        /// </summary>
        /// <param name="prefabName">The prefab name.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <returns>The instantiated game object.</returns>
        public GameObject Instantiate(string prefabName, Vector3 position, Quaternion rotation)
        {
            return PhotonNetwork.Instantiate(prefabName, position, rotation);
        }

        /// <summary>
        /// Destroys a networked object.
        /// </summary>
        /// <param name="gameObject">The game object to destroy.</param>
        public void Destroy(GameObject gameObject)
        {
            if (gameObject != null)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
} 