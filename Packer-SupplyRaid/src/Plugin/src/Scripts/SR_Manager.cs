using FistVR;
using Sodalite.Api;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using H3MP.Networking;

namespace SupplyRaid
{
    public class SR_Manager : MonoBehaviour
    {
        public static SR_Manager instance;

        /// <summary>
        /// The current character Level
        /// </summary>
        public int CurrentLevel
        {
            set { currentLevel = value; }
            get { return currentLevel; }
        }

        /// <summary>
        /// The total Captures count
        /// </summary>
        public int CapturesTotal
        {
            set { capturesTotal = value; }
            get { return capturesTotal; }
        }

        [Header("Default Settings")]
        [Tooltip("Multiplier for multiplayer or harder games"), HideInInspector]
        public int optionPlayerCount = 1;
        [Tooltip("Default 1 = Normal, 0.5 = Easier, 2 = Double enemy Stats"), Range(0.5f, 2)]
        public float optionDifficulty = 1f;
        [Tooltip("Players currency enabled or disabled")]
        public bool optionFreeBuyMenu = false;
        [Tooltip("false = Limited Ammo / true = spawn locking")]
        public bool optionSpawnLocking = true;
        [Tooltip("What level do we start on? Gain all points from those prev levels"), HideInInspector]
        public int optionStartLevel = 0;
        [Tooltip("One Hit = 0, Half = 1, Standard = 2, Extra = 3, Double = 4, Too Much HP = 5"), Range(0, 4)]
        public int optionPlayerHealth = 2;
        [Tooltip("Item Spawner")]
        public bool optionItemSpawner = false;
        [Tooltip("Is the capture Zone being used")]
        public bool optionCaptureZone = true;
        [Tooltip("Right Hand = True, Left Hand = False"), HideInInspector]
        public bool optionHand = false;
        [Tooltip("Do we do the supply points in Array Order, \n0 = Random Order\n1 = Random\n2 = Ordered")]
        public int optionCaptureOrder = 0;
        [Tooltip("How many captures until the game completes, 0 = Infinite, 1+ = Number of Captures")]
        public int optionCaptures = 5;
        [Tooltip("Can the players respawn after dying?")]
        public bool optionRespawn = true;
        [Tooltip("How many allowed to be alive at a time")]
        public int optionMaxEnemies = 12;

        [Header("Game Stats")]
        public Stats stats = new Stats();

        [Header("Game Options")]
        [Tooltip("Force the players supply point to never move, useful for a none moving central buy zone")]
        public bool forceStaticPlayerSupplyPoint = false;
        [Tooltip("Force the map to be a specific Supply Points order \n-1 - Off\n0 - Random Order\n1 - Random\n2 - Ordered"), Range(-1, 2)]
        public int forceCaptureOrder = -1;
        [Tooltip("Forced Starting Points, -1 = Use Character Points, 0,1,2...etc = Map specific starting points")]
        public int forceStartPointsOverride = -1;

        [Header("Gameplay")]
        [HideInInspector]
        public SR_CharacterPreset character;
        [HideInInspector]
        public SR_SosigFaction faction;
        [HideInInspector, Tooltip("Has the game been launched and is running")]
        public bool gameRunning = false;
        [HideInInspector]
        public float captureProtection = 0;
        private int currentLevel = 0;
        private int capturesTotal = 0;
        [HideInInspector]
        public bool gameCompleted = false;
        [HideInInspector]
        public bool inEndless = false;

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
        private int currentEnemies = 0;
        private int remainEnemies = 0;
        private float rabbitHoleTimer = 0;

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
        public List<SR_CharacterPreset> characters;
        public List<SR_SosigFaction> factions;
        public List<SR_ItemCategory> itemCategories;
        [HideInInspector] public LootTable lt_RequiredAttachments;

        [Header("Sosig Setup")]
        public SosigSettings sosigGuard = new SosigSettings();
        public SosigSettings sosigSniper = new SosigSettings();
        public SosigSettings sosigPatrol = new SosigSettings();
        public SosigSettings sosigSquad = new SosigSettings();

