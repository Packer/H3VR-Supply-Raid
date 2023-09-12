using FistVR;
using H3MP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SupplyRaid
{

	public class SR_SpawnMenu : MonoBehaviour
	{

        //Inspector
        [Header("H3MP")]
        [SerializeField] GameObject h3mp;
        [SerializeField] GameObject playerPrefab;
        [SerializeField] Transform playerContent;


        //Internal
        List<SR_PlayerSpawn> playerList = new List<SR_PlayerSpawn>();
        private int lastPlayerCount = 0;
        private int playerCount = 0;    //Use H3MP for this


        void Start()
        {

        }

        void Update()
        {
            //if(h3mp.activeSelf != true && Mod.managerObject != null)
            //    h3mp.SetActive(true);

            if (h3mp.activeSelf == true && playerCount != lastPlayerCount)
            {
                //Update List

                for (int i = 0; i < playerList.Count; i++)
                {
                    Destroy(playerList[i].gameObject);
                }
                playerList.Clear();

                for (int i = 0; i < playerCount; i++)
                {
                    //TODO Make sure we don't add the local player, its pointless

                    SR_PlayerSpawn playerBtn = Instantiate(playerPrefab, playerContent).GetComponent<SR_PlayerSpawn>();
                    playerBtn.gameObject.SetActive(true);

                    //Setup
                    playerBtn.playerName.text = "Default Username"; //Todo get player name

                    if (playerBtn.player)
                        playerBtn.player = transform;   //TODO get player transform/position

                    playerList.Add(playerBtn);
                }
                lastPlayerCount = playerCount;
            }
        }

		public void RespawnAtLastSupply()
		{
            int lastID = SR_Manager.instance.lastSupplyID;
            //Teleport to spawn
            GM.CurrentMovementManager.TeleportToPoint(SR_Manager.instance.supplyPoints[lastID].respawn.position,
                true,
                SR_Manager.instance.supplyPoints[lastID].respawn.forward);
        }
	}
}