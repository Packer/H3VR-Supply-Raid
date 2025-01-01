using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using H3MP;
using H3MP.Networking;
using System;
using System.Reflection;

namespace SupplyRaid
{
    public class SR_Networking : MonoBehaviour
    {
        public static SR_Networking instance;

        public bool isClient = false;

        //Packet IDs
        private int levelUpdate_ID = -1;
        private int gameOptions_ID = -1;
        private int capturedZone_ID = -1;
        private int serverRunning_ID = -1;
        private int updateStats_ID = -1;
        private int requestSync_ID = -1;

        void Awake()
        {
            instance = this;
        }

        void StartNetworking()
        {
            if (Networking.ServerRunning())
            {
                //Mod.OnConnection += OnConnection;

                if (Networking.IsHost())
                {
                    //Only Sends data if they join the server IN the map.
                    //ServerClient.OnSendPostWelcomeData += OnPlayerJoined;
                }

                if (Networking.IsHost() || Client.isFullyConnected)
                {
                    SetupPacketTypes();
                }

                if (Networking.IsClient())
                    SR_Manager.instance.SetLocalAsClient();

                //Invoke("OnConnection", 3);
            }
        }

        // Use this for initialization
        void Start()
        {
            if(SupplyRaidPlugin.h3mpEnabled)
                StartNetworking();
        }

        /*
        void OnConnection()
        {
            SetupPacketTypes();

            if (Networking.IsClient())
                SR_Manager.instance.SetLocalAsClient();
        }
        */

        /// <summary>
        /// When a player joins the server MID game.
        /// </summary>
        /// <param name="clientID"></param>
        void OnPlayerJoined(int clientID)
        {
            if (clientID == 0)
                return;
        }
		
