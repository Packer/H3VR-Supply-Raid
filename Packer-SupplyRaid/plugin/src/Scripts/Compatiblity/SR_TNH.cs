using FistVR;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlas.MappingComponents.TakeAndHold;


namespace SupplyRaid
{
    public class SR_TNH : MonoBehaviour
    {
        public TNH_Manager tnhManager;
        public SR_Manager srManager;
        public TNH_ManagerOverride tnHOverideManager;

        public static float maxBoundsSize = 10;
        public static float spawnHalfExtent = 20;
        public static float navRange = 30;


        public void Start()
        {
            if(tnHOverideManager)
                StartCoroutine(DelaySetup());
            else
                StartCoroutine(SetupSupplyRaid());
        }

        public IEnumerator DelaySetup()
        {
            yield return null;
            tnhManager = FindObjectOfType<TNH_Manager>();

            yield return null;
            if(tnhManager != null)
                StartCoroutine(SetupSupplyRaid());
        }

        public IEnumerator SetupSupplyRaid()
        {
            //Disable TnH Content
            if (tnhManager)
            {
                tnhManager.FMODController.SwitchTo(0, 0, true, true);
                tnhManager.ClearMiscEnemies();
                tnhManager.gameObject.SetActive(false);
                tnhManager.FMODController.SetMasterVolume(0);

                for (int i = 0; i < tnhManager.HoldPoints.Count; i++)
                {
                    tnhManager.HoldPoints[i].ForceClearConfiguration();
                    tnhManager.HoldPoints[i].gameObject.SetActive(false);
                }

                //Supply Points
                for (int i = 0; i < tnhManager.SupplyPoints.Count; i++)
                {
                    tnhManager.SupplyPoints[i].ClearConfiguration();
                    tnhManager.SupplyPoints[i].gameObject.SetActive(false);
                }
            }
            if (tnHOverideManager)
            {
                tnHOverideManager.gameObject.SetActive(false);
            }

            //Clear anything spawned
            VaultSystem.ClearExistingSaveableObjects(true);

            //Generate Our Supply Raid Content
            Instantiate(new GameObject()).AddComponent<SR_ModLoader>();

            yield return null;

            SetupManager();

            yield return null;

            //Generate Supply Points then load our assets
            yield return StartCoroutine(SetupSupplyPoints());
            yield return null;
            yield return StartCoroutine(SetupSupplyPointsHold());

            Debug.Log("Supply Raid: SR Supply Points Total: " + SR_Manager.instance.supplyPoints.Count);

            //Load our Assets
            StartCoroutine(SR_ModLoader.LoadSupplyRaidAssets());
        }

        void SetupManager()
        {
            //Create our Manager
            GameObject SRTNH = Instantiate(new GameObject());
            srManager = SRTNH.AddComponent<SR_Manager>();

            //Menus
            Transform spawnPoint = new GameObject().transform;
            Transform menus = new GameObject().transform;
            menus.SetPositionAndRotation(tnhManager.ScoreDisplayPoint.position + (Vector3.down * 1.5f), tnhManager.ScoreDisplayPoint.rotation);
            spawnPoint.SetPositionAndRotation(menus.position + -(menus.forward * 2), menus.rotation);
            Transform outside = new GameObject().transform;
            outside.position = tnhManager.ScoreDisplayPoint.position + (Vector3.down * 5);

            srManager.srMenu = menus;
            srManager.spawnMenu = menus;
            srManager.resultsMenu = menus;
            srManager.spawnPoint = spawnPoint;
            srManager.spawnStation = menus;
            srManager.itemSpawner = tnhManager.ItemSpawner.transform;
            srManager.itemSpawner.transform.SetParent(null);
            srManager.itemSpawner.SetPositionAndRotation(
                menus.position + Vector3.up + (-srManager.itemSpawner.forward * 2), 
                menus.rotation * Quaternion.Euler(0, 180, 0));

            srManager.buyMenu = outside;
            srManager.ammoStation = outside;
            srManager.recycler = outside;
            srManager.duplicator = outside;
            srManager.attachmentStation = outside;
            srManager.itemSpawner = outside;
            srManager.captureZone = outside.gameObject.AddComponent<SR_CaptureZone>();
            srManager.captureZone.zone = transform;

            GM.CurrentMovementManager.TeleportToPoint(srManager.spawnPoint.position, true, srManager.spawnPoint.forward);
        }

        public Transform TryGetLocation(List<Transform> safeLocations, int index, List<Transform> tryLocations = null)
        {
            if (tryLocations != null && tryLocations.Count > index)
                return tryLocations[index];

            if (safeLocations.Count > index)
            {
                return safeLocations[index];
            }

            return safeLocations[Random.Range(0, safeLocations.Count)];
        }

