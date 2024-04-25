using FistVR;
using H3MP.Networking;
using Sodalite.Api;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace SupplyRaid
{
    public class SR_Manager : MonoBehaviour
    {
        public static SR_Manager instance;

        public int CurrentCaptures
        {
            set { currentCaptures = value; }
            get { return currentCaptures; }
        }

        //[Header("Default Settings")]
        [Tooltip("Multiplier for multiplayer or harder games"), HideInInspector]
        public float optionPlayerCount = 1;
        [Tooltip("Default 1 = Normal, 0.5 = Easier, 2 = Double enemy Stats"), Range(0.5f, 2), HideInInspector]
        public float optionDifficulty = 1f;
        [Tooltip("Players currency enabled or disabled"), HideInInspector]
        public bool optionFreeBuyMenu = false;
        [Tooltip("false = Limited Ammo / true = spawn locking"), HideInInspector]
        public bool optionSpawnLocking = true;
        [Tooltip("What level do we start on? Gain all points from those prev levels"), HideInInspector]
        public int optionStartLevel = 0;
        //[Tooltip("One Hit = 0, Half = 1, Standard = 2, Extra = 3, Double = 4, Too Much HP = 5"), Range(0, 4), HideInInspector]
        public int optionPlayerHealth = 5000;
        [Tooltip("Item Spawner"), HideInInspector]
        public bool optionItemSpawner = false;
        [Tooltip("Is the capture Zone being used"), HideInInspector]
        public bool optionCaptureZone = true;
        [Tooltip("Right Hand = True, Left Hand = False"), HideInInspector]
        public bool optionHand = false;
        [Tooltip("Do we do the supply points in Array Order, \n0 = Random Order\n1 = Random\n2 = Ordered"), HideInInspector]
        public int optionCaptureOrder = 0;
        [Tooltip("How many captures until the game completes, 0 = Infinite, 1+ = Number of Captures"), HideInInspector]
        public int optionCaptures = 5;
        [Tooltip("Can the players respawn after dying?"), HideInInspector]
        public bool optionRespawn = true;
        [Tooltip("How many allowed to be alive at a time"), HideInInspector]
        public int optionMaxEnemies = 12;
        [Tooltip("How many allowed to be alive at a time"), HideInInspector]
        public int optionMaxSquadEnemies = 8;

        [Header("Game Stats")]
        public Stats stats = new Stats();

        [Header("Game Options")]
        [Tooltip("The character name that will be auto selected for this map")]
        public string defaultCharacter = "";
        [Tooltip("The character category that players can only select from")]
        public string forceCharacterCategory = "";
        [Tooltip("Force the players supply point to never move, useful for a none moving central buy zone")]
        public bool forceStaticPlayerSupplyPoint = false;
        [Tooltip("Force the map to be a specific Supply Points order \n-1 - Off\n0 - Random Order\n1 - Random\n2 - Ordered"), Range(-1, 2)]
        public int forceCaptureOrder = -1;
        [Tooltip("Forced Starting Points, -1 = Use Character Points, 0,1,2...etc = Map specific starting points")]
        public int forceStartPointsOverride = -1;
        [Tooltip("Forces Patrol Sosigs to spawn on rabbitholes instead of their patrol waypoints")]
        public bool forcePatrolInitialSpawnOnRabbitHoles = false;

        [Header("Gameplay")]
        private int points = 0;
        [HideInInspector]
        public SR_CharacterPreset character;
        [HideInInspector]
        public SR_SosigFaction faction;
        [HideInInspector, Tooltip("Has the game been launched and is running")]
        public bool gameRunning = false;
        [HideInInspector]
        public float captureProtection = 0;
        [HideInInspector]
        public int endlessLevel = 0;
        private int currentCaptures = 0;
        [HideInInspector]
        public bool gameCompleted = false;
        [HideInInspector]
        public bool inEndless = false;
        public ObstacleAvoidanceType avoidanceQuailty = ObstacleAvoidanceType.MedQualityObstacleAvoidance;

        [Header("Supply Points")]
        [HideInInspector, Tooltip("The next supply point that is being attacked by the player")]
        public int attackSupplyID = 0;
        [HideInInspector, Tooltip("The supply point the players menus are at")]
        public int playerSupplyID = 0;
        [HideInInspector]
        public int supplyOrderIndex = 0;
        [HideInInspector]
        public List<int> supplyOrder = new List<int>();
        public List<SR_SupplyPoint> supplyPoints = new List<SR_SupplyPoint>();

        // Trackers
        private bool ignoreKillStat = false;
        private int currentDefenders = 0;
        private int remainDefenders = 0;
        private float rabbitHoleTimer = 0;

        private int currentSquad = 0;
        private bool spawningSquad = false;
        List<int> squadGroups = new List<int>();
        List<int> squadIFFs = new List<int>();
        private float squadRespawnTimer = 0;

        [Header("World Transforms")]
        public Transform srMenu;
        public Transform spawnMenu;
        public Transform buyMenu;
        public Transform ammoStation;
        public Transform recycler;
        public Transform duplicator;
        public Transform attachmentStation;
        public Transform itemSpawner;
        public Transform resultsMenu;
        public Transform spawnPoint;
        public Transform spawnStation;
        public SR_CaptureZone captureZone;

        [Header("Supply Raid Data")]
        public Sprite fallbackThumbnail;
        [HideInInspector]
        public float sosigAlertness = 0;
        [HideInInspector]
        public List<SR_CharacterPreset> characters;
        [HideInInspector]
        public List<SR_SosigFaction> factions;
        [HideInInspector]
        public List<SR_ItemCategory> itemCategories;
        [HideInInspector] 
        public LootTable lt_RequiredAttachments;

        [Header("Sosig Setup")]
        public static float sosigSightMultiplier = 1;
        private float sosigSightMultiplierLast = 1;
        public SosigSettings sosigGuard = new SosigSettings();
        public SosigSettings sosigSniper = new SosigSettings();
        public SosigSettings sosigPatrol = new SosigSettings();
        public SosigSettings sosigSquad = new SosigSettings();
        private float sosigSpawnTick = 0.9f;

        private LayerMask enviromentLayer;

        [HideInInspector]
        public TNH_SosiggunShakeReloading shakeReloading = TNH_SosiggunShakeReloading.Off;
        private readonly List<Sosig> sosigs = new List<Sosig>();
        public readonly List<Sosig> defenderSosigs = new List<Sosig>();
        private readonly List<Sosig> squadSosigs = new List<Sosig>();

        //For ease of modifacation
        private readonly List<Sosig> sosigGuards = new List<Sosig>();
        private readonly List<Sosig> sosigSnipers = new List<Sosig>();
        private readonly List<Sosig> sosigPatrols = new List<Sosig>();
        private readonly List<Sosig> sosigSquads = new List<Sosig>();

        public readonly SosigAPI.SpawnOptions _spawnOptions = new SosigAPI.SpawnOptions
        {
            SpawnState = Sosig.SosigOrder.PathTo,
            SpawnActivated = true,
            EquipmentMode = SosigAPI.SpawnOptions.EquipmentSlots.All,
            SpawnWithFullAmmo = true,
        };

        [Header("Networking")]
        [HideInInspector]
        public bool gameServerRunning = false;
        [HideInInspector]
        public bool isClient = false;   //Are we a client in Multiplayer

        public delegate void LaunchedDelegate();
        public static event LaunchedDelegate LaunchedEvent;

        public delegate void PointGainDelegate(int i);
        public static event PointGainDelegate PointEvent;

        public delegate void SupplyPointChangeDelegate();
        public static event SupplyPointChangeDelegate SupplyPointChangeEvent;

        public delegate void GameCompleteDelegate();
        public static event GameCompleteDelegate GameCompleteEvent;

        public delegate void ObjectiveDelegate();
        public static event ObjectiveDelegate ObjectiveEvent;

        public delegate void EndlessDelegate();
        public static event EndlessDelegate EndlessEvent;

        [Header("Audio")]
        public AudioSource globalAudio;
        public AudioClip audioConfirm;  //Accepted Input
        public AudioClip audioFail;     //Failed Input
        public AudioClip audioError;    //Something went wrong
        public AudioClip audioRearm;    //Reload Sound
        public AudioClip audioPointsGain;    //Gaining Points
        [Header("Audio - Capture")]
        public AudioClip audioTick;     //Clock Ticking
        public AudioClip audioTickAlmost;    //Clock Ticking last 5 seconds
        public AudioClip audioCaptureComplete;     //Capture complete
        public AudioClip audioFailCapture;

        public int Points
        {
            set
            {
                if (optionFreeBuyMenu)
                    points = 99999;
                else
                    points = Mathf.Clamp(value, 0, int.MaxValue);

                if (PointEvent != null)
                    PointEvent.Invoke(points);

                Debug.Log("Points: " + points);
            }
            get { return points; }
        }

        // Use this for initialization
        void Awake()
        {
            //Load External Assets
            StartCoroutine(SR_ModLoader.LoadSupplyRaidAssets());

            //JSONExample();
            instance = this;
            //SetupGameData();

        }

        public void LoadInAssets()
        {
            SR_Assets asset = SR_ModLoader.srAssets;

            //------------------------------------------------------------------
            // Menus
            //------------------------------------------------------------------

            //SR Menu - Disable and Replace
            srMenu.gameObject.SetActive(false);
            srMenu = Instantiate(asset.srMenu, srMenu.position, srMenu.rotation, srMenu.parent).transform;
            //SR_Menu.instance = srMenu.GetComponent<SR_Menu>();

            //SR Buy Menu
            buyMenu.gameObject.SetActive(false);
            buyMenu = Instantiate(asset.srBuyMenu, buyMenu.position, buyMenu.rotation, buyMenu.parent).transform;

            //SR Ammo Spawner/Station
            ammoStation.gameObject.SetActive(false);
            ammoStation = Instantiate(asset.srAmmoSpawner, ammoStation.position, ammoStation.rotation, ammoStation.parent).transform;

            //SR Magzine Duplicator
            duplicator.gameObject.SetActive(false);
            duplicator = Instantiate(asset.srMagazineDuplicator, duplicator.position, duplicator.rotation, duplicator.parent).transform;

            //SR Mod Table
            attachmentStation.gameObject.SetActive(false);
            attachmentStation = Instantiate(asset.srModTable, attachmentStation.position, attachmentStation.rotation, attachmentStation.parent).transform;

            //SR Recycler
            recycler.gameObject.SetActive(false);
            recycler = Instantiate(asset.srRecycler, recycler.position, recycler.rotation, recycler.parent).transform;

            //SR Results Menu
            resultsMenu.gameObject.SetActive(false);
            resultsMenu = Instantiate(asset.srResultsMenu, resultsMenu.position, resultsMenu.rotation, resultsMenu.parent).transform;

            //SR Spawn Menu
            spawnMenu.gameObject.SetActive(false);
            spawnMenu = Instantiate(asset.srSpawnMenu, spawnMenu.position, spawnMenu.rotation, spawnMenu.parent).transform;

            //SR Spawn Station
            spawnStation.gameObject.SetActive(false);
            spawnStation = Instantiate(asset.srSpawnStation, spawnStation.position, spawnStation.rotation, spawnStation.parent).transform;

            //------------------------------------------------------------------
            // Gameplay
            //------------------------------------------------------------------

            //SR Compass
            SR_Compass.instance.gameObject.SetActive(false);
            SR_Compass.instance = Instantiate(
                asset.srCompass, 
                SR_Compass.instance.transform.position, 
                SR_Compass.instance.transform.rotation, 
                SR_Compass.instance.transform.parent).GetComponent<SR_Compass>();

            /*
            //SR Capture Zone - If NOT replaced then spawn the newer one
            if (!captureZone.replaced)
            {
                captureZone.gameObject.SetActive(false);
                captureZone = Instantiate(
                    asset.srCaptureZone,
                    captureZone.transform.position,
                    captureZone.transform.rotation,
                    captureZone.transform.parent).GetComponent<SR_CaptureZone>();
            }
            */
            
            optionPlayerHealth = 5000;
        }

        /*
        void ExportJSONs()
        {

            for (int i = 0; i < characters.Count; i++)
            {
                characters[i].ExportJson();
            }

            
            SR_CharacterPreset car = new SR_CharacterPreset();
            car.purchaseCategories = new SR_PurchaseCategory[1];
            car.purchaseCategories[0] = new SR_PurchaseCategory();
            car.purchaseCategories[0].cost = 2;
            car.purchaseCategories[0].name = "Cool";
            car.purchaseCategories[0].itemCategoryName = "item";
            car.ExportJson();

            SR_SosigFaction fac = new SR_SosigFaction();
            fac.levels = new FactionLevel[1];
            fac.levels[0]  = new FactionLevel();
            fac.levels[0].name = "Level 0";
            fac.levels[0].guardPool = new SosigEnemyID[4];
            fac.levels[0].guardPool[0] = SosigEnemyID.M_Popsicles_Guard;
            fac.levels[0].guardPool[1] = SosigEnemyID.M_Popsicles_Scout;
            fac.levels[0].guardPool[2] = SosigEnemyID.M_Popsicles_Sniper;
            fac.levels[0].guardPool[3] = SosigEnemyID.M_Popsicles_Heavy;

            //SosigEnemyID.GetNames(typeof(SosigEnemyID)).Length;
            fac.ExportJson();
        }
        */

        public void SetupGameData()
        {
            itemCategories = SR_ModLoader.LoadItemCategories();
            factions = SR_ModLoader.LoadFactions();
            characters = SR_ModLoader.LoadCharacters();

            Debug.Log("Supply Raid - Items Count:" + itemCategories.Count + " Factions Count: " + factions.Count +  " Characters Count: " + characters.Count);

            //Characters
            for (int i = 0; i < characters.Count; i++)
            {
                if (characters[i] != null)
                    characters[i].SetupCharacterPreset(itemCategories);
            }

            SR_Menu.instance.Setup();
        }

        void Start()
        {
            resultsMenu.gameObject.SetActive(false);
            SetupAttachmentLootTable();

            enviromentLayer = LayerMask.NameToLayer("Enviroment");

            //TODO set active to true for when we have instructions
            SR_HelpMenu.instance.SetActive(false);

            //Random the Random
            Random.InitState((int)Time.realtimeSinceStartup);

            if (SupplyRaidPlugin.bgmEnabled)
            {
                Invoke("LoadBGM", 0.25f);
            }
        }

        void LoadBGM()
        {
            //BGM
            if (SupplyRaidPlugin.bgmEnabled)
            {
                BGM.SpawnPanel(spawnMenu.transform.position + Vector3.up, spawnMenu.transform.rotation);
                BGM.InitializeSoundtrackInterface();
            }
        }

        void OnDisable()
        {
            //Events
            GM.CurrentSceneSettings.SosigKillEvent -= CurrentSceneSettingsOnSosigKillEvent;
            GM.CurrentSceneSettings.PlayerDeathEvent -= PlayerDeathEvent;
        }

        // Update is called once per frame
        void Update()
        {
            if (gameRunning)
            {
                if (captureProtection > 0)
                    captureProtection -= Time.deltaTime;

                CaptureHotkey();

                stats.GameTime += Time.deltaTime;
                UpdateRabbithole();
                UpdateSquadSpawner();
                UpdateSosigs();
            }
        }

        public void LaunchGame()
        {
            if (character == null || faction == null)
            {
                Debug.LogError("Supply Raid: Missing Character or Faction");
                PlayErrorSFX();
                return;
            }

            //Client Setup and Check
            isClient = Networking.IsClient();

            //Events
            GM.CurrentSceneSettings.PlayerDeathEvent += PlayerDeathEvent;
            GM.CurrentSceneSettings.SosigKillEvent += CurrentSceneSettingsOnSosigKillEvent;

            //Item locking
            GM.CurrentSceneSettings.IsSpawnLockingEnabled = optionSpawnLocking;

            //Health
            GM.CurrentPlayerBody.SetHealthThreshold(optionPlayerHealth);

            //Capture Zones
            if (optionCaptureZone)
                captureZone.gameObject.SetActive(optionCaptureZone);

            //Item Spawner
            itemSpawner.gameObject.SetActive(optionItemSpawner);

            //Free Buy
            if (optionFreeBuyMenu)
                Points = 99999;
            //Game Started
            else if (forceStartPointsOverride >= 0)
                Points = forceStartPointsOverride;
            //Starting Points
            else if (optionStartLevel > 0)
            {
                CatchupPoints(optionStartLevel);
            }
            else //Default level 0 Character Points
            {
                Points = character.pointsLevel[0];
            }

            //Spawn Gear
            if (character.StartGearLength() > 0)
                spawnStation.gameObject.SetActive(true);

            //Show Game Panels
            SetGamePanels(true);
            SR_HelpMenu.instance.SetActive(false);

            //Setup Ammo Type Prices
            SR_AmmoSpawner.instance.Setup();

            //Setup Attachment Prices
            SR_ModTable.instance.Setup();

            //Set Starting Supply ID as Host
            SetupSupplyPoints();

            //Set our next level
            CurrentCaptures = optionStartLevel;
            SetLevel_Server();

            gameRunning = true;

            //Send Network Data
            if (Networking.IsHost())
            {
                gameServerRunning = true;
                SR_Networking.instance.GameOptions_Send();
                SR_Networking.instance.ServerRunning_Send();
            }

            //Delay Rabbit Hole by 5 seconds
            rabbitHoleTimer = 5;
            squadRespawnTimer = 5;

            //Game Launched
            Debug.Log("Supply Raid: Launched Game");
            if (LaunchedEvent != null)
                LaunchedEvent.Invoke();

            //DEBUG
            //StartCoroutine(SR_Global.SpawnAllLevelSosigs());

            //BGM
            if (SupplyRaidPlugin.bgmEnabled)
            {
                BGM.SetTakeMusic(CurrentCaptures);
            }
        }

        void SetupSupplyPoints()
        {
            //Host Only
            if (isClient)
                return;

            //Error, not enough supply points
            if (supplyPoints.Count < 2)
            {
                Debug.LogError("Supply Raid: There are less than 2 supply points on the map, map will stop working.");
                return;
            }

            //Apply Forced capture Order
            if (forceCaptureOrder != -1)
                optionCaptureOrder = forceCaptureOrder;

            //-------------------------------------------------------
            //Setup Supply Order
            //-------------------------------------------------------

            if (optionCaptureOrder == 0) //RANDOM ORDER
            {
                //Random Order
                while (true)
                {
                    int id = Random.Range(0, supplyPoints.Count);
                    if (!supplyOrder.Contains(id))
                    {
                        supplyOrder.Add(id);
                        Debug.Log("Supply ID:" + id + " - index: " + (supplyOrder.Count - 1));
                    }

                    if (supplyOrder.Count >= supplyPoints.Count)
                        break;
                }
            }
            else if (optionCaptureOrder == 1) //RANDOM
            {
                playerSupplyID = Random.Range(0, supplyPoints.Count);
                attackSupplyID = Random.Range(0, supplyPoints.Count);

                //Debug.LogError("Supply Points Count: " + supplyPoints.Count);
                //Debug.LogError("Player: " + playerSupplyID + " - Attack: " + attackSupplyID);
            }
            else if (optionCaptureOrder == 2) //ORDERED
            {
                //Ordered
                for (int i = 0; i < supplyPoints.Count; i++)
                {
                    //Add all supply points in order
                    supplyOrder.Add(i);
                }
            }

            //-------------------------------------------------------
            //Forced Spawn Supply
            //-------------------------------------------------------
            if (supplyOrder.Count > 0)
            {
                //Ordered
                for (int i = 0; i < supplyOrder.Count; i++)
                {
                    if (supplyPoints[supplyOrder[i]].forceFirstSpawn)
                    {
                        supplyOrderIndex = i;

                        if (supplyOrderIndex >= supplyOrder.Count)
                            supplyOrderIndex = 0;

                        playerSupplyID = supplyOrder[supplyOrderIndex];
                        break;
                    }
                }
            }
            else
            {
                //Random
                for (int i = 0; i < supplyPoints.Count; i++)
                {
                    if (supplyPoints[i].forceFirstSpawn)
                    {
                        playerSupplyID = i;
                        break;
                    }
                }
            }

            //-------------------------------------------------------
            //FIRST ATTACK POINTS
            //-------------------------------------------------------
            if (optionCaptureOrder == 1) //RANDOM
            {
                while (playerSupplyID == attackSupplyID)
                {
                    attackSupplyID = Random.Range(0, supplyPoints.Count);
                    //Debug.LogError("Attack: " + attackSupplyID);
                }
            }
            else //ORDERED
            {
                //Change the attack Supply ID via the OrderIndex
                int playerSupply = supplyOrderIndex;
                while (playerSupply == supplyOrderIndex)
                {
                    if (supplyOrderIndex + 1 >= supplyOrder.Count)
                        supplyOrderIndex = 0;
                    else
                        supplyOrderIndex++;
                }

                //Attack position next on the supply order
                attackSupplyID = supplyOrder[supplyOrderIndex];
            }

            //STATIC SUPPLY POINT, REMOVE IT FROM THE LIST
            if (forceStaticPlayerSupplyPoint == true)
            {
                
            }
        }

        private void CurrentSceneSettingsOnSosigKillEvent(Sosig s)
        {
            //Stat Tracking before game management
            if (gameRunning && !ignoreKillStat)
            {
                stats.Kills++;
                if(SupplyRaidPlugin.h3mpEnabled)
                    SR_Networking.instance.UpdateStats_Send();
            }

            // Make sure the sosig is managed by us
            var sosig = sosigs.FirstOrDefault(x => x == s);
            if (!sosig) return;

            //Loot Drop
            if(!ignoreKillStat)
                LootDrop(s);

            if (squadSosigs.Contains(s))
                currentSquad--;
            else
                currentDefenders--;

            // Start a coroutine to respawn this sosig
            StartCoroutine(ClearSosig(sosig));
        }

        void LootDrop(Sosig s)
        {
            //Loop through each loot category
            for (int i = 0; i < character.lootCategories.Count; i++)
            {
                //Lock if not correct level
                if (character.lootCategories[i].levelUnlock > CurrentCaptures
                    || character.lootCategories[i].levelLock > -1 
                    && character.lootCategories[i].levelLock <= CurrentCaptures)
                    continue;

                //Roll to see if we meet the chance
                if (Random.Range(0.00f, 1.00f) <= character.lootCategories[i].chance)
                {
                    //Spawn Loot
                    SR_ItemCategory item = itemCategories[character.lootCategories[i].GetIndex()];
                    Transform[] spawns = new Transform[] { s.transform, s.transform, s.transform, s.transform, s.transform };
                    SR_Global.SpawnLoot(item.InitializeLootTable(), item, spawns);
                }
            }

            /*
            float lootSize = 1; //Default 100% chance of no loot

            List<float> lootLow = new List<float>();
            List<float> lootHigh = new List<float>();

            //Collect all valid Loot
            for (int i = 0; i < character.lootCategories.Count; i++)
            {
                //Loot
                lootLow.Add(lootSize);
                lootSize += character.lootCategories[i].chance;
                lootHigh.Add(lootSize);
            }

            //Get Complete Loot Range
            float lootRange = Random.Range(0f, lootSize);

            //Loop through each low and high and compare with the 
            for (int i = 0; i < lootLow.Count; i++)
            {
                if (lootRange >= lootLow[i] && 
                    lootRange < lootHigh[i])
                {
                    //Spawn Loot
                    SR_ItemCategory item = itemCategories[character.lootCategories[i].GetIndex()];
                    Transform[] spawns = new Transform[] { s.transform, s.transform, s.transform, s.transform, s.transform };
                    SR_Global.SpawnLoot(item.InitializeLootTable(), item, spawns);
                    return;
                }
            }
            */
        }

        private void PlayerDeathEvent(bool killedSelf)
        {
            if (gameRunning)
            {
                stats.Deaths++;

                //If respawning not enabled
                if (!optionRespawn)
                {
                    stats.ObjectiveComplete = false;
                    if(ObjectiveEvent != null)
                        ObjectiveEvent.Invoke();

                    if (!isClient)
                        CompleteGame();
                }
            }
        }

        private IEnumerator ClearSosig(Sosig sosig)
        {
            // Wait for 5 seconds then splode the Sosig
            yield return new WaitForSeconds(5f);

            if (sosig != null)
                sosig.ClearSosig();

            if (sosigs.Contains(sosig))
                sosigs.Remove(sosig);

            if (defenderSosigs.Contains(sosig))
                defenderSosigs.Remove(sosig);

            if (squadSosigs.Contains(sosig))
                squadSosigs.Remove(sosig);


            if (sosigGuards.Contains(sosig))
                sosigGuards.Remove(sosig);

            if (sosigSnipers.Contains(sosig))
                sosigSnipers.Remove(sosig);

            if (sosigPatrols.Contains(sosig))
                sosigPatrols.Remove(sosig);

            if (sosigSquads.Contains(sosig))
                sosigSquads.Remove(sosig);



            // Wait a little bit after before checking Complete
            yield return new WaitForSeconds(sosigSpawnTick);

            //No left alive
            if (defenderSosigs.Count <= 0 && captureProtection <= 0)
            {
                //Level End
                if (!isClient)
                    CapturedPoint();
            }
        }

        public void CapturedPoint()
        {
            //Debug.Log("Game Check");
            if (!gameRunning || isClient)
                return;

            //Increase Captures
            Debug.Log("Supply Raid: Captured Point: " + CurrentCaptures);

            CurrentCaptures++;

            //BGM
            if (SupplyRaidPlugin.bgmEnabled)
            {
                BGM.SetTakeMusic(CurrentCaptures);
            }

            if (inEndless)
                endlessLevel++;

            //Game Complete?
            if (optionCaptures > 0 && CurrentCaptures - optionStartLevel >= optionCaptures)
            {
                stats.ObjectiveComplete = true;
                if(ObjectiveEvent != null)
                    ObjectiveEvent.Invoke();
                CompleteGame();
            }
            else //Continue To Next level
            {
                SetLevel_Server();
            }
        }

        public void CompleteGame()
        {
            if (!gameRunning)
                return;

            if(ObjectiveEvent != null)
                ObjectiveEvent.Invoke();

            //Game Complete
            resultsMenu.gameObject.SetActive(true);


            DisableGamePanels();
            if (SR_ResultsMenu.instance != null)
                SR_ResultsMenu.instance.UpdateResults();
            else
                Debug.Log("Missing Results");

            //TODO play game complete sound
            PlayCompleteSFX();

            //game is not longer running
            gameRunning = false;
            gameServerRunning = false;

            //Clear all enemies from the world
            ClearSosigs();

            //Teleport player to Spawn
            GM.CurrentMovementManager.TeleportToPoint(spawnPoint.position, true, spawnPoint.rotation.eulerAngles);

            //Convert All Coal on player to Money
            for (int i = 0; i < GM.CurrentPlayerBody.QBSlots_Internal.Count; i++)
            {
                FVRPhysicalObject obj = GM.CurrentPlayerBody.QBSlots_Internal[i].CurObject;

                if (obj != null && obj.gameObject.name == "CharcoalBriquette(Clone)")
                {
                    FVRObject mainObject = null;
                    IM.OD.TryGetValue("Cartridge69CashMoneyD1", out mainObject);

                    if (mainObject == null)
                        continue;

                    //Create Money
                    GameObject moneyObj = Instantiate(mainObject.GetGameObject(), obj.transform.position, obj.transform.rotation);

                    //Destroy Coal
                    Destroy(GM.CurrentPlayerBody.QBSlots_Internal[i].CurObject.gameObject);

                    //Attach money
                    moneyObj.GetComponent<FVRPhysicalObject>().SetQuickBeltSlot(GM.CurrentPlayerBody.QBSlots_Internal[i]);
                }
            }


            //Tell Clients game is over!
            if (SupplyRaidPlugin.h3mpEnabled && Networking.IsHost())
            {
                gameCompleted = true;
                SR_Networking.instance.UpdateStats_Send();
                SR_Networking.instance.ServerRunning_Send();
                SR_Networking.instance.LevelUpdate_Send(gameCompleted);
            }

            if(GameCompleteEvent != null)
                GameCompleteEvent.Invoke();

            Debug.Log("Supply Raid: Game Complete");
        }

        void MovePanelsToLastSupply()
        {
            buyMenu.SetPositionAndRotation(LastSupplyPoint().buyMenu.position, LastSupplyPoint().buyMenu.rotation);

            ammoStation.SetPositionAndRotation(LastSupplyPoint().ammoStation.position, LastSupplyPoint().ammoStation.rotation);

            recycler.SetPositionAndRotation(LastSupplyPoint().recycler.position, LastSupplyPoint().recycler.rotation);

            duplicator.SetPositionAndRotation(LastSupplyPoint().duplicator.position, LastSupplyPoint().duplicator.rotation);

            attachmentStation.SetPositionAndRotation(LastSupplyPoint().attachmentStation.position, LastSupplyPoint().attachmentStation.rotation);
            
        }

        /// <summary>
        /// Sets all gameplay buy menu panels to state and srMenu to the inverse state
        /// </summary>
        /// <param name="state"></param>
        void SetGamePanels(bool state)
        {
            if (character.disableBuyMenu)
                buyMenu.gameObject.SetActive(false);
            else
                buyMenu.gameObject.SetActive(state);

            if (character.disableAmmoTable)
                ammoStation.gameObject.SetActive(false);
            else
                ammoStation.gameObject.SetActive(state);

            if(character.disableRecycler)
                recycler.gameObject.SetActive(false);
            else
                recycler.gameObject.SetActive(state);

            if (character.disableDuplicator)
                duplicator.gameObject.SetActive(false);
            else
                duplicator.gameObject.SetActive(state);

            if (character.disableModtable)
                attachmentStation.gameObject.SetActive(false);
            else
                attachmentStation.gameObject.SetActive(state);

            spawnMenu.gameObject.SetActive(state);
            srMenu.gameObject.SetActive(!state);

        }

        void DisableGamePanels()
        {
            buyMenu.gameObject.SetActive(false);
            ammoStation.gameObject.SetActive(false);
            recycler.gameObject.SetActive(false);
            duplicator.gameObject.SetActive(false);
            spawnMenu.gameObject.SetActive(false);
            attachmentStation.gameObject.SetActive(false);
            srMenu.gameObject.SetActive(false);
        }

        //----------------------------------------------------------------------
        // Level Setup
        //----------------------------------------------------------------------

        public static int GetPointLevel()
        {
            int level = instance.CurrentCaptures;

            if (level >= instance.character.pointsLevel.Length)
                level = instance.character.pointsLevel.Length - 1;

            return instance.character.pointsLevel[level];
        }

        void CatchupPoints(int level)
        {
            if (!instance.character.pointsCatchup)
                return;

            int newPoints = 0;

            if (level >= character.pointsLevel.Length)
                level = character.pointsLevel.Length - 1;

            for (int i = 0; i < level; i++)
                newPoints += character.pointsLevel[i];

            Points = newPoints;
        }

        public void SetLevel_Client(int totalCaptures, int attackSupply, int playerSupply, bool isEndless)
        {
            //New Player catchup
            if (CurrentCaptures == 0 && totalCaptures >= 2)
            {
                CatchupPoints(totalCaptures);
                PlayPointsGainSFX();
                CurrentCaptures = totalCaptures;
            }

            //Captured the Point
            if (totalCaptures != CurrentCaptures)
            {
                CurrentCaptures = totalCaptures;
                GivePlayerLevelPoints();
                PlayCompleteSFX();
            }

            attackSupplyID = attackSupply;
            playerSupplyID = playerSupply;
            inEndless = isEndless;

            //Update Panels
            MovePanelsToLastSupply();

            //Update Capture Zone
            if (AttackSupplyPoint() != null)
                captureZone.MoveCaptureZone(AttackSupplyPoint().captureZone);

            if (SR_ResultsMenu.instance != null)
                SR_ResultsMenu.instance.UpdateResults();

            //Capture / Supply Point Change
            if (SupplyPointChangeEvent != null)
                SupplyPointChangeEvent.Invoke();
        }

        void GivePlayerLevelPoints()
        {
            if (character == null)
            {
                Debug.LogError("Supply Raid: CHARACTER IS MISSING");
                Points += 1;
            }

            Points += GetPointLevel();
        }

        public void SetLevel_Server()
        {
            if (isClient)
                return;

            //Capture Cooldown
            captureProtection = 10;

            if (gameRunning)
            {
                PlayCompleteSFX();

                //Clear All Sosigs
                ClearSosigs();

                //Debug.Log("Supply Raid: Post Clear + Points");
            }

            //Give player points
            if (gameRunning)
            {
                GivePlayerLevelPoints();
            }

            //Debug.Log("Supply Raid: Post Current level Change");

            //Set player Supply to old attack position, unless forced static
            if (!forceStaticPlayerSupplyPoint)
                playerSupplyID = attackSupplyID;

            //Error Check, only 1 or less supply points, don't do while loop
            if (supplyPoints.Count > 1)
            {
                /*
                if (AttackSupplyPoint().nextSupplyPoints.Length > 0)
                {
                    int spIndex = Random.Range(0, AttackSupplyPoint().nextSupplyPoints.Length);

                    //Loop through each supply point until it matches
                    for (int i = 0; i < supplyPoints.Count; i++)
                    {
                        if (supplyPoints[i] == AttackSupplyPoint().nextSupplyPoints[spIndex])
                        {
                            attackSupplyID = i;
                            break;
                        }
                    }

                }
                */
                if (optionCaptureOrder == 1) //Random
                {
                    while (playerSupplyID == attackSupplyID)
                    {
                        //Debug.Log("Set Level Server SP While");
                        attackSupplyID = Random.Range(0, supplyPoints.Count);
                    }
                }
                else
                {
                    //Ordered & Random Ordered

                    if (supplyOrderIndex + 1 < supplyOrder.Count())
                        supplyOrderIndex++;
                    else
                        supplyOrderIndex = 0;

                    attackSupplyID = supplyOrder[supplyOrderIndex];
                }

                //Debug.Log("Supply Raid: Post Supply Setup");

                //Update Capture Zone
                captureZone.MoveCaptureZone(AttackSupplyPoint().captureZone);

                SetupSquadSosigs(GetFactionLevel());
                StartCoroutine(SetupDefenderSosigs(GetFactionLevel()));

                //Debug.Log("Supply Raid: Post Sosig Setup");
            }


            //Update Panels Last
            MovePanelsToLastSupply();

            //AttackSupplyPoint().SetActiveSupplyPoint(true);
            //LastSupplyPoint().SetActiveSupplyPoint(false);

            //Send to Clients
            if (Networking.ServerRunning())
            {
                gameCompleted = false;
                SR_Networking.instance.LevelUpdate_Send(gameCompleted);
                SR_Networking.instance.UpdateStats_Send();
            }

            if (SupplyPointChangeEvent != null)
                SupplyPointChangeEvent.Invoke();

            Debug.Log("Supply Raid: End of Set Level");
        }

        //----------------------------------------------------------------------
        // Sosig Setup
        //----------------------------------------------------------------------

        int GetSquadIFF()
        {
            int teamIFF;
            switch (GetFactionLevel().squadTeamRandomized)
            {
                case TeamEnum.RandomTeam:
                    teamIFF = Random.Range(0, 4);
                    break;
                case TeamEnum.RandomEnemyTeam:
                    teamIFF = Random.Range(1, 4);
                    break;

                default: //int Set Team IFF
                    teamIFF = (int)GetFactionLevel().squadTeamRandomized;
                    break;
            }

            //Add Random Team
            return teamIFF;
        }

        int GetRandomSquadSize(FactionLevel currentLevel)
        {
            //Setup sizes and safty check
            int minSquad = Mathf.Clamp(currentLevel.squadSizeMin, 0, int.MaxValue);
            int maxSquad = Mathf.Clamp(currentLevel.squadSizeMax, 1, optionMaxSquadEnemies);

            if (maxSquad < minSquad)
                maxSquad = minSquad;

            return Random.Range(minSquad, maxSquad);
        }

        void SetupSquadSosigs(FactionLevel currentLevel)
        {
            //Are squads enabled for this level?
            if (currentLevel == null || currentLevel.squadCount <= 0 || currentLevel.squadPool.Count() <= 0
                || currentLevel.squadSizeMax <= 0)
                return;

            //Group Setup
            squadGroups.Clear();

            //Team
            squadIFFs.Clear();

            if (currentLevel.infiniteSquadEnemies)
            {
                //Create only one group
                int groupSize = GetRandomSquadSize(currentLevel);
                squadGroups.Add(groupSize);

                //Add Random Team
                squadIFFs.Add(GetSquadIFF());
            }
            else
            {
                for (int i = 0; i < currentLevel.squadCount; i++)
                {
                    int groupSize = GetRandomSquadSize(currentLevel);

                    if (groupSize != 0)
                    {
                        squadGroups.Add(groupSize);
                        squadIFFs.Add(GetSquadIFF());
                    }
                }
            }
        }

        private IEnumerator SpawnSquadSosigs(int groupSize, int iff, SR_SupplyPoint spawnPoint, FactionLevel currentLevel)
        {
            //We're spawning Sosigs
            spawningSquad = true;

            if (spawnPoint == null || currentLevel == null)
            {
                Debug.LogError("Supply Raid - Missing spawnPoint or currentLevel");
                yield break;
            }

            //Squad Move To Supply Point
            Transform target = null;

            if (currentLevel.squadBehaviour == SquadBehaviour.CaptureSupplyPoint)
                target = AttackSupplyPoint().squadPoint;
            else if (currentLevel.squadBehaviour == SquadBehaviour.HuntPlayer)
            {
                spawnPoint = AttackSupplyPoint();

                if (SupplyRaidPlugin.h3mpEnabled && Networking.ServerRunning())
                {
                    //Multiplayer
                    int playerID = Random.Range(0, Networking.GetPlayerCount());

                    if(playerID == Networking.GetPlayerCount())
                        target = GM.CurrentPlayerBody.Head;
                    else
                        target = Networking.GetPlayer(playerID).head;
                }
                else
                {
                    //Single Player
                    target = GM.CurrentPlayerBody.Head;
                }
            }
            else
            {
                //Random Supply Point
                while (true)
                {
                    //Debug.Log("Spawn Squads Sosigs While");
                    target = supplyPoints[Random.Range(0, supplyPoints.Count)].squadPoint;

                    //If not Player or Defender Supplypoint
                    if (target != spawnPoint || target == null)
                    {
                        break;
                    }
                }
            }

            if (target == null)
            {
                Debug.LogError("ERROR: Cannot find Target supply point in SpawnSquadSosigs");
                yield break;
            }

            //Setup Team
            _spawnOptions.IFF = iff;

            //Slight delay to allow other spawns
            yield return new WaitForSeconds(sosigSpawnTick);
            for (int i = 0; i < groupSize; i++)
            {
                if (!gameRunning)
                    yield break;

                SpawnSquadSosig(spawnPoint, target, currentLevel);
                yield return new WaitForSeconds(sosigSpawnTick);
            }

            //Remove spawned group
            squadGroups.RemoveAt(0);
            squadIFFs.RemoveAt(0);

            //Generate new squad for infinite
            if (GetFactionLevel().infiniteSquadEnemies)
            {
                squadGroups.Add(GetRandomSquadSize(currentLevel));
                squadIFFs.Add(GetSquadIFF());
            }
            //Finished Spawning
            spawningSquad = false;
        }

        private IEnumerator SetupDefenderSosigs(FactionLevel currentLevel)
        {
            int teamID = (int)faction.teamID;
            if (teamID == -1) //Random
                teamID = Random.Range(0,4);
            else if(teamID == -2) //Random Enemy
                teamID = Random.Range(1, 4);

            //Assign Team (Random)
            _spawnOptions.IFF = teamID;

            //Enemy Level Count
            int enemyCount = Mathf.CeilToInt(currentLevel.enemiesTotal * optionPlayerCount);

            //Sniper Setup
            if (currentLevel.sniperCount > 0 && currentLevel.sniperPool.Count() > 0)
            {
                //List<Transform> usedSpots = new List<Transform>();

                int sniperCount = Mathf.CeilToInt(currentLevel.sniperCount * optionPlayerCount);
                for (int i = 0; i < sniperCount; i++)
                {
                    Transform spot = AttackSupplyPoint().GetRandomSniperSpawn();
                    SpawnSniperSosig(spot, spot.position, spot.rotation, currentLevel);
                    yield return new WaitForSeconds(sosigSpawnTick);

                    /*
                    //While we haven't filled all the spots
                    while (true)
                    {
                        Debug.Log("Defender While");
                        Transform spot = AttackSupplyPoint().GetRandomSniperSpawn();

                        if ((usedSpots.Count >= AttackSupplyPoint().sniperPoints.Count))
                        {
                            usedSpots.Clear();
                            yield return new WaitForSeconds(sosigSpawnTick);
                        }

                        //Already in use, continue
                        if (usedSpots.Contains(spot))
                            continue;
                        else if (spot != null)
                        {
                            enemyCount--;
                            usedSpots.Add(spot);
                            SpawnSniperSosig(spot.position, spot.rotation, currentLevel);

                            yield return new WaitForSeconds(sosigSpawnTick);
                            break;
                        }
                    }
                    */
                }
            }

            //Boss Setup
            if (currentLevel.bossCount > 0 && currentLevel.bossPool.Count() > 0)
            {
                yield return new WaitForSeconds(sosigSpawnTick);

                int bossCount = Mathf.CeilToInt(currentLevel.bossCount);

                for (int i = 0; i < bossCount; i++)
                {
                    Transform spot = AttackSupplyPoint().GetBossSpawn();
                    SpawnGuardSosig(spot.position, spot.rotation, currentLevel);
                    yield return new WaitForSeconds(sosigSpawnTick);
                }
            }

            //Guard Setup
            if (currentLevel.guardCount > 0 && currentLevel.guardPool.Count() > 0)
            {
                yield return new WaitForSeconds(sosigSpawnTick);
                //List<Transform> usedSpots = new List<Transform>();

                int guardCount = Mathf.CeilToInt(currentLevel.guardCount * optionPlayerCount);
                for (int i = 0; i < guardCount; i++)
                {
                    Transform spot = AttackSupplyPoint().GetRandomGuardSpawn();
                    SpawnGuardSosig(spot.position, spot.rotation, currentLevel);
                    yield return new WaitForSeconds(sosigSpawnTick);


                    /*
                    //While we haven't filled all the spots
                    while (true)
                    {
                        Debug.Log("Guard While");
                        Debug.Log("spotCount = " + usedSpots.Count);
                        Transform spot = AttackSupplyPoint().GetRandomGuardSpawn();

                        if (usedSpots.Count >= AttackSupplyPoint().guardPoints.Count)
                        {
                            Debug.Log("Guard While Clear");
                            usedSpots.Clear();
                            yield return new WaitForSeconds(1f);
                        }

                        //Already in use, continue
                        if (usedSpots.Contains(spot))
                        {
                            Debug.Log("Guard While Continue");
                            continue;
                        }
                        else if(spot != null)
                        {
                            Debug.Log("Guard While Enemy COunt");
                            enemyCount--;
                            usedSpots.Add(spot);
                            SpawnGuardSosig(spot.position, spot.rotation, currentLevel);
                            yield return new WaitForSeconds(1f);
                            break;
                        }
                    }
                    */
                }
            }

            yield return new WaitForSeconds(sosigSpawnTick);

            if (currentLevel.patrolPool.Count() <= 0)
                yield break;

            //Patrol Setup
            int minGroupSize = currentLevel.minPatrolSize;

            List<int> groups = new List<int>();
            bool filling = true;
            while (filling)
            {
                int newGroup;
                if (enemyCount > minGroupSize)
                {
                    newGroup = minGroupSize + Random.Range(0, Mathf.CeilToInt(enemyCount / 3));
                    enemyCount -= newGroup;
                }
                else
                {
                    //Remainding Enemies
                    newGroup = enemyCount;
                    enemyCount = 0;
                }

                groups.Add(newGroup);
                if (enemyCount <= 0)
                    filling = false;
            }

            int enemiesTotal = 0;

            for (int i = 0; i < groups.Count; i++)
            {
                enemiesTotal += groups[i];
            }

            bool sharePaths = false;
            //Make sure we have enough paths
            if (AttackSupplyPoint().patrolPaths.Length < groups.Count)
                sharePaths = true;

            List<int> usedPaths = new List<int>();

            //For each Patrol Path, create even amount of sosigs
            for (int y = 0; y < groups.Count; y++)
            {
                int pathID = 0;
                PatrolPath pp = null;
                if (sharePaths)
                {
                    pathID = Random.Range(0, AttackSupplyPoint().patrolPaths.Length);
                    pp = AttackSupplyPoint().patrolPaths[pathID];
                }
                else
                {
                    while (pp == null)
                    {
                        Debug.Log("PP While");
                        pathID = Random.Range(0, AttackSupplyPoint().patrolPaths.Length);

                        if (!usedPaths.Contains(pathID))
                        {
                            usedPaths.Add(pathID);
                            pp = AttackSupplyPoint().patrolPaths[pathID];
                        }
                    }
                }

                int patrolPoint = pp.GetRandomPoint();

                _spawnOptions.SosigTargetPosition = AttackSupplyPoint().patrolPaths[pathID].patrolPoints[patrolPoint].position;

                // Spawn Sosig group in Square
                int count = groups[y];
                int d = Mathf.FloorToInt(Mathf.Sqrt(count));    //Single Dimension of Units (x * x) etc
                int extra = 0;  //Remaining Units
                if (d * d < count)
                    extra = count - (d * d);

                Vector3 newPos;
                for (int z = 0, i = 0; z < d + extra; z++)
                {
                    for (int x = 0; x < d; x++, i++)
                    {
                        if (i < count)
                        {
                            if (forcePatrolInitialSpawnOnRabbitHoles)
                                newPos = AttackSupplyPoint().GetRandomSosigSpawn().position;
                            else
                                newPos = AttackSupplyPoint().patrolPaths[pathID].patrolPoints[patrolPoint].position
                                    + GetSquaredPosition(x, z, d);

                            SpawnPatrolSosig(
                                newPos,
                                AttackSupplyPoint().patrolPaths[pathID].patrolPoints[patrolPoint].rotation,
                                pp,
                                currentLevel);

                            enemiesTotal--;

                            yield return new WaitForSeconds(sosigSpawnTick);

                            //Stop if nothing is running or we hit the enemy cap
                            if (!gameRunning || currentDefenders >= optionMaxEnemies)
                            {
                                remainDefenders = enemiesTotal;
                                //Debug.Log("Remaining Enemies: " + remainEnemies);

                                yield break;
                            }
                        }
                    }
                }
            }
        }

        void UpdateSosigs()
        {
            //Only update if data has changed
            if (sosigSightMultiplier == sosigSightMultiplierLast)
                return;
            else
                sosigSightMultiplierLast = sosigSightMultiplier;

            //Guards
            for (int i = 0; i < sosigGuards.Count; i++)
            {
                sosigGuards[i].StateSightRangeMults = sosigGuard.stateSightRangeMults * sosigSightMultiplier;
            }

            //Snipers
            for (int i = 0; i < sosigSnipers.Count; i++)
            {
                sosigSnipers[i].StateSightRangeMults = sosigSniper.stateSightRangeMults * sosigSightMultiplier;
            }

            //Patrol
            for (int i = 0; i < sosigPatrols.Count; i++)
            {
                sosigPatrols[i].StateSightRangeMults = sosigPatrol.stateSightRangeMults * sosigSightMultiplier;
            }

            //Squads
            for (int i = 0; i < sosigSquads.Count; i++)
            {
                sosigSquads[i].StateSightRangeMults = sosigSquad.stateSightRangeMults * sosigSightMultiplier;
            }
        }

        void UpdateRabbithole()
        {
            if (GetFactionLevel() == null || isClient)
                return;

            bool infiniteEnemies = GetFactionLevel().infiniteEnemies;

            if (infiniteEnemies && optionCaptureZone == false)
                infiniteEnemies = false;

            //No more spawning enemies
            if (!infiniteEnemies && remainDefenders <= 0)
                return;

            //Do once a second check
            if ((rabbitHoleTimer -= Time.deltaTime) <= 0)
            {
                //Check once a second
                rabbitHoleTimer = GetFactionLevel().enemySpawnTimer;

                int maxEnemies = optionMaxEnemies;  //Max On Screen Enemies

                //Infinite Enemies limitations
                if (infiniteEnemies && GetFactionLevel().enemiesTotal < optionMaxEnemies)
                    maxEnemies = GetFactionLevel().enemiesTotal; //Max Extra Enemies in infinite mode

                //If a enemy slot has freed up
                if (currentDefenders < maxEnemies)
                {
                    if(!infiniteEnemies)
                        remainDefenders--;

                    SpawnRabbitholeSosig(GetFactionLevel());
                }
            }
        }

        void UpdateSquadSpawner()
        {
            if (isClient || GetFactionLevel() == null || GetFactionLevel().squadCount <= 0 || squadGroups.Count <= 0 || spawningSquad)
                return;

            if (squadIFFs.Count == 0)
            {
                Debug.LogError("Supply Raid - no IFF been set");
                return;
            }

            //Infinite Enemies
            bool infiniteSquad = GetFactionLevel().infiniteSquadEnemies;

            //Do once a second check
            if ((squadRespawnTimer -= Time.deltaTime) <= 0)
            {
                squadRespawnTimer = GetFactionLevel().enemySpawnTimer;

                //Enough room to spawn more - Max Onscreen - Current alive = remaining
                if (squadGroups[0] > optionMaxSquadEnemies - currentSquad)
                {
                    //Not enough space to spawn next group
                    return;
                }

                //Make sure group is not empty
                if (!infiniteSquad && squadGroups[0] <= 0)
                {
                    squadGroups.RemoveAt(0);
                    squadIFFs.RemoveAt(0);
                    return;
                }

                //Spawn Location
                SR_SupplyPoint spawnPoint = null;
                List<SR_SupplyPoint> collectedSP = new List<SR_SupplyPoint>();

                //Collect possible spawn points
                for (int i = 0; i < supplyPoints.Count; i++)
                {
                    //Supply Point matches players supply
                    if (supplyPoints[i] == LastSupplyPoint())
                    {
                        //Allow if Friendly
                        if (squadIFFs[0] == (int)TeamEnum.Ally0)
                        {
                            collectedSP.Add(supplyPoints[i]);
                        }

                    }
                    else if (supplyPoints[i] == AttackSupplyPoint())
                    {
                        //Allow if Enemy
                        if (squadIFFs[0] != (int)TeamEnum.Ally0)
                        {
                            collectedSP.Add(supplyPoints[i]);
                        }
                    }
                    else
                    {
                        //Add Supply Point
                        collectedSP.Add(supplyPoints[i]);
                    }
                }

                spawnPoint = collectedSP[Random.Range(0, collectedSP.Count)];

                //Error Check
                if (spawnPoint == null)
                    return;

                StartCoroutine(SpawnSquadSosigs(squadGroups[0], squadIFFs[0], spawnPoint, GetFactionLevel()));
            }
        }

        void SpawnRabbitholeSosig(FactionLevel currentLevel)
        {
            int pathID = Random.Range(0, AttackSupplyPoint().patrolPaths.Length);
            PatrolPath pp = AttackSupplyPoint().patrolPaths[pathID];
            Transform sosigSpawn = null; // AttackSupplyPoint().GetRandomSosigSpawn();

            if (Networking.ServerRunning())
            {
                //Check all players
                sosigSpawn = AttackSupplyPoint().GetRandomSosigSpawn();
            }
            else
            {
                //Default to only spawn point if there isn't any extra
                if (AttackSupplyPoint().sosigSpawns.Length <= 1)
                    sosigSpawn = AttackSupplyPoint().sosigSpawns[0];
                else
                {
                    //Single Player
                    bool[] spawnList = new bool[AttackSupplyPoint().sosigSpawns.Length];

                    while (sosigSpawn == null)
                    {
                        int index = Random.Range(0, AttackSupplyPoint().sosigSpawns.Length);
                        spawnList[index] = true;

                        Transform spawnPoint = AttackSupplyPoint().sosigSpawns[index];

                        Vector3 headPosition = GM.CurrentPlayerBody.Head.position + (GM.CurrentPlayerBody.Head.forward * 0.5f);
                        Vector3 spawnPosition = spawnPoint.position + (Vector3.up * 1.7f);

                        //Make sure the player isn't within range
                        if (SR_Global.Distance2D(headPosition, spawnPosition) >= AttackSupplyPoint().playerNearby)
                        {
                            //If Line of Sight is blocked
                            if (Physics.Linecast(spawnPosition, headPosition, out RaycastHit hit ,enviromentLayer))
                            {
                                //Spawn is safe to use
                                sosigSpawn = spawnPoint;
                                break;
                            }
                        }

                        bool spawnLeft = false;
                        //Loop through all spawn points
                        for (int i = 0; i < spawnList.Length; i++)
                        {
                            if (spawnList[i] == false)
                            {
                                //Spawn point still left
                                spawnLeft = true;
                                break;
                            }
                        }

                        //No spawns left, pick one at random
                        if (spawnLeft == false)
                        {
                            sosigSpawn = AttackSupplyPoint().GetRandomSosigSpawn();
                        }
                    }
                }
            }

            if (sosigSpawn == null)
            {
                Debug.LogError("Supply Raid - Missing Rabbithole spawn position " + pathID);
                return;
            }

            SpawnPatrolSosig(sosigSpawn.position, sosigSpawn.rotation,
                                pp,
                                currentLevel);
            Debug.Log("Supply Raid - Rabbithole Sosig " + sosigSpawn.position);
        }

        Vector3 GetSquaredPosition(int x, int z, int d)
        {
            return new Vector3(x * 1 - ((d / 2) * 1), 0, -z * 1);
        }

        void SpawnGuardSosig(Vector3 position, Quaternion rotation, FactionLevel currentLevel)
        {
            //Error Check
            if (currentLevel == null)
            {
                Debug.Log("Supply Raid - Missing Guard Spawn Data");
                return;
            }
            float size = AttackSupplyPoint().spawnRadius / 2;
            //Todo make this better?
            position.x += Random.Range(-size, size);
            position.z += Random.Range(-size, size);

            Sosig sosig = CreateSosig(_spawnOptions, position, rotation, currentLevel.guardPool, currentLevel.name);

            if (sosig == null)
            {
                Debug.LogError("Supply Raid - No Guard Sosig to Spawn");
                return;
            }


            sosig.m_pathToPoint = position;
            sosig.SetCurrentOrder(Sosig.SosigOrder.GuardPoint);
            sosig.SetMovementSpeed(Sosig.SosigMoveSpeed.Still);
            sosig.MoveSpeed = Sosig.SosigMoveSpeed.Still;
            sosig.SetAssaultSpeed(Sosig.SosigMoveSpeed.Crawling);
            sosig.SetGuardInvestigateDistanceThreshold(10);
            sosig.CoverSearchRange = 10;
            sosig.m_guardPoint = position;
            sosig.m_guardDominantDirection = rotation.eulerAngles;

            //Default Sosig Values
            sosigGuard.AssignToSosig(sosig);

            sosigs.Add(sosig);
            currentDefenders++;
            defenderSosigs.Add(sosig);
            sosigGuards.Add(sosig);
        }

        void SpawnSniperSosig(Transform spot, Vector3 position, Quaternion rotation, FactionLevel currentLevel)
        {
            //Error Check
            if (currentLevel == null)
            {
                Debug.Log("Supply Raid - Missing Sniper Spawn Data");
                return;
            }

            //Random placement via transform
            float xScale = spot.localScale.x / 2;
            float zScale = spot.localScale.z / 2;

            position.x += Random.Range(-xScale, xScale);
            position.z += Random.Range(-zScale, zScale);

            Sosig sosig = CreateSosig(_spawnOptions, position, rotation, currentLevel.sniperPool, currentLevel.name);

            if (sosig == null)
            {
                Debug.LogError("Supply Raid - No Sniper Sosig to Spawn");
                return;
            }

            sosig.Speed_Walk = 0.01f;
            sosig.Speed_Run = 0.01f;
            sosig.Speed_Crawl = 0.01f;
            sosig.Speed_Sneak = 0.01f;

            sosig.m_pathToPoint = position;
            sosig.SetCurrentOrder(Sosig.SosigOrder.StaticShootAt);
            sosig.SetMovementSpeed(Sosig.SosigMoveSpeed.Still);
            sosig.MoveSpeed = Sosig.SosigMoveSpeed.Still;
            sosig.SetAssaultSpeed(Sosig.SosigMoveSpeed.Still);
            sosig.SetGuardInvestigateDistanceThreshold(0);
            sosig.m_guardPoint = position;
            sosig.m_guardDominantDirection = rotation.eulerAngles;

            //Default Sosig Values
            sosigSniper.AssignToSosig(sosig);

            sosigs.Add(sosig);
            //currentDefenders++;
            //defenderSosigs.Add(sosig);
            sosigSnipers.Add(sosig);
        }

        void SpawnPatrolSosig(Vector3 position, Quaternion rotation, PatrolPath pp, FactionLevel currentLevel)
        {
            //Error Check
            if (pp == null || currentLevel == null)
            {
                Debug.Log("Supply Raid - Missing Patrol Spawn Data");
                return;
            }

            Sosig sosig = CreateSosig(_spawnOptions, position, rotation, currentLevel.patrolPool, currentLevel.name);

            if (sosig == null)
            {
                Debug.LogError("Supply Raid - No Patrol Sosig to Spawn");
                return;
            }

            List<Vector3> pathPoints = pp.GetPathPositionList();
            List<Vector3> pathDirs = pp.GetPathRotationList();

            sosig.CommandPathTo(
                pathPoints,
                pathDirs,
                1,
                Vector2.one * 4,
                2f,
                Sosig.SosigMoveSpeed.Walking,
                Sosig.PathLoopType.LoopEndless,
                null,
                0.2f,
                1f,
                true,
                50f);

            //Default Sosig Values
            sosigPatrol.AssignToSosig(sosig);

            sosigs.Add(sosig);
            currentDefenders++;
            defenderSosigs.Add(sosig);
            sosigPatrols.Add(sosig);
        }

        void SpawnSquadSosig(SR_SupplyPoint spawnSupply, Transform target, FactionLevel currentLevel)
        {
            //Error Check
            if (spawnSupply == null || spawnSupply.squadPoint == null || currentLevel == null)
            {
                Debug.Log("Supply Raid - Missing Squad Spawn Data");
                return;
            }

            //Place sosig on the squad point
            Transform sosigSpawn = spawnSupply.GetRandomSosigSpawn();

            Sosig sosig = CreateSosig(_spawnOptions, sosigSpawn.position, sosigSpawn.rotation, currentLevel.squadPool, currentLevel.name);

            if (sosig == null)
            {
                Debug.LogError("Supply Raid - No Squad Sosig to Spawn");
                return;
            }

            currentSquad++;

            //Randomize in the sosig Squad Waypoint scale
            sosig.m_pathToPoint = target.position + new Vector3(
                Random.Range(-target.lossyScale.x, target.lossyScale.x) / 2,
                0,
                Random.Range(-target.lossyScale.z, target.lossyScale.z) / 2);

            List<Vector3> pathPoints = new List<Vector3>
            {
                sosig.m_pathToPoint,    //Target Point
                sosigSpawn.position,    //Spawn Point
            };

            List<Vector3> pathDirs = new List<Vector3>
            {
                target.rotation.eulerAngles,
                sosigSpawn.rotation.eulerAngles,
            };

            /*
            if (currentLevel.squadBehaviour == SquadBehaviour.RandomSupplyPoint)
            {
                pathPoints.Add(target.squadPoint.position);
                pathDirs.Add(target.squadPoint.rotation.eulerAngles);
            }
            */

            sosig.CommandPathTo(
                pathPoints,
                pathDirs,
                1,
                Vector2.one * 4,
                2f,
                Sosig.SosigMoveSpeed.Walking,
                Sosig.PathLoopType.LoopEndless,
                null,
                0.2f,
                1f,
                true,
                75f);

            //Default Sosig Values
            sosigSquad.AssignToSosig(sosig);

            sosigs.Add(sosig);

            //Squad Tracking
            squadSosigs.Add(sosig);
            sosigSquads.Add(sosig);
        }

        public Sosig CreateSosig(SosigAPI.SpawnOptions spawnOptions, Vector3 position, Quaternion rotation, SosigPool pool, string poolName)
        {
            //Debug.Log("Supply Raid - Spawning Sosig at position: " + position);

            //Get Valid Sosig ID
            if (pool.Count() <= 0)
            {
                Debug.LogError("Supply Raid - No squad enemy IDs assigned to faction level " + poolName);
                return null;
            }

            SosigEnemyID id = SR_Global.GetRandomSosigIDFromPool(pool.sosigEnemyID);

            if (id == SosigEnemyID.None)
            {
                Debug.LogError("Supply Raid - No valid squad enemy IDs in faction level " + poolName);
                return null;
            }

            //Get Valid Nav Mesh
            position = SR_Global.GetValidNavPosition(position, 25f);

            Sosig sosig =
                SosigAPI.Spawn(
                    IM.Instance.odicSosigObjsByID[id],
                    spawnOptions,
                    position,
                    rotation);

            //TODO this does nothing
            sosig.m_isBlinded = false;

            //Set Agents to quailty level
            NavMeshAgent agent = sosig.GetComponent<NavMeshAgent>();

            agent.obstacleAvoidanceType = avoidanceQuailty;
            agent.stoppingDistance = 1;

            return sosig;
        }

        void ClearSosigs()
        {
            ignoreKillStat = true;
            remainDefenders = 0;
            currentDefenders = 0;

            currentSquad = 0;

            for (int i = 0; i < sosigs.Count; i++)
            {
                if(sosigs[i] != null)
                    sosigs[i].KillSosig();
            }
            sosigs.Clear();

            ignoreKillStat = false;
        }

        //----------------------------------------------------------------------
        // Helpers
        //----------------------------------------------------------------------

        public Transform GetCompassMarker()
        {
            return AttackSupplyPoint().captureZone;
        }

        public Transform GetLastSupplyPoint()
        {
            return LastSupplyPoint().captureZone;
        }

        void SetupAttachmentLootTable()
        {
            lt_RequiredAttachments = new LootTable();

            LootTable.LootTableType type = LootTable.LootTableType.Attachments;

            List<FVRObject.OTagEra> eras = new List<FVRObject.OTagEra>
            {
                FVRObject.OTagEra.Modern,
                FVRObject.OTagEra.PostWar,
            };

            List<FVRObject.OTagFirearmMount> mounts = new List<FVRObject.OTagFirearmMount>
            {
                FVRObject.OTagFirearmMount.Picatinny,
            };

            List<FVRObject.OTagAttachmentFeature> features = new List<FVRObject.OTagAttachmentFeature>
            {
                FVRObject.OTagAttachmentFeature.IronSight,
                FVRObject.OTagAttachmentFeature.Reflex,
            };


            List<FVRObject.OTagSet> set = new List<FVRObject.OTagSet>
            {
                FVRObject.OTagSet.Real,
                FVRObject.OTagSet.TNH,
            };

            lt_RequiredAttachments.Initialize(type, eras, null, null, null, null, null, mounts, null, features, null, null, null, null, -1, -1);

            //Tag Set Removal
            for (int num = lt_RequiredAttachments.Loot.Count - 1; num >= 0; num--)
            {
                FVRObject fVRObject = lt_RequiredAttachments.Loot[num];
                if (set != null && !set.Contains(fVRObject.TagSet))
                {
                    lt_RequiredAttachments.Loot.RemoveAt(num);
                    continue;
                }
            }
        }

        //----------------------------------------------------------------------
        // Static
        //----------------------------------------------------------------------

        /// <summary>
        /// Returns Sosig Faction thats currently selected
        /// </summary>
        /// <returns></returns>
        public static SR_SosigFaction Faction()
        {
            return instance.faction;
        }

        /// <summary>
        /// Returns Character Preset thats currently selected
        /// </summary>
        /// <returns></returns>
        public static SR_CharacterPreset Character()
        {
            return instance.character;
        }

        public static FactionLevel GetFactionLevel()
        {
            int level = instance.CurrentCaptures;

            if (!instance.inEndless)
            {
                //Captures outside of levels, must be endless
                if (level >= instance.faction.levels.Length && instance.faction.endless.Length > 0)
                {
                    instance.inEndless = true;
                    if (EndlessEvent != null)
                        EndlessEvent.Invoke();
                    level = instance.endlessLevel = 0;
                }
                else if(level >= instance.faction.levels.Length) //No endless, just last level
                    level = instance.faction.levels.Length - 1;
            }
            else
            {
                if (instance.endlessLevel >= instance.faction.endless.Length)
                    instance.endlessLevel = 0;

                //Set to what endless we're at
                level = instance.endlessLevel;
            }

            if (instance.inEndless)
                return instance.faction.endless[level];
            else
                return instance.faction.levels[level];
        }

        public static int AddSupplyPoint(SR_SupplyPoint sp)
        {
            if (instance == null)
            {
                Debug.Log("There is no Supply Raid Manager loaded in this scene");
                return 0;
            }

            if (!instance.supplyPoints.Contains(sp))
                instance.supplyPoints.Add(sp);

            return instance.supplyPoints.IndexOf(sp);
        }

        public static void RemoveSupplyPoint(SR_SupplyPoint sp)
        {
            if (instance == null)
            {
                Debug.Log("There is no Supply Raid Manager loaded in this scene");
                return;
            }

            //Remove Supply Points that are in the order.
            if(instance.supplyOrder.Contains(sp.index))
                instance.supplyOrder.Remove(sp.index);

            //Remove from list
            if (!instance.supplyPoints.Contains(sp))
                instance.supplyPoints.Remove(sp);
        }

        public static bool SpendPoints(int amount)
        {
            if (instance != null)
            {
                if (instance.optionFreeBuyMenu)
                    return true;

                if (instance.Points >= amount)
                {
                    instance.Points -= amount;
                    return true;
                }
                else
                    return false;
            }
            else //No Manager, assume it works
                return true;
        }

        public static bool EnoughPoints(int amount)
        {
            if (instance == null)
                return false;

            if (instance.optionFreeBuyMenu)
                return true;

            if (instance.Points >= amount)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Returns the currently active Supply Point
        /// </summary>
        /// <returns></returns>
        public static SR_SupplyPoint AttackSupplyPoint()
        {
            if (instance != null)
                return instance.supplyPoints[instance.attackSupplyID];

            return null;
        }

        /// <summary>
        /// Returns the Previous Supply point
        /// </summary>
        /// <returns></returns>
        public static SR_SupplyPoint LastSupplyPoint()
        {
            if (instance != null)
                return instance.supplyPoints[instance.playerSupplyID];

            return null;
        }

        //----------------------------------------------------------------------
        // Networking
        //----------------------------------------------------------------------

        void CaptureHotkey()
        { 
            if ((Networking.ServerRunning() && !Networking.IsHost()) || captureProtection > 0)
                return;

            if (Input.GetKey(KeyCode.RightControl) && Input.GetKeyDown(KeyCode.C))
            {
                CapturedPoint();
            }
        }

        public void SetLocalAsClient()
        {
            isClient = true;
        }

        public void Network_GameOptions(
            float playerCount, float difficulty, bool freeBuyMenu, bool spawnLocking, int startLevel, int playerHealth,
            bool itemSpawner, bool captureZone, int order, int captures, bool respawn, int maxEnemies, int maxSquadEnemies,
            string factionID)
        {
            optionPlayerCount = playerCount;
            optionDifficulty = difficulty;
            optionFreeBuyMenu = freeBuyMenu;
            optionSpawnLocking = spawnLocking;
            optionStartLevel = startLevel;
            optionPlayerHealth = playerHealth;
            optionItemSpawner = itemSpawner;
            optionCaptureZone = captureZone;
            optionCaptureOrder = order;
            optionCaptures = captures;
            optionRespawn = respawn;
            optionMaxEnemies = maxEnemies;
            optionMaxSquadEnemies = maxSquadEnemies;

            SR_Menu.instance.SetFactionByName(factionID);
            //instance.factionID = factionID;

            //DO visual update here
            if (SR_Menu.instance != null)
                SR_Menu.instance.UpdateGameOptions();
        }

        //----------------------------------------------------------------------
        // Audio
        //----------------------------------------------------------------------
        public static void PlayConfirmSFX()
        {
            if (instance != null)
                instance.globalAudio.PlayOneShot(instance.audioConfirm);
        }

        public static void PlayFailSFX()
        {
            if (instance != null)
                instance.globalAudio.PlayOneShot(instance.audioFail);
        }

        public static void PlayErrorSFX()
        {
            if (instance != null)
                instance.globalAudio.PlayOneShot(instance.audioError);
        }
        public static void PlayCompleteSFX()
        {
            if (instance != null)
                instance.globalAudio.PlayOneShot(instance.audioCaptureComplete);
        }
        public static void PlayRearmSFX()
        {
            if (instance != null)
                instance.globalAudio.PlayOneShot(instance.audioRearm);
        }

        public static void PlayPointsGainSFX()
        {
            if (instance != null)
                instance.globalAudio.PlayOneShot(instance.audioPointsGain);
        }

        public static void PlayTickSFX()
        {
            if (instance != null)
                instance.globalAudio.PlayOneShot(instance.audioTick);
        }
        public static void PlayTickAlmostSFX()
        {
            if (instance != null)
                instance.globalAudio.PlayOneShot(instance.audioTickAlmost);
        }

        public static void PlayExtractionTickSFX()
        {
            if (instance != null && SR_Menu.instance.audioExtractionTick)
                instance.globalAudio.PlayOneShot(SR_Menu.instance.audioExtractionTick);
        }

        //----------------------------------------------------------------------
        // Classes
        //----------------------------------------------------------------------

        public class Stats
        {
            private float gameTime = 0;
            private bool objectiveComplete = false;
            private int deaths = 0;
            private int kills = 0;

            public float GameTime
            {
                set { gameTime = value; }
                get { return gameTime; }
            }

            public bool ObjectiveComplete
            {
                set { objectiveComplete = value; }
                get { return objectiveComplete; }
            }

            public int Deaths
            {
                set { deaths = value; }
                get { return deaths; }
            }

            public int Kills
            {
                set { kills = value; }
                get { return kills; }
            }

            public int GetScore()
            {
                int score = 0;
                score = Kills * 100;         //100 points per kill
                score += instance.CurrentCaptures * 2500;   //2500 points for captures (25 kills)
                score = ObjectiveComplete ? score : score / 2;  //half points for losing
                score -= Mathf.FloorToInt(gameTime);
                score /= Deaths + 1;    //divid score by deaths

                return score;
            }
        }

        public enum CaptureOrderEnum
        {
            RandomOrder = 0,    //Each capture position will be used in randomly selected order.
            Random = 1,         //Next capture position will always be random
            Ordered = 2,        //Each capture position will always be the same
        }

        [System.Serializable]
        public class SosigSettings
        {
            //Default Sosig Values
            [Tooltip("Max hearing range, Default - 300")]
            public float maxHearingRange = 300; //300
            [Tooltip("Max FOV, Default - 105")]
            public float maxFOV = 105;
            [Tooltip("Max Sight Range, Default - 250")]
            public float maxSightRange = 250; //250

            [Tooltip("Multiplier that affects sosig hearing\nX - Unaware\n Y - Can hear the player\n Z - In Combat, can hear player")]
            public Vector3 stateHearingRangeMults = new Vector3(0.6f, 1f, 1f);
            [Tooltip("Multiplier that affects sosig FOV\nX = Unaware of player, Y = Aware of player, Z = Combat/Can see player")]
            public Vector3 stateFOVMults = new Vector3(0.5f, 0.6f, 1f);
            [Tooltip("Multiplier that affects sosig Sight\nX = Unaware/At Ease, Y = Aware of player, Z = Combat/Can see player")]
            public Vector3 stateSightRangeMults = new Vector3(0.1f, 0.35f, 1f);

            public void AssignToSosig(Sosig sosig)
            {
                //Default Sosig Values
                sosig.MaxHearingRange = maxHearingRange;
                sosig.MaxFOV = maxFOV;
                sosig.MaxSightRange = maxSightRange;

                sosig.StateHearingRangeMults = stateHearingRangeMults;
                sosig.StateFOVMults = stateFOVMults;
                sosig.StateSightRangeMults = stateSightRangeMults;
            }
        }

        //----------------------------------------------------------------------
        // Editor Gizmos
        //----------------------------------------------------------------------
        void OnDrawGizmos()
        {
            //Matrix Manipulated Below        
            if (itemSpawner != null)
            {
                Gizmos.color = new Color(0.4f, 0.4f, 0.9f, 0.5f);
                Gizmos.matrix = itemSpawner.localToWorldMatrix;
                //Item Spawner
                Vector3 vector = new Vector3(0f, 0.7f, 0.25f);
                Vector3 size = new Vector3(2.3f, 1.2f, 0.5f);
                Vector3 vector2 = Vector3.forward;
                Gizmos.DrawCube(vector, size);
                Gizmos.DrawLine(vector, vector + vector2 * 0.5f);
            }
        
        }
    }
}