        void SetupPacketTypes()
        {
            //Server
            if (Networking.IsHost())
            {
                if (Mod.registeredCustomPacketIDs.ContainsKey("SR_LevelUpdate"))
                    levelUpdate_ID = Mod.registeredCustomPacketIDs["SR_LevelUpdate"];
                else
                    levelUpdate_ID = Server.RegisterCustomPacketType("SR_LevelUpdate");
                Mod.customPacketHandlers[levelUpdate_ID] = LevelUpdate_Handler;

                //levelUpdate_ID = Networking.RegisterHostCustomPacket("SR_LevelUpdate");
                //Mod.customPacketHandlers[levelUpdate_ID] = LevelUpdate_Handler;

                if (Mod.registeredCustomPacketIDs.ContainsKey("SR_GameOptions"))
                    gameOptions_ID = Mod.registeredCustomPacketIDs["SR_GameOptions"];
                else
                    gameOptions_ID = Server.RegisterCustomPacketType("SR_GameOptions");
                Mod.customPacketHandlers[gameOptions_ID] = GameOptions_Handler;


                //gameOptions_ID = Networking.RegisterHostCustomPacket("SR_GameOptions");
                //Mod.customPacketHandlers[gameOptions_ID] = GameOptions_Handler;

                if (Mod.registeredCustomPacketIDs.ContainsKey("SR_CapturedZone"))
                    capturedZone_ID = Mod.registeredCustomPacketIDs["SR_CapturedZone"];
                else
                    capturedZone_ID = Server.RegisterCustomPacketType("SR_CapturedZone");
                Mod.customPacketHandlers[capturedZone_ID] = CapturedZone_Handler;

                //capturedZone_ID = Networking.RegisterHostCustomPacket("SR_CapturedZone");
                //Mod.customPacketHandlers[capturedZone_ID] = CapturedZone_Handler;

                if (Mod.registeredCustomPacketIDs.ContainsKey("SR_ServerRunning"))
                    serverRunning_ID = Mod.registeredCustomPacketIDs["SR_ServerRunning"];
                else
                    serverRunning_ID = Server.RegisterCustomPacketType("SR_ServerRunning");
                Mod.customPacketHandlers[serverRunning_ID] = ServerRunning_Handler;

                //serverRunning_ID = Networking.RegisterHostCustomPacket("SR_ServerRunning");
                //Mod.customPacketHandlers[serverRunning_ID] = ServerRunning_Handler;

                if (Mod.registeredCustomPacketIDs.ContainsKey("SR_UpdateStats"))
                    updateStats_ID = Mod.registeredCustomPacketIDs["SR_UpdateStats"];
                else
                    updateStats_ID = Server.RegisterCustomPacketType("SR_UpdateStats");
                Mod.customPacketHandlers[updateStats_ID] = UpdateStats_Handler;

                //
                //updateStats_ID = Networking.RegisterHostCustomPacket("SR_UpdateStats");
                //Mod.customPacketHandlers[updateStats_ID] = UpdateStats_Handler;

                if (Mod.registeredCustomPacketIDs.ContainsKey("SR_RequestSync"))
                    requestSync_ID = Mod.registeredCustomPacketIDs["SR_RequestSync"];
                else
                    requestSync_ID = Server.RegisterCustomPacketType("SR_RequestSync");
                Mod.customPacketHandlers[requestSync_ID] = RequestSync_Handler;

                //requestSync_ID = Networking.RegisterHostCustomPacket("SR_RequestSync");
                //Mod.customPacketHandlers[requestSync_ID] = RequestSync_Handler;

            }
            else //Client
            {
                if (Mod.registeredCustomPacketIDs.ContainsKey("SR_LevelUpdate"))
                {
                    levelUpdate_ID = Mod.registeredCustomPacketIDs["SR_LevelUpdate"];
                    Mod.customPacketHandlers[levelUpdate_ID] = LevelUpdate_Handler;
                }
                else
                {
                    ClientSend.RegisterCustomPacketType("SR_LevelUpdate");
                    Mod.CustomPacketHandlerReceived += LevelUpdate_Received;
                }

                //Game Options
                if (Mod.registeredCustomPacketIDs.ContainsKey("SR_GameOptions"))
                {
                    gameOptions_ID = Mod.registeredCustomPacketIDs["SR_GameOptions"];
                    Mod.customPacketHandlers[gameOptions_ID] = GameOptions_Handler;
                }
                else
                {
                    ClientSend.RegisterCustomPacketType("SR_GameOptions");
                    Mod.CustomPacketHandlerReceived += GameOptions_Received;
                }

                //Capture
                if (Mod.registeredCustomPacketIDs.ContainsKey("SR_CapturedZone"))
                {
                    capturedZone_ID = Mod.registeredCustomPacketIDs["SR_CapturedZone"];
                    Mod.customPacketHandlers[capturedZone_ID] = CapturedZone_Handler;
                }
                else
                {
                    ClientSend.RegisterCustomPacketType("SR_CapturedZone");
                    Mod.CustomPacketHandlerReceived += CapturedZone_Received;
                }

                //Server Running
                if (Mod.registeredCustomPacketIDs.ContainsKey("SR_ServerRunning"))
                {
                    serverRunning_ID = Mod.registeredCustomPacketIDs["SR_ServerRunning"];
                    Mod.customPacketHandlers[serverRunning_ID] = ServerRunning_Handler;
                }
                else
                {
                    ClientSend.RegisterCustomPacketType("SR_ServerRunning");
                    Mod.CustomPacketHandlerReceived += ServerRunning_Received;
                }

                //Update Stats
                if (Mod.registeredCustomPacketIDs.ContainsKey("SR_UpdateStats"))
                {
                    updateStats_ID = Mod.registeredCustomPacketIDs["SR_UpdateStats"];
                    Mod.customPacketHandlers[updateStats_ID] = UpdateStats_Handler;
                }
                else
                {
                    ClientSend.RegisterCustomPacketType("SR_UpdateStats");
                    Mod.CustomPacketHandlerReceived += UpdateStats_Received;
                }

                //Request Sync
                if (Mod.registeredCustomPacketIDs.ContainsKey("SR_RequestSync"))
                {
                    requestSync_ID = Mod.registeredCustomPacketIDs["SR_RequestSync"];
                    Mod.customPacketHandlers[requestSync_ID] = RequestSync_Handler;
                }
                else
                {
                    ClientSend.RegisterCustomPacketType("SR_RequestSync");
                    Mod.CustomPacketHandlerReceived += RequestSync_Received;
                }
            }
        }

        //---------------------------------------------------------------
        //  Send and Receive
        //---------------------------------------------------------------

        //Level Update Send
        public void LevelUpdate_Send(bool gameComplete)
        {
            if (!Networking.ServerRunning() || Networking.IsClient())
                return;

            Packet packet = new Packet(levelUpdate_ID);

            packet.Write(SR_Manager.instance.CurrentCaptures);

            packet.Write(SR_Manager.instance.attackSupplyID);
            packet.Write(SR_Manager.instance.playerSupplyID);

            packet.Write(SR_Manager.instance.inEndless);
            packet.Write(SR_Manager.instance.stats.ObjectiveComplete);
            packet.Write(gameComplete);
            ServerSend.SendTCPDataToAll(packet, true);

            Debug.Log("Supply Raid: Host - Level Update: " + SR_Manager.instance.CurrentCaptures);
        }

