using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Supply_Raid_Editor
{
    public class ItemCategoryUI : MonoBehaviour
    {
        public static ItemCategoryUI instance;

        [Header("Pages")]
        public GameObject objectGroupsPage;
        public GameObject lootTablePage;
        public GameObject subtractivePage;


        [Header("Input Fields")]
        public Image itemThumbnail;
        public InputField itemName;
        public InputField minCapacity;
        public InputField maxCapacity;
        public Dropdown categoryDropdown;

        private void Awake()
        {
            instance = this;

            //Disable all prefabs
            objectGroupPrefab.SetActive(false);
            objectIDPrefab.SetActive(false);
            subtractiveIDPrefab.SetActive(false);
            itemTablePrefab.SetActive(false);
        }

        private void Start()
        {
            GenerateItemTables();
        }

        public void OpenPage(int i)
        {
            CloseAllPages();
            switch (i)
            {
                case 0:
                    objectGroupsPage.SetActive(true);
                    break;
                case 1:
                    lootTablePage.SetActive(true);
                    break;
                case 2:
                    subtractivePage.SetActive(true);
                    break;
            }
        }

        public void CloseAllPages()
        {
            objectGroupsPage.SetActive(false);
            lootTablePage.SetActive(false);
            subtractivePage.SetActive(false);
        }

        private void Update()
        {

            /*
            if (objectGroupIndex == -1)
                newObjectIDBtn.SetActive(false);
            else
                newObjectIDBtn.SetActive(true);
            */
        }

        //-----------------------------------------------------------------------------
        //Item Category Creation
        //-----------------------------------------------------------------------------

        public void CreateItemCategory()
        {
            LoadItemCategory(new SR_ItemCategory());
            itemThumbnail.sprite = null;
        }


        public void LoadItemCategory(SR_ItemCategory item)
        {
            DataManager.instance.itemCategory = item;

            UpdateUI();
            CloseAllPages();
        }

        /// <summary>
        /// Applies ALL Item category data from Editor to the SR Item Category Class
        /// </summary>
        public void UpdateItemCategory()
        {
            SR_ItemCategory item = DataManager.instance.itemCategory;

            //Name
            item.name = itemName.text;

            //Capacity
            item.minCapacity = int.Parse(minCapacity.text);
            item.maxCapacity = int.Parse(maxCapacity.text);

            //Ammo Count
            item.ammoLimitedCount = int.Parse(limitedAmmoCount.text);
            item.ammoSpawnLockedCount = int.Parse(spawnLockAmmoCount.text);

            //Loot Tags
            item.lootTagsEnabled = lootTagsEnabled.isOn;

            //Type
            item.type = (LootTable.LootTableType)categoryDropdown.value;

            //Object Groups
            item.objectGroups = objectGroupsData;

            //Loot Table

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

            //Update UI
            UpdateUI();
        }

        public void UpdateUI()
        {
            //Clear old UI elements
            ClearAllObjectGroups();
            ClearAllObjectIDList();
            ClearAllSubtractiveIDList();

            //Close relevent Elements
            objectIDPanel.SetActive(false);

            //Load Data
            SR_ItemCategory item = DataManager.instance.itemCategory;

            //Name
            itemName.text = item.name;

            //Capacity
            minCapacity.text = item.minCapacity.ToString();
            maxCapacity.text = item.maxCapacity.ToString();

            //Ammo Count
            limitedAmmoCount.text = item.ammoLimitedCount.ToString();
            spawnLockAmmoCount.text = item.ammoSpawnLockedCount.ToString();

            //Loot Tags
            lootTagsEnabled.isOn = item.lootTagsEnabled;
            //lootTagsButton.SetActive(item.lootTagsEnabled);

            //Type
            categoryDropdown.value = (int)item.type;

            //Load Object Groups Data
            objectGroupsData = item.objectGroups;

            //Populate Object Groups
            for (int i = 0; i < item.objectGroups.Count; i++)
            {
                CreateObjectGroupUI(item.objectGroups[i]);
            }

            //Subtraction ID
            for (int i = 0; i < item.subtractionID.Count; i++)
            {
                CreateSubtractiveIDUI(item.subtractionID[i]);
            }

            //Loot Table

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

        /// <summary>
        /// Try to Save the Item Category to Json
        /// </summary>
        public void TrySaveItemCategory()
        {
            if (DataManager.instance.itemCategory == null)
                return;
            //MenuManager.instance.PopupWarning("SaveCharacter", "This will overwrite the character file, are you sure?");
            SaveItemCategory();
        }

        ///Save to Json
        public void SaveItemCategory()
        {
            if (DataManager.ItemCategory() == null)
                return;

            //Final update to make sure settings are correct
            UpdateItemCategory();

            //SAVE CHARACTER
            string json = JsonUtility.ToJson(DataManager.ItemCategory(), true);
            DataManager.instance.OnSaveDialogue(JSONTypeEnum.ItemCategory, json, DataManager.ItemCategory().name);
        }

        #region Loot

        //-----------------------------------------------------------------------------
        //Loot Tables
        //-----------------------------------------------------------------------------

        [Header("Loot Table")]
        public GameObject itemTablePrefab;
        public Transform itemTableContent;
        public GameObject itemDropdownPrefab;

        //Internal Data
        private ItemTableContent[] tables;

        //-----------------------------------------------------------------------------
        //Generation
        //-----------------------------------------------------------------------------

        void GenerateItemTables()
        {
            tables = new ItemTableContent[14];

            for (int i = 0; i < tables.Length; i++)
            {
                tables[i] = Instantiate(itemTablePrefab, itemTableContent).GetComponent<ItemTableContent>();
                tables[i].gameObject.SetActive(true);
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

        void ClearAllItemTables()
        {
            for (int i = 0; i < tables.Length; i++)
            {
                tables[i].ClearAllDropdowns();
            }
        }
        void SetupDropdown(ItemTableContent table, List<int> array)
        {
            //Eras
            for (int i = 0; i < array.Count; i++)
            {
                table.AddDropdown(itemDropdownPrefab);
            }

            for (int i = 0; i < array.Count; i++)
            {
                table.dropdowns[i].value = array[i];
            }
        }


        #endregion


        #region Object IDS
        //-----------------------------------------------------------------------------
        //OBJECT Groups
        //-----------------------------------------------------------------------------

        [Header("Object Groups")]
        public GameObject objectGroupPrefab;
        public Transform objectGroupsContent;
        public List<ObjectGroupUI> objectGroupsList = new List<ObjectGroupUI>();

        [Header("Object IDs")]
        public GameObject objectIDPanel;
        public Text objectIDTitle;
        public GameObject newObjectIDBtn;
        public GameObject objectIDPrefab;
        public Transform objectIDsContent;
        public List<ObjectIDUI> objectIDsList = new List<ObjectIDUI>();

        //Object Data - Load in from the Item Category and saved back to item category
        private ObjectGroup objectGroupSelectedData; //A Selection of ObjectGroupsData
        private List<ObjectGroup> objectGroupsData = new List<ObjectGroup>();


        [Header("Subtractive IDs")]
        public GameObject subtractiveIDPrefab;
        public Transform subtractiveIDContent;
        public List<SubtractiveIDUI> subtractiveIDList = new List<SubtractiveIDUI>();

        //---------------------
        // Object Group Data Management
        //---------------------

        //-- CREATE DATA --
        public void CreateNewObjectGroup()
        {
            SR_ItemCategory item = DataManager.instance.itemCategory;

            ObjectGroup objectGroup = new ObjectGroup { name = "New Group" };

            item.objectGroups.Add(objectGroup);

            //Update Item Category
            UpdateItemCategory();

            //Auto Open newest group
            OpenObjectGroup(objectGroupsList[objectGroupsList.Count - 1]);
        }

        public void CreateNewObjectID()
        {
            CreateObjectID("");
            OpenObjectGroup(GetObjectGroupUI(objectGroupSelectedData));
        }

        void CreateObjectID(string id)
        {
            if (objectGroupSelectedData == null)
                return;

            objectGroupSelectedData.objectID.Add(id);

            //Update Item Category
            UpdateItemCategory();
        }

        public void CreateNewSubtractiveID()
        {
            CreateSubtractiveID("");
        }

        void CreateSubtractiveID(string id)
        {
            if (DataManager.ItemCategory() == null)
                return;

            DataManager.ItemCategory().subtractionID.Add(id);

            //Update Item Category
            UpdateItemCategory();
        }

        //-- REMOVE DATA --

        public void RemoveObjectGroup(ObjectGroup group)
        {
            SR_ItemCategory item = DataManager.instance.itemCategory;
            item.objectGroups.Remove(group);

            //Update UI
            UpdateItemCategory();
        }

        public void RemoveObjectID(string id)
        {
            objectGroupSelectedData.objectID.Remove(id);

            //Update UI
            UpdateItemCategory();
        }

        public void RemoveSubtractionID(string id)
        {
            DataManager.ItemCategory().subtractionID.Remove(id);

            UpdateItemCategory();
        }

        //---------------------
        // Object Group UI
        //---------------------

        //-- OPEN UI --

        public void OpenObjectGroup(ObjectGroupUI group)
        {
            objectIDTitle.text = group.objectGroup.name + " IDs";

            //Clear All objectIDs
            ClearAllObjectIDList();

            //Load all ObjectIDs
            for (int i = 0; i < group.objectGroup.objectID.Count; i++)
            {
                CreateObjectIDUI(group.objectGroup.objectID[i]);
            }
            
            //Set as Selected
            objectGroupSelectedData = group.objectGroup;

            objectIDPanel.SetActive(true);
        }

        // -- CREATION UI --

        void CreateObjectIDUI(string id)
        {
            //Setup UI
            ObjectIDUI objectID = Instantiate(objectIDPrefab, objectIDsContent).GetComponent<ObjectIDUI>();
            objectID.gameObject.SetActive(true);
            objectID.inputField.text = id;
            objectID.objectGroup = objectGroupSelectedData;

            //Add to UI List
            objectIDsList.Add(objectID);

            objectID.index = objectIDsList.Count - 1;
        }

        void CreateObjectGroupUI(ObjectGroup group)
        {
            Debug.Log("New GoupUI");
            ObjectGroupUI objectGroup = Instantiate(objectGroupPrefab, objectGroupsContent).GetComponent<ObjectGroupUI>();
            objectGroup.gameObject.SetActive(true);
            objectGroup.inputName.text = group.name;
            objectGroup.objectGroup = group;

            objectGroupsList.Add(objectGroup);
        }

        void CreateSubtractiveIDUI(string id)
        {
            //Setup UI
            SubtractiveIDUI subID = Instantiate(subtractiveIDPrefab, subtractiveIDContent).GetComponent<SubtractiveIDUI>();
            subID.gameObject.SetActive(true);
            subID.inputField.text = id;
            subID.category = DataManager.ItemCategory();

            //Add to UI List
            subtractiveIDList.Add(subID);

            subID.index = subtractiveIDList.Count - 1;
        }

        // -- Clear UI --

        void ClearAllObjectGroups()
        {
            for (int i = 0; i < objectGroupsList.Count; i++)
            {
                if (objectGroupsList[i] != null)
                    Destroy(objectGroupsList[i].gameObject);
            }

            objectGroupsList.Clear();
        }

        void ClearAllObjectIDList()
        {
            for (int i = 0; i < objectIDsList.Count; i++)
            {
                if (objectIDsList[i] != null)
                    Destroy(objectIDsList[i].gameObject);
            }

            objectIDsList.Clear();
        }

        void ClearAllSubtractiveIDList()
        {
            for (int i = 0; i < subtractiveIDList.Count; i++)
            {
                if (subtractiveIDList[i] != null)
                    Destroy(subtractiveIDList[i].gameObject);
            }

            subtractiveIDList.Clear();
        }

        //-- Misc --

        ObjectGroupUI GetObjectGroupUI(ObjectGroup group)
        {
            for (int i = 0; i < objectGroupsList.Count; i++)
            {
                if (objectGroupsList[i].objectGroup == group)
                    return objectGroupsList[i];
            }

            return null;
        }

        //-----------------------------------------------------------------------------
        // END OF OBJECT GROUPS
        //-----------------------------------------------------------------------------


        [Header("Ammo Count")]
        public InputField limitedAmmoCount;
        public InputField spawnLockAmmoCount;

        public Toggle lootTagsEnabled;
        public GameObject lootTagsButton;

        #endregion
    }
}
