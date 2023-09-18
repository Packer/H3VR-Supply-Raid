using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using System.IO;

namespace SupplyRaid
{
    [System.Serializable]
    public class SR_ItemCategory
    {
        public string name;

        [Header("UI")]
        private string thumbnailPath = "icon.png";
        private Sprite thumbnail;

        [Tooltip("Magazine/Clip Min Capacity for this loot table")]
        public int minCapacity = -1;
        [Tooltip("Magazine/Clip Max Capacity for this loot table")]
        public int maxCapacity = -1;

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

        [Header("Subtraction Items")]
        [Tooltip("These defined ObjectIDs will be subtracted from the category pool")]
        public List<string> subtractionID = new List<string>();


        public void ExportJson()
        {
            Debug.Log("Exporting Item");
            using (StreamWriter streamWriter = new StreamWriter(Application.dataPath + "/items/SR_IC_" + name + ".json"))
            {
                string json = JsonUtility.ToJson(this, true);
                streamWriter.Write(json);
            }
        }

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


            //Tag Set Removal
            for (int num = table.Loot.Count - 1; num >= 0; num--)
            {
                FVRObject fVRObject = table.Loot[num];
                if (set != null && !set.Contains(fVRObject.TagSet))
                {
                    table.Loot.RemoveAt(num);
                    continue;
                }
            }

            //Remove any subtraction Items
            if (subtractionID != null && subtractionID.Count > 0)
            {
                //Collect items
                List<int> detractItems = new List<int>();
                for (int i = 0; i < table.Loot.Count; i++)
                {
                    if (subtractionID.Contains(table.Loot[i].ItemID))
                    {
                        detractItems.Add(i);
                    }
                }

                //Remove Items
                for (int i = 0; i < detractItems.Count; i++)
                {
                    table.Loot.RemoveAt(detractItems[i]);
                }
            }

            return table;
        }

        public class ObjectGroup
        {
            public string name;
            public List<string> objectID = new List<string>();
        }
    }
}