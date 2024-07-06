using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using SFB;

namespace Supply_Raid_Editor
{
    public class DataManager : MonoBehaviour
    {
        public static DataManager instance;

        [Header("Character")]
        [SerializeField] string characterPath;
        public SR_CharacterPreset character = null;

        [Header("Faction")]
        [SerializeField] string factionPath;
        public SR_SosigFaction faction = null;

        [Header("Item Categories")]
        [SerializeField] string categoryPath;
        public SR_ItemCategory itemCategory = null;

        [Header("Mod Folder")]
        [SerializeField] string modPath;
        public string lastCharacterDirectory = "";
        public string lastFactionDirectory = "";
        public string lastItemDirectory = "";


        [Header("Custom Sosigs")]
        public List<Sprite> customSosigs = new List<Sprite>();

        //Loaded Categories from mod folder
        public List<SR_ItemCategory> loadedCategories = new List<SR_ItemCategory>();

        [Header("Debug")]
        [SerializeField] Text debugLine;
        private string debugLog = "";

        private void Awake()
        {
            instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            //Debug.Log(Application.dataPath);
            modPath = PlayerPrefs.GetString("ModPath", Application.dataPath);

            lastCharacterDirectory = PlayerPrefs.GetString("lastCharacterDirectory");
            lastFactionDirectory = PlayerPrefs.GetString("lastFactionDirectory");
            lastItemDirectory = PlayerPrefs.GetString("lastItemDirectory");
        }

        // Update is called once per frame
        void Update()
        {
        }

