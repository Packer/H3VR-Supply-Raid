using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Supply_Raid_Editor
{
    public class MenuManager : MonoBehaviour
    {
        public static MenuManager instance;

        public Text versionText;

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

        [Header("IGNORE THIS STUFF, NOT USED")]

        [Header("Character")]
        public Image characterThumbnail;
        public InputField characterName;
        public InputField characterDescription;
        public InputField characterCategory;
        public InputField characterFaction;
        public Toggle characterPointsCatchup;

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

        private void Awake()
        {
            instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            versionText.text = "v" + Application.version;

            OpenMenuPanel();
            //GenerateItemTables();
        }

        // Update is called once per frame
        void Update()
        {
            characterTabBtn.SetActive(characterLoaded);
            factionTabBtn.SetActive(factionLoaded);
            itemCategoryTabBtn.SetActive(itemLoaded);


            //TODO make this not horrible performance
            /*
            if (itemLoaded)
            {
                GridLayoutGroup group = itemTableContent.GetComponent<GridLayoutGroup>();
                Vector2 cell = group.cellSize;
                cell.y = itemTableContent.GetComponent<RectTransform>().rect.height / 2;
                group.cellSize = cell;
            }
            */
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

        public void TryLoadItemCategory()
        {
            if (itemLoaded)
                PopupWarning("LoadItemCategory", "Loading a Item Category will overwrite the previous one, are you sure?");
            else
                LoadItemCategory();
        }

        void LoadItemCategory()
        {
            if (DataManager.instance.OnLoadDialogue(JSONTypeEnum.ItemCategory))
            {
                itemLoaded = true;
            }

            ItemCategoryUI.instance.UpdateUI();
            //RefreshItemCategory();
        }

        public void TryNewItemCategory()
        {
            if (itemLoaded)
                PopupWarning(nameof(NewItemCategory), "A new item category will overwrite the previous one, are you sure?");
            else
                NewItemCategory();
        }

        void NewItemCategory()
        {
            ItemCategoryUI.instance.CreateItemCategory();
            itemLoaded = true;
        }

        //FACTION -------------------

        public void TryNewFaction()
        {
            if (factionLoaded)
                PopupWarning(nameof(NewFaction), "A new Faction will overwrite the previous one, are you sure?");
            else
                NewFaction();

        }

        void NewFaction()
        {
            DataManager.instance.faction = new SR_SosigFaction();
            FactionUI.instance.CreateNewFaction();
            factionLoaded = true;
        }


        public void TryLoadFaction()
        {
            if (factionLoaded)
                PopupWarning(nameof(LoadFaction), "Loading a Faction will overwrite the previous one, are you sure?");
            else
                LoadFaction();
        }
        
        void LoadFaction()
        {
            if (DataManager.instance.OnLoadDialogue(JSONTypeEnum.Faction))
            {
                factionLoaded = true;
            }

            FactionUI.instance.UpdateUI();
            //RefreshItemCategory();
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

        //Points Per Level


        //Character -------------------------------------------------------------------

        /*
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
                //category.inputField.text = chara.purchaseCategories[i].name;
                category.inputFieldB.text = chara.purchaseCategories[i].itemCategory;
                category.inputFieldC.text = chara.purchaseCategories[i].cost.ToString();
            }
        }
        */

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

            CharacterUI.instance.NewCharacter();

            //RefreshCharacter();
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
            if (DataManager.instance.OnLoadDialogue(JSONTypeEnum.Character))
            {
                characterLoaded = true;
            }

            //RefreshCharacter();
        }

        public void TrySaveCharacter()
        {
            //PopupWarning("SaveCharacter", "This will overwrite the character file, are you sure?");
            SaveCharacter();
        }

        void SaveCharacter()
        {
            if (DataManager.Character() == null)
                return;

            CharacterUI.instance.SaveCharacter();

            //SAVE CHARACTER
            string json = JsonUtility.ToJson(DataManager.Character(), true);
            DataManager.instance.OnSaveDialogue(JSONTypeEnum.Character, json, DataManager.Character().name);
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

        public void OpenLink(string url)
        {
            Application.OpenURL(url);
        }

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