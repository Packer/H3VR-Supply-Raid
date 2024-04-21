using BepInEx;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SupplyRaid
{
    [System.Serializable]
    public class SR_CharacterPreset
    {
        [Tooltip("(REQUIRED) Name of this character")]
        public string name = "Character Name";
        [Tooltip("Short explanation of this character"), Multiline(6)]
        public string description = "Put a brief explination of the character here";
        [Tooltip("(REQUIRED) The menu category this character will go in, Recommend mod creator names etc")]
        public string category = "Mod";
        public string factionName = "";
        [Tooltip("Points player receives per capture, endless reuses the last array position")]
        public int[] pointsLevel = new int[1];
        public bool pointsCatchup = true;

        // Duplicator
        [Tooltip("Cost of a new Magazine - if -1 disable")]
        public float newMagazineCost = 1;
        [Tooltip("Cost of upgrading a magazine - if -1 disable")]
        public float upgradeMagazineCost = 2;
        [Tooltip("Cost of Duplicating a magazine - if -1 disable")]
        public float duplicateMagazineCost = 1;

        //New
        public float[] powerMultiplier = new float[10];
        public bool perRound = false;

        [Tooltip("Custom Mod Cost")]
        public int modCost = 1;

        //Recycler
        [Tooltip("How many points the players get for recycling weapons")]
        public int recyclerPoints = 1;

        [Tooltip("How many points the players get for recycling the golden token")]
        public int recyclerTokens = 1;

        //Ammo Table
        [Tooltip("0 = False\n1 = True\n3 = Buy Once\n 4 = Pay Every Time")]
        public int modeRearming = 1;
        public int rearmingCost = 0;
        [Tooltip("0 = False\n1 = True\n3 = Buy Once\n 4 = Pay Every Time")]
        public int modeSpeedLoaders = 1;
        public int speedLoadersCost = 0;
        [Tooltip("0 = False\n1 = True\n3 = Buy Once\n 4 = Pay Every Time")]
        public int modeClips = 1;
        public int clipsCost = 0;
        [Tooltip("0 = False\n1 = True\n3 = Buy Once\n 4 = Pay Every Time")]
        public int modeRounds = 1;
        public int roundsCost = 0;

        //Panels
        public bool disableAmmoTable = false;
        public bool disableModtable = false;
        public bool disableBuyMenu = false;
        public bool disableDuplicator = false;
        public bool disableRecycler = false;

        //Death
        public int deathMode = 0;   //0 = Instant Respawn, 1 = count down Timer, 2 = respawn on next Capture, 3 = lives
        public float deathCount = 0;   //lives, Timer etc

        [Tooltip("Cost of each ammo upgrade, 0 is normally free as its the standard - 28 Ammo Types - if set to -1 disable")]
        public int[] ammoUpgradeCost = new int[28];

        //Mod Table
        [Tooltip("Cost of each attachment - 16 Attachment Types, if set to -1 disable")]
        public int[] attachmentsCost = new int[16];


        public List<string> startGearCategories = new List<string>();
        [Tooltip("(REQUIRED) What buy categories are available to this character")]
        public List<SR_PurchaseCategory> purchaseCategories = new List<SR_PurchaseCategory>();

        
        public List<SR_LootCategory> lootCategories = new List<SR_LootCategory>();

        [Tooltip("Globally Removes these ObjectIDs from ALL system including Attachments and Ammo types")]
        public List<string> subtractionObjectIDs = new List<string>();

        //PRIVATE after we create the functions
        [Tooltip("Preview image of the character when selected")]
        private Sprite thumbnail;
        private int[] startGearIndex = new int[0];

        private string thumbnailPath = "";

        /*
        public void ExportJson()
        {
            using (StreamWriter streamWriter = new StreamWriter(Application.dataPath + "/SR_Character_" + name + ".json"))
            {
                string json = JsonUtility.ToJson(this, true);
                streamWriter.Write(json);
            }
        }
        */

        public void SetupCharacterPreset(List<SR_ItemCategory> items)
        {
            //Debug.Log("Setting up Character Gear for " + name);
            startGearIndex = new int[startGearCategories.Count];

            //Setup Start Gear
            for (int x = 0; x < startGearCategories.Count; x++)
            {
                startGearIndex[x] = -1;
                for (int y = 0; y < items.Count; y++)
                {
                    if (items[y] != null && startGearCategories[x] == items[y].name)
                    {
                        startGearIndex[x] = y;
                        break;
                    }
                }

                if (startGearIndex[x] == -1)
                { 
                    Debug.LogError("Supply Raid - Missing character start gear index: " + x);
                }
            }

            //Setup Purchase Categories Indexes
            List<SR_PurchaseCategory> removeCategory = new List<SR_PurchaseCategory>();

            for (int i = 0; i < purchaseCategories.Count; i++)
            {
                for (int y = 0; y < items.Count; y++)
                {
                    if (purchaseCategories[i].itemCategory == items[y].name)
                    {
                        purchaseCategories[i].SetIndex(y);
                        break;
                    }
                }

                if (purchaseCategories[i].GetIndex() == -1)
                {
                    Debug.Log("Supply Raid - Purchase Category " + purchaseCategories[i].itemCategory + " could not set item Category");
                    removeCategory.Add(purchaseCategories[i]);
                }
            }

            //Remove any non-functional Purchase category
            for (int i = 0; i < removeCategory.Count; i++)
            {
                purchaseCategories.Remove(removeCategory[i]);
            }

            List<SR_LootCategory> removeLootCategory = new List<SR_LootCategory>();

            //Loot Categories
            for (int i = 0; i < lootCategories.Count; i++)
            {
                //Already setup
                if (lootCategories[i].GetIndex() != -1)
                    continue;

                for (int y = 0; y < items.Count; y++)
                {
                    if (lootCategories[i].itemCategory == items[y].name)
                    {
                        lootCategories[i].SetIndex(y);
                        break;
                    }
                }

                if (lootCategories[i].GetIndex() == -1)
                {
                    Debug.Log("Supply Raid - Purchase Category " + lootCategories[i].itemCategory + " could not set item Category");
                    removeLootCategory.Add(lootCategories[i]);
                }
            }

            //Remove any non-functional Loot category
            for (int i = 0; i < removeLootCategory.Count; i++)
            {
                lootCategories.Remove(removeLootCategory[i]);
            }

            //Assign the Faction
            if (factionName != "" && factionName != " ")
            {
                for (int i = 0; i < SR_Manager.instance.factions.Count; i++)
                {
                    if (SR_Manager.instance.factions[i] != null && factionName == SR_Manager.instance.factions[i].name)
                    {
                        break;
                    }
                }
            }

            //Debug.Log("Finished setup for " + name);
        }

        public void SetupThumbnailPath(string thumbPath)
        {
            thumbnailPath = thumbPath;
        }
        public Sprite Thumbnail()
        {
            if (thumbnailPath == "")
            {
                Debug.LogError("Supply Raid - Thumbnail not defined for character : " + category + "/" + name);
                return null;
            }

            if (thumbnail == null)
                thumbnail = SR_Global.LoadSprite(thumbnailPath);

            if (thumbnail == null)
                return SR_Manager.instance.fallbackThumbnail;

            return thumbnail;
        }

        public int StartGearLength()
        {
            return startGearCategories.Count;
        }

        public SR_ItemCategory StartGear(int i)
        {
            if (i != -1 && startGearIndex[i] != -1)
                return SR_Manager.instance.itemCategories[startGearIndex[i]];

            return null;
        }


        public bool HasRequirement()
        {
            if (name == "")
                return false;
            if (category == "")
                return false;

            return true;
        }

	}

    [System.Serializable]
    public class SR_LootCategory
    {
        private int index = -1;
        public float chance = 0.01f;
        public string itemCategory;
        public int levelUnlock = 0;
        public int levelLock = -1;

        public void SetIndex(int i)
        {
            index = i;
        }

        public int GetIndex()
        {
            return index;
        }

        public SR_ItemCategory ItemCategory()
        {
            if (index != -1)
            {
                return SR_Manager.instance.itemCategories[index];
            }

            return null;
        }
    }


    [System.Serializable]
    public class SR_PurchaseCategory
    {
        private int index = -1;
        public string itemCategory;
        public int cost = 1;

        public void SetIndex(int i)
        {
            index = i;
        }
        public int GetIndex()
        {
            return index;
        }

        public SR_ItemCategory ItemCategory()
        {
            if (index != -1)
            {
                return SR_Manager.instance.itemCategories[index];
            }

            return null;
        }
    }
}