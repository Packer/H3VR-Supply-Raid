using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Supply_Raid_Editor
{
    [System.Serializable]
    public class SR_ItemCategory
    {
        public string name;
        public string category = "";

        [Tooltip("Magazine/Clip Min Capacity for this loot table")]
        public int minCapacity = -1;
        [Tooltip("Magazine/Clip Max Capacity for this loot table")]
        public int maxCapacity = -1;

        [Tooltip("Min Level before this category can be spawned")]
        public int minLevel = -1;
        [Tooltip("Inclusive Max Level this category is available for")]
        public int maxLevel = -1;

        [Tooltip("Magazine/Clip/Speed Loaders/Rounds that spawn with this itemCategory")]
        public int ammoLimitedCount = -1;
        public int ammoLimitedCountMin = -1; //If less than min then gets set to min

        public int ammoLimitedMagazineCount = -1;
        public int ammoLimitedMagazineCountMin = -1;

        public int ammoLimitedClipCount = -1;
        public int ammoLimitedClipCountMin = -1;

        public int ammoLimitedSpeedLoaderCount = -1;
        public int ammoLimitedSpeedLoaderCountMin = -1;

        public int ammoLimitedRoundCount = -1;
        public int ammoLimitedRoundCountMin = -1;

        //-----------------------------------
        public int ammoSpawnLockedCount = -1;
        public int ammoSpawnLockedCountMin = -1;
        //-----------------------------------
        
        [Tooltip("Do weapons spawn with their extra attachments")]
        public bool requiredAttachments = true;

        //How many of the same item spawns in the category
        public int spawnCount = 1;
        
        //Do we use the loot tag system
        public bool lootTagsEnabled = true;

        //Get Loot relevent to player from loot drops
        public bool lootTagsFromQuickbelt = false;

        [Header("Manual Setup Table")]
        //Groups of objects that get spawned if selected
        public List<ObjectGroup> objectGroups = new List<ObjectGroup>();
        
        [Header("Loot Table")]
        public LootTable.LootTableType type = LootTable.LootTableType.Firearm;
        public List<FVRObject.OTagSet> set = new List<FVRObject.OTagSet>();
        public List<FVRObject.OTagEra> eras = new List<FVRObject.OTagEra>();
        public List<FVRObject.OTagFirearmSize> sizes = new List<FVRObject.OTagFirearmSize>();
        public List<FVRObject.OTagFirearmAction> actions = new List<FVRObject.OTagFirearmAction>();
        public List<FVRObject.OTagFirearmFiringMode> modes = new List<FVRObject.OTagFirearmFiringMode>(); 
        public List<FVRObject.OTagFirearmFiringMode> excludeModes = new List<FVRObject.OTagFirearmFiringMode>();
        public List<FVRObject.OTagFirearmFeedOption> feedoptions = new List<FVRObject.OTagFirearmFeedOption>(); 
        public List<FVRObject.OTagFirearmMount> mounts = new List<FVRObject.OTagFirearmMount>();
        public List<FVRObject.OTagFirearmRoundPower> roundPowers = new List<FVRObject.OTagFirearmRoundPower>(); 
        public List<FVRObject.OTagAttachmentFeature> features = new List<FVRObject.OTagAttachmentFeature>();
        public List<FVRObject.OTagMeleeStyle> meleeStyles = new List<FVRObject.OTagMeleeStyle>(); 
        public List<FVRObject.OTagMeleeHandedness> meleeHandedness = new List<FVRObject.OTagMeleeHandedness>();
        public List<FVRObject.OTagPowerupType> powerupTypes = new List<FVRObject.OTagPowerupType>(); 
        public List<FVRObject.OTagThrownType> thrownTypes = new List<FVRObject.OTagThrownType>();
        public List<FVRObject.OTagThrownDamageType> thrownDamage = new List<FVRObject.OTagThrownDamageType>();
        public List<FVRObject.OTagFirearmCountryOfOrigin> countryOfOrigins = new List<FVRObject.OTagFirearmCountryOfOrigin>();

        [Header("Subtraction Items")]
        [Tooltip("These defined ObjectIDs will be subtracted from the category pool")]
        public List<string> subtractionID = new List<string>();
    }

    [System.Serializable]
    public class ObjectGroup
    {
        public string name;
        public List<string> objectID = new List<string>();
        private int index = -1;


        public int Index   // property
        {
            get { return index; }   // get method
            set { index = value; }  // set method
        }

    }
}