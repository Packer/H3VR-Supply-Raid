using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Supply_Raid_Editor
{
    public class CharacterUI : MonoBehaviour
    {
        public static CharacterUI instance;

        public GameObject[] pages;

        [Header("Infomation")]
        public Image thumbnail;
        public InputField characterName;
        public InputField description;
        public InputField category;
        public InputField defaultFaction;
        public Toggle pointsCatchup;

        //--------------------------------------
        //List Based Items

        [Header("Points Per Level")]
        public GameObject pointsTabPrefab;
        public Transform pointsContent;
        public List<PointsUI> pointsList = new List<PointsUI>();

        [Header("Subtraction ID")]
        public GameObject subtractionPrefab;
        public Transform subtractionContent;
        public List<SubtractiveIDUI> subtractionList = new List<SubtractiveIDUI>();

        [Header("Loot Drop")]
        public GameObject lootDropPrefab;
        public Transform lootDropContent;
        public List<LootDropUI> lootDropList = new List<LootDropUI>();

        [Header("Purchase Categories")]
        public GameObject purchaseCategoryPrefab;
        public Transform purchaseCategoryContent;
        public List<PurchaseCategoryUI> purchaseCategoryList = new List<PurchaseCategoryUI>();

        [Header("Start Gear")]
        public GameObject startGearPrefab;
        public List<StartGearUI> startGearLists = new List<StartGearUI>();
        public Transform startGearContent;

        //--------------------------------------

        [Header("Supply Points Panels")]
        public InputField newMagazine;
        public InputField upgradeMagazine;
        public InputField duplicateMagazine;
        public InputField customMod;
        public InputField pointsRecycle;
        public InputField tokenRecycle;

        [Header("Ammo Rearm")]
        public InputField rearmCost;
        public Dropdown rearmDropdown;
        public InputField speedloaderCost;
        public Dropdown speedLoaderDropdown;
        public InputField clipsCost;
        public Dropdown clipsDropdown;
        public InputField roundsCost;
        public Dropdown roundsDropdown;
        public List<InputField> powerMultipliers;
        public Toggle perRound;

        [Header("Panels")]
        public Toggle disableModTable;
        public Toggle disableAmmoRearm;
        public Toggle disableBuyMenu;
        public Toggle disableDuplicator;
        public Toggle disableRecycler;

        //--------------------------------------
        //Mod Tables

        [Header("Mod Table")]
        public InputField[] modTable;

        //--------------------------------------

        [Header("Ammo Types")]
        public InputField[] ammoTypes;

        //--------------------------------------

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            pointsTabPrefab.SetActive(false);
            startGearPrefab.SetActive(false);
            subtractionPrefab.SetActive(false);
            lootDropPrefab.SetActive(false);
            purchaseCategoryPrefab.SetActive(false);
        }

        public void NewCharacter()
        {

            //Defaults
            //Ammo Upgrades Costs
            for (int i = 0; i < DataManager.Character().ammoUpgradeCost.Length; i++)
            {
                if (i != 0)
                    DataManager.Character().ammoUpgradeCost[i] = 1;
                else
                    DataManager.Character().ammoUpgradeCost[i] = 0;
            }
            DataManager.Character().ammoUpgradeCost[DataManager.Character().ammoUpgradeCost.Length - 1] = 0;    //Special
            DataManager.Character().ammoUpgradeCost[17] = 0;    //Grenade

            //Mod Cost
            for (int i = 0; i < DataManager.Character().attachmentsCost.Length; i++)
            {
                DataManager.Character().attachmentsCost[i] = 1;
            }

            //Points Catchup
            DataManager.Character().pointsCatchup = true;

            //Power Multiplier
            for (int i = 0; i < DataManager.Character().powerMultiplier.Length; i++)
            {
                DataManager.Character().powerMultiplier[i] = 1;
            }

            //Default to at least 1 level point
            NewLevelPoints();

            thumbnail.sprite = null;
        }

        public void LoadCharacter()
        {
            UpdateCharacterUI();
        }

        public void SaveCharacter()
        {
            UpdateCharacter();
        }

        public void UpdateCharacterUI()
        {
            CloseAllPages();

            //Clear all tabs
            ClearAllPoints();
            ClearAllSubtractionsUI();
            ClearAllLootDropsUI();
            ClearAllPurchaseCategoryUI();
            ClearAllStartGear();

            SR_CharacterPreset character = DataManager.instance.character;

            defaultFaction.text = character.factionName;

            characterName.text = character.name;
            description.text = character.description;
            category.text = character.category;
            pointsCatchup.isOn = character.pointsCatchup;

            //Points auto assigned

            newMagazine.text = character.newMagazineCost.ToString();
            upgradeMagazine.text = character.upgradeMagazineCost.ToString();
            duplicateMagazine.text = character.duplicateMagazineCost.ToString();
            customMod.text = character.modCost.ToString();
            pointsRecycle.text = character.recyclerPoints.ToString();
            tokenRecycle.text = character.recyclerTokens.ToString();

            rearmCost.text = character.rearmingCost.ToString();
            rearmDropdown.value = character.modeRearming;
            speedloaderCost.text = character.speedLoadersCost.ToString();
            speedLoaderDropdown.value = character.modeSpeedLoaders;
            clipsCost.text = character.clipsCost.ToString();
            clipsDropdown.value = character.modeClips;
            roundsCost.text = character.roundsCost.ToString();
            roundsDropdown.value = character.modeRounds;

            //Power Multiplier
            for (int i = 0; i < character.powerMultiplier.Length; i++)
            {
                powerMultipliers[i].SetTextWithoutNotify(character.powerMultiplier[i].ToString());
            }

            perRound.isOn = character.perRound;

            //Panels
            disableAmmoRearm.isOn = character.disableAmmoTable;
            disableModTable.isOn = character.disableModtable;
            disableBuyMenu.isOn = character.disableBuyMenu;
            disableDuplicator.isOn = character.disableDuplicator;
            disableRecycler.isOn = character.disableRecycler;

            //Mod Table
            for (int i = 0; i < character.attachmentsCost.Length; i++)
            {
                modTable[i].text = character.attachmentsCost[i].ToString();
            }

            //Ammo Types
            for (int i = 0; i < character.ammoUpgradeCost.Length; i++)
            {
                ammoTypes[i].text = character.ammoUpgradeCost[i].ToString();
            }

            //points per level
            for (int i = 0; i < character.pointsLevel.Count; i++)
            {
                AddPointTab(character.pointsLevel[i]);
            }

            //Subtraction
            for (int i = 0; i < character.subtractionObjectIDs.Count; i++)
            {
                AddSubtractionUI(character.subtractionObjectIDs[i]);
            }

            //Loot Drops
            for (int i = 0; i < character.lootCategories.Count; i++)
            {
                AddLootDropUI(character.lootCategories[i]);
            }

            //Purchase Categories
            for (int i = 0; i < character.purchaseCategories.Count; i++)
            {
                AddPurchaseCategoryUI(character.purchaseCategories[i]);
            }

            //Start Gear
            for (int i = 0; i < character.startGearCategories.Count; i++)
            {
                AddStartGearUI(character.startGearCategories[i]);
            }
        }


        public void UpdateCharacter()
        {
            SR_CharacterPreset character = DataManager.instance.character;

            character.name = characterName.text;
            character.description = description.text;
            character.category = category.text;
            character.factionName = defaultFaction.text;
            character.pointsCatchup = pointsCatchup.isOn;

            //Points auto assigned

            character.newMagazineCost = float.Parse(newMagazine.text);
            character.upgradeMagazineCost = float.Parse(upgradeMagazine.text);
            character.duplicateMagazineCost = float.Parse(duplicateMagazine.text);
            character.modCost = int.Parse(customMod.text);
            character.recyclerPoints = int.Parse(pointsRecycle.text);
            character.recyclerTokens = int.Parse(tokenRecycle.text);

            //Power Multiplier
            for (int i = 0; i < powerMultipliers.Count; i++)
            {
                character.powerMultiplier[i] = float.Parse(powerMultipliers[i].text);
            }

            character.perRound = perRound.isOn;

            character.rearmingCost = int.Parse(rearmCost.text);
            character.modeRearming = rearmDropdown.value;
            character.speedLoadersCost = int.Parse(speedloaderCost.text);
            character.modeSpeedLoaders = speedLoaderDropdown.value;
            character.clipsCost = int.Parse(clipsCost.text);
            character.modeClips = clipsDropdown.value;
            character.roundsCost = int.Parse(roundsCost.text);
            character.modeRounds = roundsDropdown.value;

            character.disableAmmoTable = disableAmmoRearm.isOn;
            character.disableModtable = disableModTable.isOn;
            character.disableBuyMenu = disableBuyMenu.isOn;
            character.disableDuplicator = disableDuplicator.isOn;
            character.disableRecycler = disableRecycler.isOn;

            //Mod Table
            for (int i = 0; i < character.attachmentsCost.Length; i++)
            {
                if (modTable[i].text == "")
                    modTable[i].text = "-1";

                character.attachmentsCost[i] = int.Parse(modTable[i].text);
            }

            //Ammo Types
            for (int i = 0; i < character.ammoUpgradeCost.Length; i++)
            {
                if (ammoTypes[i].text == "")
                    ammoTypes[i].text = "-1";

                character.ammoUpgradeCost[i] = int.Parse(ammoTypes[i].text);
            }

            //Points per level
            character.pointsLevel.Clear();
            for (int i = 0; i < pointsList.Count; i++)
            {
                character.pointsLevel.Add(int.Parse(pointsList[i].inputField.text));
            }

            //Subtraction
            character.subtractionObjectIDs.Clear();
            for (int i = 0; i < subtractionList.Count; i++)
            {
                character.subtractionObjectIDs.Add(subtractionList[i].inputField.text);
            }

            //Loot Drops
            character.lootCategories.Clear();
            for (int i = 0; i < lootDropList.Count; i++)
            {
                SR_LootCategory loot = new SR_LootCategory();
                loot.chance = Mathf.Clamp01(float.Parse(lootDropList[i].chance.text));
                loot.itemCategory = lootDropList[i].itemCategory.text;

                character.lootCategories.Add(loot);
            }

            //Purchase Categories
            character.purchaseCategories.Clear();
            for (int i = 0; i < purchaseCategoryList.Count; i++)
            {
                //Empty PASS
                if (purchaseCategoryList[i].itemCategory.text == "")
                    continue;

                SR_PurchaseCategory category = new SR_PurchaseCategory();
                category.cost = int.Parse(purchaseCategoryList[i].cost.text);
                category.itemCategory = purchaseCategoryList[i].itemCategory.text;

                character.purchaseCategories.Add(category);
            }

            //Start Gear
            character.startGearCategories.Clear();
            for (int i = 0; i < startGearLists.Count; i++)
            {
                //Empty Slot
                if(startGearLists[i].itemCategory.text != "")
                    character.startGearCategories.Add(startGearLists[i].itemCategory.text);
            }
        }

        //--------------------------------------------------
        // Purchase Categories
        //--------------------------------------------------
        public void NewPurchaseCategory()
        {
            SR_PurchaseCategory category = new SR_PurchaseCategory();

            //Default Settings go here

            DataManager.Character().purchaseCategories.Add(category);
            UpdateCharacterUI();
            OpenPage(4);
        }

        void AddPurchaseCategoryUI(SR_PurchaseCategory category)
        {
            PurchaseCategoryUI ui = Instantiate(purchaseCategoryPrefab, purchaseCategoryContent).GetComponent<PurchaseCategoryUI>();
            ui.gameObject.SetActive(true);
            ui.itemCategory.text = category.itemCategory;
            ui.cost.text = category.cost.ToString();
            purchaseCategoryList.Add(ui);
        }

        public void RemovePurchaseCategoryUI(PurchaseCategoryUI ui)
        {
            if (purchaseCategoryList.Count <= 0)
                return;

            if (purchaseCategoryList.Contains(ui))
                purchaseCategoryList.Remove(ui);

            Destroy(ui.gameObject);

            UpdateCharacter();

            OpenPage(4);
        }

        void ClearAllPurchaseCategoryUI()
        {
            for (int i = 0; i < purchaseCategoryList.Count; i++)
            {
                Destroy(purchaseCategoryList[i].gameObject);
            }
            purchaseCategoryList.Clear();
        }

        //--------------------------------------------------
        // Loot Drops
        //--------------------------------------------------
        public void NewLootDrop()
        {
            SR_LootCategory loot = new SR_LootCategory();

            //Default Settings go here

            DataManager.Character().lootCategories.Add(loot);
            UpdateCharacterUI();
            OpenPage(5);
        }

        void AddLootDropUI(SR_LootCategory category)
        {
            LootDropUI loot = Instantiate(lootDropPrefab, lootDropContent).GetComponent<LootDropUI>();
            loot.gameObject.SetActive(true);
            loot.itemCategory.text = category.itemCategory;
            loot.chance.text = Mathf.Clamp01(category.chance).ToString();
            lootDropList.Add(loot);
        }

        public void RemoveLootDropUI(LootDropUI ui)
        {
            if (lootDropList.Count <= 0)
                return;

            if (lootDropList.Contains(ui))
                lootDropList.Remove(ui);

            Destroy(ui.gameObject);

            UpdateCharacter();

            OpenPage(5);
        }

        void ClearAllLootDropsUI()
        {
            for (int i = 0; i < lootDropList.Count; i++)
            {
                Destroy(lootDropList[i].gameObject);
            }
            lootDropList.Clear();
        }

        //--------------------------------------------------
        // Subtraction
        //--------------------------------------------------

        public void NewSubtraction()
        {
            DataManager.Character().subtractionObjectIDs.Add("");

            UpdateCharacterUI();
            OpenPage(5);
        }
        
        void AddSubtractionUI(string objectID)
        {
            SubtractiveIDUI ui = Instantiate(subtractionPrefab, subtractionContent).GetComponent<SubtractiveIDUI>();
            ui.gameObject.SetActive(true);
            ui.inputField.text = objectID;

            subtractionList.Add(ui);
            ui.index = subtractionList.Count;

        }

        public void RemoveSubtraction(SubtractiveIDUI ui)
        {
            if (subtractionList.Count <= 0)
                return;

            if (subtractionList.Contains(ui))
                subtractionList.Remove(ui);

            Destroy(ui.gameObject);

            UpdateCharacter();

            OpenPage(5);
        }

        void ClearAllSubtractionsUI()
        {
            for (int i = 0; i < subtractionList.Count; i++)
            {
                Destroy(subtractionList[i].gameObject);
            }
            subtractionList.Clear();
        }

        //--------------------------------------------------
        // Points
        //--------------------------------------------------

        public void NewLevelPoints()
        {
            DataManager.Character().pointsLevel.Add(1);

            UpdateCharacterUI();
            OpenPage(0);
        }

        void AddPointTab(int amount)
        {
            PointsUI points = Instantiate(pointsTabPrefab, pointsContent).GetComponent<PointsUI>();
            points.gameObject.SetActive(true);
            points.inputField.text = amount.ToString();

            points.text.text = pointsList.Count.ToString();
            pointsList.Add(points);
            points.index = pointsList.Count;
        }

        public void RemovePointsTab(PointsUI tab)
        {
            if (pointsList.Count <= 1)
                return;

            if (pointsList.Contains(tab))
                pointsList.Remove(tab);

            Destroy(tab.gameObject);

            UpdateCharacter();
            OpenPage(0);
        }

        void ClearAllPoints()
        {
            for (int i = 0; i < pointsList.Count; i++)
            {
                Destroy(pointsList[i].gameObject);
            }
            pointsList.Clear();
        }

        //--------------------------------------------------
        // Start Gear
        //--------------------------------------------------

        public void NewStartGear()
        {
            DataManager.Character().startGearCategories.Add("");

            UpdateCharacterUI();
            OpenPage(4);
        }

        void AddStartGearUI(string objectID)
        {
            StartGearUI gear = Instantiate(startGearPrefab, startGearContent).GetComponent<StartGearUI>();
            gear.gameObject.SetActive(true);
            gear.itemCategory.text = objectID;

            startGearLists.Add(gear);
        }

        public void RemoveStartGear(StartGearUI gear)
        {
            if (startGearLists.Count <= 0)
                return;

            if (startGearLists.Contains(gear))
                startGearLists.Remove(gear);

            Destroy(gear.gameObject);

            UpdateCharacter();
            OpenPage(4);
        }

        void ClearAllStartGear()
        {
            for (int i = 0; i < startGearLists.Count; i++)
            {
                Destroy(startGearLists[i].gameObject);
            }
            startGearLists.Clear();
        }

        //--------------------------------------------------

        public void OpenPage(int i)
        {
            CloseAllPages();
            pages[i].SetActive(true);
        }

        void CloseAllPages()
        {
            for (int i = 0; i < pages.Length; i++)
            {
                if (pages[i] != null)
                    pages[i].SetActive(false);
            }
        }
    }
}