        public IEnumerator SetupSupplyPoints()
        {
            yield return null;

            Debug.Log("Supply Raid: Found Supply Points - " + tnhManager.SupplyPoints.Count);

            Random.InitState(tnhManager.m_seed);

            //Collection of valid positions for everything
            List<Transform> validPanels = new List<Transform>();
            List<Transform> validSosigPoints = new List<Transform>();
            List<Transform> validPatrolPoints = new List<Transform>();

            //Supply Point for each supply point
            for (int x = 0; x < tnhManager.SupplyPoints.Count; x++)
            {
                TNH_SupplyPoint tnhSP = tnhManager.SupplyPoints[x];

                //Valid Content
                validPanels.Clear();
                validSosigPoints.Clear();
                validPatrolPoints.Clear();

                //Panel Positions
                validPanels.AddRange(tnhSP.SpawnPoints_Panels);
                validPanels.AddRange(tnhSP.SpawnPoint_Tables);
                validPanels.Add(tnhSP.SpawnPoint_PlayerSpawn);
                validPanels.AddRange(tnhSP.SpawnPoints_Boxes);
                validPanels.AddRange(tnhSP.SpawnPoints_Sosigs_Defense);
                validPanels.AddRange(tnhSP.SpawnPoints_Turrets);

                //Sosig Postisons
                validSosigPoints.Add(tnhSP.SpawnPoint_PlayerSpawn);
                validSosigPoints.AddRange(tnhSP.SpawnPoints_Boxes);
                validSosigPoints.AddRange(tnhSP.SpawnPoints_Sosigs_Defense);
                validSosigPoints.AddRange(tnhSP.SpawnPoints_Turrets);
                validSosigPoints.AddRange(tnhSP.SpawnPoints_Panels);

                for (int i = 0; i < tnhSP.CoverPoints.Count; i++)
                {
                    validSosigPoints.Add(tnhSP.CoverPoints[i].transform);
                }

                for (int i = validSosigPoints.Count - 1; i >= 0; i--)
                {
                    if (validSosigPoints[i] == null)
                        validSosigPoints.RemoveAt(i);
                }
                //------------------------

                SR_SupplyPoint sp = Instantiate(new GameObject()).AddComponent<SR_SupplyPoint>();
                sp.name = "SupplyPoint_" + x;

                if (tnhSP.GetComponent<AtlasSupplyPoint>())
                {
                    Debug.Log("Supply Raid: Found First Spawn Atlas");
                    if (tnhSP.GetComponent<AtlasSupplyPoint>().ForceSpawnHere)
                        sp.forceFirstSpawn = true;
                }
                else if (tnhSP.GetComponent("WurstMod.MappingComponents.TakeAndHold.ForcedSpawn"))
                {
                    Debug.Log("Supply Raid: Found First Spawn Wurst");
                    sp.forceFirstSpawn = true;
                }

                /*
                if (sp.forceFirstSpawn)
                    srManager.forceStaticPlayerSupplyPoint = true;
                */

                //Key Points
                sp.respawn = tnhSP.SpawnPoint_PlayerSpawn;
                sp.captureZone = tnhSP.Bounds;
                sp.squadPoint = tnhSP.SpawnPoint_PlayerSpawn;

                //Capture Time based on space
                float largestSize = Mathf.Max(sp.captureZone.localScale.x, Mathf.Max(sp.captureZone.localScale.y, sp.captureZone.localScale.z)) * 1.5f;
                sp.captureTime = Mathf.CeilToInt(Mathf.Clamp((sp.captureTime < largestSize ? largestSize : sp.captureTime), 20, 45));

                //Panels
                sp.buyMenu = TryGetLocation(validPanels, 0);
                sp.buyMenu.transform.Rotate(0,90,0);

                sp.ammoStation = TryGetLocation(validPanels, 3);
                sp.ammoStation.transform.Rotate(0, 90, 0);

                sp.attachmentStation = TryGetLocation(validPanels, 1);
                sp.attachmentStation.transform.Rotate(0, 90, 0);

                sp.recycler = TryGetLocation(validPanels, 2);
                sp.recycler.transform.Rotate(0, 90, 0);

                sp.duplicator = TryGetLocation(validPanels, 4);
                sp.duplicator.transform.Rotate(0, 90, 0);

                //Sosig Spawn points

                //Rabbit Holes
                sp.sosigSpawns = new Transform[4]; ;
                for (int z = 0; z < sp.sosigSpawns.Length; z++)
                {
                    sp.sosigSpawns[z] = TryGetLocation(validSosigPoints, z);
                }

                //Snipers - Use Turret spawns else defense sosigs spawns
                sp.sniperPoints.AddRange(validSosigPoints);

                //Guards - Use Box spawns
                sp.guardPoints.AddRange(validSosigPoints);

                //Patrols
                PatrolPath[] patrols = new PatrolPath[2];

                //Generate Custom Paths

                for (int i = 0; i < 8; i++)
                {
                    Transform spot = new GameObject().transform;
                    validPatrolPoints.Add(spot);
                    spot.position = sp.captureZone.position;

                    spot.position += new Vector3(
                        Random.Range(-spawnHalfExtent, spawnHalfExtent),
                        Random.Range(-spawnHalfExtent, spawnHalfExtent),
                        Random.Range(-spawnHalfExtent, spawnHalfExtent));

                    UnityEngine.AI.NavMeshHit hit;
                    if (UnityEngine.AI.NavMesh.SamplePosition(spot.position, out hit, navRange, UnityEngine.AI.NavMesh.AllAreas))
                    {
                        spot.position = hit.position;
                    }
                }

                for (int z = 0; z < patrols.Length; z++)
                {
                    patrols[z] = new PatrolPath();
                    patrols[z].patrolPoints.AddRange(validPatrolPoints);

                    //Reverse order after each list
                    validPatrolPoints.Reverse();
                }
                sp.patrolPaths = patrols;
            }
        }