        public TNH_SosiggunShakeReloading shakeReloading = TNH_SosiggunShakeReloading.On;
        private readonly List<Sosig> sosigs = new List<Sosig>();
        private readonly List<Sosig> defenderSosigs = new List<Sosig>();

        private readonly SosigAPI.SpawnOptions _spawnOptions = new SosigAPI.SpawnOptions
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

        public delegate void PostCaptureDelegate();
        public static event PostCaptureDelegate PostCaptureEvent;

        public delegate void PointGainDelegate(int i);
        public static event PointGainDelegate PointEvent;

        private int points = 0;

        public int Points
        {
            set
            {
                if (optionFreeBuyMenu)
                    points = 999;
                else
                    points = Mathf.Clamp(value, 0, int.MaxValue);

                if (PointEvent != null)
                    PointEvent.Invoke(points);

                Debug.Log("Points: " + points);
            }
            get { return points; }
        }

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

        // Use this for initialization
        void Awake()
        {
            //JSONExample();
            instance = this;
            SetupGameData();
        }

        void JSONExample()
        {
            /*
            for (int i = 0; i < itemCategories.Count; i++)
            {
                itemCategories[i].ExportJson();
            }
            */


            for (int i = 0; i < characters.Count; i++)
            {
                characters[i].ExportJson();
            }

            /*
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
            */
        }

        void SetupGameData()
        {
            itemCategories = SR_ModLoader.LoadItemCategories();
            factions = SR_ModLoader.LoadFactions();
            characters = SR_ModLoader.LoadCharacters();

            Debug.Log("Supply Raid - Items Count:" + itemCategories.Count + " Factions Count: " + factions.Count +  " Characters Count: " + characters.Count);

            //Characters
            for (int i = 0; i < characters.Count; i++)
            {
                if(characters[i] != null)
                    characters[i].SetupCharacterPreset(itemCategories, factions);
            }
        }

        void Start()
        {
            resultsMenu.gameObject.SetActive(false);
            SetupAttachmentLootTable();
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

                stats.GameTime += Time.deltaTime;
                UpdateRabbithole();
            }
        }

        public void LaunchGame()
        {
            if (character == null || faction == null)
                return;

            //Client Setup and Check
            isClient = Networking.IsClient();

            //Events
            GM.CurrentSceneSettings.PlayerDeathEvent += PlayerDeathEvent;
            GM.CurrentSceneSettings.SosigKillEvent += CurrentSceneSettingsOnSosigKillEvent;

            //Item locking
            GM.CurrentSceneSettings.IsSpawnLockingEnabled = optionSpawnLocking;

            //Health
            switch (optionPlayerHealth)
            {
                case 0:
                    GM.CurrentPlayerBody.SetHealthThreshold(100f);
                    break;
                case 1:
                    GM.CurrentPlayerBody.SetHealthThreshold(2500f);
                    break;
                case 2:
                    GM.CurrentPlayerBody.SetHealthThreshold(5000f);
                    break;
                case 3:
                    GM.CurrentPlayerBody.SetHealthThreshold(7500f);
                    break;
                case 4:
                    GM.CurrentPlayerBody.SetHealthThreshold(10000f);
                    break;
                case 5:
                    GM.CurrentPlayerBody.SetHealthThreshold(500000f);
                    break;

                default:
                    GM.CurrentPlayerBody.SetHealthThreshold(5000f);
                    break;
            }

            //Capture Zones
            if (optionCaptureZone)
                captureZone.gameObject.SetActive(optionCaptureZone);

            //Item Spawner
            itemSpawner.gameObject.SetActive(optionItemSpawner);

            //Free Buy
            if (optionFreeBuyMenu)
                Points = 999;
            //Game Started
            else if (forceStartPointsOverride >= 0)
                Points = forceStartPointsOverride;
            //Starting Points
            else if (optionStartLevel > 0)
            {
                int newPoints = 0;
                int level = optionStartLevel;

                if (optionStartLevel > character.pointsLevel.Length)
                    level = character.pointsLevel.Length - 1;

                for (int i = 0; i < level; i++)
                    newPoints += character.pointsLevel[i];

                Points = newPoints;
            }
            else //Default level 0 Character Points
                Points = character.pointsLevel[0];

            //Spawn Gear
            if (character.StartGearLength() > 0)
                spawnStation.gameObject.SetActive(true);

            //Show Game Panels
            SetGamePanels(true);

            //Setup Ammo Type Prices
            SR_AmmoSpawner.instance.Setup();

            //Setup Attachment Prices
            SR_ModTable.instance.Setup();

            //Set Starting Supply ID as Host
            SetupSupplyPoints();

            //Set our next level
            CurrentLevel = optionStartLevel;
            SetLevel_Server();

            gameRunning = true;

            //Send Network Data
            if (Networking.IsHost())
            {
                gameServerRunning = true;
                SR_Networking.instance.GameOptions_Send();
                SR_Networking.instance.ServerRunning_Send();
            }

            //Game Launched
            Debug.Log("Supply Raid: Launched Game");
            if (LaunchedEvent != null)
                LaunchedEvent.Invoke();
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
        }

