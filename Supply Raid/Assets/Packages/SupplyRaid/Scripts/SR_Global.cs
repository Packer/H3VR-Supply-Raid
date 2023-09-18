using FistVR;
using SupplyRaid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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
                    Debug.Log("Cannot find object with id: " + idGroup[z]);
                    continue;
                }

                //Main Weapon
                if (mainObject != null)
                    ammoObject = GetLowestCapacityAmmoObject(mainObject, null, itemCategory.minCapacity);
                else
                    return false;
            }
            else
            {
                //Weapon + Ammo references
                mainObject = table.GetRandomObject();
                if (mainObject != null)
                {
                    int minCapacity = itemCategory != null ? itemCategory.minCapacity : -1;
                    ammoObject = GetLowestCapacityAmmoObject(mainObject, table.Eras, minCapacity);
                }
                else
                {
                    Debug.Log("NO WEAPON FOUND");
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
            if (!mainObject.UsesRoundTypeFlag
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

            if (ammoCount < 12)
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

}