        public IEnumerator SetupSupplyPointsHold()
        {
            if (tnhManager == null || tnhManager.HoldPoints == null)
                yield break;

            Debug.Log("Supply Raid: Found Hold Points - " + tnhManager.HoldPoints.Count);

            //Collection of valid positions for everything
            List<Transform> validPanels = new List<Transform>();
            List<Transform> validSosigPoints = new List<Transform>();
            List<Transform> validPatrolPoints = new List<Transform>();


            //Supply Point for each supply point
            for (int x = 0; x < tnhManager.HoldPoints.Count; x++)
            {
                TNH_HoldPoint tnhHP = tnhManager.HoldPoints[x];

                if (tnhHP == null)
                    continue;

                //Valid Content
                validPanels.Clear();
                validSosigPoints.Clear();

                //Panel Positions --
                if(tnhHP.m_systemNode != null && tnhHP.m_systemNode.NodeCenter != null)
                    validPanels.Add(tnhHP.m_systemNode.NodeCenter);


                for (int i = 0; i < tnhHP.CoverPoints.Count; i++)
                {
                    //Cover barriers
                    if(tnhHP.CoverPoints[i] != null && tnhHP.CoverPoints[i].transform != null)
                        validPanels.Add(tnhHP.CoverPoints[i].transform);
                }

                validPanels.AddRange(tnhHP.SpawnPoints_Sosigs_Defense);
                validPanels.AddRange(tnhHP.SpawnPoints_Turrets);

                //Sosig Postisons --
                for (int y = 0; y < tnhHP.AttackVectors.Count; y++)
                {
                    if (tnhHP.AttackVectors[y] != null)
                    {
                        if(tnhHP.AttackVectors[y].SpawnPoints_Sosigs_Attack.Count > 0)
                            validSosigPoints.AddRange(tnhHP.AttackVectors[y].SpawnPoints_Sosigs_Attack);
                        if(tnhHP.AttackVectors[y].GrenadeVector != null)
                            validSosigPoints.Add(tnhHP.AttackVectors[y].GrenadeVector);
                    }
                }

                validSosigPoints.AddRange(tnhHP.SpawnPoints_Sosigs_Defense);
                validSosigPoints.AddRange(tnhHP.SpawnPoints_Turrets);

                for (int i = 0; i < tnhHP.CoverPoints.Count; i++)
                {
                    if (tnhHP.CoverPoints[i] != null && tnhHP.CoverPoints[i].transform != null)
                        validSosigPoints.Add(tnhHP.CoverPoints[i].transform);
                }

                if (tnhHP.m_systemNode != null && tnhHP.m_systemNode.NodeCenter != null)
                    validSosigPoints.Add(tnhHP.m_systemNode.NodeCenter);

                for (int i = validSosigPoints.Count - 1; i >= 0; i--)
                {
                    if (validSosigPoints[i] == null)
                        validSosigPoints.RemoveAt(i);
                }
                //------------------------

                SR_SupplyPoint sp = Instantiate(new GameObject()).AddComponent<SR_SupplyPoint>();
                sp.name = "SupplyPoint_" + x;

                AtlasSupplyPoint ASP = tnhHP.GetComponent<AtlasSupplyPoint>();
                if (ASP != null)
                {
                    Debug.Log("Supply Raid: Found First Spawn Atlas");
                    if (ASP.ForceSpawnHere)
                        sp.forceFirstSpawn = true;
                }
                else if (tnhHP.GetComponent("WurstMod.MappingComponents.TakeAndHold.ForcedSpawn"))
                {
                    Debug.Log("Supply Raid: Found First Spawn Wurst");
                    sp.forceFirstSpawn = true;
                }

                //Key Points
                if (tnhHP.m_systemNode != null && tnhHP.m_systemNode.NodeCenter != null)
                    sp.respawn = tnhHP.m_systemNode.NodeCenter;
                else
                    sp.respawn = validSosigPoints[Random.Range(0, validSosigPoints.Count)];

                //Use Level Bounds
                Transform bounds = null;
                Transform boundsBiggest = null;
                Vector3 boundsScale = Vector3.zero;
                for (int i = 0; i < tnhHP.Bounds.Count; i++)
                {
                    if (tnhHP.Bounds[i].localScale.x > boundsScale.x
                        || tnhHP.Bounds[i].localScale.y > boundsScale.y
                        || tnhHP.Bounds[i].localScale.z > boundsScale.z)
                    {
                        boundsBiggest = tnhHP.Bounds[i];

                        if (tnhHP.Bounds[i].localScale.x <= maxBoundsSize
                            || tnhHP.Bounds[i].localScale.y <= maxBoundsSize
                            || tnhHP.Bounds[i].localScale.z <= maxBoundsSize)
                        {
                            bounds = tnhHP.Bounds[i];
                        }
                    }
                }

                if (bounds == null)
                {
                    bounds = Instantiate(new GameObject()).transform;
                    bounds.localScale = Vector3.one * maxBoundsSize;

                    if (tnhHP.m_systemNode != null && tnhHP.m_systemNode.NodeCenter != null)
                        bounds.position = tnhHP.m_systemNode.NodeCenter.position;
                    else
                    {
                        if (tnhHP.Bounds.Count > 0)
                        {
                            UnityEngine.AI.NavMeshHit hit;
                            if (UnityEngine.AI.NavMesh.SamplePosition(tnhHP.Bounds[0].position, out hit, navRange, UnityEngine.AI.NavMesh.AllAreas))
                            {
                                bounds.position = hit.position;
                            }
                        }
                        else
                            bounds.position = tnhHP.transform.position;
                    }
                }

                sp.captureZone = bounds;
                sp.squadPoint = sp.respawn;

                //Capture Time based on space
                float largestSize = Mathf.Max(bounds.localScale.x, Mathf.Max(bounds.localScale.y, bounds.localScale.z)) * 1.5f;
                sp.captureTime = Mathf.CeilToInt(Mathf.Clamp((sp.captureTime < largestSize ? largestSize : sp.captureTime), 20, 45));

                //Panels
                sp.buyMenu = TryGetLocation(validPanels, 0);
                sp.buyMenu.transform.Rotate(0, 90, 0);

                sp.ammoStation = TryGetLocation(validPanels, 3);
                sp.ammoStation.transform.Rotate(0, 90, 0);

                sp.attachmentStation = TryGetLocation(validPanels, 1);
                sp.attachmentStation.transform.Rotate(0, 90, 0);

                sp.recycler = TryGetLocation(validPanels, 2);
                sp.recycler.transform.Rotate(0, 90, 0);

                sp.duplicator = TryGetLocation(validPanels, 4);
                sp.duplicator.transform.Rotate(0, 90, 0);

                //Sosig Spawn points

                //Rabbit Holes
                sp.sosigSpawns = new Transform[4]; ;
                for (int z = 0; z < sp.sosigSpawns.Length; z++)
                {
                    sp.sosigSpawns[z] = TryGetLocation(validSosigPoints, z);
                }

                //Snipers - Use Turret spawns else defense sosigs spawns
                sp.sniperPoints.AddRange(validSosigPoints);

                //Guards - Use Box spawns
                sp.guardPoints.AddRange(validSosigPoints);

                //Patrols
                PatrolPath[] patrols = new PatrolPath[2];

                //Generate Custom Paths

                for (int i = 0; i < 8; i++)
                {
                    Transform spot = new GameObject().transform;
                    validPatrolPoints.Add(spot);
                    spot.position = sp.captureZone.position;

                    spot.position += new Vector3(
                        Random.Range(-spawnHalfExtent, spawnHalfExtent),
                        Random.Range(-spawnHalfExtent, spawnHalfExtent) * 0.5f,
                        Random.Range(-spawnHalfExtent, spawnHalfExtent));

                    UnityEngine.AI.NavMeshHit hit;
                    if (UnityEngine.AI.NavMesh.SamplePosition(spot.position, out hit, navRange, UnityEngine.AI.NavMesh.AllAreas))
                    {
                        spot.position = hit.position;
                    }
                }

                for (int z = 0; z < patrols.Length; z++)
                {
                    patrols[z] = new PatrolPath();
                    patrols[z].patrolPoints.AddRange(validPatrolPoints);

                    //Reverse order after each list
                    validPatrolPoints.Reverse();
                }
                sp.patrolPaths = patrols;

                yield return null;
            }
        }
    }
}