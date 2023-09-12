using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

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

        public void ExportJson(string path)
        {
            using (StreamWriter streamWriter = new StreamWriter(path + "/SR_Character_" + name + ".json"))
            {
                string json = JsonSerializer.Serialize(this);
                streamWriter.Write(json);
            }
        }
    }

    [Serializable]
    public class SR_PurchaseCategory
    {
        public string name = "Item";
        public string itemCategory = "";
        public int cost = 1;
    }
}