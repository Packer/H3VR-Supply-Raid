using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FistVR;

namespace SupplyRaid
{
    [CreateAssetMenu(fileName = "IC_", menuName = "Supply Raid/Create Item Category", order = 1)]
    public class SR_ItemCategory : ScriptableObject
    {
        [Header("UI")]
        public Sprite thumbnail;

        [Tooltip("Magazine/Clip Min Capacity for this loot table")]
        public int minCapacity = -1;
        //[Tooltip("Magazine/Clip Max Capacity for this loot table")]
        //public int MaxCapacity = -1;

        [Header("Manual Setup Table")]
        [Tooltip("If populated, this will be used instead of the Loot Table")]
        public string[] objectID;
        
        [Header("Loot Table")]
        public LootTable.LootTableType type = LootTable.LootTableType.Firearm;
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

        public LootTable InitializeLootTable()
        {
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
                -1);
            return table;
        }
    }
}