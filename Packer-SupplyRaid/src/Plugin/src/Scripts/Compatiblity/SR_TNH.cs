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
              

            //Generate Our Supply Raid Content
            Instantiate(new GameObject()).AddComponent<SR_ModLoader>();

            yield return null;

            SetupManager();

            yield return null;

            //Generate Supply Points then load our assets
            yield return StartCoroutine(SetupSupplyPoints());

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

            Random.InitState(tnhManager.m_seed);

            //Collection of valid positions for everything
            List<Transform> validPanels = new List<Transform>();
            List<Transform> validSosigPoints = new List<Transform>();

            //Supply Point for each supply point
            for (int x = 0; x < tnhManager.SupplyPoints.Count; x++)
            {
                TNH_SupplyPoint tnhSP = tnhManager.SupplyPoints[x];

                //Valid Content
                validPanels.Clear();
                validSosigPoints.Clear();

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

                //Panels
                sp.buyMenu = TryGetLocation(validPanels, 0);
                sp.buyMenu.transform.Rotate(0,90,0);

                sp.attachmentStation = TryGetLocation(validPanels, 1);
                sp.attachmentStation.transform.Rotate(0, 90, 0);

                sp.recycler = TryGetLocation(validPanels, 2);
                sp.recycler.transform.Rotate(0, 90, 0);

                sp.ammoStation = TryGetLocation(validPanels, 3);
                sp.ammoStation.transform.Rotate(0, 90, 0);

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
                                
                for (int z = 0; z < patrols.Length; z++)
                {
                    patrols[z] = new PatrolPath();
                    patrols[z].patrolPoints.AddRange(validSosigPoints);

                    //Reverse order after each list
                    validSosigPoints.Reverse();
                }
                sp.patrolPaths = patrols;
            }
        }
    }
}