        //Level Update Receive
        void LevelUpdate_Handler(int clientID, Packet packet)
        {
            int totalCaptures = packet.ReadInt();

            int supply = packet.ReadInt();
            int lastSupply = packet.ReadInt();
            bool endless = packet.ReadBool();
            bool objective = packet.ReadBool();
            bool gameComplete = packet.ReadBool();

            if (gameComplete)
            {
                //Stats
                //SR_Manager.instance.CurrentCharacterLevel = characterLevel;
                //SR_Manager.instance.CurrentFactionLevel = factionLevel;
                SR_Manager.instance.CurrentCaptures = totalCaptures;

                SR_Manager.instance.gameCompleted = gameComplete;
                SR_Manager.instance.stats.ObjectiveComplete = objective;
                SR_Manager.instance.CompleteGame();
            }
            else
                SR_Manager.instance.SetLevel_Client(totalCaptures, supply, lastSupply, endless);


            Debug.Log("Supply Raid: Client - Level Update: " + totalCaptures);
        }

        //GameOptions Send ----------------------------------------------
        public void GameOptions_Send()
        {
            if (!Networking.ServerRunning() || Networking.IsClient())
                return;

            Packet packet = new Packet(gameOptions_ID);
            packet.Write(SR_Manager.profile.playerCount);
            packet.Write(SR_Manager.profile.difficulty);
            packet.Write(SR_Manager.profile.freeBuyMenu);
            packet.Write(SR_Manager.profile.spawnLocking);
            packet.Write(SR_Manager.profile.startLevel);
            packet.Write(SR_Manager.profile.playerHealth);
            packet.Write(SR_Manager.profile.itemSpawner);
            packet.Write(SR_Manager.profile.captureZone);
            packet.Write(SR_Manager.profile.captureOrder);
            packet.Write(SR_Manager.profile.captures);
            packet.Write(SR_Manager.profile.respawn);
            packet.Write(SR_Manager.profile.itemsDrop);
            packet.Write(SR_Manager.profile.maxEnemies);
            packet.Write(SR_Manager.profile.maxSquadEnemies);

            string faction = "";
            if (SR_Manager.Faction() != null)
                faction = SR_Manager.Faction().name;

            packet.Write(faction);

            packet.Write(SR_Manager.profile.sosigWeapons);

            ServerSend.SendTCPDataToAll(packet, true);
        }

        //Game Options Received
        void GameOptions_Handler(int clientID, Packet packet)
        {
            float optionPlayerCount = packet.ReadFloat();
            float optionDifficulty = packet.ReadFloat();
            bool optionFreeBuyMenu = packet.ReadBool();
            bool optionSpawnLocking = packet.ReadBool();
            int optionStartLevel = packet.ReadInt();
            int optionPlayerHealth = packet.ReadInt();
            bool optionItemSpawner = packet.ReadBool();
            bool optionCaptureZone = packet.ReadBool();
            int optionOrder = packet.ReadInt();
            int optionCaptures = packet.ReadInt();
            bool optionRespawn = packet.ReadBool();
            int optionItemsDrop = packet.ReadInt();
            int optionMaxEnemies = packet.ReadInt();
            int optionSquadMaxEnemies = packet.ReadInt();
            string factionID = packet.ReadString();
            bool sosigWeapons = packet.ReadBool();

            SR_Manager.instance.Network_GameOptions(optionPlayerCount, optionDifficulty, optionFreeBuyMenu, optionSpawnLocking, optionStartLevel,
                optionPlayerHealth, optionItemSpawner, optionCaptureZone, optionOrder, optionCaptures, optionRespawn, optionItemsDrop, optionMaxEnemies, 
                optionSquadMaxEnemies, factionID, sosigWeapons);
        }

        //Captured Send ----------------------------------------------
        public void CapturedZone_Send()
        {
            if (!Networking.ServerRunning() || Networking.IsHost())
                return;

            Debug.Log("Networking: Client Sending Captured Zone");

            Packet packet = new Packet(capturedZone_ID);
                ClientSend.SendTCPData(packet, true);
        }

        //Captured Received
        void CapturedZone_Handler(int clientID, Packet packet)
        {
            Debug.Log("Networking: Received Client Captured zone");
            if(SR_Manager.instance.captureProtection <= 0)
                SR_Manager.instance.CapturedPoint();
        }

        //SERVER RUNNING Send ----------------------------------------------
        public void ServerRunning_Send()
        {
            if (!Networking.ServerRunning() || Networking.IsClient())
                return;

            Packet packet = new Packet(serverRunning_ID);
                packet.Write(SR_Manager.instance.gameServerRunning);
                ServerSend.SendTCPDataToAll(packet, true);
        }

