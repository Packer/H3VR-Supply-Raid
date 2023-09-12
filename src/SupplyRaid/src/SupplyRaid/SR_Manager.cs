using FistVR;
using Sodalite.Api;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using H3MP;
using H3MP.Networking;

namespace SupplyRaid
{
    public class SR_Manager : MonoBehaviour
    {
        public static SR_Manager instance = null;

        [Header("Player Stats")]
        [Tooltip("Current game level + difficulty")]
        public int level = 0;

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
        [Tooltip("One Hit = 0, Half = 1, Standard = 2, Armoured = 3, Invulnerable = 4"), Range(0, 4)]
        public int optionPlayerHealth = 2;
        [Tooltip("Item Spawner")]
        public bool optionItemSpawner = false;
        [Tooltip("Is the capture Zone being used")]
        public bool optionCaptureZone = true;
        [Tooltip("Right Hand = True, Left Hand = False"), HideInInspector]
        public bool optionHand = false;
        [Tooltip("Do we do the supply points in Array Order")]
        public bool optionLinear = true;
        [Tooltip("How many captures until the game completes, 0 = Infinite, 1+ = Number of Captures")]
        public int optionCaptures = 5;
        [Tooltip("Can the players respawn after dying?")]
        public bool optionRespawn = true;
        [Tooltip("How many allowed to be alive at a time")]
        public int optionMaxEnemies = 12;

        [Header("Game Stats")]
        [HideInInspector, Tooltip("Did the player succesfully beat the game?")]
        public bool statObjectiveComplete = false;
        [HideInInspector]
        public float gameTime = 0;
        [HideInInspector]
        public int statDeaths = 0;
        [HideInInspector]
        public int statCaptures = 0;
        [HideInInspector]
        public int statKills = 0;

        [Header("Game Options")]
        [Tooltip("Force the map to be linear Supply Points only")]
        public bool linearOnly = false;
        [Tooltip("Starting Points, -1 = Use Character Points, 0+ = Map specific starting points")]
        public int startPointsOverride = -1;

        [Header("Gameplay")]
        public SR_CharacterPreset character;
        public SR_SosigFaction faction;
        [HideInInspector]
        public bool running = false;
        [HideInInspector]
        public int supplyID = 0;
        [HideInInspector]
        public int lastSupplyID = 0;

        public Transform srMenu;
        public Transform spawnMenu;
        public Transform buyMenu;
        public Transform ammoStation;
        public Transform recycler;
        public Transform duplicator;
        public Transform itemSpawner;
        public Transform resultsMenu;
        public Transform spawnPoint;
        public Transform? spawnStation;
        public SR_CaptureZone captureZone;

        [Header("World")]
        public SR_CharacterPreset[] characters;
        public SR_SosigFaction[] factions;
        public List<SR_SupplyPoint> supplyPoints = new List<SR_SupplyPoint>();
        [HideInInspector]
        public LootTable lt_RequiredAttachments;
        //public SR_SupplyPoint[] supplyPoints;  //Locations we attack

        private readonly List<Sosig> sosigs = new List<Sosig>();
        private readonly List<Sosig> defenderSosigs = new List<Sosig>();

        private readonly SosigAPI.SpawnOptions _spawnOptions = new SosigAPI.SpawnOptions
        {
            SpawnState = Sosig.SosigOrder.PathTo,
            SpawnActivated = true,
            EquipmentMode = SosigAPI.SpawnOptions.EquipmentSlots.All,
            SpawnWithFullAmmo = true,
        };

        public TNH_SosiggunShakeReloading shakeReloading = TNH_SosiggunShakeReloading.On;

        [Header("Networking")]
        [HideInInspector]
        public bool isH3MP = false;     //Are we doing Multiplayer
        [HideInInspector]
        public bool isClient = false;   //Are we a client in Multiplayer

        public delegate void LaunchedDelegate();
        public static event LaunchedDelegate LaunchedEvent;

        public delegate void PointGainDelegate(int i);
        public static event PointGainDelegate PointEvent;

