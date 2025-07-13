// <copyright file="PhotonNetworkService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PEAKUnlimited.Core.Services
{
    using PEAKUnlimited.Core.Interfaces;
    using Photon.Pun;
    using UnityEngine;

    /// <summary>
    /// Photon network service implementation.
    /// </summary>
    public class PhotonNetworkService : INetworkService
    {
        /// <summary>
        /// Gets a value indicating whether gets whether the network is connected.
        /// </summary>
        public bool IsConnected => PhotonNetwork.IsConnected;

        /// <summary>
        /// Gets a value indicating whether gets whether this client is the master client.
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