        private void CurrentSceneSettingsOnSosigKillEvent(Sosig s)
        {
            // Make sure the sosig is managed by us
            var sosig = sosigs.FirstOrDefault(x => x == s);
            if (!sosig) return;

            if (gameRunning && !ignoreKillStat)
            {
                stats.Kills++;
                SR_Networking.instance.UpdateStats_Send();
            }
            
            currentEnemies--;

            // Start a coroutine to respawn this sosig
            StartCoroutine(ClearSosig(sosig));
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

            // Wait a little bit after before checking Complete
            yield return new WaitForSeconds(1f);

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
            CapturesTotal++;

            //Game Complete?
            if (optionCaptures > 0 && CapturesTotal >= optionCaptures)
            {
                stats.ObjectiveComplete = true;
                CompleteGame();
            }
            else //Continue To Next level
            {
                SetLevel_Server();
            }

            //Post Capture Event
            if(PostCaptureEvent != null)
                PostCaptureEvent.Invoke();
        }

        public void CompleteGame()
        {
            if (!gameRunning)
                return;

            stats.ObjectiveComplete = true;

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

            //Tell Clients game is over!
            if (Networking.IsHost())
            {
                gameCompleted = true;
                SR_Networking.instance.UpdateStats_Send();
                SR_Networking.instance.ServerRunning_Send();
                SR_Networking.instance.LevelUpdate_Send(gameCompleted);
            }
        }

