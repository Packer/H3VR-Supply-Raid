using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

namespace Supply_Raid_Editor
{
    public class MenuManager : MonoBehaviour
    {
        public static MenuManager instance;

        [Header("Tabs")]
        public GameObject menuTabBtn;
        public GameObject characterTabBtn;
        public GameObject factionTabBtn;
        public GameObject itemCategoryTabBtn;

        [Header("Warning Panels")]
        public GameObject warningPanel;
        public Text warningMessage;
        public string popupConfirmMethod = "";


        [Header("Panels")]
        public GameObject menuPanel;
        public GameObject characterPanel;
        public GameObject factionPanel;
        public GameObject itemCategoryPanel;
        //Check
        public bool characterLoaded = false;
        public bool factionLoaded = false;
        public bool itemLoaded = false;


        [Header("Character")]
        public Image characterThumbnail;
        public InputField characterName;
        public InputField characterDescription;
        public InputField characterCategory;
        public InputField characterFaction;

        public InputField characterPoints;
        public InputField characterPointsPerLevel;
        public InputField characterNewMagazineCost;
        public InputField characterUpgradeMagazineCost;
        public InputField characterDuplicateMagazineCost;
        public InputField characterRecyclerPoints;

        public GameObject purchaseCategoryPrefab;
        public Transform purchaseCategoryContent;
        public List<ListContainer> purchaseCategoryLists = new List<ListContainer>();


        public GameObject startGearPrefab;
        public List<ListContainer> startGearLists = new List<ListContainer>();
        public Transform characterStartGearContent;


        [Header("Item Category")]
        public Image itemThumbnail;
        public InputField itemName;
        public InputField itemMinCapacity;
        public ItemTableContent[] tables;
        public GameObject itemTablePrefab;
        public Transform itemTableContent;

        public GameObject itemObjectIDPrefab;
        public Transform itemObjectIDContent;
        public List<ListContainer> itemObjectIDList = new List<ListContainer>();
        public GameObject itemSubObjectIDPrefab;
        public Transform itemSubObjectIDContent;
        public List<ListContainer> itemSubtractIDList = new List<ListContainer>();

        //Type Loot
        public string[] itemTypeList;
        public Dropdown itemTypeDropdown;


        public GameObject dropdownPrefab;



        private void Awake()
        {
            instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            OpenMenuPanel();
            GenerateItemTables();
        }

        // Update is called once per frame
        void Update()
        {
            characterTabBtn.SetActive(characterLoaded);
            factionTabBtn.SetActive(factionLoaded);
            itemCategoryTabBtn.SetActive(itemLoaded);


            //TODO make this not horrible performance
            if (itemLoaded)
            {
                GridLayoutGroup group = itemTableContent.GetComponent<GridLayoutGroup>();
                Vector2 cell = group.cellSize;
                cell.y = itemTableContent.GetComponent<RectTransform>().rect.height / 2;
                group.cellSize = cell;
            }
        }

        //Panels  -------------------------------------------------------------------
        public void OpenCharacterPanel()
        {
            CloseAllPanels();
            characterPanel.SetActive(true);
        }
        public void OpenFactionPanel()
        {
            CloseAllPanels();
            factionPanel.SetActive(true);
        }
        public void OpenItemCategoryPanel()
        {
            CloseAllPanels();
            itemCategoryPanel.SetActive(true);
        }

        public void OpenMenuPanel()
        {
            CloseAllPanels();
            menuPanel.SetActive(true);
        }

        public void CloseAllPanels()
        {
            menuPanel.SetActive(false);
            characterPanel.SetActive(false);
            factionPanel.SetActive(false);
            itemCategoryPanel.SetActive(false);
        }

        //ITEM CATEGORY -------------------------------------------------------------------

        public void TrySaveItemCategory()
        {
            //PopupWarning("SaveCharacter", "This will overwrite the character file, are you sure?");
            SaveItemCategory();
        }

