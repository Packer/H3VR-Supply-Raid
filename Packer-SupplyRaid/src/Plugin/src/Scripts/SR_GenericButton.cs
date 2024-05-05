using FistVR;
using H3MP.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace SupplyRaid
{
	public class SR_GenericButton : MonoBehaviour
	{
		public int index = -1;
        public int value = -1;
		public Image thumbnail;
		public Button button;
        public Text text;
        public Text textB;
        public GameObject go;

        public void SelectCharacter()
        {
            if (SR_Manager.instance.characters.Count > 0)
            {
                SR_Manager.instance.character = SR_Manager.instance.characters[index];
                SR_Manager.PlayConfirmSFX();
            }
            else
            {
                SR_Manager.PlayErrorSFX();
                return;
            }

            SR_Menu.instance.UpdateCharacter();

            //Update Faction if one is assigned
            if (SR_Manager.Character().factionName != "")
            {
                SR_Manager.instance.faction = SR_Global.GetFactionByName(SR_Manager.Character().factionName);
                SR_Menu.instance.UpdateFaction();
            }
        }

        public void SelectCharacterCategory()
        {
            go.GetComponent<SR_Menu>().OpenCharacterCategory(index);
            SR_Manager.PlayConfirmSFX();
        }

        public void SelectFactionCategory()
        {
            go.GetComponent<SR_Menu>().OpenFactionCategory(index);
            SR_Manager.PlayConfirmSFX();
        }

        public void SelectFaction()
        {
            if (index < SR_Manager.instance.factions.Count)
            {
                SR_Manager.instance.faction = SR_Manager.instance.factions[index];
                SR_Manager.PlayConfirmSFX();
            }
            else
            {
                SR_Manager.PlayErrorSFX();
                return;
            }

            SR_Manager.PlayConfirmSFX();
            SR_Menu.instance.UpdateFaction();
        }

        public void OpenBuyMenu()
        {
            SR_BuyMenu.instance.OpenMenu(index);
        }

        public void BuyLoot()
        {
            SR_BuyMenu.instance.SpawnLootButton(index);
        }

        public void SpawnAtPlayer()
        {
            //if (index == -1 || Networking.GetPlayer(index).IFF != GM.CurrentPlayerBody.GetPlayerIFF())
            if (index == -1)
            {
                Debug.Log("Supply Raid: Attempting to spawn at player without an index set!");
                return;
            }

            Transform playerHead = Networking.GetPlayer(index).head;

            if (playerHead != null)
            {
                //TODO do physics checks to find best position for player
                Vector3 newPos = playerHead.position - playerHead.forward;
                newPos.y = playerHead.position.y - 0.4f;

                GM.CurrentMovementManager.TeleportToPoint(newPos, true, playerHead.rotation.eulerAngles);
            }
            else
                Debug.LogError("Supply Raid: Player " + index + " is missing their teleport transform");
        }

        public void BuyModAttachment()
        {
            go.GetComponent<SR_ModTable>().BuyAttachment(index);
        }

        public void TryBuyAmmo()
        {
            if (!TryPurchaseAmmoType())
                return;

            //Set Ammo Type
            if (SR_AmmoSpawner.instance.purchasedAmmoTypes[index])
            {
                SR_Manager.PlayConfirmSFX();
                //SetAmmoType
                //Debug.Log("Setting Via Button");
                SR_AmmoSpawner.instance.SetAmmoType((AmmoEnum)index);
            }
        }

        bool TryPurchaseAmmoType()
        {
            if (SR_AmmoSpawner.instance == null || SR_Manager.instance.character.ammoUpgradeCost[index] <= -1)
                return false;

            //Try Buy
            if (SR_AmmoSpawner.instance.purchasedAmmoTypes[index] == false)
            {
                if (SR_Manager.EnoughPoints(SR_Manager.instance.character.ammoUpgradeCost[index]))
                {
                    if (SR_Manager.SpendPoints(SR_Manager.instance.character.ammoUpgradeCost[index]))
                    {
                        SR_AmmoSpawner.instance.purchasedAmmoTypes[index] = true;
                        text.text = ""; //Blank Cost because we own it

                        SR_Manager.PlayPointsGainSFX();
                    }
                }
                else
                {
                    SR_Manager.PlayFailSFX();
                    return false;
                }
            }

            return true;
        }

        public void TryBuyAmmoRound()
        {
            if (!TryPurchaseAmmoType())
                return;

            SR_AmmoSpawner.instance.BuySpecificRound(index);
        }

        public void SelectProfile()
        {
            SR_Menu.instance.SetProfile(text.text);
        }
    }
}