        void MovePanelsToLastSupply()
        {
            buyMenu.SetPositionAndRotation(LastSupplyPoint().buyMenu.position, LastSupplyPoint().buyMenu.rotation);

            ammoStation.SetPositionAndRotation(LastSupplyPoint().ammoStation.position, LastSupplyPoint().ammoStation.rotation);
            //ammoStation.gameObject.GetComponent<SR_AmmoSpawner>().SetRandomSeed();

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

        public void SetLevel_Client(int newLevel, int attackSupply, int playerSupply, bool isEndless)
        {
            //Assign Values
            CurrentLevel = newLevel;
            attackSupplyID = attackSupply;
            playerSupplyID = playerSupply;
            inEndless = isEndless;

            PlayCompleteSFX();

            //Update Panels
            MovePanelsToLastSupply();

            //AttackSupplyPoint().SetActiveSupplyPoint(true);
            //LastSupplyPoint().SetActiveSupplyPoint(false);

            //Apply Gameplay
            GivePlayerLevelPoints();


            //Update Capture Zone
            if (AttackSupplyPoint() != null)
                captureZone.MoveCaptureZone(AttackSupplyPoint().captureZone.position, AttackSupplyPoint().captureZone.localScale);

            if (SR_ResultsMenu.instance != null)
                SR_ResultsMenu.instance.UpdateResults();

            //Post Capture Event
            if(PostCaptureEvent != null)
                PostCaptureEvent.Invoke();
        }

        void GivePlayerLevelPoints()
        {
            if (character == null)
            {
                Debug.LogError("Supply Raid: Character Missing for Client");
                Points += 1;
            }

            //Points
            if (inEndless)
            {
                Points += character.pointsLevel[CurrentLevel];


                Points += (faction.levels.Length + CurrentLevel + character.pointsLevel[character.pointsLevel.Length - 1]);

            }
            else
            {
                int points = 0;

                //Outside of the characters point range, use last points
                if (CurrentLevel >= character.pointsLevel.Length)
                {
                    points = character.pointsLevel[character.pointsLevel.Length - 1];
                }


                Points += (CurrentLevel + character.pointsLevel[0]);
            }


            //Points += character.pointsLevel[CurrentLevel];
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

            //Cap at max character level
            if (!inEndless && CurrentLevel + 1 < faction.levels.Length)
            {
                //Debug.Log("Supply Raid: level Increase");
                CurrentLevel++;
            }
            else if (!inEndless && faction.endless.Length <= 0)
            {
                //Endless not setup - Reuse last level
                CurrentLevel = faction.levels.Length - 1;
                //Debug.Log("Supply Raid: No Endless");
            }
            else
            {
                //Debug.Log("Supply Raid: Endless");
                //ENDLESS
                if (!inEndless) //Not in endless yet
                {
                    inEndless = true;
                    CurrentLevel = 0;
                }
                else //Increase Level on endless repeat
                    CurrentLevel++;

                //If we're outside endless, start it over
                if (CurrentLevel >= faction.endless.Length)
                    CurrentLevel = 0;
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
                if (optionCaptureOrder == 1) //Random
                {
                    while (playerSupplyID == attackSupplyID)
                    {
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
                captureZone.MoveCaptureZone(AttackSupplyPoint().captureZone.position, AttackSupplyPoint().captureZone.localScale);

                SetupSquadSosigs(GetCurrentLevel());
                StartCoroutine(SetupDefenderSosigs(GetCurrentLevel()));

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
            Debug.Log("Supply Raid: End of Set Level");
        }

        //----------------------------------------------------------------------
        // Sosig Setup
        //----------------------------------------------------------------------

        void SetupSquadSosigs(FactionLevel currentLevel)
        {
            //Are squads enabled for this level?
            if (currentLevel.squadCount <= 0 || currentLevel.squadPool.Length <= 0)
                return;

            //Team
            switch (currentLevel.squadTeamRandomized)
            {
                case TeamEnum.Ally0:
                case TeamEnum.Team1:
                case TeamEnum.Team2:
                case TeamEnum.Team3:
                    _spawnOptions.IFF = (int)currentLevel.squadTeamRandomized;
                    break;
                case TeamEnum.RandomTeam:
                    _spawnOptions.IFF = Random.Range(0, 4);
                    break;
                case TeamEnum.RandomEnemyTeam:
                    _spawnOptions.IFF = Random.Range(1, 4);
                    break;
                default:
                    _spawnOptions.IFF = 0;
                    break;
            }

            //Setup sizes and safty check
            int minSquad = Mathf.Clamp(currentLevel.squadSizeMin, 0, int.MaxValue);
            int maxSquad = Mathf.Clamp(currentLevel.squadSizeMax, 1, int.MaxValue);

            if (maxSquad < minSquad)
                maxSquad = minSquad;


            //Group Setup
            List<int> groups = new List<int>();
            for (int i = 0; i < currentLevel.squadCount; i++)
            {
                int groupSize = Random.Range(minSquad, maxSquad + 1);

                if (groupSize != 0)
                    groups.Add(groupSize);
            }

            //Spawn Location
            SR_SupplyPoint spawnPoint = null;
            List<SR_SupplyPoint> usedSP = new List<SR_SupplyPoint>();

            //Spawn All Groups
            for (int x = 0; x < groups.Count; x++)
            {
                //Get Supply Point unoccupied
                //Make sure we have enough supply points
                if (supplyPoints.Count > 2)
                {
                    spawnPoint = null;

                    //Clear UsedSPs if we run out
                    if (usedSP.Count >= supplyPoints.Count + 2)
                        usedSP.Clear();

                    while (true)
                    {
                        spawnPoint = supplyPoints[Random.Range(0, supplyPoints.Count)];

                        //If not Player or Defender Supplypoint
                        if (spawnPoint != AttackSupplyPoint() && _spawnOptions.IFF == (int)TeamEnum.Ally0)
                        {
                            usedSP.Add(spawnPoint);
                            break;
                        }
                        else if (spawnPoint != AttackSupplyPoint() && spawnPoint != LastSupplyPoint())
                        {
                            usedSP.Add(spawnPoint);
                            break;
                        }
                    }
                }
                else
                    return; //Not enough for Squads to work TODO Make this work with ALLY squads

                //Error Check
                if (spawnPoint == null)
                    continue;

                StartCoroutine(SpawnSquadSosigs(groups[x], spawnPoint, currentLevel));
            }
        }

        private IEnumerator SpawnSquadSosigs(int groupSize, SR_SupplyPoint spawnPoint, FactionLevel currentLevel)
        {
            //Squad Move To Target
            SR_SupplyPoint target = null;
            if (currentLevel.squadBehaviour == SquadBehaviour.CaptureSupplyPoint)
                target = AttackSupplyPoint();
            else
            {
                while (true)
                {
                    target = supplyPoints[Random.Range(0, supplyPoints.Count)];

                    //If not Player or Defender Supplypoint
                    if (target != spawnPoint)
                    {
                        break;
                    }
                }
            }

            if (target == null)
            {
                Debug.Log("ERROR: Cannot find Target supply point in SpawnSquadSosigs");
                yield break;
            }

            //Slight delay to allow other spawns
            yield return new WaitForSeconds(1f);
            for (int i = 0; i < groupSize; i++)
            {
                if (!gameRunning)
                    yield break;

                SpawnSquadSosig(spawnPoint, target.squadPoint, currentLevel);
                yield return new WaitForSeconds(2f);
            }
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
            int enemyCount = currentLevel.enemiesTotal * optionPlayerCount;

            //Sniper Setup
            if (currentLevel.sniperCount > 0 && currentLevel.sniperPool.Length > 0)
            {
                List<Transform> usedSpots = new List<Transform>();

                int sniperCount = currentLevel.sniperCount * optionPlayerCount;
                for (int i = 0; i < sniperCount; i++)
                {
                    //While we haven't filled all the spots
                    while (true)
                    {
                        Transform spot = AttackSupplyPoint().sniperPoints[Random.Range(0, AttackSupplyPoint().sniperPoints.Length)];

                        if ((usedSpots.Count >= AttackSupplyPoint().sniperPoints.Length))
                        {
                            usedSpots.Clear();
                            yield return new WaitForSeconds(1f);
                        }

                        //Already in use, continue
                        if (usedSpots.Contains(spot))
                            continue;
                        else
                        {
                            enemyCount--;
                            usedSpots.Add(spot);
                            SpawnSniperSosig(spot.position, spot.rotation, currentLevel);

                            yield return new WaitForSeconds(0.25f);
                            break;
                        }
                    }
                }
            }

            yield return new WaitForSeconds(1f);

            //Guard Setup
            if (currentLevel.guardCount > 0 && currentLevel.guardPool.Length > 0)
            {
                List<Transform> usedSpots = new List<Transform>();

                int guardCount = currentLevel.guardCount * optionPlayerCount;
                for (int i = 0; i < guardCount; i++)
                {
                    //While we haven't filled all the spots
                    while (true)
                    {
                        Transform spot = AttackSupplyPoint().guardPoints[Random.Range(0, AttackSupplyPoint().guardPoints.Length)];

                        if ((usedSpots.Count >= AttackSupplyPoint().guardPoints.Length))
                        {
                            usedSpots.Clear();
                            yield return new WaitForSeconds(1f);
                        }

                        //Already in use, continue
                        if (usedSpots.Contains(spot))
                            continue;
                        else
                        {
                            enemyCount--;
                            usedSpots.Add(spot);
                            SpawnGuardSosig(spot.position, spot.rotation, currentLevel);
                            yield return new WaitForSeconds(0.25f);
                            break;
                        }
                    }
                }
            }

            yield return new WaitForSeconds(1f);

            if (currentLevel.patrolPool.Length <= 0)
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
                            newPos = AttackSupplyPoint().patrolPaths[pathID].patrolPoints[patrolPoint].position
                                + GetSquaredPosition(x, z, d);
                            SpawnPatrolSosig(
                                newPos,
                                AttackSupplyPoint().patrolPaths[pathID].patrolPoints[patrolPoint].rotation,
                                pp,
                                currentLevel);

                            enemiesTotal--;

                            yield return new WaitForSeconds(0.75f);

                            //Stop if nothing is running or we hit the enemy cap
                            if (!gameRunning || currentEnemies >= optionMaxEnemies)
                            {
                                remainEnemies = enemiesTotal;
                                //Debug.Log("Remaining Enemies: " + remainEnemies);

                                yield break;
                            }
                        }
                    }
                }
            }
        }