        void SaveItemCategory()
        {
            if (DataManager.instance.itemCategory == null)
                return;
            SR_ItemCategory item = DataManager.instance.itemCategory;

            item.name = itemName.text;
            item.minCapacity = int.Parse(itemMinCapacity.text);
            item.type = (LootTable.LootTableType)itemTypeDropdown.value;


            item.objectID.Clear();
            for (int i = 0; i < itemObjectIDList.Count; i++)
            {
                item.objectID.Add(itemObjectIDList[i].inputField.text);
            }

            item.subtractionID.Clear();
            for (int i = 0; i < itemSubtractIDList.Count; i++)
            {
                item.subtractionID.Add(itemSubtractIDList[i].inputField.text);
            }

            //TABLES

            item.set.Clear();
            for (int i = 0; i < tables[0].dropdowns.Count; i++)
            {
                item.set.Add((FVRObject.OTagSet)tables[0].dropdowns[i].value);
            }

            item.eras.Clear();
            for (int i = 0; i < tables[1].dropdowns.Count; i++)
            {
                item.eras.Add((FVRObject.OTagEra)tables[1].dropdowns[i].value);
            }

            item.sizes.Clear();
            for (int i = 0; i < tables[2].dropdowns.Count; i++)
            {
                item.sizes.Add((FVRObject.OTagFirearmSize)tables[2].dropdowns[i].value);
            }

            item.actions.Clear();
            for (int i = 0; i < tables[3].dropdowns.Count; i++)
            {
                item.actions.Add((FVRObject.OTagFirearmAction)tables[3].dropdowns[i].value);
            }

            item.modes.Clear();
            for (int i = 0; i < tables[4].dropdowns.Count; i++)
            {
                item.modes.Add((FVRObject.OTagFirearmFiringMode)tables[4].dropdowns[i].value);
            }

            item.excludeModes.Clear();
            for (int i = 0; i < tables[5].dropdowns.Count; i++)
            {
                item.excludeModes.Add((FVRObject.OTagFirearmFiringMode)tables[5].dropdowns[i].value);
            }

            item.feedoptions.Clear();
            for (int i = 0; i < tables[6].dropdowns.Count; i++)
            {
                item.feedoptions.Add((FVRObject.OTagFirearmFeedOption)tables[6].dropdowns[i].value);
            }

            item.mounts.Clear();
            for (int i = 0; i < tables[7].dropdowns.Count; i++)
            {
                item.mounts.Add((FVRObject.OTagFirearmMount)tables[7].dropdowns[i].value);
            }

            item.roundPowers.Clear();
            for (int i = 0; i < tables[8].dropdowns.Count; i++)
            {
                item.roundPowers.Add((FVRObject.OTagFirearmRoundPower)tables[8].dropdowns[i].value);
            }

            item.features.Clear();
            for (int i = 0; i < tables[9].dropdowns.Count; i++)
            {
                item.features.Add((FVRObject.OTagAttachmentFeature)tables[9].dropdowns[i].value);
            }

            item.meleeStyles.Clear();
            for (int i = 0; i < tables[10].dropdowns.Count; i++)
            {
                item.meleeStyles.Add((FVRObject.OTagMeleeStyle)tables[10].dropdowns[i].value);
            }

            item.meleeHandedness.Clear();
            for (int i = 0; i < tables[11].dropdowns.Count; i++)
            {
                item.meleeHandedness.Add((FVRObject.OTagMeleeHandedness)tables[11].dropdowns[i].value);
            }

            item.powerupTypes.Clear();
            for (int i = 0; i < tables[12].dropdowns.Count; i++)
            {
                item.powerupTypes.Add((FVRObject.OTagPowerupType)tables[12].dropdowns[i].value);
            }

            item.thrownTypes.Clear();
            for (int i = 0; i < tables[13].dropdowns.Count; i++)
            {
                item.thrownTypes.Add((FVRObject.OTagThrownType)tables[13].dropdowns[i].value);
            }

            //SAVE CHARACTER
            string json = JsonUtility.ToJson(item, true);
            DataManager.instance.OnSaveDialogue(JSONTypeEnum.ItemCategory, json, item.name);
        }

        public void TryLoadItemCategory()
        {
            if (itemLoaded)
                PopupWarning("LoadItemCategory", "Loading a Item Category will overwrite the previous one, are you sure?");
            else
                LoadItemCategory();
        }

