using BepInEx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SupplyRaid
{
    public class SR_ModLoader : MonoBehaviour
    {
        public static SR_Assets srAssets;
        public static AssetBundle srBundle;
        public static bool assetsLoading = false;
        public static bool loadedAssets = false;
        public static float timeout = 0;

        void OnDestroy()
        {
            //SRScene = null;
        }


        public static IEnumerator LoadSupplyRaidAssets()
        {
            if (assetsLoading || timeout > Time.time)
                yield break;

            /*
            //Already loaded assets, just load
            if (loadedAssets)
            {
                SR_Manager.instance.LoadInAssets();
                assetsLoading = false;

                SR_Manager.instance.SetupGameData();
                yield break;
            }
            */

            assetsLoading = true;

            string path = Paths.PluginPath + "/Packer-SupplyRaid/supplyraid.sr";

            if (!loadedAssets)
            {
                AssetBundleCreateRequest asyncBundleRequest
                    = AssetBundle.LoadFromFileAsync(path);

                yield return asyncBundleRequest;
                AssetBundle localAssetBundle = asyncBundleRequest.assetBundle;

                if (localAssetBundle == null)
                {
                    Debug.LogError("Failed to load Supply Raid AssetBundle");
                    yield break;
                }

                srBundle = localAssetBundle;
            }


            AssetBundleRequest assetRequest = srBundle.LoadAssetWithSubAssetsAsync<SR_Assets>("SupplyRaid");
            yield return assetRequest;

            if (assetRequest == null)
            {
                Debug.LogError("Supply Raid - Missing SR Assets");
                yield break;
            }

            srAssets = assetRequest.asset as SR_Assets;
            loadedAssets = true;

            //--------------------------------------------------------------------------------------------------------

            SR_Manager.instance.LoadInAssets();
            assetsLoading = false;

            SR_Manager.instance.SetupGameData();
            timeout = Time.time + 10f;
        }

        public static List<SR_ItemCategory> LoadItemCategories()
        {
            List<string> directories = GetItemCategoriesDirectory();

            if (directories.Count == 0)
            {
                Debug.LogError("No Item Categories were found!");
                return null;
            }

            List<SR_ItemCategory> items = new List<SR_ItemCategory>();

            //Load up each of our categories
            for (int i = 0; i < directories.Count; i++)
            {
                SR_ItemCategory category;

                //Load each Category via the Directory
                using (StreamReader streamReader = new StreamReader(directories[i]))
                {
                    string json = streamReader.ReadToEnd();

                    try 
                    {
                        category = JsonUtility.FromJson<SR_ItemCategory>(json);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex.Message);
                        return null;
                    }

                    //Add to our item category pool
                    items.Add(category);
                    string newDirectory = directories[i];
                    newDirectory = newDirectory.Remove(newDirectory.Length - 4) + "png";
                    category.SetupThumbnailPath(newDirectory);

                    Debug.Log("Supply Raid: Loaded Item Category " + category.name);
                }
            }
            return items;
        }

        public static List<SR_SosigFaction> LoadFactions()
        {
            List<string> directories = GetFactionDirectory();

            if (directories.Count == 0)
            {
                Debug.LogError("No Factions were found!");
                return null;
            }

            List<SR_SosigFaction> factions = new List<SR_SosigFaction>();

            //Load up each of our categories
            for (int i = 0; i < directories.Count; i++)
            {
                SR_SosigFaction faction;

                //Load each Category via the Directory
                using (StreamReader streamReader = new StreamReader(directories[i]))
                {
                    string json = streamReader.ReadToEnd();

                    try
                    {
                        faction = JsonUtility.FromJson<SR_SosigFaction>(json);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex.Message);
                        return null;
                    }

                    //Add to our item category pool
                    factions.Add(faction);
                    string newDirectory = directories[i];
                    newDirectory = newDirectory.Remove(newDirectory.Length - 4) + "png";
                    faction.SetupThumbnailPath(newDirectory);

                    Debug.Log("Supply Raid: Loaded Faction " + faction.name);
                }
            }
            return factions;
        }

        public static List<SR_CharacterPreset> LoadCharacters()
        {
            List<string> directories = GetCharactersDirectory();

            if (directories.Count == 0)
            {
                Debug.LogError("No Characters were found!");
                return null;
            }

            List<SR_CharacterPreset> characters = new List<SR_CharacterPreset>();

            //Load up each of our categories
            for (int i = 0; i < directories.Count; i++)
            {
                SR_CharacterPreset character;

                //Load each Category via the Directory
                using (StreamReader streamReader = new StreamReader(directories[i]))
                {
                    string json = streamReader.ReadToEnd();

                    try
                    {
                        character = JsonUtility.FromJson<SR_CharacterPreset>(json);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex.Message);
                        return null;
                    }

                    //Add to our item category pool
                    characters.Add(character);
                    string newDirectory = directories[i];
                    newDirectory = newDirectory.Remove(newDirectory.Length - 4) + "png";
                    character.SetupThumbnailPath(newDirectory);

                    Debug.Log("Supply Raid: Loaded Character " + character.name);
                }
            }
            return characters;
        }

        public static List<string> GetCharactersDirectory()
        {
            return Directory.GetFiles(Paths.PluginPath, "*.cpsr", SearchOption.AllDirectories).ToList();
        }

        public static List<string> GetFactionDirectory()
        {
            return Directory.GetFiles(Paths.PluginPath, "*.sfsr", SearchOption.AllDirectories).ToList();
        }

        public static List<string> GetItemCategoriesDirectory()
        {
            return Directory.GetFiles(Paths.PluginPath, "*.icsr", SearchOption.AllDirectories).ToList();
        }

        public static List<string> GetCustomSosigDirectory()
        {
            return Directory.GetFiles(Paths.PluginPath, "*.cssr", SearchOption.AllDirectories).ToList();
        }

    }
}