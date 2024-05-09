using UnityEngine;
using FistVR;
using System.Collections.Generic;
using System.IO;
using BepInEx;

namespace SupplyRaid
{
    [System.Serializable]
    public class SR_SosigEnemyTemplate
    {
        public string displayName = "New Sosig";
        public int sosigEnemyCategory = 0;
        public int sosigEnemyID = -1;
        public SR_CustomSosig[] customSosig;

        public string[] weaponOptionsID;
        public string[] weaponOptions_SecondaryID;
        public float secondaryChance = 0;
        public string[] weaponOptions_TertiaryID;
        public float tertiaryChance = 0;

        public SR_OutfitConfig[] outfitConfig;
        public SR_SosigConfigTemplate[] configTemplates;

        public SosigEnemyTemplate Initialize()
        {
            SosigEnemyTemplate template = ScriptableObject.CreateInstance<SosigEnemyTemplate>();

            //Outfits
            template.OutfitConfig = new List<SosigOutfitConfig>();
            for (int i = 0; i < outfitConfig.Length; i++)
            {
                template.OutfitConfig.Add(outfitConfig[i].Initialize());
            }

            //Configs & Prefabs
            template.ConfigTemplates = new List<SosigConfigTemplate>();
            for (int i = 0; i < configTemplates.Length; i++)
            {
                template.ConfigTemplates.Add(configTemplates[i].Initialize());
            }

            //Custom Base
            template.SosigPrefabs = new List<FVRObject>();
            for (int i = 0; i < customSosig.Length; i++)
            {
                SosigEnemyTemplate baseTemplate = IM.Instance.odicSosigObjsByID[customSosig[i].baseSosigID];
                FVRObject newSosig = baseTemplate.SosigPrefabs[Random.Range(0, baseTemplate.SosigPrefabs.Count)];
                template.SosigPrefabs.Add(newSosig);
            }

            template.WeaponOptions = new List<FVRObject>();
            SR_Global.ItemIDToList(weaponOptionsID, template.WeaponOptions);
            template.WeaponOptions_Secondary = new List<FVRObject>();
            SR_Global.ItemIDToList(weaponOptions_SecondaryID, template.WeaponOptions_Secondary);
            template.WeaponOptions_Tertiary = new List<FVRObject>();
            SR_Global.ItemIDToList(weaponOptions_TertiaryID, template.WeaponOptions_Tertiary);

            template.SecondaryChance = secondaryChance;
            template.TertiaryChance = tertiaryChance;

            return template;
        }

        public void ExportJson()
        {
            using (StreamWriter streamWriter = new StreamWriter(Paths.PluginPath + "\\Packer-SupplyRaid\\" + displayName + ".json"))
            {
                string json = JsonUtility.ToJson(this, true);
                streamWriter.Write(json);
            }
        }
    }

    [System.Serializable]
    public class SR_CustomSosig
    {
        public SosigEnemyID baseSosigID = SosigEnemyID.Misc_Dummy;
        public string customTextureName = "";

        //Scale
        public Vector3 scaleBody = Vector3.one;
        public Vector3 scaleHead = Vector3.one;
        public Vector3 scaleTorso = Vector3.one;
        public Vector3 scaleLegsUpper = Vector3.one;
        public Vector3 scaleLegsLower = Vector3.one;

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

