using H3MP;
using H3MP.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SupplyRaid
{
    public class SR_Menu : MonoBehaviour
    {
        public static SR_Menu instance;

        public Text playerCountText;
        public Text difficulty;
        public Text freeBuyMenu;
        public Text limitedAmmo;
        public Text startLevel;
        public Text healthMode;
        public Text respawn;
        public Text itemSpawner;
        public Text captureMode;
        public Text playerHand;
        public Text captureCount;
        public Text linearSupplyPoints;
        public Text maxEnemies;

        public GameObject linearOption;

        public float optionDifficulty = 1f;

        [Header("Side Menus")]
        [SerializeField] GameObject categoryContentPrefab;  //Content container for Characters/Factions

        [Header("Character")]
        [SerializeField] Text characterMenuTitle;
        [SerializeField] Text characterName;
        [SerializeField] Text characterDescription;
        [SerializeField] Image characterThumbnail;
        [SerializeField] Transform characterCategoryContent;
        [SerializeField] Transform characterCategoryContainer;
        [SerializeField] GameObject characterButtonPrefab;
        [SerializeField] GameObject characterCategoryButtonPrefab;   //Button to Open a Category
        List<GameObject> characterCategories = new List<GameObject>();


        [Header("Sosigs")]
        [SerializeField] Text factionMenuTitle;
        [SerializeField] Text factionTitle;
        [SerializeField] Text factionDescription;
        [SerializeField] Image factionThumbnail;
        [SerializeField] Transform factionCategoryContent;
        [SerializeField] Transform factionCategoryContainer;
        [SerializeField] GameObject factionButtonPrefab;
        [SerializeField] GameObject factionCategoryButtonPrefab;   //Button to Open a Category
        List<GameObject> factionCategories = new List<GameObject>();


        [Header("Networking")]
        public GameObject[] clientObjects;

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            SetOptions();
            PopulateCharacters();
            PopulateFactions();

            //Open first of everything TODO maybe disable this ?
            OpenCharacterCategory(0);
            OpenFactionCategory(0);

            //Force Select first character
            SR_Manager.instance.character = SR_Manager.instance.characters[0];
            UpdateCharacter();

            if (SR_Manager.instance.character.faction != null)
            {
                SR_Manager.instance.faction = SR_Manager.instance.character.faction;
                UpdateFaction();
            }
        }


        public void OpenCharacterCategory(int i)
        {
            if (characterCategories.Count <= 0)
                return;

            HideAllCharacterCategories();
            characterCategories[i].SetActive(true);
            characterMenuTitle.text = characterCategories[i].name;
        }
        public void OpenFactionCategory(int i)
        {
            if (factionCategories.Count <= 0)
                return;

            HideAllFactionCategories();
            factionCategories[i].SetActive(true);
            factionMenuTitle.text = factionCategories[i].name;
        }


        void PopulateCharacters()
        {
            List<string> createdCategories = new List<string>();

            //Get all our Category Names
            for (int i = 0; i < SR_Manager.instance.characters.Length; i++)
            {
                if (SR_Manager.instance.characters[i].category == "")
                    continue;

                if (!createdCategories.Contains(SR_Manager.instance.characters[i].category))
                {
                    createdCategories.Add(SR_Manager.instance.characters[i].category);
                }
            }

            PopulateCategories(createdCategories, characterCategoryContent, characterCategoryContainer, characterCategoryButtonPrefab, characterCategories);
            
            //Create Our Characters Buttons
            for (int i = 0; i < SR_Manager.instance.characters.Length; i++)
            {
                if (!SR_Manager.instance.characters[i].HasRequirement())
                    return;

                GameObject content = null;
                //Loop through all created categories until we get ours
                for (int x = 0; x < characterCategories.Count; x++)
                {
                    if (characterCategories[x].name == SR_Manager.instance.characters[i].category)
                    {
                        content = characterCategories[x];
                        break;
                    }
                }

                if (content == null)
                {
                    //No category, continue
                    Debug.LogError(SR_Manager.instance.characters[i].title + " does not have a defined category");
                    continue;
                }

                GameObject characterButton = Instantiate(characterButtonPrefab, content.transform);
                characterButton.GetComponent<SR_GenericButton>().index = i;
                characterButton.GetComponent<Text>().text = SR_Manager.instance.characters[i].title;
                characterButton.SetActive(true);
            }
        }

        void PopulateFactions()
        {
            List<string> createdCategories = new List<string>();

            //Get all our Category Names
            for (int i = 0; i < SR_Manager.instance.factions.Length; i++)
            {
                if (SR_Manager.instance.factions[i].category == "")
                    continue;

                if (!createdCategories.Contains(SR_Manager.instance.factions[i].category))
                {
                    createdCategories.Add(SR_Manager.instance.factions[i].category);
                }
            }

            PopulateCategories(createdCategories, factionCategoryContent, factionCategoryContainer, factionButtonPrefab, factionCategories);

            //Create Our Characters Buttons
            for (int i = 0; i < SR_Manager.instance.factions.Length; i++)
            {
                GameObject content = null;
                //Loop through all created categories until we get ours
                for (int x = 0; x < factionCategories.Count; x++)
                {
                    if (factionCategories[x].name == SR_Manager.instance.factions[i].category)
                    {
                        content = factionCategories[x];
                        break;
                    }
                }

                if (content == null)
                {
                    //No category, continue
                    Debug.LogError(SR_Manager.instance.factions[i].title + " does not have a defined category");
                    continue;
                }

                GameObject factionButton = Instantiate(factionButtonPrefab, content.transform);
                factionButton.GetComponent<SR_GenericButton>().index = i;
                factionButton.GetComponent<Text>().text = SR_Manager.instance.factions[i].title;
                factionButton.SetActive(true);
            }
        }
        void PopulateCategories(List<string> createdCategories, Transform categoryContent,
            Transform container, GameObject categoryBtnPrefab, List<GameObject> categories)
        {
            //Create the Character Category Content Containers
            for (int i = 0; i < createdCategories.Count; i++)
            {
                //Create new Content
                GameObject newCategory = Instantiate(categoryContentPrefab, container);
                newCategory.name = createdCategories[i];
                categories.Add(newCategory);
                newCategory.SetActive(false);
            }

            //Create Buttons For Categories
            for (int i = 0; i < createdCategories.Count; i++)
            {
                GameObject categoryButton = Instantiate(categoryBtnPrefab, categoryContent);
                categoryButton.GetComponent<SR_GenericButton>().index = i;  //Each category content index
                categoryButton.GetComponent<Text>().text = createdCategories[i];
                categoryButton.SetActive(true);
            }
        }

        void SetOptions()
        {
            //Map override
            linearOption.SetActive(SR_Manager.instance.linearOnly ? false : true);
            if (linearOption.activeSelf)
            {
                linearSupplyPoints.text = SR_Manager.instance.optionLinear ? "In Order" : "Random";
            }
            SR_Networking.instance.GameOptions_Send();
        }


        public void ToggleRespawn()
        {
            SR_Manager.instance.optionRespawn = !SR_Manager.instance.optionRespawn;
            respawn.text = SR_Manager.instance.optionRespawn ? "Enabled" : "Disabled";
            SR_Networking.instance.GameOptions_Send();
        }

        public void ToggleLinearSP()
        {
            SR_Manager.instance.optionLinear = !SR_Manager.instance.optionLinear;
            linearSupplyPoints.text = SR_Manager.instance.optionLinear ? "In Order" : "Random";
            SR_Networking.instance.GameOptions_Send();
        }

        public void SetDifficulty(float level)
        {
            SR_Manager.instance.optionDifficulty = level;
            difficulty.text = SR_Manager.instance.optionDifficulty.ToString();
            SR_Networking.instance.GameOptions_Send();
        }

        public void ToggleCaptureMode()
        {
            SR_Manager.instance.optionCaptureZone = !SR_Manager.instance.optionCaptureZone;
            captureMode.text = SR_Manager.instance.optionCaptureZone ? "Enabled" : "Disabled";
            SR_Networking.instance.GameOptions_Send();
        }

        public void AdjustStartLevel(int i)
        {
            SR_Manager.instance.optionStartLevel = Mathf.Clamp(SR_Manager.instance.optionStartLevel + i, 0, 10);
            startLevel.text = SR_Manager.instance.optionStartLevel.ToString();

            //Catch starting level being ahead of captures
            if (SR_Manager.instance.optionCaptures < SR_Manager.instance.optionStartLevel)
            {
                SR_Manager.instance.optionCaptures = SR_Manager.instance.optionStartLevel;
                captureCount.text = (SR_Manager.instance.optionCaptures == 0) ? "Marathon" : SR_Manager.instance.optionCaptures.ToString();
            }
            SR_Networking.instance.GameOptions_Send();
        }

        public void ChangeMaxEnemies(int i)
        {
            SR_Manager.instance.optionMaxEnemies = Mathf.Clamp(SR_Manager.instance.optionMaxEnemies + i, 8, int.MaxValue);
            maxEnemies.text = SR_Manager.instance.optionMaxEnemies.ToString();
            maxEnemies.color = Color.Lerp(Color.white, Color.red, Mathf.InverseLerp(12, 20, SR_Manager.instance.optionMaxEnemies));
            SR_Networking.instance.GameOptions_Send();
        }

        public void ChangeCaptureCount(int i)
        {
            SR_Manager.instance.optionCaptures = Mathf.Clamp(SR_Manager.instance.optionCaptures + i, 0, int.MaxValue);
            captureCount.text = (SR_Manager.instance.optionCaptures == 0) ? "Marathon" : SR_Manager.instance.optionCaptures.ToString();
            SR_Networking.instance.GameOptions_Send();
        }

        public void ChangePlayerCount(int i)
        {
            SR_Manager.instance.optionPlayerCount = Mathf.Clamp(SR_Manager.instance.optionPlayerCount + i, 1, 8);

            playerCountText.text = SR_Manager.instance.optionPlayerCount.ToString();
            playerCountText.color = Color.Lerp(Color.white, Color.red, Mathf.InverseLerp(1, 4, SR_Manager.instance.optionPlayerCount));

            SR_Networking.instance.GameOptions_Send();
        }

        public void ChangeFreeBuyMenu()
        {
            SR_Manager.instance.optionFreeBuyMenu = !SR_Manager.instance.optionFreeBuyMenu;
            freeBuyMenu.text = SR_Manager.instance.optionFreeBuyMenu ? "Enabled" : "Disabled";
            SR_Networking.instance.GameOptions_Send();
        }

        public void ChangeHealth(int health)
        {
            SR_Manager.instance.optionPlayerHealth = Mathf.Clamp(SR_Manager.instance.optionPlayerHealth + health, 0, 4);

            string displayText;
            switch (SR_Manager.instance.optionPlayerHealth)
			{
				case 0:
					displayText = "One Hit";
                    break;
                case 1:
                    displayText = "Half Health";
                    break;
                case 2:
                    displayText = "Standard";
                    break;
                case 3:
                    displayText = "Double Health";
                    break;
                case 4:
                    displayText = "Too Much Health";
                    break;
                default:
                    displayText = "Standard";
                    break;
			}
			healthMode.text = displayText;
            SR_Networking.instance.GameOptions_Send();
        }

        public void ChangeItemSpawner()
        {
            SR_Manager.instance.optionItemSpawner = !SR_Manager.instance.optionItemSpawner;
            itemSpawner.text = SR_Manager.instance.optionItemSpawner ? "Enabled" : "Disabled";

            SR_Networking.instance.GameOptions_Send();
        }

		public void ChangeHand()
		{
			SR_Manager.instance.optionHand = !SR_Manager.instance.optionHand;
			playerHand.text = SR_Manager.instance.optionHand ? "Right Hand" : "Left Hand";
        }

        public void ChangeItemLock()
		{

			SR_Manager.instance.optionSpawnLocking = !SR_Manager.instance.optionSpawnLocking;
            limitedAmmo.text = SR_Manager.instance.optionSpawnLocking ? "Spawnlock Enabled" : "Limited Ammo";

            SR_Networking.instance.GameOptions_Send();
        }


        public void UpdateCharacter()
        {
            var character = SR_Manager.instance.character;
            characterName.text = character.title;
            characterDescription.text = character.description;
            characterThumbnail.sprite = character.thumbnail;
        }

        public void UpdateFaction()
        {
            var faction = SR_Manager.instance.faction;
            factionTitle.text = faction.title;
            factionDescription.text = faction.description;
            factionThumbnail.sprite = faction.thumbnail;
            SR_Networking.instance.GameOptions_Send();
        }

        void HideAllCharacterCategories()
        {
            for (int i = 0; i < characterCategories.Count; i++)
            {
                characterCategories[i].SetActive(false);
            }
        }

        void HideAllFactionCategories()
        {
            for (int i = 0; i < factionCategories.Count; i++)
            {
                factionCategories[i].SetActive(false);
            }
        }

        //----------------------------------------
        //NETWORKING
        //----------------------------------------

        public void UpdateGameOptions()
        {
            //Update Arrows
            if(Mod.managerObject != null)
                SetAllClientObjects(ThreadManager.host);

            if (difficulty != null)
                difficulty.text = SR_Manager.instance.optionDifficulty.ToString();

            playerCountText.text = SR_Manager.instance.optionPlayerCount.ToString();
            freeBuyMenu.text = SR_Manager.instance.optionFreeBuyMenu ? "Enabled" : "Disabled";
            limitedAmmo.text = SR_Manager.instance.optionSpawnLocking ? "Spawnlock Enabled" : "Limited Ammo";
            startLevel.text = SR_Manager.instance.optionStartLevel.ToString();
            itemSpawner.text = SR_Manager.instance.optionItemSpawner ? "Enabled" : "Disabled";
            captureMode.text = SR_Manager.instance.optionCaptureZone ? "Enabled" : "Disabled";
            linearSupplyPoints.text = SR_Manager.instance.optionLinear ? "In Order" : "Random";
            captureCount.text = (SR_Manager.instance.optionCaptures == 0) ? "Marathon" : SR_Manager.instance.optionCaptures.ToString();
            respawn.text = SR_Manager.instance.optionRespawn ? "Enabled" : "Disabled";
            maxEnemies.text = SR_Manager.instance.optionMaxEnemies.ToString();

            string displayText;
            switch (SR_Manager.instance.optionPlayerHealth)
            {
                case 0:
                    displayText = "One Hit";
                    break;
                case 1:
                    displayText = "Half Health";
                    break;
                case 2:
                    displayText = "Standard";
                    break;
                case 3:
                    displayText = "Double Health";
                    break;
                case 4:
                    displayText = "Too Much Health";
                    break;
                default:
                    displayText = "Standard";
                    break;
            }
            healthMode.text = displayText;

            //Local Only
            playerHand.text = SR_Manager.instance.optionHand ? "Right Hand" : "Left Hand";

        }

        void SetAllClientObjects(bool state)
        {
            for (int i = 0; i < clientObjects.Length; i++)
            {
                clientObjects[i].SetActive(state);
            }
        }
    }
}