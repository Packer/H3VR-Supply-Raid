using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using H3MP.Networking;
using H3MP;

namespace H3MP.Networking
{
    public class Networking
    {
        /// <summary>
        /// Returns true if a server is running
        /// </summary>
        /// <returns></returns>
        public static bool ServerRunning()
        {
            if (Mod.managerObject == null)
                return false;

            return true;
        }

        /// <summary>
        /// Returns true if a server is Running AND the local player is a client
        /// </summary>
        /// <returns></returns>
        public static bool IsClient()
        {
            if (Mod.managerObject == null)
                return false;

            if (ThreadManager.host == false)
                return true;
            return false;
        }

        /// <summary>
        /// Returns true if a server is Running AND the local player is the host
        /// </summary>
        /// <returns></returns>
        public static bool IsHost()
        {
            if (Mod.managerObject == null)
                return false;

            if (ThreadManager.host == true)
                return true;
            return false;
        }

        public static Scripts.PlayerManager GetPlayer(int i)
        {
            //Do error Checks
            return GameManager.players[i];
        }
    }
}