        void UpdateRabbithole()
        {
            bool infiniteEnemies = faction.levels[CurrentLevel].infiniteEnemies;

            //No more spawning enemies
            if (!infiniteEnemies && remainEnemies <= 0 || isClient)
                return;

            //Do once a second check
            if ((rabbitHoleTimer -= Time.deltaTime) <= 0)
            {
                //Check once a second
                rabbitHoleTimer = faction.levels[CurrentLevel].enemySpawnTimer;

                int maxEnemies = optionMaxEnemies;  //Max On Screen Enemies

                //Infinite Enemies limitations
                if (infiniteEnemies && faction.levels[CurrentLevel].enemiesTotal < optionMaxEnemies)
                    maxEnemies = faction.levels[CurrentLevel].enemiesTotal; //Max Extra Enemies in infinite mode

                //If a enemy slot has freed up
                if (currentEnemies < maxEnemies)
                {
                    if(!infiniteEnemies)
                        remainEnemies--;

                    SpawnRabbitholeSosig(GetCurrentLevel());
                }
            }
        }

        void SpawnRabbitholeSosig(FactionLevel currentLevel)
        {
            int pathID = Random.Range(0, AttackSupplyPoint().patrolPaths.Length);
            PatrolPath pp = AttackSupplyPoint().patrolPaths[pathID];
            Transform sosigSpawn = AttackSupplyPoint().GetRandomSosigSpawn();

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

            Sosig sosig = CreateSosig(_spawnOptions, position, rotation, currentLevel.guardPool, currentLevel.name);

            if (sosig == null)
            {
                Debug.LogError("Supply Raid - No Guard Sosig to Spawn");
                return;
            }

            currentEnemies++;

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
            defenderSosigs.Add(sosig);
        }

