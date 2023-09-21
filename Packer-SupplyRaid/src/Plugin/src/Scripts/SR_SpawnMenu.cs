using FistVR;
using H3MP.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SupplyRaid
{

	public class SR_SpawnMenu : MonoBehaviour
    {
        [SerializeField] Text respawnTimer;
        [SerializeField] GameObject spawnButtons;

        //Inspector
        [Header("H3MP")]
        [SerializeField] GameObject h3mp;
        [SerializeField] GameObject playerPrefab;
        [SerializeField] Transform playerContent;



        //Internal
        List<GameObject> playerButtons = new List<GameObject>();
        private int playerCount = 0;


        void Start()
        {
            if (Networking.ServerRunning())
                h3mp.SetActive(true);
        }

        void Update()
        {
            if (!Networking.ServerRunning())
                return;

            if (playerCount != Networking.GetPlayerCount())
            {
                playerCount = Networking.GetPlayerCount();

                //Clear old buttons
                for (int i = 0; i < playerButtons.Count; i++)
                {
                    Destroy(playerButtons[i]);
                }
                playerButtons.Clear();

                int[] playerIDs = Networking.GetPlayerIDs();

                //Create buttons
                for (int i = 0; i < playerIDs.Length; i++)
                {
                    SR_GenericButton playerBtn = Instantiate(playerPrefab, playerContent).GetComponent<SR_GenericButton>();
                    playerBtn.gameObject.SetActive(true);

                    //Setup
                    playerBtn.text.text = Networking.GetPlayer(playerIDs[i]).username;
                    playerBtn.index = playerIDs[i];

                    playerButtons.Add(playerBtn.gameObject);
                }
            }
        }

		public void RespawnAtLastSupply()
		{
            int lastID = SR_Manager.instance.playerSupplyID;
            //Teleport to spawn
            GM.CurrentMovementManager.TeleportToPoint(SR_Manager.instance.supplyPoints[lastID].respawn.position,
                true,
                SR_Manager.instance.supplyPoints[lastID].respawn.forward);

            SR_Manager.PlayConfirmSFX();
        }
    }
}