        void LoadItemCategory()
        {
            DataManager.instance.OnLoadDialogue(JSONTypeEnum.ItemCategory);
            RefreshItemCategory();
        }

        public void TryNewItemCategory()
        {
            if (characterLoaded)
                PopupWarning("NewItemCategory", "A new item category will overwrite the previous one, are you sure?");
            else
                NewItemCategory();
        }

        void NewItemCategory()
        {
            DataManager.instance.itemCategory = new SR_ItemCategory();
            itemLoaded = true;
            
            RefreshItemCategory();
        }

        void ClearAllItemTables()
        {
            for (int i = 0; i < tables.Length; i++)
            {
                tables[i].ClearAllDropdowns();
            }
        }

        public void RefreshItemCategory()
        {
            if (DataManager.instance.itemCategory == null)
                return;

            SR_ItemCategory item = DataManager.instance.itemCategory;

            itemName.text = item.name;
            itemMinCapacity.text = item.minCapacity.ToString();
            itemTypeDropdown.value = (int)item.type;


            ClearAllObjectIDs();
            //Create new Strings
            for (int i = 0; i < item.objectID.Count; i++)
            {
                ListContainer gear = NewObjectID();
                gear.inputField.text = item.objectID[i];
            }

            ClearAllSubObjectIDs();
            //Create new Strings
            for (int i = 0; i < item.subtractionID.Count; i++)
            {
                ListContainer gear = NewSubObjectID();
                gear.inputField.text = item.subtractionID[i];
            }

            //-----------------------------------------
            //Tables
            ClearAllItemTables();

            List<int> setList = new List<int>();
            for (int i = 0; i < item.set.Count; i++)
            {
                setList.Add((int)item.set[i]);
            }

            List<int> eraList = new List<int>();
            for (int i = 0; i < item.eras.Count; i++)
            {
                eraList.Add((int)item.eras[i]);
            }

            List<int> sizeList = new List<int>();
            for (int i = 0; i < item.sizes.Count; i++)
            {
                sizeList.Add((int)item.sizes[i]);
            }

            List<int> actionList = new List<int>();
            for (int i = 0; i < item.actions.Count; i++)
            {
                actionList.Add((int)item.actions[i]);
            }

            List<int> fireModeList = new List<int>();
            for (int i = 0; i < item.modes.Count; i++)
            {
                fireModeList.Add((int)item.modes[i]);
            }

            List<int> excludeModeList = new List<int>();
            for (int i = 0; i < item.excludeModes.Count; i++)
            {
                excludeModeList.Add((int)item.excludeModes[i]);
            }

            List<int> feedList = new List<int>();
            for (int i = 0; i < item.feedoptions.Count; i++)
            {
                feedList.Add((int)item.feedoptions[i]);
            }

            List<int> mountList = new List<int>();
            for (int i = 0; i < item.mounts.Count; i++)
            {
                mountList.Add((int)item.mounts[i]);
            }

            List<int> roundPowersList = new List<int>();
            for (int i = 0; i < item.roundPowers.Count; i++)
            {
                roundPowersList.Add((int)item.roundPowers[i]);
            }

            List<int> featuresList = new List<int>();
            for (int i = 0; i < item.features.Count; i++)
            {
                featuresList.Add((int)item.features[i]);
            }

            List<int> meleeList = new List<int>();
            for (int i = 0; i < item.meleeStyles.Count; i++)
            {
                meleeList.Add((int)item.meleeStyles[i]);
            }

            List<int> meleeHandList = new List<int>();
            for (int i = 0; i < item.meleeHandedness.Count; i++)
            {
                meleeHandList.Add((int)item.meleeHandedness[i]);
            }

            List<int> powerUpList = new List<int>();
            for (int i = 0; i < item.powerupTypes.Count; i++)
            {
                powerUpList.Add((int)item.powerupTypes[i]);
            }


            List<int> thrownList = new List<int>();
            for (int i = 0; i < item.thrownTypes.Count; i++)
            {
                thrownList.Add((int)item.thrownTypes[i]);
            }

            //Tables
            SetupDropdown(tables[0], setList);
            SetupDropdown(tables[1], eraList);
            SetupDropdown(tables[2], sizeList);
            SetupDropdown(tables[3], actionList);
            SetupDropdown(tables[4], fireModeList);
            SetupDropdown(tables[5], excludeModeList);
            SetupDropdown(tables[6], feedList);
            SetupDropdown(tables[7], mountList);
            SetupDropdown(tables[8], roundPowersList);
            SetupDropdown(tables[9], featuresList);
            SetupDropdown(tables[10], meleeList);
            SetupDropdown(tables[11], meleeHandList);
            SetupDropdown(tables[12], powerUpList);
            SetupDropdown(tables[13], thrownList);
        }