    [System.Serializable]
    public class SR_OutfitConfig
    {
        public SosigOutfitConfig Initialize()
        {
            SosigOutfitConfig outfit = ScriptableObject.CreateInstance<SosigOutfitConfig>();

            outfit.HeadUsesTorsoIndex = headUsesTorsoIndex;
            outfit.PantsUsesTorsoIndex = pantsUsesTorsoIndex;
            outfit.PantsLowerUsesPantsIndex = pantsLowerUsesPantsIndex;

            outfit.Headwear = new List<FVRObject>();
            SR_Global.ItemIDToList(headwearID, outfit.Headwear);
            outfit.Eyewear = new List<FVRObject>();
            SR_Global.ItemIDToList(eyewearID, outfit.Eyewear);
            outfit.Torsowear = new List<FVRObject>();
            SR_Global.ItemIDToList(torsowearID, outfit.Torsowear);
            outfit.Pantswear = new List<FVRObject>();
            SR_Global.ItemIDToList(pantswearID, outfit.Pantswear);
            outfit.Pantswear_Lower = new List<FVRObject>();
            SR_Global.ItemIDToList(pantswear_LowerID, outfit.Pantswear_Lower);
            outfit.Backpacks = new List<FVRObject>();
            SR_Global.ItemIDToList(backpacksID, outfit.Backpacks);
            outfit.TorosDecoration = new List<FVRObject>();
            SR_Global.ItemIDToList(torsoDecorationID, outfit.TorosDecoration);
            outfit.Belt = new List<FVRObject>();
            SR_Global.ItemIDToList(beltID, outfit.Belt);

            outfit.Chance_Headwear = chance_HeadWear;
            outfit.Chance_Eyewear = chance_Eyewear;
            outfit.Chance_Torsowear = chance_Torsowear;
            outfit.Chance_Pantswear = chance_Pantswear;
            outfit.Chance_Pantswear_Lower = chance_Pantswear_Lower;
            outfit.Chance_Backpacks = chance_Backpacks;
            outfit.Chance_TorosDecoration = chance_TorsoDecoration;
            outfit.Chance_Belt = chance_belt;

            return outfit;
        }

        public string[] headwearID;
        public float chance_HeadWear = 0;
        public bool headUsesTorsoIndex = false;

        public string[] eyewearID;
        public float chance_Eyewear = 0;

        public string[] torsowearID;
        public float chance_Torsowear = 0;

        public string[] pantswearID;
        public float chance_Pantswear = 0;
        public bool pantsUsesTorsoIndex = false;

        public string[] pantswear_LowerID;
        public float chance_Pantswear_Lower = 0;
        public bool pantsLowerUsesPantsIndex = false;

        public string[] backpacksID;
        public float chance_Backpacks = 0;

        public string[] torsoDecorationID;
        public float chance_TorsoDecoration = 0;

        public string[] beltID;
        public float chance_belt = 0;
    }

