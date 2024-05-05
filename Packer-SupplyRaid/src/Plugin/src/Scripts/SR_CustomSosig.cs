using UnityEngine;
using FistVR;
using System.Collections.Generic;

namespace SupplyRaid
{    
    public class SR_EnemyTemplate : SosigEnemyTemplate
    {
        public void PopulateEnemyTemplate()
        {
            //Outfits
            for (int i = 0; i < outfitConfig.Length; i++)
            {
                outfitConfig[i].PopulateOutfit();
                OutfitConfig.Add(outfitConfig[i]);
            }

            SR_Global.ItemIDToList(weaponOptionsID, WeaponOptions);
            SR_Global.ItemIDToList(weaponOptions_SecondaryID, WeaponOptions_Secondary);
            SR_Global.ItemIDToList(weaponOptions_TertiaryID, WeaponOptions_Tertiary);
        }

        public SR_EnemyTemplate[] configTemplates;
        public SR_OutfitConfig[] outfitConfig;
        public string[] weaponOptionsID;
        public string[] weaponOptions_SecondaryID;
        public string[] weaponOptions_TertiaryID;
    }

    public class SR_CustomSosig
    {
        public SosigEnemyID baseSosigID;

        //Scale
        public Vector3 scaleBody = Vector3.one;
        public float scaleHead = 1;
        public float scaleTorso = 1;
        public float scaleLegsUpper = 1;
        public float scaleLegsLower = 1;

        //Materials
        public bool useCustomSkin = false;  //Default or White
        public Color color;
        public float metallic = 0;
        public float specularity = 0.3f;
        public float specularTint = 0f;
        public float roughness = 1f;
        public float normalStrength = 1f;
        public bool specularHighlights = true;
        public bool glossyReflections = true;
    }

    public class SR_SosigTempalte : SosigConfigTemplate
    {
    }

    public class SR_OutfitConfig : SosigOutfitConfig
    {
        public void PopulateOutfit()
        {
            SR_Global.ItemIDToList(headwearID, Headwear);
            SR_Global.ItemIDToList(eyewearID, Eyewear);
            SR_Global.ItemIDToList(torsowearID, Torsowear);
            SR_Global.ItemIDToList(pantswearID, Pantswear);
            SR_Global.ItemIDToList(pantswear_LowerID, Pantswear_Lower);
            SR_Global.ItemIDToList(backpacksID, Backpacks);
            SR_Global.ItemIDToList(torsoDecorationID, TorosDecoration);
            SR_Global.ItemIDToList(beltID, Belt);
        }

        public string[] headwearID;
        //public float chance_HeadWear = 0;

        public string[] eyewearID;
        //public float chance_Eyewear = 0;

        public string[] torsowearID;
        //public float chance_Torsowear = 0;

        public string[] pantswearID;
        //public float chance_Pantswear = 0;

        public string[] pantswear_LowerID;
        //public float chance_Pantswear_Lower = 0;

        public string[] backpacksID;
        //public float chance_Backpacks = 0;

        public string[] torsoDecorationID;
        //public float chance_TorsoDecoration = 0;

        public string[] beltID;
        //public float chance_belt = 0;
    }
}