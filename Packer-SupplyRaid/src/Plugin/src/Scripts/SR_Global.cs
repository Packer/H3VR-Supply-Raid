using FistVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using BepInEx;
using System.Linq;

namespace SupplyRaid
{

    public class SR_Global : MonoBehaviour
    {
        public static Sprite LoadSprite(string path)
        {
            //Debug.Log("Supply Raid: Loading: " + path);
            Texture2D tex = null;

            byte[] fileData;

            if (File.Exists(path) && tex == null)
            {
                fileData = File.ReadAllBytes(path);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);
            }

            if (tex == null)
            {
                Debug.LogError("Supply Raid: Texture Not Found: " + path);
                return null;
            }
            Sprite NewSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), 100.0f);

            return NewSprite;
        }

        public static IEnumerator SpawnAllLevelSosigs()
        {
            Debug.Log("Supply Raid: Spawning all Sosigs");
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
            Debug.Log("Supply Raid: Spawning: " + (int)id + " - " + id);

            Sosig sosig =
                Sodalite.Api.SosigAPI.Spawn(
                    IM.Instance.odicSosigObjsByID[id],
                    SR_Manager.instance._spawnOptions,
                    spawnPoint.position,
                    spawnPoint.rotation);

            if (sosig.Hand_Primary.HeldObject != null)
                Destroy(sosig.Hand_Primary.HeldObject);

            if (sosig.Hand_Secondary.HeldObject != null)
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
                    Debug.LogError("Supply Raid: Faction level " + SR_Manager.GetFactionLevel().name + " has no valid SosigEnemyIDs, Not Spawning Sosigs");
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

        public static Vector3 GetValidNavPosition(Vector3 position, float distance)
        {
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(position, out hit, distance, UnityEngine.AI.NavMesh.AllAreas))
            {
                return hit.position;
            }

            return position;
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

        public static SR_SosigFaction GetFactionByName(string factionName)
        {
            for (int i = 0; i < SR_Manager.instance.factions.Count; i++)
            {
                if (SR_Manager.instance.factions[i].name == factionName)
                    return SR_Manager.instance.factions[i];
            }

            return null;
        }

        public static bool IsSosigInCombat(Sosig sosig)
        {
            switch (sosig.CurrentOrder)
            {
                default:
                case Sosig.SosigOrder.Disabled:
                case Sosig.SosigOrder.GuardPoint:
                case Sosig.SosigOrder.Wander:
                case Sosig.SosigOrder.Idle:
                case Sosig.SosigOrder.PathTo:
                    return false;
                case Sosig.SosigOrder.Skirmish:
                case Sosig.SosigOrder.SearchForEquipment:
                case Sosig.SosigOrder.TakeCover:
                case Sosig.SosigOrder.Investigate:
                case Sosig.SosigOrder.Assault:
                case Sosig.SosigOrder.Flee:
                case Sosig.SosigOrder.StaticShootAt:
                case Sosig.SosigOrder.StaticMeleeAttack:
                    return true;
            }
        }

        public static bool IsFVRObjectInDropProtection(FVRObject fvr)
        {
            if (fvr == null)
                return false;
            if (SR_Manager.Character().dropProtectionObjectIDs.Contains(fvr.ItemID))
                return true;

            return true;
        }

        public static LootTable RemoveGlobalSubtractionOnTable(LootTable table)
        {
            //Collect items
            List<int> detractItems = new List<int>();

            //Remove GLOBAL character subtractions from character list
            if (SR_Manager.Character() != null && SR_Manager.Character().subtractionObjectIDs.Count > 0)
            {
                for (int x = 0; x < table.Loot.Count; x++)
                {
                    if (SR_Manager.Character().subtractionObjectIDs.Contains(table.Loot[x].ItemID))
                    {
                        detractItems.Add(x);
                    }
                }
            }

            //Remove Items in reverse
            for (int y = detractItems.Count - 1; y >= 0; y--)
            {
                table.Loot.RemoveAt(detractItems[y]);
            }

            return table;
        }

        /// <summary>
        /// Attempts to spawn input gear, otherwise returns false
        /// </summary>
        /// <returns></returns>
        public static bool SpawnLoot(LootTable table, SR_ItemCategory itemCategory, Transform[] spawns, bool forceSecondary = false)
        {
            if (table == null || table.Loot.Count == 0 && itemCategory.objectGroups.Count == 0)
                return false;
            FVRObject mainObject;
            FVRObject ammoObject;

            //Create one Spawn
            int itemCount = 1;

            //Randomize from both pools
            int objectCount = 0;

            //The different loot pools
            bool useLootTags = true;
            bool useGroupIDs = false;

            int spawnCount = 1;

            //Item Category settings
            if (itemCategory != null)
            {
                if (itemCategory.spawnCount > 1)
                    spawnCount = itemCategory.spawnCount;

                //Max level
                if (itemCategory.maxLevel > -1)
                {
                    if (itemCategory.maxLevel < SR_Manager.instance.CurrentCaptures)
                        return false;
                }

                //Min Level
                if (itemCategory.minLevel > -1)
                {
                    if (itemCategory.minLevel > SR_Manager.instance.CurrentCaptures)
                        return false;
                }

                //Using Group ids
                if (itemCategory.objectGroups.Count > 0)
                {
                    useGroupIDs = true;
                    objectCount += (itemCategory.objectGroups.Count);
                }

                //Using the Loot Tags
                if (itemCategory.lootTagsEnabled)
                    objectCount += table.Loot.Count;
                else //No Loot tags enabled
                    useLootTags = false;
            }
            else //Default to table loot
                objectCount += table.Loot.Count;

            //Random Select from Table Loot and Group IDs
            Random.InitState(System.DateTime.Now.Second * System.DateTime.Now.Minute);
            int index = Random.Range(0, objectCount);

            //Group IDs
            List<string> idGroup = new List<string>();

            //Using Group IDs
            if (useGroupIDs)
            {
                if (useLootTags)
                    index -= table.Loot.Count - 1;

                //Get Random Group
                if (itemCategory != null && itemCategory.objectGroups != null && itemCategory.objectGroups.Count > 0)
                {
                    idGroup.AddRange(itemCategory.objectGroups[index].objectID);
                }

                //Increase Items to Spawn
                if (idGroup.Count > 1)
                    itemCount = idGroup.Count;
            }

            bool spawnedObject = false;

            //Min Magazine Size
            int minCapacity = -1;
            if (itemCategory != null)
                minCapacity = itemCategory.minCapacity;

            //Loop through each group ID
            for (int z = 0; z < itemCount; z++)
            {
                //Use Manual weapon IDs?
                if (useGroupIDs)
                {
                    if (idGroup[z] == "" || idGroup[z] == null)
                        return false;

                    //Try to find the weapon ID
                    if (!IM.OD.TryGetValue(idGroup[z], out mainObject))
                    {
                        Debug.Log("Supply Raid: Cannot find object with id: " + idGroup[z]);
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
                    mainObject = table.Loot[index];
                    if (mainObject != null)
                    {
                        ammoObject = GetLowestCapacityAmmoObject(mainObject, table.Eras, minCapacity);
                    }
                    else
                    {
                        if (itemCategory != null)
                            Debug.Log("Supply Raid: NO OBJECT FOUND IN ITEM CATEGORY: " + itemCategory.name);
                        else
                            Debug.Log("Supply Raid: NO OBJECT FOUND ");
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

                //Spawn with Required Attachments (Such as iron sights for anti material snipers)
                if (itemCategory != null && itemCategory.requiredAttachments)
                {
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
                }

                //Spawned Refferences
                GameObject spawnedMain = null;
                GameObject spawnedAmmo = null;
                GameObject spawnedAttach0 = null;
                GameObject spawnedAttach1 = null;
                GameObject spawnedAttach2 = null;

                int mainSpawn = spawnCount;

                //If single fire disposible weapon, create several.
                if (!SR_Manager.profile.spawnLocking && !mainObject.UsesRoundTypeFlag
                    && mainObject.TagFirearmFeedOption.Contains(FVRObject.OTagFirearmFeedOption.None)
                    && mainObject.TagFirearmFiringModes.Contains(FVRObject.OTagFirearmFiringMode.SingleFire))
                {
                    mainSpawn = 3;
                }

                //Spawn the item objects in the game
                if (mainObject.GetGameObject() != null)
                {
                    for (int i = 0; i < mainSpawn; i++)
                    {
                        spawnedMain = Instantiate(mainObject.GetGameObject(), spawns[0].position + ((Vector3.up * 0.25f) * i) + ((Vector3.up * 0.2f) * z), spawns[0].rotation);

                        //Iron Sights or other things
                        if (forceSecondary && mainObject.RequiredSecondaryPieces != null && mainObject.RequiredSecondaryPieces.Count > 0)
                        {
                            for (int s = 0; s < mainObject.RequiredSecondaryPieces.Count; s++)
                            {
                                if(mainObject.RequiredSecondaryPieces[s] != null)
                                    Instantiate(mainObject.RequiredSecondaryPieces[s].GetGameObject(), spawns[0].position + ((Vector3.up * 0.26f) * s) + ((Vector3.up * 0.2f) * z), spawns[0].rotation);
                            }
                        }

                        if (spawnedMain.name == "CharcoalBriquette(Clone)")
                        {
                            Material gold = SetToGoldMaterial(spawnedMain.transform.GetChild(0).GetComponent<Renderer>().material);

                            if(spawnedMain.GetComponent<Renderer>())
                                spawnedMain.GetComponent<Renderer>().material = gold;
                            else if (spawnedMain.transform.GetChild(0))
                                spawnedMain.transform.GetChild(0).GetComponent<Renderer>().material = gold;
                        }
                    }
                }

                //Default 4 - Limited Ammo Magazines in mind
                int ammoCount = GetCategoryAmmoTypeCount(mainObject, itemCategory);

                /*
                //Rounds
                if (UsesRounds(mainObject))
                {
                    FVRFireArm firearm = spawnedMain.GetComponent<FVRFireArm>();
                    if(firearm != null)
                        ammoCount = firearm.GetChamberRoundList().Count * 2;
                }
                */



                /*
                int lockedDefaultCount = 3; //Spawn Locked Ammo, 1 mag in, 1 mag lock, 1 for loading extra round in
                int limitedAmmoDefault = 4; //Limited Ammo, 1 in, 3 Reserve

                //Item Category ammo that will spawn, -1 = default

                if (SR_Manager.instance.optionSpawnLocking)
                {
                    if (itemCategory != null)
                    {
                        ammoCount = (itemCategory.ammoSpawnLockedCount <= -1) ? lockedDefaultCount : itemCategory.ammoSpawnLockedCount;

                        if (ammoCount > 0)
                        {
                            if(itemCategory.ammoSpawnLockedCountMin >= 0)
                            {
                                ammoCount
                                    = Random.Range(
                                        itemCategory.ammoSpawnLockedCountMin,
                                        itemCategory.ammoSpawnLockedCount);
                            }
                        }
                    }
                    else
                        ammoCount = lockedDefaultCount; 
                }
                else
                {
                    if (itemCategory != null)
                    {
                        ammoCount = itemCategory.ammoLimitedCount <= -1 ? limitedAmmoDefault : itemCategory.ammoLimitedCount;

                        if (mainObject != null)
                        {
                            ammoCount = GetCategoryAmmoTypeCount(mainObject, itemCategory);
                        }
                        else if (ammoCount > 0)
                        {
                            //Default Limited
                            if (itemCategory.ammoLimitedCountMin >= 0)
                            {
                                ammoCount = GetAmmoCount(
                                    ammoCount,
                                    itemCategory.ammoLimitedCountMin,
                                    itemCategory.ammoLimitedCount);
                            }
                        }
                    }
                    else
                        ammoCount = limitedAmmoDefault;  //No item Category, give em 4
                }
                */

                //Ammo
                if (ammoObject != null && ammoObject.GetGameObject() != null)
                {
                    //Multiply by Spawn Count
                    ammoCount *= spawnCount;

                    for (int x = 0; x < ammoCount; x++)
                    {
                        spawnedAmmo = Instantiate(ammoObject.GetGameObject(), spawns[1].position + ((Vector3.up * 0.25f) * x), spawns[1].rotation);
                    }
                }

                //Attachments
                for (int i = 0; i < spawnCount; i++)
                {
                    if (attach0 != null && attach0.GetGameObject() != null)
                        spawnedAttach0 = Instantiate(attach0.GetGameObject(), spawns[2].position, spawns[2].rotation);

                    if (attach1 != null && attach1.GetGameObject() != null)
                        spawnedAttach1 = Instantiate(attach1.GetGameObject(), spawns[3].position, spawns[3].rotation);

                    if (attach2 != null && attach2.GetGameObject() != null)
                        spawnedAttach2 = Instantiate(attach2.GetGameObject(), spawns[4].position, spawns[4].rotation);
                }

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
            //FVRObject item = IM.OD[o.ItemID];
            if (o.CompatibleMagazines.Count > 0
                || o.CompatibleClips.Count > 0
                || o.CompatibleSpeedLoaders.Count > 0)
                return true;

            return false;   //Default to magazine
        }


        public static int GetAmmoCount(int ammoCount, int min = -1, int max = -1)
        {
            if (max > -1)
            {
                //Random Range
                if (min > -1)
                    ammoCount = Random.Range(min, max);
                else //Set Count
                    ammoCount = max;
            }

            return ammoCount;
        }

        public static int GetCategoryAmmoTypeCount(FVRObject mainObject, SR_ItemCategory itemCategory)
        {
            int ammoCount = 0;

            if (itemCategory == null)
            {
                if (SR_Manager.profile.spawnLocking)
                    return 3;
                else
                    return 4;
            }
            else
            {
                //Default Limited
                ammoCount = GetAmmoCount(
                    4,
                    itemCategory.ammoLimitedCountMin,
                    itemCategory.ammoLimitedCount);

            }

            //Default Spawnlocking to ammo Spawn Locked
            if (SR_Manager.profile.spawnLocking)
            {
                return GetAmmoCount(
                    3,
                    itemCategory.ammoSpawnLockedCountMin, 
                    itemCategory.ammoSpawnLockedCount);
            }

            if (mainObject.CompatibleMagazines.Count > 0
                || mainObject.Category == FVRObject.ObjectCategory.Firearm && mainObject.MagazineType != FireArmMagazineType.mNone)
            {
                return GetAmmoCount(
                        ammoCount,
                        itemCategory.ammoLimitedMagazineCountMin,
                        itemCategory.ammoLimitedMagazineCount);
            }

            if (mainObject.CompatibleClips.Count > 0
                || mainObject.Category == FVRObject.ObjectCategory.Firearm && mainObject.ClipType != FireArmClipType.None)
            {
                return GetAmmoCount(
                        ammoCount,
                        itemCategory.ammoLimitedClipCountMin,
                        itemCategory.ammoLimitedClipCount);
            }

            if (mainObject.CompatibleSpeedLoaders.Count > 0)
            {
                return GetAmmoCount(
                        ammoCount,
                        itemCategory.ammoLimitedSpeedLoaderCountMin,
                        itemCategory.ammoLimitedSpeedLoaderCount);
            }

            if (mainObject.CompatibleSingleRounds.Count > 0)
            {
                return GetAmmoCount(
                    ammoCount,
                    itemCategory.ammoLimitedRoundCountMin,
                    itemCategory.ammoLimitedRoundCount);
            }

            return ammoCount;
        }

        /*
        public static AmmoContainerType GetAmmoContainerType(FVRObject o)
        {
            if (o.CompatibleMagazines.Count > 0
                || o.Category == FVRObject.ObjectCategory.Firearm && o.MagazineType != FireArmMagazineType.mNone)
            {
                return AmmoContainerType.Magazine;
            }

            if (o.CompatibleClips.Count > 0)
            {
                return AmmoContainerType.Clip;
            }

            if (o.CompatibleSpeedLoaders.Count > 0)
            {
                return AmmoContainerType.SpeedLoader;
            }

            if (o.CompatibleSingleRounds.Count > 0)
            {
                return AmmoContainerType.Round;
            }
            
            return AmmoContainerType.None;
        }
        */
        public static int GetRoundValue(string itemID)
        {
            switch (itemID)
            {
                case "Cartridge69CashMoneyD1":
                    return 1;
                case "Cartridge69CashMoneyD5":
                    return 5;
                case "Cartridge69CashMoneyD10":
                    return 10;
                case "Cartridge69CashMoneyD25":
                    return 25;
                case "Cartridge69CashMoneyD100":
                    return 100;
                case "Cartridge69CashMoneyD1000":
                    return 1000;
                default:
                    return 0;
            }
        }

        public static string GetHighestValueCashMoney(int cash)
        {
            if (cash >= 1000)
                return "Cartridge69CashMoneyD1000";
            else if (cash >= 100)
                return "Cartridge69CashMoneyD100";
            else if (cash >= 25)
                return "Cartridge69CashMoneyD25";
            else if (cash >= 10)
                return "Cartridge69CashMoneyD10";
            else if (cash >= 5)
                return "Cartridge69CashMoneyD5";
            else if (cash >= 1)
                return "Cartridge69CashMoneyD1";

            return "";
        }

        public static IEnumerator WaitandCreate(GameObject prefab, float waitTime, Transform spawnPoint)
        {
            yield return new WaitForSeconds(waitTime);
            Instantiate(
                prefab, 
                spawnPoint.position 
                    + (Vector3.up * 0.05f) 
                    + (new Vector3(Random.Range(-0.001f, 0.001f), Random.Range(-0.001f, 0.001f), Random.Range(-0.001f, 0.001f))), 
                spawnPoint.rotation);
        }

        public static List<FVRObject> RemoveTags(List<FVRObject> mags, List<FVRObject.OTagEra> eras = null, int Min = -1, int Max = -1, List<FVRObject.OTagSet> sets = null)
        {
            for (int i = mags.Count - 1; i >= 0; i--)
            {
                if (Min > -1 && mags[i].MagazineCapacity < Min)
                {
                    mags.RemoveAt(i);
                }
                else if (Max > -1 && mags[i].MagazineCapacity > Max)
                {
                    mags.RemoveAt(i);
                }
                else if (eras != null && !eras.Contains(mags[i].TagEra))
                {
                    mags.RemoveAt(i);
                }
                else if (sets != null && !sets.Contains(mags[i].TagSet))
                {
                    mags.RemoveAt(i);
                }
            }

            return mags;
        }

        public static FVRObject GetLowestCapacityAmmoObject(
            FVRObject o, List<FVRObject.OTagEra> eras = null, int Min = -1, int Max = -1, List<FVRObject.OTagSet> sets = null)
        {
            if (o == null)
                return null;

            //MAGAZINES
            if (o.MagazineType != FireArmMagazineType.mNone)
            {
                //List<FVRObject> ammoCollection = o.CompatibleMagazines == null ? new List<FVRObject>() : o.CompatibleMagazines;

                //Populate Magazines
                List<FVRObject> mags = new List<FVRObject>(ManagerSingleton<IM>.Instance.odicTagCategory[FVRObject.ObjectCategory.Magazine]);
                for (int i = mags.Count - 1; i >= 0; i--)
                {
                    if (mags[i].MagazineType != o.MagazineType)
                        mags.RemoveAt(i);
                }

                if (mags.Count == 0)
                {
                    //Debug.Log("No Mags");
                    return null;
                }

                mags = GetLowestCapacity(mags, Min);
                return mags[Random.Range(0, mags.Count)];
            }

            //CLIPS
            if (o.CompatibleClips != null && o.CompatibleClips.Count > 0 
                || o.ClipType != FireArmClipType.None)
            {
                //Populate Clips
                List<FVRObject> clips = new List<FVRObject>(ManagerSingleton<IM>.Instance.odicTagCategory[FVRObject.ObjectCategory.Clip]);
                for (int i = clips.Count - 1; i >= 0; i--)
                {
                    if (clips[i].ClipType != o.ClipType)
                        clips.RemoveAt(i);
                }

                if (clips.Count == 0)
                {
                    //Debug.Log("No Clips");
                    return null;
                }

                clips = GetLowestCapacity(clips, Min);
                return clips[Random.Range(0, clips.Count)];
            }

            //Speed Loaders
            if (o.CompatibleSpeedLoaders != null && o.CompatibleSpeedLoaders.Count > 0)
            {
                List<FVRObject> ammoCollection = GetLowestCapacity(o.CompatibleSpeedLoaders, Min);
                return ammoCollection[Random.Range(0, ammoCollection.Count)];
            }

            //Single Rounds
            if (o.CompatibleSingleRounds != null && o.CompatibleSingleRounds.Count > 0)
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

        public static float Distance2D(Vector3 a, Vector3 b)
        {
            Vector2 start = new Vector2(a.x, a.z);
            Vector2 end = new Vector2(b.x, b.z);

            return Vector2.Distance(start, end);
        }

        public static Material goldMaterial;

        public static Material SetToGoldMaterial(Material input)
        {
            if(goldMaterial != null)
                return goldMaterial;


            Material gold = input;

            Color goldColor = new Color(1, 0.75f, 0, 1);

            gold.SetColor("_Color", goldColor);
            gold.SetColor("_DecalColor", goldColor);
            gold.SetColor("_EmissionColor", goldColor);

            gold.SetFloat("_EmissionWeight", 1);
            gold.SetFloat("_Emission", 1);
            gold.SetFloat("_Mode", 0);
            gold.SetFloat("_Metal", 0.666f);
            gold.SetFloat("_Roughness", 0.5f);
            gold.SetFloat("_SpecularTint", 1);
            

            goldMaterial = gold;
            return gold;
        }

        public static Bounds GetBounds(GameObject obj)
        {

            Bounds bounds = new Bounds();

            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

            if (renderers.Length > 0)

            {

                //Find first enabled renderer to start encapsulate from it

                foreach (Renderer renderer in renderers)

                {

                    if (renderer.enabled)

                    {

                        bounds = renderer.bounds;

                        break;

                    }

                }

                //Encapsulate for all renderers

                foreach (Renderer renderer in renderers)

                {

                    if (renderer.enabled)

                    {

                        bounds.Encapsulate(renderer.bounds);

                    }

                }

            }

            return bounds;

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

        /*
        public static List<string> GetCustomSosigDirectories()
        {
            return Directory.GetFiles(Paths.PluginPath, "*.csosig", SearchOption.AllDirectories).ToList();
        }
        
        public static void LoadCustomSosigs()
        {
            List<string> directories = GetCustomSosigDirectories();

            if (directories.Count == 0)
            {
                Debug.Log("Custom Sosigs: No Custom Sosigs were found!");
                return;
            }

            //List<Custom_SosigEnemyTemplate> items = new List<Custom_SosigEnemyTemplate>();

            //Load up each of our categories
            for (int i = 0; i < directories.Count; i++)
            {
                SR_SosigEnemyTemplate sosigTemplate;

                //Load each Category via the Directory
                using (StreamReader streamReader = new StreamReader(directories[i]))
                {
                    string json = streamReader.ReadToEnd();

                    try
                    {
                        sosigTemplate = JsonUtility.FromJson<SR_SosigEnemyTemplate>(json);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.Log(ex.Message);
                        return;
                    }

                    //Add to our collection
                    SupplyRaidPlugin.customSosigs.Add(sosigTemplate.sosigEnemyID, sosigTemplate);

                    Debug.Log("Supply Raid: Loaded Custom Sosig  " + sosigTemplate.sosigEnemyID + " - " + sosigTemplate.displayName);
                }
            }
        }
        */

        public static void ItemIDToList(string[] itemIDs, List<FVRObject> input)
        {


            for (int i = 0; i < itemIDs.Length; i++)
            {
                FVRObject mainObject;
                if (IM.OD.TryGetValue(itemIDs[i], out mainObject))
                    input.Add(mainObject);
                else
                    Debug.Log("Supply Raid: Could not find " + itemIDs);
            }
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

    public enum AmmoContainerType
    {
        None = -1,
        Round = 0,
        Magazine = 1,
        Clip = 2,
        SpeedLoader = 3,
    }
}