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
        public Text itemsDrop;
        public Text sosigWeapons;
        public Text itemSpawner;
        public Text captureMode;
        public Text playerHand;
        public Text captureCount;
        public Text linearSupplyPoints;
        public Text maxEnemies;
        public Text maxSquadEnemies;

        public GameObject linearOption;
        public GameObject lauchGameButton;
        public GameObject clientRefreshButton;

        public float optionDifficulty = 1f;

        public GameObject settingsButton;
        public GameObject settingsMenu;
        public GameObject settingsPrefab;

        [Header("Extraction")]
        public Text countDownText;
        public GameObject countDownCanvas;

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

        [Header("Profile")]
        [SerializeField] GameObject profileLoadButton;
        [SerializeField] InputField profileInput;
        [SerializeField] GameObject profileBtnPrefab;
        [SerializeField] List<SR_GenericButton> profileButtons = new List<SR_GenericButton>();
        [SerializeField] SR_GenericButton profileSelected;

        [Header("Networking")]
        public GameObject[] clientObjects;

        [Header("Audio - Extraction")]
        public AudioClip audioExtractionTick;     //Clock Ticking

        void Awake()
        {
            instance = this;
        }

        public void Setup()
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
                clientRefreshButton.SetActive(true);
                lauchGameButton.SetActive(SR_Manager.instance.gameServerRunning);
                UpdateGameOptions();
                profileLoadButton.SetActive(false);
            }

            //Force Hide Settings Menu
            if(settingsButton != null)
                settingsButton.SetActive(false);    //Hide for Release
            if(settingsMenu != null)
                settingsMenu.SetActive(false);

            //Populate Settings Menu
            SR_ModLoader.LoadProfiles();
        }

        void SetOptions()
        {
            //Map override
            linearOption.SetActive(SR_Manager.instance.forceCaptureOrder == -1 ? true : false);
            if (linearOption.activeSelf)
            {
                switch (SR_Manager.profile.captureOrder)
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
            SR_Manager.profile.respawn = !SR_Manager.profile.respawn;
            UpdateGameOptions();
        }

        public void ToggleLinearSP()
        {
            SR_Manager.profile.captureOrder++;
            if (SR_Manager.profile.captureOrder > 2)
                SR_Manager.profile.captureOrder = 0;
            UpdateGameOptions();
        }

        public void SetDifficulty(float level)
        {
            SR_Manager.profile.difficulty += level;
            UpdateGameOptions();
        }

        public void ToggleCaptureMode()
        {
            SR_Manager.profile.captureZone = !SR_Manager.profile.captureZone;
            UpdateGameOptions();
        }

        public void AdjustStartLevel(int i)
        {
            if(SR_Manager.Faction() != null)
                SR_Manager.profile.startLevel = Mathf.Clamp(SR_Manager.profile.startLevel + i, 0, SR_Manager.instance.faction.levels.Length - 1);

            /*
            //Catch starting level being ahead of captures
            if (SR_Manager.profile.Captures < SR_Manager.profile.StartLevel)
                SR_Manager.profile.Captures = SR_Manager.profile.StartLevel;
            */

            UpdateGameOptions();
        }

        public void ChangeMaxEnemies(int i)
        {
            SR_Manager.profile.maxEnemies = Mathf.Clamp(SR_Manager.profile.maxEnemies + i, 3, int.MaxValue);
            UpdateGameOptions();
        }

        public void ChangeMaxSquadEnemies(int i)
        {
            SR_Manager.profile.maxSquadEnemies = Mathf.Clamp(SR_Manager.profile.maxSquadEnemies + i, 0, int.MaxValue);
            UpdateGameOptions();
        }

        public void ChangeCaptureCount(int i)
        {
            SR_Manager.profile.captures = Mathf.Clamp(SR_Manager.profile.captures + i, 0, int.MaxValue);

            UpdateGameOptions();
        }

        public void ChangePlayerCount(float i)
        {
            SR_Manager.profile.playerCount = Mathf.Clamp(SR_Manager.profile.playerCount + i, 0.25f, 8f);
            UpdateGameOptions();
        }

        public void ChangeFreeBuyMenu()
        {
            SR_Manager.profile.freeBuyMenu = !SR_Manager.profile.freeBuyMenu;
            UpdateGameOptions();
        }

        public void ChangeDropRate(int rate)
        {
            SR_Manager.profile.itemsDrop += rate;

            if (SR_Manager.profile.itemsDrop > 100)
                SR_Manager.profile.itemsDrop = 0;
            else if (SR_Manager.profile.itemsDrop < 0)
                SR_Manager.profile.itemsDrop = 100;

            UpdateGameOptions();
        }

        public void ChangeHealth(int health)
        {
            //Change Health to 100 intervals when below 1000
            if (health == -1000)
            {
                if (SR_Manager.profile.playerHealth <= 1000)
                    health = -100;
                else if (SR_Manager.profile.playerHealth > 10000)
                    health = -2000;
            }
            else if (health == 1000)
            {
                if (SR_Manager.profile.playerHealth >= 10000)
                    health = 2000;
                else if (SR_Manager.profile.playerHealth == 1)
                    health = 99;
                else if(SR_Manager.profile.playerHealth < 1000)
                    health = 100;
            }

            SR_Manager.profile.playerHealth = Mathf.Clamp(SR_Manager.profile.playerHealth + health, 1, int.MaxValue);
            UpdateGameOptions();
        }

        public void ChangeItemSpawner()
        {
            SR_Manager.profile.itemSpawner = !SR_Manager.profile.itemSpawner;
            UpdateGameOptions();
        }

        public void ChangeSosigWeapons()
        {
            SR_Manager.profile.sosigWeapons = !SR_Manager.profile.sosigWeapons;
            UpdateGameOptions();
        }

		public void ChangeHand()
		{
			SR_Manager.profile.hand = !SR_Manager.profile.hand;
            UpdateGameOptions();
        }

        public void ChangeItemLock()
		{
			SR_Manager.profile.spawnLocking = !SR_Manager.profile.spawnLocking;
            UpdateGameOptions();
        }

        public void UpdateCharacter()
        {
            if (SR_Manager.instance.character == null)
                return;

            SR_CharacterPreset character = SR_Manager.instance.character;
            characterName.text = character.name;
            characterDescription.text = character.description;
            characterThumbnail.sprite = character.Thumbnail();
        }

        public void UpdateFaction()
        {
            if (SR_Manager.instance == null)
                return;

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

            if(factionTitle)
                factionTitle.text = faction.name;
            if(factionDescription)
                factionDescription.text = faction.description;
            if(factionThumbnail)
                factionThumbnail.sprite = faction.Thumbnail();

            //Network Faction - Mostly for icon displaying
            if(SupplyRaidPlugin.h3mpEnabled && SR_Networking.instance)
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
        public void SetCharacterByName(string characterName)
        {
            if (characterName == "")
                return;

            for (int i = 0; i < SR_Manager.instance.characters.Count; i++)
            {
                if (SR_Manager.instance.characters[i].name == characterName)
                {
                    SR_Manager.instance.character = SR_Manager.instance.characters[i];
                    UpdateCharacter();
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
                difficulty.text = SR_Manager.profile.difficulty.ToString();

            if (playerCountText != null)
            {
                playerCountText.text = SR_Manager.profile.playerCount.ToString();
                playerCountText.color = Color.Lerp(Color.white, Color.red, Mathf.InverseLerp(1, 4, SR_Manager.profile.playerCount));
            }

            if(freeBuyMenu != null)
                freeBuyMenu.text =  SR_Manager.profile.freeBuyMenu ? "Enabled" : "Disabled";

            if(limitedAmmo != null)
                limitedAmmo.text =  SR_Manager.profile.spawnLocking ? "Spawnlock Enabled" : "Limited Ammo";

            if(startLevel != null)
                startLevel.text =   SR_Manager.profile.startLevel.ToString();

            if(sosigWeapons != null)
                sosigWeapons.text = SR_Manager.profile.sosigWeapons ? "Enabled" : "Disabled";

            if (itemSpawner != null)
                itemSpawner.text =  SR_Manager.profile.itemSpawner ? "Enabled" : "Disabled";

            if(captureMode != null)
                captureMode.text =  SR_Manager.profile.captureZone ? "Enabled" : "Disabled";

            if(respawn != null)
                respawn.text =      SR_Manager.profile.respawn ? "Enabled" : "Disabled";

            if (itemsDrop != null)
            {
                itemsDrop.text = SR_Manager.profile.itemsDrop.ToString() + "%";
                itemsDrop.color = Color.Lerp(Color.white, Color.red, Mathf.InverseLerp(0, 100, SR_Manager.profile.itemsDrop));
            }

            if (captureCount != null)
            {
                captureCount.text = (SR_Manager.profile.captures == 0)
                    ? "Marathon" : SR_Manager.profile.captures.ToString() + " (" + SR_ResultsMenu.FloatToTime(SR_Manager.profile.captures * 7) + ")";
            }

            int totalMaxEnemies = SR_Manager.profile.maxEnemies + SR_Manager.profile.maxSquadEnemies;

            if (maxEnemies != null)
            {
                maxEnemies.text = SR_Manager.profile.maxEnemies.ToString();
                maxEnemies.color = Color.Lerp(Color.white, Color.red, Mathf.InverseLerp(16, 24, totalMaxEnemies));
            }

            if (maxSquadEnemies != null)
            {
                maxSquadEnemies.text = SR_Manager.profile.maxSquadEnemies.ToString();
                maxSquadEnemies.color = Color.Lerp(Color.white, Color.red, Mathf.InverseLerp(16, 24, totalMaxEnemies));
            }

            //Capture Order
            if (linearSupplyPoints)
            {
                switch (SR_Manager.profile.captureOrder)
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
                switch (SR_Manager.profile.playerHealth)
                {
                    case 2500:
                        healthMode.text = "2500 - Half Health x0.5";
                        break;
                    case 5000:
                        healthMode.text = "5000 - Standard x1.0";
                        break;
                    case 7500:
                        healthMode.text = "7500 - Extra Health x1.5";
                        break;
                    case 10000:
                        healthMode.text = "10000 - Double Health x2.0";
                        break;
                    default:
                        if (SR_Manager.profile.playerHealth <= 100)
                            healthMode.text = SR_Manager.profile.playerHealth + " - One Hit";
                        else if (SR_Manager.profile.playerHealth > 20000)
                            healthMode.text = SR_Manager.profile.playerHealth + " - Too Much Health";
                        else
                            healthMode.text = SR_Manager.profile.playerHealth.ToString();
                        break;
                }

                //Standard or Below
                if(SR_Manager.profile.playerHealth <= 5000)
                    healthMode.color = Color.Lerp(Color.red, Color.white, Mathf.InverseLerp(0, 5000, SR_Manager.profile.playerHealth));
                else
                    healthMode.color = Color.Lerp(Color.white, Color.green, Mathf.InverseLerp(5000, 15000, SR_Manager.profile.playerHealth));

            }

            //Local Only
            if(playerHand != null)
                playerHand.text = SR_Manager.profile.hand ? "Right Hand" : "Left Hand";
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
                    Debug.LogError("Supply Raid:" + SR_Manager.instance.characters[i].name + " does not have a defined category");
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
                    Debug.LogError("Supply Raid: " + SR_Manager.instance.factions[i].name + " does not have a defined category");
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

        //--------------------------------------------------------------------------------------------------------
        // Profiles
        //--------------------------------------------------------------------------------------------------------
        public void PopulateProfiles()
        {
            for (int i = 0; i < profileButtons.Count; i++)
            {
                Destroy(profileButtons[i].gameObject);
            }
            profileButtons.Clear();

            for (int i = 0; i < SR_ModLoader.profiles.Count; i++)
            {
                if (SR_ModLoader.profiles[i] == null)
                    continue;

                SR_GenericButton btn = Instantiate(profileBtnPrefab, profileBtnPrefab.transform.parent).GetComponent<SR_GenericButton>();
                btn.gameObject.SetActive(true);
                btn.text.text = SR_ModLoader.profiles[i].name;
                profileButtons.Add(btn);
            }
        }

        public void SetProfile(string profileName)
        {
            if (profileName != "")
            {
                profileInput.text = profileName;
                SR_Manager.PlayConfirmSFX();
            }
            else
            {
                SR_Manager.PlayErrorSFX();
            }
        }

        public void SaveProfile()
        {
            if (profileInput.text == "")
            {
                SR_Manager.PlayErrorSFX();
                return;
            }

            if (SR_ModLoader.SaveProfile(profileInput.text))
            {
                SR_Manager.PlayConfirmSFX();
                SR_ModLoader.LoadProfiles();
                PopulateProfiles();
                UpdateGameOptions();
            }
            else
                SR_Manager.PlayErrorSFX();
        }

        public void LoadProfile()
        {
            if (profileInput.text == "")
            {
                SR_Manager.PlayErrorSFX();
                return;
            }

            for (int i = 0; i < SR_ModLoader.profiles.Count; i++)
            {
                if (SR_ModLoader.profiles[i].name == profileInput.text)
                {
                    Debug.Log("Supply Raid: Loading Profile - " + SR_ModLoader.profiles[i].name);

                    SR_Manager.profile = SR_ModLoader.profiles[i];
                    if(SR_Manager.profile.character != "")
                        SetCharacterByName(SR_Manager.profile.character);
                    if (SR_Manager.profile.faction != "")
                        SetFactionByName(SR_Manager.profile.faction);

                    UpdateGameOptions();
                    SR_Manager.PlayConfirmSFX();
                    return;
                }
            }
            SR_Manager.PlayErrorSFX();
        }


        public void ClientRequestSync()
        {
            if (SupplyRaidPlugin.h3mpEnabled)
            {
                SR_Manager.PlayConfirmSFX();
                SR_Networking.instance.RequestSync_Send();
            }
        }
    }
}