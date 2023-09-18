using System.Collections.Generic;
using H3MP.Scripts;

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

        public static int GetPlayerCount()
        {
            return GameManager.players.Count;
        }

        /// <summary>
        /// Returns the Gamemanager player at index i, does not include the local player.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static PlayerManager GetPlayer(int i)
        {
            //Do error Checks
            return GameManager.players[i];
        }

        /// <summary>
        /// Returns array of PlayerManagers of the current connected players (Not including the local player).
        /// </summary>
        /// <returns></returns>
        public static PlayerManager[] GetPlayers()
        {
            PlayerManager[] playerArray = new PlayerManager[GameManager.players.Count];

            int i = 0;
            foreach (KeyValuePair<int, PlayerManager> entry in GameManager.players)
            {
                playerArray[i] = entry.Value;
                i++;
            }
            return playerArray;
        }

        /// <summary>
        /// Returns array of all players (Not including local player) IDs
        /// </summary>
        /// <returns></returns>
        public static int[] GetPlayerIDs()
        {
            int[] playerArray = new int[GameManager.players.Count];

            int i = 0;
            foreach (KeyValuePair<int, PlayerManager> entry in GameManager.players)
            {
                playerArray[i] = entry.Key;
                i++;
            }

            return playerArray;
        }

        /// <summary>
        /// Returns the local players id.
        /// </summary>
        /// <returns></returns>
        public static int GetLocalID()
        {
            return GameManager.ID;
        }

        /// <summary>
        /// Returns the Custom Packet ID
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public static int RegisterHostCustomPacket(string identifier)
        {
            int id;
            if (Mod.registeredCustomPacketIDs.ContainsKey(identifier))
                id = Mod.registeredCustomPacketIDs[identifier];
            else
                id = Server.RegisterCustomPacketType(identifier);

            return id;
        }
    }
}