        void SetupDropdown(ItemTableContent table, List<int> array)
        {
            //Eras
            for (int i = 0; i < array.Count; i++)
            {
                table.AddDropdown(dropdownPrefab);
            }

            for (int i = 0; i < array.Count; i++)
            {
                table.dropdowns[i].value = array[i];
            }
        }


        void ClearAllSubObjectIDs()
        {
            for (int i = 0; i < itemSubtractIDList.Count; i++)
            {
                Destroy(itemSubtractIDList[i].gameObject);
            }
            itemSubtractIDList.Clear();
        }

        void ClearAllObjectIDs()
        {
            for (int i = 0; i < itemObjectIDList.Count; i++)
            {
                Destroy(itemObjectIDList[i].gameObject);
            }
            itemObjectIDList.Clear();
        }

        public void CreateNewObjectID()
        {
            ListContainer input = Instantiate(itemObjectIDPrefab, itemObjectIDContent).GetComponent<ListContainer>();
            itemObjectIDList.Add(input);
        }

        public ListContainer NewObjectID()
        {
            ListContainer input = Instantiate(itemObjectIDPrefab, itemObjectIDContent).GetComponent<ListContainer>();
            itemObjectIDList.Add(input);

            return input;
        }

        public void CreateNewSubObjectID()
        {
            ListContainer input = Instantiate(itemSubObjectIDPrefab, itemSubObjectIDContent).GetComponent<ListContainer>();
            itemSubtractIDList.Add(input);
        }
        public ListContainer NewSubObjectID()
        {
            ListContainer input = Instantiate(itemSubObjectIDPrefab, itemSubObjectIDContent).GetComponent<ListContainer>();
            itemSubtractIDList.Add(input);
            return input;
        }

        public void DeleteObjectID(ListContainer container)
        {
            itemObjectIDList.Remove(container);
            Destroy(container.gameObject);
        }
        public void DeleteSubtractObjectID(ListContainer container)
        {
            itemSubtractIDList.Remove(container);
            Destroy(container.gameObject);
        }

        void SetupItemTypeDropdown()
        {
            itemTypeList = Enum.GetNames(typeof(LootTable.LootTableType));

            List <Dropdown.OptionData> list = new List<Dropdown.OptionData>();

            for (int i = 0; i < itemTypeList.Length; i++)
            {
                Dropdown.OptionData item = new Dropdown.OptionData { text = itemTypeList[i] };
                list.Add(item);
            }

            itemTypeDropdown.ClearOptions();
            itemTypeDropdown.AddOptions(list);
        }