        public Sprite LoadSprite(string path)
        {
            Texture2D tex = null;

            //Clean path
            path = Path.GetFullPath(path);
            path = path.Replace("%20", " ");

            byte[] fileData;

            if (File.Exists(path) && tex == null)
            {
                fileData = File.ReadAllBytes(path);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
                                         //if (tex.texelSize.x > 256)
                                         //    tex.Resize(256, 256);
                tex.name = Path.GetFileName(path);
            }

            if (tex == null)
            {
                Log("Texture Not Found at path: " + path);
                return null;
            }
            Sprite NewSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), 100.0f);
            NewSprite.name = Path.GetFileName(path);
            Log("Loaded External Image at " + path);
            return NewSprite;
        }

        
        public void LoadCustomSosigs()
        {
            string modFolder = Application.dataPath + "/../" + "CustomSosigs";

            modFolder =  Directory.CreateDirectory(modFolder).FullName;
            Log("Mod Folder is at: " + modFolder);

            List<string> directories = Directory.GetFiles(modFolder, "*.png", SearchOption.AllDirectories).ToList();

            Debug.Log("Count: " + directories.Count);
            if (directories.Count == 0)
                return;

            //Load up each of our categories
            for (int i = 0; i < directories.Count; i++)
            {
                Sprite newSprite = LoadSprite(directories[i]);

                if (!customSosigs.Contains(newSprite))
                    customSosigs.Add(newSprite);
            }
        }
        

        public bool OnLoadDialogue(JSONTypeEnum loadType)
        {
            string[] paths;


            string modPath = "";

            switch (loadType)
            {
                case JSONTypeEnum.Character:
                    modPath = lastCharacterDirectory;
                    break;
                case JSONTypeEnum.Faction:
                    modPath = lastFactionDirectory;
                    break;
                case JSONTypeEnum.ItemCategory:
                    modPath = lastItemDirectory;
                    break;
            }

            if (modPath == "")
                modPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/r2modmanPlus-local/H3VR/profiles/";

            if (!Directory.Exists(modPath))
                modPath = "";

            switch (loadType)
            {
                case JSONTypeEnum.Faction:
                    paths = StandaloneFileBrowser.OpenFilePanel("Load Faction", modPath, "sfsr", false);
                    /*
                    if (paths.Length > 0 && !paths[0].Contains("SR_Faction"))
                    {
                        LogError("Not a valid Faction, check the file name  - " + paths[0]);
                        return;
                    }*/
                    break;

                case JSONTypeEnum.ItemCategory:
                    paths = StandaloneFileBrowser.OpenFilePanel("Load Item Category", modPath, "icsr", false);
                    /*
                    if (paths.Length > 0 && !paths[0].Contains("SR_IC"))
                    {
                        LogError("Not a valid Item Category, check the file name  - " + paths[0]);
                        return;
                    }*/
                    break;

                case JSONTypeEnum.Character:
                default:
                    paths = StandaloneFileBrowser.OpenFilePanel("Load Character", modPath, "cpsr", false);
                    /*
                    if (paths.Length > 0 && !paths[0].Contains("SR_Character"))
                    {
                        LogError("Not a valid Character Preset, check the file name  - " + paths[0]);
                        return;
                    }
                    */
                    break;
            }

            if (paths.Length > 0)
            {
                switch (loadType)
                {
                    case JSONTypeEnum.Character:
                        lastCharacterDirectory = paths[0];
                        PlayerPrefs.SetString("lastCharacterDirectory", lastCharacterDirectory);
                        break;
                    case JSONTypeEnum.Faction:
                        lastFactionDirectory = paths[0];
                        PlayerPrefs.SetString("lastFactionDirectory", lastFactionDirectory);
                        break;
                    case JSONTypeEnum.ItemCategory:
                        lastItemDirectory = paths[0];
                        PlayerPrefs.SetString("lastItemDirectory", lastItemDirectory);
                        break;
                }

                StartCoroutine(OutputRoutine(new Uri(paths[0]).AbsoluteUri, loadType));
            }
            else
            {
                Log("Loading json canceled");
                return false;
            }

            return true;
        }

        private IEnumerator OutputRoutine(string url, JSONTypeEnum loadType)
        {
            var loader = new WWW(url);
            yield return loader;

            switch (loadType)
            {
                case JSONTypeEnum.Character:
                    LoadCharacter(loader.text);
                    yield return null;
                    MenuManager.instance.characterLoaded = true;
                    MenuManager.instance.OpenCharacterPanel();
                    CharacterUI.instance.LoadCharacter();

                    url = url.Remove(url.Length - 4) + "png";
                    url = url.Remove(0, 8);
                    CharacterUI.instance.thumbnail.sprite = LoadSprite(url);
                    //MenuManager.instance.characterThumbnail.sprite = LoadSprite(url);
                    break;
                case JSONTypeEnum.Faction:
                    LoadFaction(loader.text);
                    yield return null;
                    MenuManager.instance.factionLoaded = true;
                    FactionUI.instance.LoadFaction();
                    MenuManager.instance.OpenFactionPanel();

                    url = url.Remove(url.Length - 4) + "png";
                    url = url.Remove(0, 8);
                    FactionUI.instance.thumbnail.sprite = LoadSprite(url);
                    break;
                case JSONTypeEnum.ItemCategory:
                    LoadItemCategory(loader.text);
                    yield return null;
                    MenuManager.instance.itemLoaded = true;
                    MenuManager.instance.OpenItemCategoryPanel();
                    ItemCategoryUI.instance.LoadItemCategory(itemCategory);

                    url = url.Remove(url.Length - 4) + "png";
                    url = url.Remove(0, 8);
                    ItemCategoryUI.instance.itemThumbnail.sprite = LoadSprite(url);
                    break;
                default:
                    break;
            }
        }

        public void OnSaveDialogue(JSONTypeEnum saveType, string json, string saveName)
        {
            string path;
            switch (saveType)
            {
                case JSONTypeEnum.Faction:
                    path = StandaloneFileBrowser.SaveFilePanel(
                        "Save Faction", 
                        lastFactionDirectory, 
                        saveName, 
                        "sfsr");
                    break;
                case JSONTypeEnum.ItemCategory:
                    path = StandaloneFileBrowser.SaveFilePanel(
                        "Save Item Category", 
                        lastCharacterDirectory, 
                        saveName, 
                        "icsr");
                    break;

                case JSONTypeEnum.Character:
                default:
                    path = StandaloneFileBrowser.SaveFilePanel(
                        "Save Character", 
                        lastCharacterDirectory, 
                        saveName, 
                        "cpsr");
                    break;
            }

            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, json);
                Log("Saved file " + path);
            }
        }

        public SR_CharacterPreset LoadCharacter(string json)
        {
            try
            {
                character = JsonUtility.FromJson<SR_CharacterPreset>(json);
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
                return null;
            }

            if (character != null)
            {
                Log("Loaded Character " + character.name);
            }

            return character;
        }

        public SR_ItemCategory LoadItemCategory(string json)
        {
            try
            {
                itemCategory = JsonUtility.FromJson<SR_ItemCategory>(json);
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
                return null;
            }

            if (itemCategory != null)
            {
                Log("Loaded Item Category " + itemCategory.name);
            }

            return itemCategory;
        }

        public SR_SosigFaction LoadFaction(string json)
        {
            try
            {
                faction = JsonUtility.FromJson<SR_SosigFaction>(json);
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
                return null;
            }

            if (itemCategory != null)
            {
                Log("Loaded faction " + faction.name);
            }

            return faction;
        }

        public List<SR_CharacterPreset> LoadCharacters()
        {
            List<string> directories = DataManager.instance.GetCharactersDirectory();

            if (directories.Count == 0)
            {
                LogError("No Characters were found!");
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

                    DataManager.Log("Supply Raid: Loaded Character " + character.name);

                }
            }
            return characters;
        }

        public static void LogError(string text)
        {
            instance.debugLog += "\n" + text;
            instance.debugLine.text = text;
            instance.debugLine.color = Color.red;
            Debug.LogError(text);
        }

        public static void Log(string text)
        {
            instance.debugLog += "\n" + text;
            instance.debugLine.text = text;
            instance.debugLine.color = Color.white;
            Debug.Log(text);
        }

        public List<string> GetCharactersDirectory()
        {
            return Directory.GetFiles(modPath, "*.cpsr", SearchOption.AllDirectories).ToList();
        }

        public List<string> GetFactionDirectory()
        {
            return Directory.GetFiles(modPath, "*.sfsr", SearchOption.AllDirectories).ToList();
        }

        public List<string> GetItemCategoriesDirectory()
        {
            return Directory.GetFiles(modPath, "*.icsr", SearchOption.AllDirectories).ToList();
        }


        public static SR_CharacterPreset Character()
        {
            return instance.character;
        }

        public static SR_ItemCategory ItemCategory()
        {
            return instance.itemCategory;
        }

        public static SR_SosigFaction Faction()
        {
            return instance.faction;
        }
    }
    public enum JSONTypeEnum
    {
        Character = 0,
        Faction = 1,
        ItemCategory = 2,
    }
}