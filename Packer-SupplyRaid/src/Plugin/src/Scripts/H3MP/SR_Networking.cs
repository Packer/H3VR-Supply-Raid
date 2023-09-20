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

        // Use this for initialization
        void Start()
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

        void OnConnection()
        {
            SetupPacketTypes();

            if (Networking.IsClient())
                SR_Manager.instance.SetLocalAsClient();
        }


        /// <summary>
        /// When a player joins the server MID game.
        /// </summary>
        /// <param name="clientID"></param>
        void OnPlayerJoined(int clientID)
        {
            Debug.Log(">>>>>>>>> OnPlayerJoined: " + clientID);
            if (clientID == 0)
                return;

            /*
            //Update all the clients whe a new player joins
            LevelUpdate_Send(false);
            GameOptions_Send();
            ServerRunning_Send();
            */
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

        // Update is called once per frame
        void Update()
        {
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
            packet.Write(SR_Manager.instance.CurrentLevel);
            packet.Write(SR_Manager.instance.attackSupplyID);
            packet.Write(SR_Manager.instance.playerSupplyID);
            packet.Write(SR_Manager.instance.inEndless);
            packet.Write(SR_Manager.instance.stats.ObjectiveComplete);
            packet.Write(gameComplete);
            ServerSend.SendTCPDataToAll(packet, true);
        }

        //Level Update Receive
        void LevelUpdate_Handler(int clientID, Packet packet)
        {
            //If clientID == 0(HOST)
            //If not 0 another client

            int newLevel = packet.ReadInt();
            int supply = packet.ReadInt();
            int lastSupply = packet.ReadInt();
            bool endless = packet.ReadBool();
            bool objective = packet.ReadBool();
            bool gameComplete = packet.ReadBool();

            if (gameComplete)
            {
                SR_Manager.instance.gameCompleted = gameComplete;
                SR_Manager.instance.stats.ObjectiveComplete = objective;
                SR_Manager.instance.CompleteGame();
            }
            else
                SR_Manager.instance.SetLevel_Client(newLevel, supply, lastSupply, endless);


        }

        //GameOptions Send ----------------------------------------------
        public void GameOptions_Send()
        {
            if (!Networking.ServerRunning() || Networking.IsClient())
                return;

            Packet packet = new Packet(gameOptions_ID);
                packet.Write(SR_Manager.instance.optionPlayerCount);
                packet.Write(SR_Manager.instance.optionDifficulty);
                packet.Write(SR_Manager.instance.optionFreeBuyMenu);
                packet.Write(SR_Manager.instance.optionSpawnLocking);
                packet.Write(SR_Manager.instance.optionStartLevel);
                packet.Write(SR_Manager.instance.optionPlayerHealth);
                packet.Write(SR_Manager.instance.optionItemSpawner);
                packet.Write(SR_Manager.instance.optionCaptureZone);
                packet.Write(SR_Manager.instance.optionCaptureOrder);
                packet.Write(SR_Manager.instance.optionCaptures);
                packet.Write(SR_Manager.instance.optionRespawn);
                packet.Write(SR_Manager.instance.optionMaxEnemies);
                //packet.Write(SR_Manager.instance.faction.name);
            ServerSend.SendTCPDataToAll(packet, true);
        }

        //Game Options Received
        void GameOptions_Handler(int clientID, Packet packet)
        {
            int optionPlayerCount = packet.ReadInt();
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
            int optionMaxEnemies = packet.ReadInt();
            //string factionID = packet.ReadString();

            SR_Manager.instance.Network_GameOptions(optionPlayerCount, optionDifficulty, optionFreeBuyMenu, optionSpawnLocking, optionStartLevel,
                optionPlayerHealth, optionItemSpawner, optionCaptureZone, optionOrder, optionCaptures, optionRespawn, optionMaxEnemies);
        }

        //Captured Send ----------------------------------------------
        public void CapturedZone_Send()
        {
            if (!Networking.ServerRunning() || Networking.IsHost())
                return;

            Debug.Log("Client Sending CaptureZone");

            Packet packet = new Packet(capturedZone_ID);
                ClientSend.SendTCPData(packet, true);
        }

        //Captured Received
        void CapturedZone_Handler(int clientID, Packet packet)
        {
            Debug.Log("Received Client Captured zone");
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

            packet.Write(SR_Manager.instance.CapturesTotal);
            packet.Write(SR_Manager.instance.stats.GameTime);
            packet.Write(SR_Manager.instance.stats.Kills);

            ServerSend.SendTCPDataToAll(packet, true);
        }

        //Stats Received
        void UpdateStats_Handler(int clientID, Packet packet)
        {
            int captures = packet.ReadInt();
            if(captures > SR_Manager.instance.CapturesTotal)
                SR_Manager.instance.CapturesTotal = captures;

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

            Debug.Log("Client Sending Request Sync");

            Packet packet = new Packet(requestSync_ID);
            ClientSend.SendTCPData(packet, true);
        }

        //RequestSync Received TODO make corouteen
        void RequestSync_Handler(int clientID, Packet packet)
        {
            Debug.Log("Received Client Request Sync");

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