        void GenerateItemTables()
        {
            SetupItemTypeDropdown();

            tables = new ItemTableContent[14];

            for (int i = 0; i < tables.Length; i++)
            {
                tables[i] = Instantiate(itemTablePrefab, itemTableContent).GetComponent<ItemTableContent>();
            }

            GenerateTable(tables[0], "SET", Enum.GetNames(typeof(FVRObject.OTagSet)));
            GenerateTable(tables[1], "ERAS", Enum.GetNames(typeof(FVRObject.OTagEra)));
            GenerateTable(tables[2], "FIREARM SIZES", Enum.GetNames(typeof(FVRObject.OTagFirearmSize)));
            GenerateTable(tables[3], "FIREARM ACTION", Enum.GetNames(typeof(FVRObject.OTagFirearmAction)));
            GenerateTable(tables[4], "MODES INCLUDE", Enum.GetNames(typeof(FVRObject.OTagFirearmFiringMode)));
            GenerateTable(tables[5], "MODES EXCLUDE", Enum.GetNames(typeof(FVRObject.OTagFirearmFiringMode)));
            GenerateTable(tables[6], "FEED OPTION", Enum.GetNames(typeof(FVRObject.OTagFirearmFeedOption)));
            GenerateTable(tables[7], "MOUNTS", Enum.GetNames(typeof(FVRObject.OTagFirearmMount)));
            GenerateTable(tables[8], "ROUND POWER", Enum.GetNames(typeof(FVRObject.OTagFirearmRoundPower)));
            GenerateTable(tables[9], "ATTACHMENT FEATURES", Enum.GetNames(typeof(FVRObject.OTagAttachmentFeature)));
            GenerateTable(tables[10], "MELEE STYLE", Enum.GetNames(typeof(FVRObject.OTagMeleeStyle)));
            GenerateTable(tables[11], "MELEE HANDEDNESS", Enum.GetNames(typeof(FVRObject.OTagMeleeHandedness)));
            GenerateTable(tables[12], "POWERUP TYPE", Enum.GetNames(typeof(FVRObject.OTagPowerupType)));
            GenerateTable(tables[13], "THROWN TYPE", Enum.GetNames(typeof(FVRObject.OTagThrownType)));

        }

        public void GenerateTable(ItemTableContent table, string title, string[] enumList)
        {
            table.title.text = title;
            table.tagList = enumList;
        }


        //PURCHASE CATEGORY -------------------------------------------------------------------

        public void CreateNewPurchaseCategory()
        {
            ListContainer input = Instantiate(purchaseCategoryPrefab, purchaseCategoryContent).GetComponent<ListContainer>();
            purchaseCategoryLists.Add(input);
        }

        public ListContainer NewPurchaseCategory()
        {
            ListContainer input = Instantiate(purchaseCategoryPrefab, purchaseCategoryContent).GetComponent<ListContainer>();
            purchaseCategoryLists.Add(input);

            return input;
        }

        public void DeletePurchaseCategory(ListContainer container)
        {
            purchaseCategoryLists.Remove(container);
            Destroy(container.gameObject);
        }

        void ClearAllPurchaseCategory()
        {
            for (int i = 0; i < purchaseCategoryLists.Count; i++)
            {
                Destroy(purchaseCategoryLists[i].gameObject);
            }
            purchaseCategoryLists.Clear();
        }

        //Character -------------------------------------------------------------------

        public void RefreshCharacter()
        {
            if (DataManager.instance.character == null)
                return;

            SR_CharacterPreset chara = DataManager.instance.character;

            characterName.text = chara.name;
            characterDescription.text = chara.description;
            characterCategory.text = chara.category;
            characterFaction.text = chara.factionName;

            characterPoints.text = chara.points.ToString();
            characterPointsPerLevel.text = chara.pointsPerLevel.ToString();
            characterNewMagazineCost.text = chara.newMagazineCost.ToString();
            characterUpgradeMagazineCost.text = chara.upgradeMagazineCost.ToString();
            characterDuplicateMagazineCost.text = chara.duplicateMagazineCost.ToString();
            characterRecyclerPoints.text = chara.recyclerPoints.ToString();

            ClearAllStartGear();
            //Create new Strings
            for (int i = 0; i < chara.startGearCategories.Count; i++)
            {
                ListContainer gear = NewStartGear();
                gear.inputField.text = chara.startGearCategories[i];
            }

            ClearAllPurchaseCategory();
            for (int i = 0; i < chara.purchaseCategories.Count; i++)
            {
                ListContainer category = NewPurchaseCategory();
                category.inputField.text = chara.purchaseCategories[i].name;
                category.inputFieldB.text = chara.purchaseCategories[i].itemCategory;
                category.inputFieldC.text = chara.purchaseCategories[i].cost.ToString();
            }
        }

        public void TryNewCharacter()
        {
            if(characterLoaded)
                PopupWarning("NewCharacter", "A new character will overwrite the previous character, are you sure?");
            else
                NewCharacter();
        }

