using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Supply_Raid_Editor
{
    [System.Serializable]
    public class SR_CharacterPreset
    {
        public string name = "Character Name";
        public string description = "Put a brief explination of the character here";
        public string category = "Mod";
        public int points = 2;
        public int pointsPerLevel = 1;

        public int newMagazineCost = 1;
        public int upgradeMagazineCost = 2;
        public int duplicateMagazineCost = 1;
        public int recyclerPoints = 1;

        //JSON string refference
        public string factionName = "";
        public List<string> startGearCategories = new List<string>();
        public List<SR_PurchaseCategory> purchaseCategories = new List<SR_PurchaseCategory>();

        public void ExportJson()
        {
            Debug.Log("Exporting Item");
            using (StreamWriter streamWriter = new StreamWriter(Application.dataPath + "/items/SR_Character_" + name + ".json"))
            {
                string json = JsonUtility.ToJson(this, true);
                streamWriter.Write(json);
            }
        }
    }

    [System.Serializable]
    public class SR_PurchaseCategory
    {
        public string name = "Item";
        public string itemCategory = "";
        public int cost = 1;
    }
}