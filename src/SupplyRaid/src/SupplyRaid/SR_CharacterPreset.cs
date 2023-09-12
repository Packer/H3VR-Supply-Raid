using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SupplyRaid
{
	[CreateAssetMenu(fileName = "Character_Preset", menuName = "Supply Raid/Create Character Preset", order = 1)]
	public class SR_CharacterPreset : ScriptableObject
    {
        [Tooltip("(REQUIRED) Name of this character")]
        public string title = "Character Name";
        [Tooltip("Short explanation of this character"), Multiline(6)]
        public string description;
        [Tooltip("(REQUIRED) The menu category this character will go in, Recommend mod creator names etc")]
        public string category = "Mod";
        [Tooltip("Preview image of the character when selected")]
        public Sprite thumbnail;
        [Tooltip("Starting points to spend at the buy menu")]
        public int points = 2;
        [Tooltip("Points gained per capture * total captured")]
        public int pointsPerLevel = 1;
        [Tooltip("(REQUIRED) The default Sosig faction for this character")]
        public SR_SosigFaction faction;
        [Tooltip("Pool of starting objects")]
        public SR_ItemCategory[] startGear;
        [Tooltip("(REQUIRED) What buy categories are available to this character")]
        public SR_PurchaseCategory[] purchaseCategories;


        public bool HasRequirement()
        {
            if (title == "")
                return false;
            if (category == "")
                return false;
            if (faction == null)
                return false;

            return true;
        }
	}

    [System.Serializable]
    public class SR_PurchaseCategory
    {
        public string name;
        public SR_ItemCategory itemCategory;
        public int cost = 1;
    }
}