        void NewCharacter()
        {
            DataManager.instance.character = new SR_CharacterPreset();
            characterLoaded = true;
            RefreshCharacter();
            //Load visuals
        }

        public void TryLoadCharacter()
        {
            if (characterLoaded)
                PopupWarning("LoadCharacter", "Loading a character will overwrite the previous character, are you sure?");
            else
                LoadCharacter();
        }

        void LoadCharacter()
        {
            DataManager.instance.OnLoadDialogue(JSONTypeEnum.Character);
            RefreshCharacter();
        }

        public void TrySaveCharacter()
        {
            //PopupWarning("SaveCharacter", "This will overwrite the character file, are you sure?");
            SaveCharacter();
        }

        void SaveCharacter()
        {
            if (DataManager.instance.character == null)
                return;
            SR_CharacterPreset chara = DataManager.instance.character;

            chara.name = characterName.text;
            chara.description = characterDescription.text;
            chara.category = characterCategory.text;
            chara.factionName = characterFaction.text;

            chara.points = int.Parse(characterPoints.text);
            chara.pointsPerLevel = int.Parse(characterPointsPerLevel.text);
            chara.newMagazineCost = int.Parse(characterNewMagazineCost.text);
            chara.upgradeMagazineCost = int.Parse(characterUpgradeMagazineCost.text);
            chara.duplicateMagazineCost = int.Parse(characterDuplicateMagazineCost.text);
            chara.recyclerPoints = int.Parse(characterRecyclerPoints.text);


            //Starting Gear
            chara.startGearCategories.Clear();

            for (int i = 0; i < startGearLists.Count; i++)
            {
                chara.startGearCategories.Add(startGearLists[i].inputField.text);
            }

            //Purchase Categories

            chara.purchaseCategories.Clear();

            for (int i = 0; i < purchaseCategoryLists.Count; i++)
            {
                SR_PurchaseCategory category = new SR_PurchaseCategory();
                category.name = purchaseCategoryLists[i].inputField.text;
                category.itemCategory = purchaseCategoryLists[i].inputFieldB.text;
                category.cost = int.Parse(purchaseCategoryLists[i].inputFieldC.text);

                chara.purchaseCategories.Add(category);
            }

            //SAVE CHARACTER
            string json = JsonUtility.ToJson(chara, true);
            DataManager.instance.OnSaveDialogue(JSONTypeEnum.Character, json, chara.name);
        }

        //Start Gear -------------------------------------------------------------------

        public void CreateNewStartGear()
        {
            ListContainer input = Instantiate(startGearPrefab, characterStartGearContent).GetComponent<ListContainer>();
            startGearLists.Add(input);
        }


        public ListContainer NewStartGear()
        {
            ListContainer input = Instantiate(startGearPrefab, characterStartGearContent).GetComponent<ListContainer>();
            startGearLists.Add(input);

            return input;
        }

        public void DeleteStartGear(ListContainer container)
        {
            startGearLists.Remove(container);
            Destroy(container.gameObject);
        }

        void ClearAllStartGear()
        {
            for (int i = 0; i < startGearLists.Count; i++)
            {
                Destroy(startGearLists[i].gameObject);
            }
            startGearLists.Clear();
        }

        //Popup Warning  -------------------------------------------------------------------

        public void PopupWarning(string method, string text)
        {
            popupConfirmMethod = method;
            warningMessage.text = text;
            warningPanel.SetActive(true);
        }

        public void ConfirmWarning()
        {
            warningPanel.SetActive(false);
            if (popupConfirmMethod == "")
                return;

            Invoke(popupConfirmMethod, 0);
            popupConfirmMethod = "";
        }

        public void CloseWarning()
        {
            warningPanel.SetActive(false);
            popupConfirmMethod = "";
        }

        //Misc  -------------------------------------------------------------------

        public void TryQuitGame()
        {
            PopupWarning("QuitGame", "Any unsaved data will be lost! Are you sure you want to quit? ");
        }

        void QuitGame()
        {
            Debug.Log("Quit Game");
            Application.Quit();
        }
    }
}