using FistVR;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using BepInEx;

namespace SupplyRaid
{
    public class SR_BuyMenu : MonoBehaviour
    {
        public static SR_BuyMenu instance;

        [Header("Spawn Points")]
        [SerializeField] Transform[] spawnPoints;

        [Header("Loot Tables")]
        private List<SR_PurchaseCategory> purchaseCategories = new List<SR_PurchaseCategory>();
        private LootTable[] lootTables;

        //[Header("Loot Buttons")]
        //[HideInInspector] public SR_GenericButton[] buttons;
        //[SerializeField] AudioClip[] audioClips = new AudioClip[3];
        //[SerializeField] AudioSource audioSource;

        [Header("Prefabs")]
        [SerializeField] GameObject buttonTabPrefab;    //Tab Button
        [SerializeField] GameObject buttonContainerPrefab;       //Tab Container
        [SerializeField] GameObject buyButtonPrefab;    //Buy Button

        [Header("Tabs")]
        [SerializeField] Transform tabContainer;        //Tab Container that holds Buttons
        [SerializeField] BuyMenuContainer[] tabContainers;  //Tab Menus that open
        //[SerializeField] SR_GenericButton[] tabButtons; //Loaded Buttons in Tabs

        public Text pointDisplay;

        void Awake()
        {
            instance = this;
        }

        // Use this for initialization
        public void Setup()
        {
            if (SR_Manager.instance != null)
            {
                SR_Manager.PointEvent += UpdatePoints;
                SR_Manager.LaunchedEvent += StartGame;
                //Debug.Log("Assigned EVENTS <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                pointDisplay.text = SR_Manager.instance.Points.ToString();
            }
        }

        void OnDestroy()
        {
            SR_Manager.PointEvent -= UpdatePoints;
            SR_Manager.LaunchedEvent -= StartGame;
        }

		// Update is called once per frame
		void Update()
		{

        }

        private void StartGame()
        {
            purchaseCategories = SR_Manager.instance.character.purchaseCategories;
            GenerateLootTables();
            GenerateButtons();
        }

        public void OpenMenu(int index)
        {
            for (int x = 0; x < tabContainers.Length; x++)
            {
                //Top menus
                if (tabContainers[x].container)
                    tabContainers[x].container.SetActive(false);
            }
            tabContainers[index].container.SetActive(true);
            SR_Manager.PlayConfirmSFX();
        }

        private void UpdatePoints(int i)
        {
            if(SR_Manager.instance != null)
                pointDisplay.text = SR_Manager.instance.Points.ToString();
        }

        private void GenerateButtons()
        {
            //Setup Categories
            List<string> loadedCategories = new List<string>();
            for (int i = 0; i < purchaseCategories.Count; i++)
            {
                if (!loadedCategories.Contains(purchaseCategories[i].ItemCategory().category))
                {
                    if (purchaseCategories[i].ItemCategory().category == "")
                    {
                        string newCategory;
                        switch (purchaseCategories[i].ItemCategory().type)
                        {
                            default:
                            case LootTable.LootTableType.Firearm:
                                newCategory = "Standard_Firearm";
                                break;
                            case LootTable.LootTableType.Powerup:
                                newCategory = "Standard_Powerup";
                                break;
                            case LootTable.LootTableType.Thrown:
                                newCategory = "Standard_Thrown";
                                break;
                            case LootTable.LootTableType.Attachments:
                                newCategory = "Standard_Attachments";
                                break;
                            case LootTable.LootTableType.Melee:
                                newCategory = "Standard_Melee";
                                break;
                        }

                        if (!loadedCategories.Contains(newCategory))
                            loadedCategories.Add(newCategory);

                        purchaseCategories[i].ItemCategory().category = newCategory;
                    }
                    else
                        loadedCategories.Add(purchaseCategories[i].ItemCategory().category);
                }
            }

            //Generate Tabs and Containers
            tabContainers = new BuyMenuContainer[loadedCategories.Count];

            for (int i = 0; i < tabContainers.Length; i++)
            {
                tabContainers[i] = new BuyMenuContainer();

                //Setup Category
                tabContainers[i].name = loadedCategories[i];

                //Setup Container
                tabContainers[i].container = Instantiate(buttonContainerPrefab, buttonContainerPrefab.transform.parent);
                tabContainers[i].container.SetActive(false);

                //Setup Tab Button
                tabContainers[i].tabButton = Instantiate(buttonTabPrefab, tabContainer).GetComponent<SR_GenericButton>();
                tabContainers[i].tabButton.gameObject.SetActive(true);
                tabContainers[i].tabButton.index = i;

                //Setup Icon
                if (tabContainers[i].name != "")
                {
                    string[] directories = Directory.GetFiles(Paths.PluginPath, ("*" + tabContainers[i].name + ".png"), SearchOption.AllDirectories);
                    if (directories.Length > 0 && directories[0] != "")
                    {
                        Sprite icon = SR_Global.LoadSprite(directories[0]);
                        tabContainers[i].tabButton.thumbnail.sprite = icon;
                    }
                }
                else
                    Debug.LogError("Supply Raid: No Icon found for category " + tabContainers[i].name);
            }

            //Populate Tabs with Buy Buttons
            for (int i = 0; i < purchaseCategories.Count; i++)
            {
                //Populate all menus
                for (int x = 0; x < tabContainers.Length; x++)
                {
                    //Found Correct Tab
                    if (purchaseCategories[i].ItemCategory().category == tabContainers[x].name)
                    {
                        SR_GenericButton newBtn = Instantiate(buyButtonPrefab, tabContainers[x].container.transform).GetComponent<SR_GenericButton>();
                        newBtn.gameObject.SetActive(true);
                        newBtn.index = i;   //Purchase Category Index not Tab

                        newBtn.thumbnail.sprite = purchaseCategories[i].ItemCategory().Thumbnail();
                        newBtn.text.text = purchaseCategories[i].cost.ToString();
                        newBtn.name = purchaseCategories[i].ItemCategory().name;

                        break;
                    }
                }
            }
        }

