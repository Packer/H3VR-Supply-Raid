using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FistVR;

namespace SupplyRaid
{
    [System.Serializable]
    public class SR_ItemCategory
    {
        public string name;

        [Header("UI")]
        private string thumbnailPath = "icon.png";
        private Sprite thumbnail;

        public string category = "";
        public string subCategory = "";

        [Tooltip("Magazine/Clip Min Capacity for this loot table")]
        public int minCapacity = -1;
        [Tooltip("Magazine/Clip Max Capacity for this loot table")]
        public int maxCapacity = -1;

        [Tooltip("Magazine/Clip/Speed Loaders/Rounds that spawn with this itemCategory")]
        public int ammoLimitedCount = -1;
        public int ammoLimitedCountMin = -1; //If less than min then gets set to min
        // Loot
        public int ammoLimitedLootCount = -1;
        public int ammoLimitedLootCountMin = -1;
        //-----------------------------------
        public int ammoSpawnLockedCount = -1;
        public int ammoSpawnLockedCountMin = -1;
        // Loot
        public int ammoSpawnLockedLootCount = -1;
        public int ammoSpawnLockedLootCountMin = -1;
        //-----------------------------------

        [Tooltip("Do weapons spawn with their extra attachments")]
        public bool requiredAttachments = true;

        //Do we use the loot tag system
        public bool lootTagsEnabled = true;

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
        public int firstYearMin = -1;
        public int firstYearMax = -1;

        [Header("Subtraction Items")]
        [Tooltip("These defined ObjectIDs will be subtracted from the category pool")]
        public List<string> subtractionID = new List<string>();

        public void SetupThumbnailPath(string thumbPath)
        {
            thumbnailPath = thumbPath;
        }

        public Sprite Thumbnail()
        {
            if (thumbnailPath == "")
            {
                Debug.LogError("Supply Raid - Thumbnail not defined for Item category: " + name);
                return null;
            }

            if (thumbnail == null)
                thumbnail = SR_Global.LoadSprite(thumbnailPath);

            if (thumbnail == null)
                return SR_Manager.instance.fallbackThumbnail;

            return thumbnail;
        }

        public void ClearMissingObjectIDs()
        {
            if (objectGroups == null || objectGroups.Count <= 0)
                return;

            List<string> clearStrings = new List<string>();

            for (int x = 0; x < objectGroups.Count; x++)
            {
                for (int y = 0; y < objectGroups[x].objectID.Count; y++)
                {
                    //Remove any non-loaded mod objects
                    if (!IM.OD.ContainsKey(objectGroups[x].objectID[y]))
                        clearStrings.Add(objectGroups[x].objectID[y]);
                }

                for (int i = 0; i < clearStrings.Count; i++)
                {
                    objectGroups[x].objectID.Remove(clearStrings[i]);
                }
            }

            for (int i = objectGroups.Count - 1; i >= 0; i--)
            {
                if (objectGroups[i].objectID.Count <= 0)
                    objectGroups.Remove(objectGroups[i]);
            }
        }

        public LootTable InitializeLootTable()
        {
            //Remove Unloaded Mods
            ClearMissingObjectIDs();

            LootTable table = new LootTable();
            table.Initialize(
                type,
                eras.Count > 0 ? eras : null,
                sizes.Count > 0 ? sizes : null,
                actions.Count > 0 ? actions : null,
                modes.Count > 0 ? modes : null,
                excludeModes.Count > 0 ? excludeModes : null,
                feedoptions.Count > 0 ? feedoptions : null,
                mounts.Count > 0 ? mounts : null,
                roundPowers.Count > 0 ? roundPowers : null,
                features.Count > 0 ? features : null,
                meleeStyles.Count > 0 ? meleeStyles : null,
                meleeHandedness.Count > 0 ? meleeHandedness : null,
                powerupTypes.Count > 0 ? powerupTypes : null,
                thrownTypes.Count > 0 ? thrownTypes : null,
                minCapacity,
                maxCapacity);


            //Min Year
            if (firstYearMin != -1)
            {
                for (int num = table.Loot.Count - 1; num >= 0; num--)
                {
                    FVRObject fVRObject = table.Loot[num];
                    if(fVRObject.TagFirearmFirstYear < firstYearMin)
                    {
                        table.Loot.RemoveAt(num);
                        continue;
                    }
                }
            }

            //Max Year
            if (firstYearMax != -1)
            {
                for (int num = table.Loot.Count - 1; num >= 0; num--)
                {
                    FVRObject fVRObject = table.Loot[num];
                    if (fVRObject.TagFirearmFirstYear > firstYearMax)
                    {
                        table.Loot.RemoveAt(num);
                        continue;
                    }
                }
            }

            //Tag Set Removal
            if (set != null && set.Count > 0)
            {
                for (int num = table.Loot.Count - 1; num >= 0; num--)
                {
                    FVRObject fVRObject = table.Loot[num];
                    if (set != null && !set.Contains(fVRObject.TagSet))
                    {
                        table.Loot.RemoveAt(num);
                        continue;
                    }
                }
            }

            //Thrown Damage
            if (thrownDamage != null && thrownDamage.Count > 0)
            {
                for (int num = table.Loot.Count - 1; num >= 0; num--)
                {
                    FVRObject fVRObject = table.Loot[num];
                    if (!thrownDamage.Contains(fVRObject.TagThrownDamageType))
                    {
                        table.Loot.RemoveAt(num);
                        continue;
                    }
                }
            }

            //Country of Origin
            if (countryOfOrigins != null && countryOfOrigins.Count > 0)
            {
                for (int num = table.Loot.Count - 1; num >= 0; num--)
                {
                    FVRObject fVRObject = table.Loot[num];
                    if (!countryOfOrigins.Contains(fVRObject.TagFirearmCountryOfOrigin))
                    {
                        table.Loot.RemoveAt(num);
                        continue;
                    }
                }
            }

            //Collect items
            List<int> detractItems = new List<int>();

            //Remove any subtraction Items
            if (subtractionID != null && subtractionID.Count > 0)
            {
                for (int i = 0; i < table.Loot.Count; i++)
                {
                    if (subtractionID.Contains(table.Loot[i].ItemID))
                    {
                        detractItems.Add(i);
                    }
                }
            }

            //Remove Items - Count Down
            for (int i = detractItems.Count - 1; i >= 0; i--)
            {
                table.Loot.RemoveAt(detractItems[i]);
            }

            //Remove GLOBAL character subtractions from character list
            table = SR_Global.RemoveGlobalSubtractionOnTable(table);

            //Debug.Log("Supply Raid - Loot Table Post " + name + " | Size: " + table.Loot.Count);

            return table;
        }

        [System.Serializable]
        public class ObjectGroup
        {
            public string name;
            public List<string> objectID = new List<string>();
        }
    }
}