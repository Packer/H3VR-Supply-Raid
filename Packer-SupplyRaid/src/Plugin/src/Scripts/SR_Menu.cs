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
        public Text maxSquadEnemies;

        public GameObject linearOption;
        public GameObject lauchGameButton;

        public float optionDifficulty = 1f;

        public GameObject settingsButton;
        public GameObject settingsMenu;
        public GameObject settingsPrefab;

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
        [SerializeField] GameObject factionCategoryButtonPrefab;
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

            //Force Select First Faction
            SR_Manager.instance.faction = SR_Manager.instance.factions[0];

            //Select Default Character
            if (SR_Manager.instance.defaultCharacter != "")
            {
                for (int i = 0; i < SR_Manager.instance.characters.Count; i++)
                {
                    if (SR_Manager.instance.defaultCharacter == SR_Manager.instance.characters[i].name)
                    {
                        SR_Manager.instance.character = SR_Manager.instance.characters[i];
                        OpenCharacterCategory(SR_Manager.instance.character);
                        break;
                    }
                }                        
            }

            //If no Character selected, Assign Default Character
            if (SR_Manager.instance.character == null)
                SR_Manager.instance.character = SR_Manager.instance.characters[0];

            UpdateCharacter();

            //If no Faction Selected
            if (SR_Manager.instance.faction == null || SR_Manager.instance.faction.name == "")
                SR_Manager.instance.faction = SR_Manager.instance.factions[0];

            UpdateFaction();

            //Debug.Log("Faction: " + SR_Manager.instance.faction);

            if (Networking.IsClient())
            {
                lauchGameButton.SetActive(SR_Manager.instance.gameServerRunning);
                UpdateGameOptions();
            }

            //Force Hide Settings Menu
            if(settingsButton != null)
                settingsButton.SetActive(false);    //Hide for Release
            if(settingsMenu != null)
                settingsMenu.SetActive(false);

            //Populate Settings Menu
        }

        void SetOptions()
        {
            //Map override
            linearOption.SetActive(SR_Manager.instance.forceCaptureOrder == -1 ? true : false);
            if (linearOption.activeSelf)
            {
                switch (SR_Manager.instance.optionCaptureOrder)
                {
                    case 0:
                        linearSupplyPoints.text = "Random Order";
                        break;
                    case 1:
                        linearSupplyPoints.text = "Random";
                        break;
                    case 2:
                        linearSupplyPoints.text = "Ordered";
                        break;
                    default:
                        break;
                }
            }
            UpdateGameOptions();
        }

        public void LaunchGame()
        {
            SR_Manager.instance.LaunchGame();
        }

        public void ToggleSettingsMenu()
        {
            settingsMenu.SetActive(!settingsMenu.activeSelf);
        }

        public void ToggleRespawn()
        {
            SR_Manager.instance.optionRespawn = !SR_Manager.instance.optionRespawn;
            UpdateGameOptions();
        }

        public void ToggleLinearSP()
        {
            SR_Manager.instance.optionCaptureOrder++;
            if (SR_Manager.instance.optionCaptureOrder > 2)
                SR_Manager.instance.optionCaptureOrder = 0;
            UpdateGameOptions();
        }

        public void SetDifficulty(float level)
        {
            SR_Manager.instance.optionDifficulty += level;
            UpdateGameOptions();
        }

        public void ToggleCaptureMode()
        {
            SR_Manager.instance.optionCaptureZone = !SR_Manager.instance.optionCaptureZone;
            UpdateGameOptions();
        }

        public void AdjustStartLevel(int i)
        {
            if(SR_Manager.Faction() != null)
                SR_Manager.instance.optionStartLevel = Mathf.Clamp(SR_Manager.instance.optionStartLevel + i, 0, SR_Manager.instance.faction.levels.Length - 1);

            /*
            //Catch starting level being ahead of captures
            if (SR_Manager.instance.optionCaptures < SR_Manager.instance.optionStartLevel)
                SR_Manager.instance.optionCaptures = SR_Manager.instance.optionStartLevel;
            */

            UpdateGameOptions();
        }

        public void ChangeMaxEnemies(int i)
        {
            SR_Manager.instance.optionMaxEnemies = Mathf.Clamp(SR_Manager.instance.optionMaxEnemies + i, 6, int.MaxValue);
            UpdateGameOptions();
        }
        public void ChangeMaxSquadEnemies(int i)
        {
            SR_Manager.instance.optionMaxSquadEnemies = Mathf.Clamp(SR_Manager.instance.optionMaxSquadEnemies + i, 0, int.MaxValue);
            UpdateGameOptions();
        }

        public void ChangeCaptureCount(int i)
        {
            SR_Manager.instance.optionCaptures = Mathf.Clamp(SR_Manager.instance.optionCaptures + i, 0, int.MaxValue);

            UpdateGameOptions();
        }

        public void ChangePlayerCount(int i)
        {
            SR_Manager.instance.optionPlayerCount = Mathf.Clamp(SR_Manager.instance.optionPlayerCount + i, 1, 8);
            UpdateGameOptions();
        }

        public void ChangeFreeBuyMenu()
        {
            SR_Manager.instance.optionFreeBuyMenu = !SR_Manager.instance.optionFreeBuyMenu;
            UpdateGameOptions();
        }

        public void ChangeHealth(int health)
        {
            SR_Manager.instance.optionPlayerHealth = Mathf.Clamp(SR_Manager.instance.optionPlayerHealth + health, 0, 5);
            UpdateGameOptions();
        }

        public void ChangeItemSpawner()
        {
            SR_Manager.instance.optionItemSpawner = !SR_Manager.instance.optionItemSpawner;
            UpdateGameOptions();
        }

		public void ChangeHand()
		{
			SR_Manager.instance.optionHand = !SR_Manager.instance.optionHand;
            UpdateGameOptions();
        }

        public void ChangeItemLock()
		{
			SR_Manager.instance.optionSpawnLocking = !SR_Manager.instance.optionSpawnLocking;
            UpdateGameOptions();
        }

        public void UpdateCharacter()
        {
            if (SR_Manager.instance.character == null)
                return;

            var character = SR_Manager.instance.character;
            characterName.text = character.name;
            characterDescription.text = character.description;
            characterThumbnail.sprite = character.Thumbnail();
        }

        public void UpdateFaction()
        {
            /*
            //Assign first Faction if Blank
            if (SR_Manager.instance.faction == null)
            {
                SR_Manager.instance.faction = SR_Manager.instance.factions[0];
            }
            */

            SR_SosigFaction faction = SR_Manager.instance.faction;
            if (faction == null)
                return;

            factionTitle.text = faction.name;
            factionDescription.text = faction.description;
            factionThumbnail.sprite = faction.Thumbnail();

            //Network Faction - Mostly for icon displaying
            if(SupplyRaidPlugin.h3mpEnabled)
                SR_Networking.instance.GameOptions_Send();
        }

        public void SetFactionByName(string factionName)
        {
            if (factionName == "")
                return;

            for (int i = 0; i < SR_Manager.instance.factions.Count; i++)
            {
                if (SR_Manager.instance.factions[i].name == factionName)
                {
                    SR_Manager.instance.faction = SR_Manager.instance.factions[i];
                    UpdateFaction();
                    break;
                }
            }
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
            if (Networking.ServerRunning())
            {
                SetAllClientObjects(Networking.IsHost());
                SR_Networking.instance.GameOptions_Send(); //Only server will send this
            }

            if (difficulty != null)
                difficulty.text = SR_Manager.instance.optionDifficulty.ToString();

            if (playerCountText != null)
            {
                playerCountText.text = SR_Manager.instance.optionPlayerCount.ToString();
                playerCountText.color = Color.Lerp(Color.white, Color.red, Mathf.InverseLerp(1, 4, SR_Manager.instance.optionPlayerCount));
            }

            if(freeBuyMenu != null)
                freeBuyMenu.text =  SR_Manager.instance.optionFreeBuyMenu ? "Enabled" : "Disabled";

            if(limitedAmmo != null)
                limitedAmmo.text =  SR_Manager.instance.optionSpawnLocking ? "Spawnlock Enabled" : "Limited Ammo";

            if(startLevel != null)
                startLevel.text =   SR_Manager.instance.optionStartLevel.ToString();

            if(itemSpawner != null)
                itemSpawner.text =  SR_Manager.instance.optionItemSpawner ? "Enabled" : "Disabled";

            if(captureMode != null)
                captureMode.text =  SR_Manager.instance.optionCaptureZone ? "Enabled" : "Disabled";

            if(respawn != null)
                respawn.text =      SR_Manager.instance.optionRespawn ? "Enabled" : "Disabled";

            if (captureCount != null)
            {
                captureCount.text = (SR_Manager.instance.optionCaptures == 0)
                    ? "Marathon" : SR_Manager.instance.optionCaptures.ToString() + " (" + SR_ResultsMenu.FloatToTime(SR_Manager.instance.optionCaptures * 7) + ")";
            }

            if (maxEnemies != null)
            {
                maxEnemies.text = SR_Manager.instance.optionMaxEnemies.ToString();
                maxEnemies.color = Color.Lerp(Color.white, Color.red, Mathf.InverseLerp(10, 18, SR_Manager.instance.optionMaxEnemies));
            }

            if (maxSquadEnemies != null)
            {
                maxSquadEnemies.text = SR_Manager.instance.optionMaxSquadEnemies.ToString();
                maxSquadEnemies.color = Color.Lerp(Color.white, Color.red, Mathf.InverseLerp(8, 12, SR_Manager.instance.optionMaxSquadEnemies));
            }

            //Capture Order
            if (linearSupplyPoints)
            {
                switch (SR_Manager.instance.optionCaptureOrder)
                {
                    case 0:
                        linearSupplyPoints.text = "Random Order";
                        break;
                    case 1:
                        linearSupplyPoints.text = "Random";
                        break;
                    case 2:
                        linearSupplyPoints.text = "Ordered";
                        break;
                    default:
                        break;
                }
            }

            //Player Health
            if (healthMode != null)
            {
                switch (SR_Manager.instance.optionPlayerHealth)
                {
                    case 0:
                        healthMode.text = "One Hit";
                        break;
                    case 1:
                        healthMode.text = "Half Health x0.5";
                        break;
                    case 2:
                        healthMode.text = "Standard x1.0";
                        break;
                    case 3:
                        healthMode.text = "Extra Health x1.5";
                        break;
                    case 4:
                        healthMode.text = "Double Health x2.0";
                        break;
                    case 5:
                        healthMode.text = "Too Much Health";
                        break;
                    default:
                        healthMode.text = "Standard";
                        break;
                }
            }

            //Local Only
            if(playerHand != null)
                playerHand.text = SR_Manager.instance.optionHand ? "Right Hand" : "Left Hand";
        }

        void SetAllClientObjects(bool state)
        {
            for (int i = 0; i < clientObjects.Length; i++)
            {
                if(clientObjects[i] != null)
                    clientObjects[i].SetActive(state);
            }
        }

        //----------------------------------------
        // CHARACTERS and FACTIONS
        //----------------------------------------

        public void OpenCharacterCategory(SR_CharacterPreset character)
        {
            for (int i = 0; i < characterCategories.Count; i++)
            {
                if (characterCategories[i].name == character.category)
                {
                    OpenCharacterCategory(i);
                    break;
                }
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
            for (int i = 0; i < SR_Manager.instance.characters.Count; i++)
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
            for (int i = 0; i < SR_Manager.instance.characters.Count; i++)
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
                    Debug.LogError(SR_Manager.instance.characters[i].name + " does not have a defined category");
                    continue;
                }

                GameObject characterButton = Instantiate(characterButtonPrefab, content.transform);
                characterButton.GetComponent<SR_GenericButton>().index = i;
                characterButton.GetComponent<Text>().text = SR_Manager.instance.characters[i].name;
                characterButton.SetActive(true);
            }
        }

        void PopulateFactions()
        {
            List<string> createdCategories = new List<string>();

            //Get all our Category Names
            for (int i = 0; i < SR_Manager.instance.factions.Count; i++)
            {
                if (SR_Manager.instance.factions[i].category == "")
                    continue;

                if (!createdCategories.Contains(SR_Manager.instance.factions[i].category))
                {
                    createdCategories.Add(SR_Manager.instance.factions[i].category);
                }
            }

            PopulateCategories(createdCategories, factionCategoryContent, factionCategoryContainer, factionCategoryContent.GetChild(0).gameObject, factionCategories);

            //Create Our Faction Buttons
            for (int i = 0; i < SR_Manager.instance.factions.Count; i++)
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
                    Debug.LogError(SR_Manager.instance.factions[i].name + " does not have a defined category");
                    continue;
                }

                GameObject factionButton = Instantiate(factionButtonPrefab, content.transform);
                factionButton.GetComponent<SR_GenericButton>().index = i;
                factionButton.GetComponent<Text>().text = SR_Manager.instance.factions[i].name;
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
                categoryButton.name = createdCategories[i];
                categoryButton.SetActive(true);
            }
        }
    }
}