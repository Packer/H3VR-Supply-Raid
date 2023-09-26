using FistVR;
using SupplyRaid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace SupplyRaid
{

    public class SR_Global : MonoBehaviour
    {
        public static Sprite LoadSprite(string path)
        {
            Debug.Log("Supply Raid - Loading External Image at " + path);
            Texture2D tex = null;

            byte[] fileData;

            if (File.Exists(path) && tex == null)
            {
                fileData = File.ReadAllBytes(path);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
                                         //if (tex.texelSize.x > 256)
                                         //    tex.Resize(256, 256);
            }

            if (tex == null)
            {
                Debug.LogError("Texture Not Found at path: " + path);
                return null;
            }
            Sprite NewSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), 100.0f);

            return NewSprite;
        }

        public static IEnumerator SpawnAllLevelSosigs()
        {
            Debug.Log("Supply Raid - Spawning all Sosigs");
            Transform spawnPoint = SR_Manager.AttackSupplyPoint().respawn;
            List<SosigEnemyID> idList = new List<SosigEnemyID>();

            for (int x = 0; x < SR_Manager.instance.faction.levels.Length; x++)
            {
                //Guards
                if (SR_Manager.instance.faction.levels[x].guardPool != null && SR_Manager.instance.faction.levels[x].guardPool.sosigEnemyID != null)
                {
                    for (int y = 0; y < SR_Manager.instance.faction.levels[x].guardPool.sosigEnemyID.Length; y++)
                    {
                        if (!idList.Contains(SR_Manager.instance.faction.levels[x].guardPool.sosigEnemyID[y]))
                            idList.Add(SR_Manager.instance.faction.levels[x].guardPool.sosigEnemyID[y]);
                    }
                }

                //Sniper
                if (SR_Manager.instance.faction.levels[x].sniperPool != null && SR_Manager.instance.faction.levels[x].sniperPool.sosigEnemyID != null)
                {
                    for (int y = 0; y < SR_Manager.instance.faction.levels[x].sniperPool.sosigEnemyID.Length; y++)
                    {
                        if (!idList.Contains(SR_Manager.instance.faction.levels[x].sniperPool.sosigEnemyID[y]))
                            idList.Add(SR_Manager.instance.faction.levels[x].sniperPool.sosigEnemyID[y]);
                    }
                }

                //Patrol
                if (SR_Manager.instance.faction.levels[x].patrolPool != null && SR_Manager.instance.faction.levels[x].patrolPool.sosigEnemyID != null)
                {
                    for (int y = 0; y < SR_Manager.instance.faction.levels[x].patrolPool.sosigEnemyID.Length; y++)
                    {
                        if (!idList.Contains(SR_Manager.instance.faction.levels[x].patrolPool.sosigEnemyID[y]))
                            idList.Add(SR_Manager.instance.faction.levels[x].patrolPool.sosigEnemyID[y]);
                    }
                }

                //Squad
                if (SR_Manager.instance.faction.levels[x].squadPool != null && SR_Manager.instance.faction.levels[x].squadPool.sosigEnemyID != null)
                {
                    for (int y = 0; y < SR_Manager.instance.faction.levels[x].squadPool.sosigEnemyID.Length; y++)
                    {
                        if (!idList.Contains(SR_Manager.instance.faction.levels[x].squadPool.sosigEnemyID[y]))
                            idList.Add(SR_Manager.instance.faction.levels[x].squadPool.sosigEnemyID[y]);
                    }
                }

                //Spawn everything
                foreach (SosigEnemyID id in idList)
                {
                    SpawnTempSosig(id, spawnPoint);
                    yield return new WaitForSeconds(0.33f);
                }
            }
        }
        public static void SpawnTempSosig(SosigEnemyID id, Transform spawnPoint)
        {
            Debug.Log("Supply Raid - Spawning: " + (int)id + " - " + id);

            Sosig sosig =
                Sodalite.Api.SosigAPI.Spawn(
                    IM.Instance.odicSosigObjsByID[id],
                    SR_Manager.instance._spawnOptions,
                    spawnPoint.position,
                    spawnPoint.rotation);

            if (sosig.Hand_Primary.HeldObject != null)
                Destroy(sosig.Hand_Primary.HeldObject);

            if(sosig.Hand_Secondary.HeldObject != null)
                Destroy(sosig.Hand_Secondary.HeldObject);

            Destroy(sosig.gameObject);
            //sosig.ClearSosig();
        }

        /// <summary>
        /// Tries to get a Random Sosig Enemy ID from the input pool, returns None if not valid or not found
        /// </summary>
        /// <param name="pool"></param>
        /// <returns></returns>
        public static SosigEnemyID GetRandomSosigIDFromPool(SosigEnemyID[] pool)
        {
            SosigEnemyID id = SosigEnemyID.None;

            if (pool.Length == 0)
                return id;

            int count = 0;
            while (true)
            {
                if (count >= pool.Length)
                {
                    Debug.LogError("Supply Raid - Faction level " + SR_Manager.GetFactionLevel().name + " has no valid SosigEnemyIDs, Not Spawning Sosigs");
                    return id;
                }

                id = pool[Random.Range(0, pool.Length)];

                if (ValidSosigEnemyID(id))
                    break;
                else
                    count++;
            }

            return id;
        }

        public static bool ValidSosigEnemyID(SosigEnemyID id)
        {
            if (id == SosigEnemyID.None)
                return false;

            //(SosigEnemyID)System.Enum.Parse(typeof(SosigEnemyID), id);

            if (IM.Instance.odicSosigObjsByID.ContainsKey(id))
                return true;

            return false;
        }

        /// <summary>
        /// Attempts to spawn input gear, otherwise returns false
        /// </summary>
        /// <returns></returns>
        public static bool SpawnLoot(LootTable table, SR_ItemCategory itemCategory, Transform[] spawns)
        {
            if (table == null)
                return false;

            FVRObject mainObject = null;
            FVRObject ammoObject = null;

            //Create one Spawn
            int itemCount = 1;

            List<string> idGroup = new List<string>();
            //Get Random Group
            if (itemCategory != null && itemCategory.objectGroups != null && itemCategory.objectGroups.Count > 0)
            {
                idGroup = itemCategory.objectGroups[Random.Range(0, itemCategory.objectGroups.Count)].objectID;
            }

            if (idGroup.Count > 1)
                itemCount = idGroup.Count;

            bool spawnedObject = false;

            int minCapacity = -1;
            if (itemCategory != null)
                minCapacity = itemCategory.minCapacity;


            //Loop through each group ID
            for (int z = 0; z < itemCount; z++)
            {
                //Use Manual weapon IDs?
                if (idGroup != null && idGroup.Count > 0)
                {
                    if (idGroup[z] == "" || idGroup[z] == null)
                        return false;

                    //Try to find the weapon ID
                    if (!IM.OD.TryGetValue(idGroup[z], out mainObject))
                    {
                        Debug.Log("Supply Raid - Cannot find object with id: " + idGroup[z]);
                        continue;
                    }

                    //Main Weapon
                    if (mainObject != null)
                        ammoObject = GetLowestCapacityAmmoObject(mainObject, null, minCapacity);
                    else
                        return false;
                }
                else
                {
                    //Weapon + Ammo references
                    mainObject = table.GetRandomObject();
                    if (mainObject != null)
                    {
                        ammoObject = GetLowestCapacityAmmoObject(mainObject, table.Eras, minCapacity);
                    }
                    else
                    {
                        if(itemCategory != null)
                            Debug.Log("Supply Raid - NO OBJECT FOUND IN ITEM CATEGORY: " + itemCategory.name);
                        else
                            Debug.Log("Supply Raid - NO OBJECT FOUND ");
                        return false;
                    }
                }

                //Error Check
                if (mainObject == null)
                {
                    return false;
                }

                FVRObject attach0 = null;
                FVRObject attach1 = null;
                FVRObject attach2 = null;

                if (mainObject.RequiredSecondaryPieces.Count > 0)
                {
                    attach0 = mainObject.RequiredSecondaryPieces[0];

                    if (mainObject.RequiredSecondaryPieces.Count > 1)
                    {
                        attach1 = mainObject.RequiredSecondaryPieces[1];
                    }
                    if (mainObject.RequiredSecondaryPieces.Count > 2)
                    {
                        attach2 = mainObject.RequiredSecondaryPieces[2];
                    }
                }
                else if (mainObject.RequiresPicatinnySight)
                {
                    attach0 = SR_Manager.instance.lt_RequiredAttachments.GetRandomObject();
                    if (attach0 != null && attach0.RequiredSecondaryPieces.Count > 0)
                    {
                        attach1 = attach0.RequiredSecondaryPieces[0];
                    }
                }
                else if (mainObject.BespokeAttachments.Count > 0)
                {
                    float num4 = Random.Range(0f, 1f);
                    if (num4 > 0.75f)
                    {
                        attach0 = table.GetRandomBespokeAttachment(mainObject);
                        if (attach0.RequiredSecondaryPieces.Count > 0)
                        {
                            attach1 = attach0.RequiredSecondaryPieces[0];
                        }
                    }
                }

                //Spawned Refferences
                GameObject spawnedMain = null;
                GameObject spawnedAmmo = null;
                GameObject spawnedAttach0 = null;
                GameObject spawnedAttach1 = null;
                GameObject spawnedAttach2 = null;

                int mainSpawn = 1;

                //If single fire disposible weapon, create several.
                if (!SR_Manager.instance.optionSpawnLocking && !mainObject.UsesRoundTypeFlag
                    && mainObject.TagFirearmFeedOption.Contains(FVRObject.OTagFirearmFeedOption.None)
                    && mainObject.TagFirearmFiringModes.Contains(FVRObject.OTagFirearmFiringMode.SingleFire))
                {
                    mainSpawn = 3;
                }

                //Spawn the item objects in the game
                if (mainObject != null && mainObject.GetGameObject() != null)
                {
                    for (int i = 0; i < mainSpawn; i++)
                    {
                        spawnedMain = Instantiate(mainObject.GetGameObject(), spawns[0].position + ((Vector3.up * 0.25f) * i), spawns[0].rotation);
                    }
                }

                int ammoCount = 3;

                //Rounds
                if (UsesRounds(mainObject))
                {
                    ammoCount = mainObject.MagazineCapacity * 2;
                    //ammoCount = mainObject.MagazineCapacity > mainObject.MaxCapacityRelated ? mainObject.MagazineCapacity * 2: mainObject.MaxCapacityRelated * 2;
                }

                if (SR_Manager.instance.optionSpawnLocking)
                    ammoCount = 3;
                else if (ammoCount < 12)
                    ammoCount = 12;


                //Ammo
                if (ammoObject != null && ammoObject.GetGameObject() != null)
                {
                    for (int x = 0; x < ammoCount; x++)
                    {
                        spawnedAmmo = Instantiate(ammoObject.GetGameObject(), spawns[1].position + ((Vector3.up * 0.25f) * x), spawns[1].rotation);
                    }
                }

                //Attachments
                if (attach0 != null && attach0.GetGameObject() != null)
                    spawnedAttach0 = Instantiate(attach0.GetGameObject(), spawns[2].position, spawns[2].rotation);

                if (attach1 != null && attach1.GetGameObject() != null)
                    spawnedAttach1 = Instantiate(attach1.GetGameObject(), spawns[3].position, spawns[3].rotation);

                if (attach2 != null && attach2.GetGameObject() != null)
                    spawnedAttach2 = Instantiate(attach2.GetGameObject(), spawns[4].position, spawns[4].rotation);

                //TODO, Add to cleanup... cause we might need to do that...

                if (spawnedMain != null || spawnedAmmo != null || spawnedAttach0 != null || spawnedAttach1 != null || spawnedAttach2 != null)
                    spawnedObject = true;
            }

            return spawnedObject;
        }

        /// <summary>
        /// returns true if uses rounds instead of clips or magazines
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool UsesRounds(FVRObject o)
        {
            o = IM.OD[o.ItemID];
            if (o.CompatibleSingleRounds.Count > 0 || o.UsesRoundTypeFlag)
                return true; //Round

            return false;   //Default to magazine
        }

        public static FVRObject GetLowestCapacityAmmoObject(FVRObject o, List<FVRObject.OTagEra> eras = null, int Min = -1, int Max = -1, List<FVRObject.OTagSet> sets = null)
        {
            o = IM.OD[o.ItemID];
            if (o.CompatibleMagazines.Count > 0)
            {
                List<FVRObject> ammoCollection = GetLowestCapacity(o.CompatibleMagazines, Min);
                return ammoCollection[Random.Range(0, ammoCollection.Count)];
            }

            if (o.Category == FVRObject.ObjectCategory.Firearm && o.MagazineType != 0)
            {
                List<FVRObject> ammoCollection = GetLowestCapacity(IM.CompatMags[o.MagazineType], Min);
                return ammoCollection[Random.Range(0, ammoCollection.Count)];
            }

            if (o.CompatibleClips.Count > 0)
            {
                List<FVRObject> ammoCollection = GetLowestCapacity(o.CompatibleClips, Min);
                return ammoCollection[Random.Range(0, ammoCollection.Count)];
            }

            if (o.CompatibleSpeedLoaders.Count > 0)
            {
                List<FVRObject> ammoCollection = GetLowestCapacity(o.CompatibleSpeedLoaders, Min);
                return ammoCollection[Random.Range(0, ammoCollection.Count)];
            }

            if (o.CompatibleSingleRounds.Count > 0)
            {
                if ((eras == null || eras.Count < 1) && (sets == null || sets.Count < 1))
                {
                    return o.CompatibleSingleRounds[Random.Range(0, o.CompatibleSingleRounds.Count)];
                }

                List<FVRObject> rounds = new List<FVRObject>();
                for (int k = 0; k < o.CompatibleSingleRounds.Count; k++)
                {
                    bool flag = true;
                    if (eras != null && eras.Count > 0 && !eras.Contains(o.CompatibleSingleRounds[k].TagEra))
                    {
                        flag = false;
                    }

                    if (sets != null && sets.Count > 0 && !sets.Contains(o.CompatibleSingleRounds[k].TagSet))
                    {
                        flag = false;
                    }

                    if (flag)
                    {
                        rounds.Add(o.CompatibleSingleRounds[k]);
                    }
                }

                if (rounds.Count > 0)
                {
                    FVRObject result = rounds[Random.Range(0, rounds.Count)];
                    rounds.Clear();
                    return result;
                }

                return o.CompatibleSingleRounds[0];
            }
            return null;
        }

        public static List<FVRObject> GetLowestCapacity(List<FVRObject> ammo, int minCapacity)
        {
            List<FVRObject> collected = new List<FVRObject>();

            //Get Lowest Capacity possible
            int lowest = int.MaxValue;
            for (int i = 0; i < ammo.Count; i++)
            {
                if (ammo[i].MagazineCapacity < lowest && ammo[i].MagazineCapacity >= minCapacity)
                    lowest = ammo[i].MagazineCapacity;
            }

            //Collect all lowest capacity
            for (int i = 0; i < ammo.Count; i++)
            {
                if (ammo[i].MagazineCapacity == lowest)
                    collected.Add(ammo[i]);
            }

            return collected;
        }

        public static AmmoEnum GetAmmoEnum(FireArmRoundClass round)
        {
            switch (round)
            {

                //----ROUNDS--------------------------------
                case FireArmRoundClass.FMJ:
                case FireArmRoundClass.Ball:
                case FireArmRoundClass.Spitzer:
                case FireArmRoundClass.DSM_Swarm:
                case FireArmRoundClass.NERMAL:
                case FireArmRoundClass.MIRV:
                case FireArmRoundClass.a20FMJ:
                    return AmmoEnum.Standard;


                case FireArmRoundClass.JHP:
                case FireArmRoundClass.SP:
                case FireArmRoundClass.HighVelHP:
                case FireArmRoundClass.HyperVelHP:
                    return AmmoEnum.HollowPoint;

                case FireArmRoundClass.Tracer:
                case FireArmRoundClass.DSM_Tracer:
                case FireArmRoundClass.FLASHY:
                    return AmmoEnum.Tracer;

                case FireArmRoundClass.AP:
                case FireArmRoundClass.POINTYOWW:
                case FireArmRoundClass.DSM_TurboPenetrator:
                case FireArmRoundClass.a20AP:
                    return AmmoEnum.AP;

                case FireArmRoundClass.Incendiary:
                case FireArmRoundClass.X666_Baphomet:
                    return AmmoEnum.Incendiary;

                case FireArmRoundClass.APIncendiary:
                case FireArmRoundClass.a20APDS:
                    return AmmoEnum.API;

                case FireArmRoundClass.PlusP_FMJ:
                case FireArmRoundClass.SPESHUL:
                    return AmmoEnum.PlusP_FMJ;

                case FireArmRoundClass.PlusP_JHP:
                    return AmmoEnum.PlusP_JHP;

                case FireArmRoundClass.PlusP_API:
                    return AmmoEnum.PlusP_API;

                case FireArmRoundClass.Subsonic_FMJ:
                    return AmmoEnum.Subsonic_FMJ;

                case FireArmRoundClass.Subsonic_AP:
                    return AmmoEnum.Subsonic_AP;

                //case FireArmRoundClass.Subsonic_JHP:
                //    return AmmoEnum.Subsonic_JHP;

                //----SHELLS---------------------------

                case FireArmRoundClass.Slug:
                case FireArmRoundClass.DSM_Slugger:
                case FireArmRoundClass.KS23_Barricade:
                    return AmmoEnum.Slug;

                case FireArmRoundClass.BuckShot00:
                case FireArmRoundClass.BuckShotNo2:
                case FireArmRoundClass.BuckShotNo4:
                case FireArmRoundClass.BuckShot000:
                case FireArmRoundClass.BuckShotNo1:
                case FireArmRoundClass.Double:
                case FireArmRoundClass.MEGA:
                case FireArmRoundClass.MegaBuckShot:
                case FireArmRoundClass.KS23_Buckshot:
                    return AmmoEnum.Buckshot;

                case FireArmRoundClass.Flechette:
                    return AmmoEnum.Flechette;

                case FireArmRoundClass.DragonsBreath:
                    return AmmoEnum.Flechette;

                case FireArmRoundClass.TripleHit:
                    return AmmoEnum.TripleHit;

                //TODO add HE shells back in

                //----GRENADE LAUNCHER---------------------------

                case FireArmRoundClass.M397_AirBurst:
                case FireArmRoundClass.X214_SteelBreaker:
                case FireArmRoundClass.X477_CornerFrag:
                case FireArmRoundClass.M720A1prop0:
                case FireArmRoundClass.M720A1prop1:
                case FireArmRoundClass.M430A1:
                case FireArmRoundClass.RLV_HEF:
                case FireArmRoundClass.RLV_HEFJ:
                //case FireArmRoundClass.a35x32_HE:
                //case FireArmRoundClass.a35x32_HEDP:
                //case FireArmRoundClass.a35x32_INCEN:
                //case FireArmRoundClass.a84mm_HE441B:
                //case FireArmRoundClass.a84mm_HEAT751:
                //case FireArmRoundClass.a84mm_HEDP502:
                    return AmmoEnum.GrenadeHE;

                case FireArmRoundClass.M576_MPAPERS:
                    return AmmoEnum.GrenadeBuckshot;

                case FireArmRoundClass.M651_CSGAS:
                case FireArmRoundClass.KS23_CSGas:
                case FireArmRoundClass.Kol_Smokescreen:
                case FireArmRoundClass.RLV_SMK:
                case FireArmRoundClass.RLV_SF1:
                case FireArmRoundClass.RLV_TPM:
                //case FireArmRoundClass.a35x32_SMOKE:
                //case FireArmRoundClass.a84mm_SMOKE469C:
                    return AmmoEnum.GrenadeSmoke;

                //----Generic---------------------------

                case FireArmRoundClass.DSM_Volt:
                case FireArmRoundClass.DSM_Mag:
                //case FireArmRoundClass.a65_APDS:
                //case FireArmRoundClass.a65_Frangible:
                //case FireArmRoundClass.a65_HET:
                //case FireArmRoundClass.a65_HP:
                case FireArmRoundClass.MF13g_Buck:
                case FireArmRoundClass.MF13g_Slugger:
                case FireArmRoundClass.MF13g_Blooper:
                case FireArmRoundClass.MF13g_Bleeder:
                case FireArmRoundClass.MF13g_Moonshot:
                case FireArmRoundClass.MF1850_Barbie:
                case FireArmRoundClass.MF1850_Drongo:
                case FireArmRoundClass.MF1850_Gobsmacka:
                case FireArmRoundClass.MF1232_Bushfire:
                case FireArmRoundClass.MF1232_FunnelSpider:
                case FireArmRoundClass.MF366_Retort:
                case FireArmRoundClass.MF366_Debuff:
                case FireArmRoundClass.MF366_Salute:
                case FireArmRoundClass.MFRPG_Classic:
                case FireArmRoundClass.MFRPG_RocketPop:
                case FireArmRoundClass.MFRPG_ToTheMoon:
                case FireArmRoundClass.MFRPG_RockIt:
                case FireArmRoundClass.MFRPG_CannedMeat:
                case FireArmRoundClass.MFRPG_WRONGAMMO:
                case FireArmRoundClass.BTSP:
                case FireArmRoundClass.MFStickyFrag:
                case FireArmRoundClass.MFStickyRobbieBurns:
                case FireArmRoundClass.MFStickyRustyNail:
                case FireArmRoundClass.MFStickyHighlandFling:
                case FireArmRoundClass.MFSyringeBloodfire:
                case FireArmRoundClass.MFSyringeKnockout:
                case FireArmRoundClass.MFSyringeRage:
                case FireArmRoundClass.CM_1:
                case FireArmRoundClass.CM_5:
                case FireArmRoundClass.CM_10:
                case FireArmRoundClass.CM_20:
                case FireArmRoundClass.CM_100:
                case FireArmRoundClass.CM_1000:
                    return AmmoEnum.Special;

                case FireArmRoundClass.Freedomfetti:
                case FireArmRoundClass.Cannonball:
                case FireArmRoundClass.X1776_FreedomParty:
                    return AmmoEnum.Firework;

                case FireArmRoundClass.Mortar:
                case FireArmRoundClass.FragExplosive:
                case FireArmRoundClass.Frag12FA:
                case FireArmRoundClass.Frag12HE:
                case FireArmRoundClass.DSM_Frag:
                case FireArmRoundClass.DSM_Mine:
                case FireArmRoundClass.Mk211:
                case FireArmRoundClass.BOOOMY:
                case FireArmRoundClass.M381_HighExplosive:
                case FireArmRoundClass.Kol_Frag:
                case FireArmRoundClass.Kol_HEAT:
                case FireArmRoundClass.Kol_Megabuck:
                case FireArmRoundClass.Kol_Inferno:
                case FireArmRoundClass.a20HE:
                case FireArmRoundClass.a20HEI:
                case FireArmRoundClass.a20SAPHEI:
                    return AmmoEnum.Explosive;

                //----MISC---------------------------

                case FireArmRoundClass.Flare:
                case FireArmRoundClass.MFFlareClassic:
                case FireArmRoundClass.MFFlareDangerClose:
                case FireArmRoundClass.MFFlareSunburn:
                case FireArmRoundClass.MFFlareConflagration:
                //case FireArmRoundClass.a84mm_ILLUM545C:
                    return AmmoEnum.Flare;

                case FireArmRoundClass.KS23_Flash:
                case FireArmRoundClass.Kol_TriFlash:
                    return AmmoEnum.Flash;

                case FireArmRoundClass.M781_Practice:
                case FireArmRoundClass.X828_Aurora:
                    return AmmoEnum.Practice;

                //----Unsorted---------------------------

                default:
                    return AmmoEnum.Special;
            }
        }
    }
    /// <summary>
    /// The Ammo Table Ammo Type Array Position
    /// </summary>
    public enum AmmoEnum
    {
        None = -1,
        //----Rounds
        Standard = 0,   //FMJ / Default on Single Ammo types
        HollowPoint = 1,
        AP = 2,
        API = 3,
        Incendiary = 4,
        Tracer = 5,
        Subsonic_FMJ = 6,
        Subsonic_AP = 7,
        Subsonic_JHP = 8,
        PlusP_FMJ = 9,
        PlusP_JHP = 10,
        PlusP_API = 11,
        //----Shells
        Buckshot = 12,
        Slug = 13,
        TripleHit = 14,
        Flechette = 15,
        ShellHE = 16,
        //----Grenade Launchers
        GrenadeHE = 17,
        GrenadeSmoke = 18,
        GrenadeBuckshot = 19,
        //----Misc
        Practice = 20,
        Flare = 21,
        Flash = 22,
        Explosive = 23,
        Firework = 24,
        DragonsBreathe = 25,
        Random = 26,
        Special = 27,
    }
}