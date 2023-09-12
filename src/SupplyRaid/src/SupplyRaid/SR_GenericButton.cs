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
            SR_Manager.instance.character = SR_Manager.instance.characters[index];
            SR_Menu.instance.UpdateCharacter();

            if (SR_Manager.instance.character.faction != null)
            {
                SR_Manager.instance.faction = SR_Manager.instance.character.faction;
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

        public void DisableGameObjects()
        {
            for (int i = 0; i < disableGO.Length; i++)
            {
                disableGO[i].SetActive(false);
            }
        }
    }
}