    [System.Serializable]
    public class SR_SosigConfigTemplate
    {
        public SosigConfigTemplate Initialize()
        {
            SosigConfigTemplate config = ScriptableObject.CreateInstance<SosigConfigTemplate>();

            //AI Entity Params
            config.ViewDistance = ViewDistance;
            config.StateSightRangeMults = StateSightRangeMults;
            config.HearingDistance = HearingDistance;
            config.StateHearingRangeMults = StateHearingRangeMults;
            config.MaxFOV = MaxFOV;
            config.StateFOVMults = StateFOVMults;

            //Core Identity Params
            config.HasABrain = HasABrain;

            config.RegistersPassiveThreats = RegistersPassiveThreats;
            config.DoesAggroOnFriendlyFire = DoesAggroOnFriendlyFire;
            config.SearchExtentsModifier = SearchExtentsModifier;
            config.DoesDropWeaponsOnBallistic = DoesDropWeaponsOnBallistic;
            config.CanPickup_Ranged = CanPickup_Ranged;
            config.CanPickup_Melee = CanPickup_Melee;
            config.CanPickup_Other = CanPickup_Other;

            //TargetPrioritySystemParams
            config.TargetCapacity = TargetCapacity;
            config.TargetTrackingTime = TargetTrackingTime;
            config.NoFreshTargetTime = NoFreshTargetTime;
            config.AssaultPointOverridesSkirmishPointWhenFurtherThan = AssaultPointOverridesSkirmishPointWhenFurtherThan;
            //config.TimeInSkirmishToAlert = TimeInSkirmishToAlert;

            //Movement Params
            config.RunSpeed = RunSpeed;
            config.WalkSpeed = WalkSpeed;
            config.SneakSpeed = SneakSpeed;
            config.CrawlSpeed = CrawlSpeed;
            config.TurnSpeed = TurnSpeed;
            config.MaxJointLimit = MaxJointLimit;
            config.MovementRotMagnitude = MovementRotMagnitude;

            //Damage Params
            config.AppliesDamageResistToIntegrityLoss = AppliesDamageResistToIntegrityLoss;
            config.TotalMustard = TotalMustard;
            config.BleedDamageMult = BleedDamageMult;
            config.BleedRateMultiplier = BleedRateMultiplier;
            config.BleedVFXIntensity = BleedVFXIntensity;
            config.DamMult_Projectile = DamMult_Projectile;
            config.DamMult_Explosive = DamMult_Explosive;
            config.DamMult_Melee = DamMult_Melee;
            config.DamMult_Piercing = DamMult_Piercing;
            config.DamMult_Blunt = DamMult_Blunt;
            config.DamMult_Cutting = DamMult_Cutting;
            config.DamMult_Thermal = DamMult_Thermal;
            config.DamMult_Chilling = DamMult_Chilling;
            config.DamMult_EMP = DamMult_EMP;
            config.LinkDamageMultipliers = LinkDamageMultipliers;
            config.LinkStaggerMultipliers = LinkStaggerMultipliers;
            config.StartingLinkIntegrity = StartingLinkIntegrity;
            config.StartingChanceBrokenJoint = StartingChanceBrokenJoint;

            //Shudder Params
            config.ShudderThreshold = ShudderThreshold;

            //Confusion Params
            config.ConfusionThreshold = ConfusionThreshold;
            config.ConfusionMultiplier = ConfusionMultiplier;
            config.ConfusionTimeMax = ConfusionTimeMax;

            //Stun Params
            config.StunThreshold = StunThreshold;
            config.StunMultiplier = StunMultiplier;
            config.StunTimeMax = StunTimeMax;

            //Unconsciousness Params
            config.CanBeKnockedOut = CanBeKnockedOut;
            config.MaxUnconsciousTime = MaxUnconsciousTime;

            //Resistances
            config.CanBeGrabbed = CanBeGrabbed;
            config.CanBeSevered = CanBeSevered;
            config.CanBeStabbed = CanBeStabbed;

            //Suppression
            config.CanBeSurpressed = CanBeSurpressed;
            config.SuppressionMult = SuppressionMult;

            //Death Flags
            config.DoesJointBreakKill_Head = DoesJointBreakKill_Head;
            config.DoesJointBreakKill_Upper = DoesJointBreakKill_Upper;
            config.DoesJointBreakKill_Lower = DoesJointBreakKill_Lower;
            config.DoesSeverKill_Head = DoesSeverKill_Head;
            config.DoesSeverKill_Upper = DoesSeverKill_Upper;
            config.DoesSeverKill_Lower = DoesSeverKill_Lower;
            config.DoesExplodeKill_Head = DoesExplodeKill_Head;
            config.DoesExplodeKill_Upper = DoesExplodeKill_Upper;
            config.DoesExplodeKill_Lower = DoesExplodeKill_Lower;

            //SpawnOnLinkDestroy
            config.UsesLinkSpawns = UsesLinkSpawns;
            SR_Global.ItemIDToList(LinkSpawns.ToArray(), config.LinkSpawns);
            config.LinkSpawnChance = LinkSpawnChance;

            return config;
        }

        [Header("Supply Raid")]
        public bool forceOverrideAIEntityParams = false;
        public string[] spawnItemIDOnDestroy;

        [Header("AIEntityParams")]
        public float ViewDistance = 250f;
        public Vector3 StateSightRangeMults = new Vector3(0.1f, 0.35f, 1);
        public float HearingDistance = 300f;
        public Vector3 StateHearingRangeMults = new Vector3(0.6f, 1, 1);
        public float MaxFOV = 105f;
        public Vector3 StateFOVMults = new Vector3(0.5f, 0.6f, 1);

