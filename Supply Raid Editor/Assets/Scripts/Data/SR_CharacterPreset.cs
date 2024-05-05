using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Supply_Raid_Editor
{
    [System.Serializable]
    public class SR_CharacterPreset
    {
    
//SETTINGS
        [Tooltip("(REQUIRED) Name of this character")]
        public string name = "Character Name";
        [Tooltip("Short explanation of this character"), Multiline(6)]
        public string description = "Put a brief explination of the character here";
        [Tooltip("(REQUIRED) The menu category this character will go in, Recommend mod creator names etc")]
        public string category = "Mod";
        public string factionName = "";

        [Tooltip("Points player receives per capture, endless reuses the last array position")]
        public List<int> pointsLevel = new List<int>();

//Supply Point
        // Duplicator
        [Tooltip("Cost of a new Magazine - if -1 disable")]
        public int newMagazineCost = 1;
        [Tooltip("Cost of upgrading a magazine - if -1 disable")]
        public int upgradeMagazineCost = 2;
        [Tooltip("Cost of Duplicating a magazine - if -1 disable")]
        public int duplicateMagazineCost = 1;

        [Tooltip("Custom Mod Cost")]
        public int modCost = 1;

        //Recycler
        [Tooltip("How many points the players get for recycling weapons")]
        public int recyclerPoints = 1;
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

//BASE SETINGS
        //Death
        public int deathMode = 0;   //0 = Instant Respawn, 1 = count down Timer, 2 = respawn on next Capture, 3 = lives
        public float deathCount = 0;   //lives, Timer etc
//WMAU
        [Tooltip("Cost of each ammo upgrade, 0 is normally free as its the standard - 28 Ammo Types - if set to -1 disable")]
        public int[] ammoUpgradeCost = new int[28];

//WMAU
        [Tooltip("Cost of each attachment - 16 Attachment Types, if set to -1 disable")]
        public int[] attachmentsCost = new int[16];

//Start Gear / Purchase Categories
        public List<string> startGearCategories = new List<string>();
        [Tooltip("(REQUIRED) What buy categories are available to this character")]
        public List<SR_PurchaseCategory> purchaseCategories = new List<SR_PurchaseCategory>();

//Loot Categories / SubtractionIDS
        public List<SR_LootCategory> lootCategories = new List<SR_LootCategory>();

        [Tooltip("Globally Removes these ObjectIDs from ALL system including Attachments and Ammo types")]
        public List<string> subtractionObjectIDs = new List<string>();

        [Tooltip("These objectIDs won't drop off the player")]
        public List<string> dropProtectionObjectIDs = new List<string>();
    }

    [System.Serializable]
    public class SR_LootCategory
    {
        public float chance = 0.01f;
        public string itemCategory;
    }

    [System.Serializable]
    public class SR_PurchaseCategory
    {
        public string itemCategory;
        public int cost = 1;
    }
}