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

        void Awake()
        {
            instance = this;
        }

        // Use this for initialization
        void Start()
        {
            if(Mod.managerObject != null)
			    Mod.OnConnection += OnConnection;

			if(Mod.managerObject != null && (ThreadManager.host || Client.isFullyConnected))
			{
				SetupPacketTypes();
			}
        }
		
		void OnConnection()
		{
			SetupPacketTypes();

            if (!ThreadManager.host)
                SR_Manager.instance.SetLocalAsClient();

        }
		
        void SetupPacketTypes()
        {
            //Server
            if (Networking.IsHost())
            {
                levelUpdate_ID = Server.RegisterCustomPacketType("SR_LevelUpdate");
                Mod.customPacketHandlers[levelUpdate_ID] = LevelUpdate_Handler;

                gameOptions_ID = Server.RegisterCustomPacketType("SR_GameOptions");
                Mod.customPacketHandlers[gameOptions_ID] = GameOptions_Handler;

                capturedZone_ID = Server.RegisterCustomPacketType("SR_CapturedZone");
                Mod.customPacketHandlers[capturedZone_ID] = CapturedZone_Handler;
            }
            else //Client
            {
                if (!Mod.registeredCustomPacketIDs.ContainsKey("SR_LevelUpdate"))
                {
                    ClientSend.RegisterCustomPacketType("SR_LevelUpdate");
                    Mod.CustomPacketHandlerReceived += LevelUpdate_Received;
                }
                else
                    Mod.customPacketHandlers[Mod.registeredCustomPacketIDs["SR_LevelUpdate"]] = LevelUpdate_Handler;
                

                if (!Mod.registeredCustomPacketIDs.ContainsKey("SR_GameOptions"))
                {
                    ClientSend.RegisterCustomPacketType("SR_GameOptions");
                    Mod.CustomPacketHandlerReceived += GameOptions_Received;
                }
                else
                    Mod.customPacketHandlers[Mod.registeredCustomPacketIDs["SR_GameOptions"]] = GameOptions_Handler;

                if (!Mod.registeredCustomPacketIDs.ContainsKey("SR_CapturedZone"))
                {
                    ClientSend.RegisterCustomPacketType("SR_CapturedZone");
                    Mod.CustomPacketHandlerReceived += CapturedZone_Received;
                }
                else
                    Mod.customPacketHandlers[Mod.registeredCustomPacketIDs["SR_CapturedZone"]] = CapturedZone_Handler;
            }
        }

        // Update is called once per frame
        void Update()
        {
        }

        //---------------------------------------------------------------
        // 
        //---------------------------------------------------------------

        //Level Update Send
        public void LevelUpdate_Send(bool gameComplete)
        {
            if (!ThreadManager.host)
                return;

            Packet packet = new Packet(levelUpdate_ID);
            packet.Write(SR_Manager.instance.level);
            packet.Write(SR_Manager.instance.supplyID);
            packet.Write(SR_Manager.instance.lastSupplyID);
            packet.Write(SR_Manager.instance.endless);
            packet.Write(SR_Manager.instance.statObjectiveComplete);
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
                SR_Manager.instance.statObjectiveComplete = objective;
                SR_Manager.instance.CompleteGame();
            }
            else
                SR_Manager.instance.SetLevel_Client(newLevel, supply, lastSupply, endless);

            //Do NewLevelChangehere();
            //SR_Manager.instance.SetLevel(newLevel);
        }

        //GameOptions Send
        public void GameOptions_Send()
        {
            if (Mod.managerObject == null || !ThreadManager.host)
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
            packet.Write(SR_Manager.instance.optionLinear);
            packet.Write(SR_Manager.instance.optionCaptures);
            packet.Write(SR_Manager.instance.optionRespawn);
            packet.Write(SR_Manager.instance.optionMaxEnemies);

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
            bool optionLinear = packet.ReadBool();
            int optionCaptures = packet.ReadInt();
            bool optionRespawn = packet.ReadBool();
            int optionMaxEnemies = packet.ReadInt();

            SR_Manager.instance.Network_GameOptions(optionPlayerCount, optionDifficulty, optionFreeBuyMenu, optionSpawnLocking, optionStartLevel,
                optionPlayerHealth, optionItemSpawner, optionCaptureZone, optionLinear, optionCaptures, optionRespawn, optionMaxEnemies);
        }

        //Captured Send
        public void CapturedZone_Send()
        {
            if (Mod.managerObject == null || ThreadManager.host)
                return;

            Packet packet = new Packet(capturedZone_ID);
            ClientSend.SendTCPData(packet, true);
        }

        //Captured Received
        void CapturedZone_Handler(int clientID, Packet packet)
        {
            SR_Manager.instance.GameCompleteCheck(SR_Manager.instance.level + 1);
        }

        //---------------------------------------------------------------
        //(Client) Packet Handlers
        //---------------------------------------------------------------

        void LevelUpdate_Received(string handlerID, int index)
		{
			if(handlerID == "SR_LevelUpdate")
            {
                Mod.customPacketHandlers[index] = LevelUpdate_Handler;
                levelUpdate_ID = index;
            }

            Mod.CustomPacketHandlerReceived -= LevelUpdate_Received;
        }

        void GameOptions_Received(string handlerID, int index)
        {
            if (handlerID == "SR_GameOptions")
            {
                Mod.customPacketHandlers[index] = GameOptions_Handler;
                gameOptions_ID = index;
            }

            Mod.CustomPacketHandlerReceived -= GameOptions_Received;
        }

        void CapturedZone_Received(string handlerID, int index)
        {
            if (handlerID == "SR_CapturedZone")
            {
                Mod.customPacketHandlers[index] = CapturedZone_Handler;
                capturedZone_ID = index;
            }

            Mod.CustomPacketHandlerReceived -= CapturedZone_Received;
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
			Mod.OnConnection -= OnConnection;
			Mod.CustomPacketHandlerReceived -= LevelUpdate_Received;
			Mod.CustomPacketHandlerReceived -= GameOptions_Received;
			Mod.CustomPacketHandlerReceived -= CapturedZone_Received;
		}
    }
}