        [Header("Core Identity Params")]
        public bool HasABrain = true;
        public bool RegistersPassiveThreats = false;
        public bool DoesAggroOnFriendlyFire = false;
        public float SearchExtentsModifier = 1;
        public bool DoesDropWeaponsOnBallistic = true;
        public bool CanPickup_Ranged = true;
        public bool CanPickup_Melee = true;
        public bool CanPickup_Other = true;

        [Header("TargetPrioritySystemParams")]
        public int TargetCapacity = 5;
        public float TargetTrackingTime = 2;
        public float NoFreshTargetTime = 1.5f;
        public float AssaultPointOverridesSkirmishPointWhenFurtherThan = 200;
        public float TimeInSkirmishToAlert = 1f;

        [Header("Movement Params")]
        public float RunSpeed = 3.5f;
        public float WalkSpeed = 1.4f;
        public float SneakSpeed = 0.6f;
        public float CrawlSpeed = 0.3f;
        public float TurnSpeed = 2f;
        public float MaxJointLimit = 6f;
        public float MovementRotMagnitude = 10f;

        [Header("Damage Params")]
        public bool AppliesDamageResistToIntegrityLoss = false;
        public float TotalMustard = 100f;
        public float BleedDamageMult = 0.5f;
        public float BleedRateMultiplier = 1f;
        public float BleedVFXIntensity = 0.2f;
        public float DamMult_Projectile = 1;
        public float DamMult_Explosive = 1;
        public float DamMult_Melee = 1;
        public float DamMult_Piercing = 1;
        public float DamMult_Blunt = 1;
        public float DamMult_Cutting = 1;
        public float DamMult_Thermal = 1;
        public float DamMult_Chilling = 1;
        public float DamMult_EMP = 1;
        public List<float> LinkDamageMultipliers = new List<float> {4, 2, 1.2f, 1 };
        public List<float> LinkStaggerMultipliers = new List<float> { 8, 0.3f, 0.8f, 1 };
        public List<Vector2> StartingLinkIntegrity = new List<Vector2> { new Vector2(100, 100), new Vector2(100, 100), new Vector2(100, 100), new Vector2(100, 100) };
        public List<float> StartingChanceBrokenJoint = new List<float> { 0, 0, 0, 0 };

        [Header("Shudder Params")]
        public float ShudderThreshold = 2f;

        [Header("Confusion Params")]
        public float ConfusionThreshold = 0.3f;
        public float ConfusionMultiplier = 6f;
        public float ConfusionTimeMax = 4f;

        [Header("Stun Params")]
        public float StunThreshold = 1.4f;
        public float StunMultiplier = 2;
        public float StunTimeMax = 4f;

        [Header("Unconsciousness Params")]
        public bool CanBeKnockedOut = true;
        public float MaxUnconsciousTime = 90f;

        [Header("Resistances")]
        public bool CanBeGrabbed = true;
        public bool CanBeSevered = true;
        public bool CanBeStabbed = true;

        [Header("Suppression")]
        public bool CanBeSurpressed = true;
        public float SuppressionMult = 1;

        [Header("Death Flags")]
        public bool DoesJointBreakKill_Head = true;
        public bool DoesJointBreakKill_Upper = false;
        public bool DoesJointBreakKill_Lower = false;
        public bool DoesSeverKill_Head = true;
        public bool DoesSeverKill_Upper = true;
        public bool DoesSeverKill_Lower = true;
        public bool DoesExplodeKill_Head = true;
        public bool DoesExplodeKill_Upper = true;
        public bool DoesExplodeKill_Lower = true;

        [Header("SpawnOnLinkDestroy")]
        public bool UsesLinkSpawns = false;
        public List<string> LinkSpawns = new List<string>(4);
        public List<float> LinkSpawnChance = new List<float>(4);
    }
}