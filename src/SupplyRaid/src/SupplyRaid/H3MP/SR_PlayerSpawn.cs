using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FistVR;


namespace SupplyRaid
{
    public class SR_PlayerSpawn : MonoBehaviour
    {
        public Transform player;    //World Position of the player
        public Text playerName;

        public void SpawnAtPlayer()
        {
            if (player != null)
                GM.CurrentMovementManager.TeleportToPoint(this.player.position + -player.forward, true, player.rotation.eulerAngles);
            else
                Debug.Log("Player " + playerName.text + " is missing their teleport transform");
        }
    }
}