        void SpawnSniperSosig(Vector3 position, Quaternion rotation, FactionLevel currentLevel)
        {
            //Error Check
            if (currentLevel == null)
            {
                Debug.Log("Supply Raid - Missing Sniper Spawn Data");
                return;
            }

            Sosig sosig = CreateSosig(_spawnOptions, position, rotation, currentLevel.sniperPool, currentLevel.name);

            if (sosig == null)
            {
                Debug.LogError("Supply Raid - No Sniper Sosig to Spawn");
                return;
            }

            currentEnemies++;

            sosig.Speed_Walk = 0;
            sosig.Speed_Run = 0;
            sosig.Speed_Crawl = 0;
            sosig.Speed_Sneak = 0;

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
            defenderSosigs.Add(sosig);
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

            currentEnemies++;

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
            defenderSosigs.Add(sosig);
        }

        void SpawnSquadSosig(SR_SupplyPoint spawnSupply, Transform target, FactionLevel currentLevel)
        {
            //Error Check
            if (spawnSupply == null || target == null || currentLevel == null)
            {
                Debug.Log("Supply Raid - Missing Squad Spawn Data");
                return;
            }
            Transform sosigSpawn = AttackSupplyPoint().GetRandomSosigSpawn();

            Sosig sosig = CreateSosig(_spawnOptions, sosigSpawn.position, sosigSpawn.rotation, currentLevel.squadPool, currentLevel.name);

            if (sosig == null)
            {
                Debug.LogError("Supply Raid - No Squad Sosig to Spawn");
                return;
            }

            //Randomize in the sosig Squad Waypoint scale
            sosig.m_pathToPoint = target.position + new Vector3(
                Random.Range(-target.lossyScale.x, target.lossyScale.x) / 2,
                0,
                Random.Range(-target.lossyScale.z, target.lossyScale.z) / 2);

            List<Vector3> pathPoints = new List<Vector3>
            {
                sosig.m_pathToPoint,
            };

            List<Vector3> pathDirs = new List<Vector3>
            {
                target.rotation.eulerAngles,
            };

            if (currentLevel.squadBehaviour == SquadBehaviour.RandomSupplyPoint)
            {
                pathPoints.Add(spawnSupply.captureZone.position);
                pathDirs.Add(spawnSupply.captureZone.rotation.eulerAngles);
            }

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
        }