        public void SpawnLootButton(int i)
        {
            //Debug.Log("Press button " + i + "  - " + lootCategories[i].name);
            if (i > lootTables.Length || i > purchaseCategories.Count)
            {
                SR_Manager.PlayFailSFX();
                return;
            }


            if (SR_Manager.EnoughPoints(purchaseCategories[i].cost))
            {
                if (SR_Global.SpawnLoot(lootTables[i], purchaseCategories[i].ItemCategory(), spawnPoints))
                {
                    SR_Manager.PlayConfirmSFX();
                    SR_Manager.SpendPoints(purchaseCategories[i].cost);
                }
                else
                {
                    SR_Manager.PlayFailSFX();
                }
            }
            else
            {
                //Play Sad Sound
                SR_Manager.PlayFailSFX();
                return;
            }
        }


        private void GenerateLootTables()
        {
            Debug.Log("Supply Raid: Purchase Items: " + purchaseCategories.Count);
            //Generate All Loot Category tables
            lootTables = new LootTable[purchaseCategories.Count];

            for (int i = 0; i < purchaseCategories.Count; i++)
            {
                if (purchaseCategories[i] != null)
                    lootTables[i] = purchaseCategories[i].ItemCategory().InitializeLootTable();
                else
                    Debug.Log("Supply Raid: Missing Purchase Category");
            }
        }

        /*
        public void SetTab(int exception)
        {
            for (int i = 0; i < tabContent.Length; i++)
            {
                tabContent[i].gameObject.SetActive(false);
                
                if(i == exception)
                    tabContent[i].gameObject.SetActive(true);
            }
        }
        */

        public void SpawnObjectAtPlace(FVRObject obj, Vector3 pos, Quaternion rotation)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(obj.GetGameObject(), pos, rotation);
            FVRFireArmMagazine component = gameObject.GetComponent<FVRFireArmMagazine>();
            if (component != null && component.RoundType != FireArmRoundType.aFlameThrowerFuel)
            {
                component.ReloadMagWithTypeUpToPercentage(AM.GetDefaultRoundClass(component.RoundType), Mathf.Clamp(Random.Range(0.3f, 1f), 0.1f, 1f));
            }
        }

        public void SpawnAmmoAtPlaceForGun(FVRObject gun, Vector3 pos, Quaternion rotation)
        {
            if (gun.CompatibleMagazines.Count > 0)
            {
                SpawnObjectAtPlace(gun.CompatibleMagazines[Random.Range(0, gun.CompatibleMagazines.Count - 1)], pos, rotation);
            }
            else if (gun.CompatibleClips.Count > 0)
            {
                SpawnObjectAtPlace(gun.CompatibleClips[Random.Range(0, gun.CompatibleClips.Count - 1)], pos, rotation);
            }
            else if (gun.CompatibleSpeedLoaders.Count > 0)
            {
                SpawnObjectAtPlace(gun.CompatibleSpeedLoaders[Random.Range(0, gun.CompatibleSpeedLoaders.Count - 1)], pos, rotation);
            }
            else if (gun.CompatibleSingleRounds.Count > 0)
            {
                int num = Random.Range(2, 5);
                for (int i = 0; i < num; i++)
                {
                    Vector3 vector = pos + Vector3.up * (0.05f * (float)i);
                    SpawnObjectAtPlace(gun.CompatibleSingleRounds[Random.Range(0, gun.CompatibleSingleRounds.Count - 1)], vector, rotation);
                }
            }
        }
    }

    [System.Serializable]
    public class BuyMenuContainer
    {
        public string name;
        public SR_GenericButton tabButton;
        public SR_GenericButton[] buttons;
        public GameObject container;
    }
}