        private int points = 2;
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
            }
            get { return points; }
        }

        [Header("Audio")]
        public AudioSource globalAudio;
        public AudioClip audioComplete;

        private bool ignoreKillStat = false;
        public bool endless = false;

        private int currentEnemies = 0;
        private int remainEnemies = 0;
        private float rabbitHoleTimer = 0;

        public void ResetGame()
        {
            //Stats
            statObjectiveComplete = false;
            gameTime = 0;
            statDeaths = 0;
            statCaptures = 0;
            statKills = 0;

            //Gameplay
            running = false;
            level = 0;
            supplyID = 0;
            lastSupplyID = 0;

            ClearSosigs();

            Start();
        }

        // Use this for initialization
        void Awake()
        {
            instance = this;
            //GM.CurrentAIManager.
            //InvokeRepeating("RepeatLevel", 10, 10);


        }

        void Start()
        {
            if (linearOnly)
                optionLinear = true;

            //TODO Move this some where else after everything has setup
            //SetGamePanels(false);
            resultsMenu.gameObject.SetActive(false);
            SetupRequiredAttachmentLootTable();
        }

        void OnDisable()
        {
            //Events
            GM.CurrentSceneSettings.SosigKillEvent -= CurrentSceneSettingsOnSosigKillEvent;
            GM.CurrentSceneSettings.PlayerDeathEvent -= PlayerDeathEvent;
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
                    GM.CurrentPlayerBody.SetHealthThreshold(10000f);
                    break;
                case 4:
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

            //Starting Points
            else if (optionStartLevel > 0) //TODO make this math make sense
                Points = (optionStartLevel * character.pointsPerLevel) + character.points;

            //Spawn Gear
            if (character.startGear.Length > 0)
                spawnStation.gameObject.SetActive(true);

            //Show Game Panels
            SetGamePanels(true);

            //Set Starting Supply ID as Host
            if (!isClient)
            {
                int oldSupply = supplyID;

                int sp = -1;
                for (int i = 0; i < supplyPoints.Count; i++)
                {
                    if (supplyPoints[i].forceFirstSpawn)
                    {
                        sp = i;
                        supplyID = i;
                        break;
                    }
                }

                //Error Check, only 1 supply point don't do while loop
                if (sp == -1 && !optionLinear && supplyPoints.Count > 1)
                {
                    while (oldSupply == supplyID)
                    {
                        supplyID = Random.Range(0, supplyPoints.Count);
                    }
                }

                SetLevel_Server(optionStartLevel);
            }

            //Teleport to spawn
            /*
            GM.CurrentMovementManager.TeleportToPoint(CurrentSupplyPoint().respawn.position,
                true,
                CurrentSupplyPoint().respawn.forward);
            */

            running = true;

            //Game Launched
            Debug.Log("Launch EVENT");
            if (LaunchedEvent != null)
                LaunchedEvent.Invoke();
        }

        private void CurrentSceneSettingsOnSosigKillEvent(Sosig s)
        {
            // Make sure the sosig is managed by us
            var sosig = sosigs.FirstOrDefault(x => x == s);
            if (!sosig) return;

            if (running && !ignoreKillStat)
                statKills++;

            currentEnemies--;

            // Start a coroutine to respawn this sosig
            StartCoroutine(ClearSosig(sosig));
        }

        private void PlayerDeathEvent(bool killedSelf)
        {
            if (running)
            {
                statDeaths++;

                //If respawning not enabled
                if (!optionRespawn)
                {
                    statObjectiveComplete = false;

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
            if (defenderSosigs.Count <= 0)
            {
                //Level End
                if (Networking.IsHost())
                    GameCompleteCheck(level + 1);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (running)
            {
                gameTime += Time.deltaTime;
                UpdateRabbithole();
            }
        }

        public void GameCompleteCheck(int newLevel)
        {
            //Debug.Log("Game Check");
            if (!running)
                return;

            statCaptures++;
            //Debug.Log(optionCaptures + " Captures | newLevel = " + newLevel);

            if (optionCaptures != -1 && newLevel >= optionCaptures)
            {
                statObjectiveComplete = true;
                //Increase
                if (isClient)
                    return;

                //Set Completed Levels
                level = newLevel;

                CompleteGame();
            }
            else
            {
                //Increase
                if (isClient)
                    return;

                SetLevel_Server(newLevel);
            }
        }

        public void CompleteGame()
        {
            if (!running)
                return;

            statObjectiveComplete = true;

            //Game Complete
            resultsMenu.gameObject.SetActive(true);
            DisableGamePanels();
            if (SR_ResultsMenu.instance != null)
                SR_ResultsMenu.instance.UpdateStats();
            else
                Debug.Log("Missing Results");

            //TODO play game complete sound
            globalAudio.PlayOneShot(audioComplete);

            //game is not longer running
            running = false;

            //Clear all enemies from the world
            ClearSosigs();

            //Teleport player to Spawn
            GM.CurrentMovementManager.TeleportToPoint(spawnPoint.position, true, spawnPoint.rotation.eulerAngles);

            //Tell Clients game is over!
            if (Networking.IsHost())
                SR_Networking.instance.LevelUpdate_Send(true);
        }

        void MovePanels()
        {
            buyMenu.SetPositionAndRotation(LastSupplyPoint().buyMenu.position, LastSupplyPoint().buyMenu.rotation);

            ammoStation.SetPositionAndRotation(LastSupplyPoint().ammoStation.position, LastSupplyPoint().ammoStation.rotation);
            ammoStation.gameObject.GetComponent<SR_AmmoSpawner>().SetRandomSeed();

            recycler.SetPositionAndRotation(LastSupplyPoint().recycler.position, LastSupplyPoint().recycler.rotation);

            duplicator.SetPositionAndRotation(LastSupplyPoint().duplicator.position, LastSupplyPoint().duplicator.rotation);
        }

        /// <summary>
        /// Sets all gameplay buy menu panels to state and srMenu to the inverse state
        /// </summary>
        /// <param name="state"></param>
        void SetGamePanels(bool state)
        {
            buyMenu.gameObject.SetActive(state);
            ammoStation.gameObject.SetActive(state);
            recycler.gameObject.SetActive(state);
            duplicator.gameObject.SetActive(state);
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
            srMenu.gameObject.SetActive(false);
        }

        //----------------------------------------------------------------------
        // Level Setup
        //----------------------------------------------------------------------

        public void SetLevel_Client(int newLevel, int supply, int lastSupply, bool isEndless)
        {
            //Assign Values
            level = newLevel;
            lastSupplyID = lastSupply;
            supplyID = supply;
            endless = isEndless;

            globalAudio.PlayOneShot(audioComplete);

            //Update Panels
            MovePanels();

            //Apply Gameplay
            Points += newLevel + character.pointsPerLevel;

            //Update Capture Zone
            captureZone.MoveCaptureZone(CurrentSupplyPoint().captureZone.position, CurrentSupplyPoint().captureZone.localScale);
        }

        public void SetLevel_Server(int newLevel)
        {
            if (isClient)
                return;


            if (running)
            {
                globalAudio.PlayOneShot(audioComplete);

                //Clear All Sosigs
                ClearSosigs();

                //Points TODO change for H3MP
                Points += (newLevel + character.pointsPerLevel);
            }
            else
            {
                //Game Started
                if (startPointsOverride >= 0)
                    Points = startPointsOverride;
                else
                    Points = character.points;
            }

            //Cap at max level
            if (!endless && newLevel < faction.levels.Length)
                level = newLevel;
            else if (faction.endless.Length <= 0)
            {
                //Endless not setup - Reuse last level
                level = faction.levels.Length - 1;
            }
            else
            {
                //ENDLESS
                if (!endless)
                {
                    endless = true;
                    level = 0;
                }

                //Increase Level
                level++;

                //If we're outside endless, start it over
                if (level >= faction.endless.Length)
                    level = 0;
            }


            //Set Supply ID to something not the same
            lastSupplyID = supplyID;

            //Error Check, only 1 supply point don't do while loop
            if (supplyPoints.Count > 1)
            {
                if (optionLinear)
                {
                    //Increase only if we're not at the end of the count
                    if (supplyID + 1 < supplyPoints.Count)
                    {
                        supplyID++;
                    }
                    else
                    {
                        supplyID = 0;
                    }
                }
                else
                {
                    while (lastSupplyID == supplyID)
                    {
                        supplyID = Random.Range(0, supplyPoints.Count);
                    }
                }

                //Update Capture Zone
                captureZone.MoveCaptureZone(CurrentSupplyPoint().captureZone.position, CurrentSupplyPoint().captureZone.localScale);

                if (endless)
                {
                    SetupSquadSosigs(faction.endless[level]);
                    StartCoroutine(SetupDefenderSosigs(faction.endless[level]));
                }
                else
                {
                    SetupSquadSosigs(faction.levels[level]);
                    StartCoroutine(SetupDefenderSosigs(faction.levels[level]));
                }
            }

            //Update Panels Last
            MovePanels();

            //Send to Clients
            if (Mod.managerObject != null)
                SR_Networking.instance.LevelUpdate_Send(false);
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
                case TeamSquadEnum.Ally0:
                case TeamSquadEnum.Team1:
                case TeamSquadEnum.Team2:
                case TeamSquadEnum.Team3:
                    _spawnOptions.IFF = (int)currentLevel.squadTeamRandomized;
                    break;
                case TeamSquadEnum.RandomTeam:
                    _spawnOptions.IFF = Random.Range(0, 4);
                    break;
                case TeamSquadEnum.RandomEnemyTeam:
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
            SR_SupplyPoint sp = null;
            List<SR_SupplyPoint> usedSP = new List<SR_SupplyPoint>();

            //Spawn All Groups
            for (int x = 0; x < groups.Count; x++)
            {

                //Get Supply Point unoccupied
                //Make sure we have enough supply points
                if (supplyPoints.Count > 2)
                {
                    sp = null;

                    //Clear UsedSPs if we run out
                    if (usedSP.Count >= supplyPoints.Count + 2)
                        usedSP.Clear();

                    while (true)
                    {
                        sp = supplyPoints[Random.Range(0, supplyPoints.Count)];

                        //If not Player or Defender Supplypoint
                        if (sp != CurrentSupplyPoint() && sp != LastSupplyPoint())
                        {
                            usedSP.Add(sp);
                            break;
                        }
                    }
                }
                else
                    return; //Not enough for Squds to work TODO Make this work with ALLY squads

                //Error Check
                if (sp == null)
                    continue;

                StartCoroutine(SpawnSquadSosigs(groups[x], sp, currentLevel));
            }
        }

        private IEnumerator SpawnSquadSosigs(int groupSize, SR_SupplyPoint spawnPoint, FactionLevel currentLevel)
        {
            //Squad Move To Target
            SR_SupplyPoint target = null;
            if (currentLevel.squadBehaviour == SquadBehaviour.CaptureSupplyPoint)
                target = CurrentSupplyPoint();
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
                if (!running)
                    yield break;

                SpawnSquadSosig(spawnPoint, target.squadPoint, currentLevel);
                yield return new WaitForSeconds(2f);
            }
        }

        private IEnumerator SetupDefenderSosigs(FactionLevel currentLevel)
        {
            //Assign Team
            _spawnOptions.IFF = (int)faction.teamID;

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
                        Transform spot = CurrentSupplyPoint().sniperPoints[Random.Range(0, CurrentSupplyPoint().sniperPoints.Length)];

                        if ((usedSpots.Count >= CurrentSupplyPoint().sniperPoints.Length))
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
                        Transform spot = CurrentSupplyPoint().guardPoints[Random.Range(0, CurrentSupplyPoint().guardPoints.Length)];

                        if ((usedSpots.Count >= CurrentSupplyPoint().guardPoints.Length))
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
            if (CurrentSupplyPoint().patrolPaths.Length < groups.Count)
                sharePaths = true;

            List<int> usedPaths = new List<int>();

            //For each Patrol Path, create even amount of sosigs
            for (int y = 0; y < groups.Count; y++)
            {
                int pathID = 0;
                PatrolPath pp = null;
                if (sharePaths)
                {
                    pathID = Random.Range(0, CurrentSupplyPoint().patrolPaths.Length);
                    pp = CurrentSupplyPoint().patrolPaths[pathID];
                }
                else
                {
                    while (pp == null)
                    {
                        pathID = Random.Range(0, CurrentSupplyPoint().patrolPaths.Length);

                        if (!usedPaths.Contains(pathID))
                        {
                            usedPaths.Add(pathID);
                            pp = CurrentSupplyPoint().patrolPaths[pathID];
                        }
                    }
                }

                int patrolPoint = pp.GetRandomPoint();

                _spawnOptions.SosigTargetPosition = CurrentSupplyPoint().patrolPaths[pathID].patrolPoints[patrolPoint].position;

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
                            newPos = CurrentSupplyPoint().patrolPaths[pathID].patrolPoints[patrolPoint].position
                                + GetSquaredPosition(x, z, d);
                            SpawnPatrolSosig(
                                newPos,
                                CurrentSupplyPoint().patrolPaths[pathID].patrolPoints[patrolPoint].rotation,
                                pp,
                                pathID,
                                currentLevel);

                            enemiesTotal--;

                            yield return new WaitForSeconds(0.75f);

                            //Stop if nothing is running or we hit the enemy cap
                            if (!running || currentEnemies >= optionMaxEnemies)
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
            //No more spawning enemies
            if (remainEnemies <= 0 || isClient)
                return;

            //Do once a second check
            if ((rabbitHoleTimer -= Time.deltaTime) <= 0)
            {
                //Check once a second
                rabbitHoleTimer = 1;
                //If a enemy slot has freed up
                if (currentEnemies < optionMaxEnemies)
                {
                    remainEnemies--;
                    if (endless)
                        SpawnRabbitholeSosig(faction.endless[level]);
                    else
                        SpawnRabbitholeSosig(faction.levels[level]);
                }
            }
        }

        void SpawnRabbitholeSosig(FactionLevel currentLevel)
        {
            int pathID = Random.Range(0, CurrentSupplyPoint().patrolPaths.Length);
            PatrolPath pp = CurrentSupplyPoint().patrolPaths[pathID];
            Transform sosigSpawn = CurrentSupplyPoint().GetRandomSosigSpawn();

            SpawnPatrolSosig(sosigSpawn.position, sosigSpawn.rotation,
                                pp,
                                pathID,
                                currentLevel);
            Debug.Log("Spawned Patrol Sosig " + sosigSpawn.position);
        }

        Vector3 GetSquaredPosition(int x, int z, int d)
        {
            return new Vector3(x * 1 - ((d / 2) * 1), 0, -z * 1);
        }

        void SpawnGuardSosig(Vector3 position, Quaternion rotation, FactionLevel currentLevel)
        {
            var sosig =
                SosigAPI.Spawn(
                    IM.Instance.odicSosigObjsByID[currentLevel.GetGuardFromPool()],
                    _spawnOptions,
                    position,
                    rotation);

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
            sosig.MaxHearingRange = 300; //300
            //sosig.StateHearingRangeMults = new Vector3(0.6f, 1f, 1f);
            sosig.MaxFOV = 160 * optionDifficulty; //105
            //sosig.StateFOVMults = new Vector3(0.5f, 0.75f, 1f);
            sosig.MaxSightRange = 300 * optionDifficulty; //250
            //sosig.StateSightRangeMults = new Vector3(0.33f, 0.5f, 1f);

            sosigs.Add(sosig);
            defenderSosigs.Add(sosig);
        }

        void SpawnSniperSosig(Vector3 position, Quaternion rotation, FactionLevel currentLevel)
        {

            var sosig =
                SosigAPI.Spawn(
                    IM.Instance.odicSosigObjsByID[currentLevel.GetSniperFromPool()],
                    _spawnOptions,
                    position,
                    rotation);

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
            sosig.MaxHearingRange = 300; //300
            //sosig.StateHearingRangeMults = new Vector3(0.6f, 1f, 1f);
            sosig.MaxFOV = 160 * optionDifficulty; //105
            sosig.StateFOVMults = new Vector3(0.5f, 0.75f, 1f);
            sosig.MaxSightRange = 300 * optionDifficulty; //250
            sosig.StateSightRangeMults = new Vector3(0.33f, 0.5f, 1f);

            sosigs.Add(sosig);
            defenderSosigs.Add(sosig);
        }

        void SpawnPatrolSosig(Vector3 position, Quaternion rotation, PatrolPath pp, int pathID, FactionLevel currentLevel)
        {
            var sosig =
                SosigAPI.Spawn(
                    IM.Instance.odicSosigObjsByID[currentLevel.GetEnemyFromPool()],
                    _spawnOptions,
                    position,
                    rotation);

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
            sosig.MaxHearingRange = 350; //300
            //sosig.StateHearingRangeMults = new Vector3(0.6f, 1f, 1f);
            sosig.MaxFOV = 160 * optionDifficulty; //105
            //sosig.StateFOVMults = new Vector3(0.5f, 0.6f, 1f);
            sosig.MaxSightRange = 250 * optionDifficulty; //250
            //sosig.StateSightRangeMults = new Vector3(0.1f, 0.35f, 1f);

            sosigs.Add(sosig);
            defenderSosigs.Add(sosig);
        }

        void SpawnSquadSosig(SR_SupplyPoint spawnSupply, Transform target, FactionLevel currentLevel)
        {

            Transform sosigSpawn = CurrentSupplyPoint().GetRandomSosigSpawn();

            Debug.Log("Spawning Squad Sosig: " + sosigSpawn.position);

            var sosig =
                SosigAPI.Spawn(
                    IM.Instance.odicSosigObjsByID[currentLevel.GetSquadFromPool()],
                    _spawnOptions,
                    sosigSpawn.position,
                    sosigSpawn.rotation);

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
            sosig.MaxHearingRange = 350; //300
            //sosig.StateHearingRangeMults = new Vector3(0.6f, 1f, 1f);
            sosig.MaxFOV = 160 * optionDifficulty; //105
            //sosig.StateFOVMults = new Vector3(0.5f, 0.6f, 1f);
            sosig.MaxSightRange = 250 * optionDifficulty; //250
            //sosig.StateSightRangeMults = new Vector3(0.1f, 0.35f, 1f);

            sosigs.Add(sosig);
        }


        void ClearSosigs()
        {
            ignoreKillStat = true;

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
            return CurrentSupplyPoint().captureZone;
        }

        public Transform GetLastSupplyPoint()
        {
            return LastSupplyPoint().captureZone;
        }

        void SetupRequiredAttachmentLootTable()
        {
            if (GM.CurrentPlayerBody == null)
                return;

            lt_RequiredAttachments = new LootTable();

            LootTable.LootTableType type = LootTable.LootTableType.Attachments;

            List<FVRObject.OTagEra> eras = new List<FVRObject.OTagEra>
            {
                FVRObject.OTagEra.Modern,
                FVRObject.OTagEra.PostWar,
                FVRObject.OTagEra.TurnOfTheCentury,
                FVRObject.OTagEra.WildWest,
                FVRObject.OTagEra.WW1,
                FVRObject.OTagEra.WW2,
                FVRObject.OTagEra.Futuristic,
                FVRObject.OTagEra.Colonial
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

            lt_RequiredAttachments.Initialize(type, eras, null, null, null, null, null, mounts, null, features, null, null, null, null);
        }

        //----------------------------------------------------------------------
        // Static
        //----------------------------------------------------------------------

        public static void AddSupplyPoint(SR_SupplyPoint sp)
        {
            if (instance == null)
            {
                Debug.Log("There is no Supply Raid Manager in this scene");
                return;
            }

            if (!instance.supplyPoints.Contains(sp))
                instance.supplyPoints.Add(sp);
        }

        public static void RemoveSupplyPoint(SR_SupplyPoint sp)
        {
            if (instance == null)
            {
                Debug.Log("There is no Supply Raid Manager in this scene");
                return;
            }

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
        public static SR_SupplyPoint CurrentSupplyPoint()
        {
            if (instance != null)
                return instance.supplyPoints[instance.supplyID];

            return null;
        }

        /// <summary>
        /// Returns the Previous Supply point
        /// </summary>
        /// <returns></returns>
        public static SR_SupplyPoint LastSupplyPoint()
        {
            if (instance != null)
                return instance.supplyPoints[instance.lastSupplyID];

            return null;
        }

        public static void IncreaseLevel()
        {
            if (instance != null)
                instance.SetLevel_Server(instance.level + 1);
        }



        //----------------------------------------------------------------------
        // Networking
        //----------------------------------------------------------------------

        public void SetLocalAsClient()
        {
            isClient = true;
            SR_Menu.instance.UpdateGameOptions();
        }

        public void Network_GameOptions(
            int playerCount, float difficulty, bool freeBuyMenu, bool spawnLocking, int startLevel, int playerHealth,
            bool itemSpawner, bool captureZone, bool linear, int captures, bool respawn, int maxEnemies)
        {
            optionPlayerCount = playerCount;
            optionDifficulty = difficulty;
            optionFreeBuyMenu = freeBuyMenu;
            optionSpawnLocking = spawnLocking;
            optionStartLevel = startLevel;
            optionPlayerHealth = playerHealth;
            optionItemSpawner = itemSpawner;
            optionCaptureZone = captureZone;
            optionLinear = linear;
            optionCaptures = captures;
            optionRespawn = respawn;
            optionMaxEnemies = maxEnemies;

            //DO visual update here
            SR_Menu.instance.UpdateGameOptions();
        }

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