        //Server Running Received
        void ServerRunning_Handler(int clientID, Packet packet)
        {
            bool status = packet.ReadBool();
            SR_Manager.instance.gameServerRunning = status;
            SR_Menu.instance.lauchGameButton.SetActive(status);
        }

        //Send Stats ----------------------------------------------
        public void UpdateStats_Send()
        {
            if (!Networking.ServerRunning() || Networking.IsClient())
                return;

            Packet packet = new Packet(updateStats_ID);

            packet.Write(SR_Manager.instance.CurrentCaptures);
            packet.Write(SR_Manager.instance.stats.GameTime);
            packet.Write(SR_Manager.instance.stats.Kills);

            ServerSend.SendTCPDataToAll(packet, true);
        }

        //Stats Received
        void UpdateStats_Handler(int clientID, Packet packet)
        {
            int captures = packet.ReadInt();
            if(captures > SR_Manager.instance.CurrentCaptures)
                SR_Manager.instance.CurrentCaptures = captures;

            float gameTime = packet.ReadFloat();
            if (gameTime > SR_Manager.instance.stats.GameTime)
                SR_Manager.instance.stats.GameTime = gameTime;

            int kills = packet.ReadInt();
            if (SR_Manager.instance.stats.Kills > kills)
                SR_Manager.instance.stats.Kills = kills;

            SR_ResultsMenu.instance.UpdateResults();
        }

        //RequestSync Send ----------------------------------------------
        public void RequestSync_Send()
        {
            if (!Networking.ServerRunning() || Networking.IsHost())
                return;

            Debug.Log("Networking: Client Sending Request Sync");

            Packet packet = new Packet(requestSync_ID);
            ClientSend.SendTCPData(packet, true);
        }

        //RequestSync Received TODO make corouteen
        void RequestSync_Handler(int clientID, Packet packet)
        {
            Debug.Log("Networking: Received Client Request Sync");

            GameOptions_Send();
            LevelUpdate_Send(SR_Manager.instance.gameCompleted);
            UpdateStats_Send();
            ServerRunning_Send();
        }


        //---------------------------------------------------------------
        //(Client) Packet Handlers
        //---------------------------------------------------------------

        void LevelUpdate_Received(string identifier, int index)
		{
			if(identifier == "SR_LevelUpdate")
            {
                levelUpdate_ID = index;
                Mod.customPacketHandlers[index] = LevelUpdate_Handler;
                Mod.CustomPacketHandlerReceived -= LevelUpdate_Received;
            }
        }

        void GameOptions_Received(string handlerID, int index)
        {
            if (handlerID == "SR_GameOptions")
            {
                gameOptions_ID = index;
                Mod.customPacketHandlers[index] = GameOptions_Handler;
                Mod.CustomPacketHandlerReceived -= GameOptions_Received;
            }
        }

        void CapturedZone_Received(string handlerID, int index)
        {
            if (handlerID == "SR_CapturedZone")
            {
                capturedZone_ID = index;
                Mod.customPacketHandlers[index] = CapturedZone_Handler;
                Mod.CustomPacketHandlerReceived -= CapturedZone_Received;
            }
        }

        void ServerRunning_Received(string handlerID, int index)
        {
            if (handlerID == "SR_ServerRunning")
            {
                serverRunning_ID = index;
                Mod.customPacketHandlers[index] = ServerRunning_Handler;
                Mod.CustomPacketHandlerReceived -= ServerRunning_Received;
            }
        }

        void UpdateStats_Received(string handlerID, int index)
        {
            if (handlerID == "SR_UpdateStats")
            {
                updateStats_ID = index;
                Mod.customPacketHandlers[index] = UpdateStats_Handler;
                Mod.CustomPacketHandlerReceived -= UpdateStats_Received;
            }
        }

        void RequestSync_Received(string handlerID, int index)
        {
            if (handlerID == "SR_RequestSync")
            {
                requestSync_ID = index;
                Mod.customPacketHandlers[index] = RequestSync_Handler;
                Mod.CustomPacketHandlerReceived -= RequestSync_Received;
                
                //Clients request data once handler is setup
                RequestSync_Send();
            }
        }


        //---------------------------------------------------------------
        //Other
        //---------------------------------------------------------------

        public bool NamespaceExists(string desiredNamespace)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Namespace == desiredNamespace)
                        return true;
                }
            }
            return false;
        }
		
		void OnDestroy()
		{
			//Mod.OnConnection -= OnConnection;
        }
    }
}