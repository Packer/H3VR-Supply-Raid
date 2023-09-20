using FistVR;
using H3MP.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SupplyRaid
{
	public class SR_GenericButton : MonoBehaviour
	{
		public int index = -1;
		public Image thumbnail;
		public Button button;
        public SR_BuyMenu spawner;
        public Text text;
        public GameObject go;
        public GameObject[] disableGO;

        public void SelectCharacter()
        {
            if (SR_Manager.instance.characters.Count > 0)
                SR_Manager.instance.character = SR_Manager.instance.characters[index];
            else
                return;

            SR_Menu.instance.UpdateCharacter();

            if (SR_Manager.instance.character.Faction() != null)
            {
                SR_Manager.instance.faction = SR_Manager.instance.character.Faction();
                SR_Menu.instance.UpdateFaction();
            }
        }

        public void SelectCharacterCategory()
        {
            go.GetComponent<SR_Menu>().OpenCharacterCategory(index);
        }

        public void SelectFactionCategory()
        {
            go.GetComponent<SR_Menu>().OpenFactionCategory(index);
        }

        public void SelectFaction()
        {
            SR_Manager.instance.faction = SR_Manager.instance.factions[index];
            SR_Menu.instance.UpdateFaction();
        }

        public void BuyLoot()
        {
            spawner.SpawnLootButton(index);
        }

        public void SpawnAtPlayer()
        {
            if (Networking.GetPlayer(index).IFF != GM.CurrentPlayerBody.GetPlayerIFF())
                return;

            Transform playerHead = Networking.GetPlayer(index).head;

            if (playerHead != null)
            {
                //TODO do physics checks to find best position for player
                Vector3 newPos = playerHead.position - playerHead.forward;
                newPos.y = playerHead.position.y - 0.4f;

                GM.CurrentMovementManager.TeleportToPoint(newPos, true, playerHead.rotation.eulerAngles);
            }
            else
                Debug.LogError("Player " + index + " is missing their teleport transform");
        }

        public void BuyModAttachment()
        {
            go.GetComponent<SR_ModTable>().BuyAttachment(index);
        }

        public void TryBuyAmmo()
        {
            SR_AmmoSpawner spawner = go.GetComponent<SR_AmmoSpawner>();

            if (spawner == null)
                return;

            //Try Buy
            if (spawner.purchased[index] == false)
            {
                if (SR_Manager.EnoughPoints(SR_Manager.instance.character.ammoUpgradeCost[index]))
                {
                    if (SR_Manager.SpendPoints(SR_Manager.instance.character.ammoUpgradeCost[index]))
                    {
                        spawner.purchased[index] = true;
                        SR_Manager.PlayPointsGainSFX();
                    }
                }
                else
                {
                    SR_Manager.PlayFailSFX();
                    return;
                }
            }

            //Set Ammo Type
            if(spawner.purchased[index])
            {
                SR_Manager.PlayConfirmSFX();
                //SetAmmoType
                bool set = spawner.SetAmmoType((AmmoEnum)index);
            }
        }

        public void DisableGameObjects()
        {
            for (int i = 0; i < disableGO.Length; i++)
            {
                disableGO[i].SetActive(false);
            }
        }
    }
}