        Sosig CreateSosig(SosigAPI.SpawnOptions spawnOptions, Vector3 position, Quaternion rotation, SosigEnemyID[] pool, string poolName)
        {
            Debug.Log("Supply Raid - Spawning Sosig at position: " + position);

            //Get Valid Sosig ID
            if (pool.Length <= 0)
            {
                Debug.LogError("Supply Raid - No squad enemy IDs assigned to faction level " + poolName);
                return null;
            }

            SosigEnemyID id = GetRandomSosigIDFromPool(pool);
            if (id == SosigEnemyID.None)
            {
                Debug.LogError("Supply Raid - No valid squad enemy IDs in faction level " + poolName);
                return null;
            }

            Sosig sosig =
                SosigAPI.Spawn(
                    IM.Instance.odicSosigObjsByID[id],
                    _spawnOptions,
                    position,
                    rotation);

            //TODO not a hack job
            sosig.m_isBlinded = false;

            return sosig;
        }

        /// <summary>
        /// Tries to get a Random Sosig Enemy ID from the input pool, returns None if not valid or not found
        /// </summary>
        /// <param name="pool"></param>
        /// <returns></returns>
        SosigEnemyID GetRandomSosigIDFromPool(SosigEnemyID[] pool)
        {
            SosigEnemyID id = SosigEnemyID.None;

            if (pool.Length == 0)
                return id;

            int count = 0;
            while (true)
            {
                if (count >= pool.Length)
                {
                    Debug.LogError("Supply Raid - Faction level " + faction.levels[CurrentLevel].name + " has no valid SosigEnemyIDs, Not Spawning Sosigs");
                    return id;
                }

                id = pool[Random.Range(0, pool.Length)];

                if (ValidSosigEnemyID(id))
                    break;
                else
                    count++;
            }

            return id;
        }

        bool ValidSosigEnemyID(SosigEnemyID id)
        {
            if(id == SosigEnemyID.None)
                return false;

            if (IM.Instance.odicSosigObjsByID.ContainsKey(id))
                return true;

            return false;
        }

        void ClearSosigs()
        {
            ignoreKillStat = true;
            remainEnemies = 0;

            for (int i = 0; i < sosigs.Count; i++)
            {
                sosigs[i].KillSosig();
            }
            sosigs.Clear();

            currentEnemies = 0;
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

        public static SR_CharacterPreset GetCharacter()
        {
            return instance.character;
        }

        public static FactionLevel GetCurrentLevel()
        {
            if (instance.inEndless)
                return instance.faction.endless[instance.CurrentLevel];
            else
                return instance.faction.levels[instance.CurrentLevel];

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

        public void SetLocalAsClient()
        {
            isClient = true;
            SR_Networking.instance.RequestSync_Send();
        }

        public void Network_GameOptions(
            int playerCount, float difficulty, bool freeBuyMenu, bool spawnLocking, int startLevel, int playerHealth,
            bool itemSpawner, bool captureZone, int order, int captures, bool respawn, int maxEnemies)
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

            //SR_Menu.instance.SetFactionByName(factionID);
            //instance.factionID = factionID;

            //DO visual update here
            if(SR_Menu.instance != null)
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
                score += instance.CapturesTotal * 2500;   //2500 points for captures (25 kills)
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