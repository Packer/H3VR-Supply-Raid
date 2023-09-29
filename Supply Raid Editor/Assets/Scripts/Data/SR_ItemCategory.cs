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
    }

    [System.Serializable]
    public class ObjectGroup
    {
        public string name;
        public List<string> objectID = new List<string>();
    }
}