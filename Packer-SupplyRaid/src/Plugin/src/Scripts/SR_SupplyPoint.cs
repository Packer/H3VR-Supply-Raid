using FistVR;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SupplyRaid
{
	public class SR_SupplyPoint : MonoBehaviour
    {
        [HideInInspector]
        public int index = -1;

        [Tooltip("Force this supply point to be the first spawn in position")]
        public bool forceFirstSpawn = false;

        [Tooltip("The time it takes to capture this point")]
        public int captureTime = 20;

        [Header("Panels")]
        [Tooltip("Buy Menu spawn point when captured")]
        public Transform buyMenu;
        [Tooltip("Ammo Station spawn point when captured")]
        public Transform ammoStation;
        [Tooltip("Recycler spawn point when captured")]
        public Transform recycler;
        [Tooltip("Magazine Duplicator spawn point when captured")]
        public Transform duplicator;
        [Tooltip("Attachment Table spawn point when captured")]
        public Transform attachmentStation;

        [Header("Gameplay")]

        [Tooltip("NOT IMPLEMENTED: Used for determining what the next supply point might be.")]
        public SupplySizeEnum supplySize = SupplySizeEnum.Medium;

        [Tooltip("Enforces the next supply point to be this Next Supply Point")]
        public SR_SupplyPoint nextSupplyPoint;
        [Tooltip("Enforces that this Supply Point cannot be selected unless the Previous Supply Point has been captured")]
        public SR_SupplyPoint previousSupplyPoint;

        [Tooltip("Player Spawn/Respawn point")]
        public Transform respawn;
        [Tooltip("The capture zone transform, affected by scale")]
        public Transform captureZone;
        [Tooltip("The Sosig rabbit hole spawn points, should be hidden from")]
        public Transform[] sosigSpawns;
        [Tooltip("How far a player needs to be for a sosig spawn to work")]
        public float playerNearby = 6;

        [Header("Sosig Waypoints")]
        public float spawnRadius = 1f;
        [Tooltip("Squad waypoint when going to this supply point")]
        public Transform squadPoint;
        [Tooltip("Guard spawn points, they will move during combat/alerted")]
        public List<Transform> guardPoints = new List<Transform>();
        [Tooltip("Snipers never move, this points should be in overwatch points")]
        public List<Transform> sniperPoints = new List<Transform>();
        [Tooltip("Groups of patrol paths")]
        public PatrolPath[] patrolPaths;

        //[Header("Game Objects")]
        //[Tooltip("List of gameobjects that get enabled when this supply point is active, then disabled when not active")]
        //public GameObject[] activeObjects;

        private RaycastHit rayHit = new RaycastHit();

        void Awake()
        {
            //CleanSupplyPoint();
        }

        void Start()
        {
            if (SR_Manager.instance != null)
                index = SR_Manager.AddSupplyPoint(this);
        }

        void OnEnable()
        {
            if(SR_Manager.instance != null)
                index = SR_Manager.AddSupplyPoint(this);
        }

        void OnDisable()
        {
            if (SR_Manager.instance != null)
                SR_Manager.RemoveSupplyPoint(this);
        }

        /// <summary>
        /// Fixes and removes any doubleups
        /// </summary>
        public void CleanSupplyPoint()
        {
            List<Transform> cleanList = guardPoints.Distinct().ToList();
            guardPoints = cleanList;

            cleanList.Clear();

            cleanList = sniperPoints.Distinct().ToList();
            sniperPoints = cleanList;

        }

        /*
        public void SetActiveSupplyPoint(bool state)
        {
            for (int i = 0; i < activeObjects.Length; i++)
            {
                if (activeObjects[i] != null)
                    activeObjects[i].SetActive(state);
            }
        }
        */
        public Transform GetRandomSniperSpawn()
        {
            return sniperPoints[Random.Range(0, sniperPoints.Count - 1)];
        }

        public Transform GetBossSpawn()
        {
            return squadPoint;
        }

        public Transform GetRandomGuardSpawn()
        {
            int guardCount = guardPoints.Count;

            return guardPoints[Random.Range(0, guardCount)];
        }

        public PatrolPath GetRandomPatrolPath()
		{
			return patrolPaths[Random.Range(0, patrolPaths.Length)];
        }

        public Transform GetRandomSosigSpawn()
        {
            return sosigSpawns[Random.Range(0 , sosigSpawns.Length)];
        }

        public void PlaceAllSosigsOnGround()
        {
            //Patrol Paths
            for (int x = 0; x < patrolPaths.Length; x++)
            {
                for (int y = 0; y < patrolPaths[x].patrolPoints.Count; y++)
                {
                    if(Physics.Raycast(patrolPaths[x].patrolPoints[y].position, Vector3.down, out rayHit, 200f))
                    {
                        patrolPaths[x].patrolPoints[y].position = rayHit.point + (Vector3.up * 0.05f);
                    }
                }
            }

            //Guards
            for (int i = 0; i < guardPoints.Count; i++)
            {
                if (Physics.Raycast(guardPoints[i].position, Vector3.down, out rayHit, 200f))
                {
                    guardPoints[i].position = rayHit.point + (Vector3.up * 0.05f);
                }
            }

            //Sniper
            for (int i = 0; i < sniperPoints.Count; i++)
            {
                if (Physics.Raycast(sniperPoints[i].position, Vector3.down, out rayHit, 200f))
                {
                    sniperPoints[i].position = rayHit.point + (Vector3.up * 0.05f);
                }
            }

            //Rabbit Holes
            for (int i = 0; i < sosigSpawns.Length; i++)
            {
                if (Physics.Raycast(sosigSpawns[i].position, Vector3.down, out rayHit, 200f))
                {
                    sosigSpawns[i].position = rayHit.point + (Vector3.up * 0.05f);
                }
            }

            //Squad Waypoint
            if (Physics.Raycast(squadPoint.position, Vector3.down, out rayHit, 200f))
            {
                squadPoint.position = rayHit.point + (Vector3.up * 0.05f);
            }

            //Respawn
            if (Physics.Raycast(respawn.position, Vector3.down, out rayHit, 200f))
            {
                respawn.position = rayHit.point + (Vector3.up * 0.05f);
            }

        }

		void OnDrawGizmos()
        {
            if (previousSupplyPoint)
            {
                Vector3 arrowPos = Vector3.Lerp(transform.position, previousSupplyPoint.transform.position, 0.05f);
                Debug.DrawLine(transform.position, arrowPos + Vector3.up, Color.yellow);
                Debug.DrawLine(transform.position, arrowPos - Vector3.up, Color.yellow);

                Debug.DrawLine(transform.position, previousSupplyPoint.transform.position, Color.yellow);
            }

            Vector3 spawnSize;

            if (sosigSpawns != null)
            {
                for (int i = 0; i < sosigSpawns.Length; i++)
                {
                    Gizmos.color = new Color(1f, 0.5f, 0, 1f);
                    Gizmos.DrawLine(sosigSpawns[i].position, sosigSpawns[i].position + sosigSpawns[i].forward + Vector3.up * 0.125f);

                    Gizmos.DrawSphere(sosigSpawns[i].position + (Vector3.up * 0.125f), 0.25f);
                    Gizmos.DrawSphere(sosigSpawns[i].position + Vector3.up, 0.25f);
                    Gizmos.DrawSphere(sosigSpawns[i].position + (Vector3.up * 1.75f), 0.25f);
                }
            }

            #region Sosig Waypoints
            //Squad Waypoint
            if (squadPoint != null)
            {
                Gizmos.matrix = Matrix4x4.identity;
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(squadPoint.position + Vector3.up, squadPoint.position + Vector3.up + squadPoint.forward);

                Gizmos.DrawSphere(squadPoint.position + (Vector3.up * 0.125f), 0.25f);
                Gizmos.DrawSphere(squadPoint.position + Vector3.up, 0.25f);
                Gizmos.DrawSphere(squadPoint.position + (Vector3.up * 1.75f), 0.25f);

                spawnSize = new Vector3(squadPoint.localScale.x * spawnRadius, 0.1f, squadPoint.localScale.z * spawnRadius);
                Gizmos.matrix = Matrix4x4.TRS(squadPoint.position, Quaternion.identity, spawnSize);
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }

            //Guards
            if (guardPoints != null && guardPoints.Count > 0)
            {
                for (int i = 0; i < guardPoints.Count; i++)
                {
                    Gizmos.matrix = Matrix4x4.identity;
                    Gizmos.color = new Color(1f, 0.1f, 0.1f, 1f);
                    Gizmos.DrawLine(guardPoints[i].position + Vector3.up, guardPoints[i].position + Vector3.up + guardPoints[i].forward);

                    Gizmos.DrawSphere(guardPoints[i].position + (Vector3.up * 0.125f), 0.25f);
                    Gizmos.DrawSphere(guardPoints[i].position + Vector3.up, 0.25f);
                    Gizmos.DrawSphere(guardPoints[i].position + (Vector3.up * 1.75f), 0.25f);

                    spawnSize = new Vector3(guardPoints[i].lossyScale.x * spawnRadius, 0.1f, guardPoints[i].lossyScale.z * spawnRadius);
                    Gizmos.matrix = Matrix4x4.TRS(guardPoints[i].position, Quaternion.identity, spawnSize);
                    Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                }
            }

            //Snipers
            if (sniperPoints != null && sniperPoints.Count > 0)
            {
                for (int i = 0; i < sniperPoints.Count; i++)
                {
                    Gizmos.matrix = Matrix4x4.identity;

                    Gizmos.color = new Color(0.1f, 0.1f, 1f, 1f);
                    Gizmos.DrawLine(sniperPoints[i].position + (Vector3.up * 1.75f), sniperPoints[i].position + (Vector3.up * 1.75f) + (sniperPoints[i].forward * 3));

                    Gizmos.DrawSphere(sniperPoints[i].position + (Vector3.up * 0.125f), 0.25f);
                    Gizmos.DrawSphere(sniperPoints[i].position + Vector3.up, 0.25f);
                    Gizmos.DrawSphere(sniperPoints[i].position + (Vector3.up * 1.75f), 0.25f);

                    spawnSize = new Vector3(sniperPoints[i].lossyScale.x * spawnRadius, 0.1f, sniperPoints[i].lossyScale.z * spawnRadius);
                    Gizmos.matrix = Matrix4x4.TRS(sniperPoints[i].position, Quaternion.identity, spawnSize);
                    Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                }
            }


            for (int i = 0; i < patrolPaths.Length; i++)
            {
                Gizmos.color = new Color(1f, 0.5f, 0, 1f);

                for (int x = 0; x < patrolPaths[i].patrolPoints.Count; x++)
                {
                    if (patrolPaths[i].patrolPoints[x] == null)
                        continue;

                    Gizmos.matrix = Matrix4x4.identity;

                    spawnSize = new Vector3(patrolPaths[i].patrolPoints[x].lossyScale.x, 
                        0.1f,
                        patrolPaths[i].patrolPoints[x].lossyScale.z);

                    Gizmos.DrawWireCube(patrolPaths[i].patrolPoints[x].position, spawnSize);
                    Gizmos.DrawLine(
                        patrolPaths[i].patrolPoints[x].position + Vector3.up * 0.1f,
                        patrolPaths[i].patrolPoints[x].position + (Vector3.up * 0.1f) + patrolPaths[i].patrolPoints[x].forward);

                    if (x >= patrolPaths[i].patrolPoints.Count - 1)
                        Gizmos.DrawLine(patrolPaths[i].patrolPoints[x].position, patrolPaths[i].patrolPoints[0].position);
                    else
                        Gizmos.DrawLine(patrolPaths[i].patrolPoints[x].position, patrolPaths[i].patrolPoints[x + 1].position);
                }
            }
            #endregion

            //MATRIX

            if (captureZone != null && captureZone.gameObject.activeSelf == true)
            {
                Gizmos.matrix = Matrix4x4.TRS(captureZone.position, captureZone.rotation, captureZone.lossyScale);
                Gizmos.color = new Color(1f, 0, 1f, 0.25f);
                Gizmos.DrawSphere(Vector4.zero, 0.1f);
                Gizmos.DrawCube(Vector3.zero, Vector3.one);
                Gizmos.color = new Color(1f, 0, 1f, 1f);
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }

            if (respawn != null)
            {
                Gizmos.matrix = Matrix4x4.TRS(respawn.position, respawn.rotation, Vector3.one);
                Gizmos.color = new Color(.1f, 1f, .1f, 1f);
                Gizmos.DrawWireCube(Vector3.up, Vector3.one + Vector3.up);
                Gizmos.DrawLine(Vector3.up, Vector3.up + Vector3.forward);
            }

            #region Panels
            if (buyMenu != null)
            {
                Gizmos.color = new Color(0, 0.1f, 1f, 0.75f);
                Gizmos.matrix = Matrix4x4.TRS(buyMenu.position, buyMenu.rotation, buyMenu.lossyScale);

                Vector3 pos = new Vector3(-0.25f, 0.435f, 0);
				Vector3 size = new Vector3(0.5f, 0.825f, 1.275f);
                Gizmos.DrawCube(pos, size);

                pos = new Vector3(-0.025f, 1.48f, 0);
                size = new Vector3(0.05f, 1.275f, 1.275f);
                Gizmos.DrawCube(pos, size);
            }

            if (ammoStation != null)
            {
                Gizmos.color = new Color(0, 1, 0.1f, 0.75f);
                Gizmos.matrix = Matrix4x4.TRS(ammoStation.position, ammoStation.rotation, ammoStation.lossyScale);

                Vector3 pos = new Vector3(-0.25f, 0.435f, 0);
                Vector3 size = new Vector3(0.5f, 0.8f, 1.2f);
                Gizmos.DrawCube(pos, size);

                pos = new Vector3(-0.025f, 1.25f, 0);
                size = new Vector3(0.05f, 0.8f, 1.2f);
                Gizmos.DrawCube(pos, size);
            }

            if (recycler != null)
            {
                Gizmos.color = new Color(0.1f, 0.5f, 0.5f, 0.75f);
                Gizmos.matrix = Matrix4x4.TRS(recycler.position, recycler.rotation, recycler.lossyScale);

                Vector3 pos = new Vector3(-0.25f, 0.435f, 0);
                Vector3 size = new Vector3(0.5f, 0.8f, 0.8f);
                Gizmos.DrawCube(pos, size);

                pos = new Vector3(-0.025f, 1.25f, 0);
                size = new Vector3(0.05f, 0.8f, 0.8f);
                Gizmos.DrawCube(pos, size);
            }

            if (duplicator != null)
            {
                Gizmos.color = new Color(0.5f, 0, 1f, 0.75f);
                Gizmos.matrix = Matrix4x4.TRS(duplicator.position, duplicator.rotation, duplicator.lossyScale);

                Vector3 pos = new Vector3(-0.25f, 0.435f, 0);
                Vector3 size = new Vector3(0.5f, 0.8f, 0.8f);
                Gizmos.DrawCube(pos, size);

                pos = new Vector3(-0.025f, 1.25f, 0);
                size = new Vector3(0.05f, 0.8f, 0.8f);
                Gizmos.DrawCube(pos, size);
            }

            if (attachmentStation != null)
            {
                Gizmos.color = new Color(1f, 0.25f, 0.25f, 0.75f);
                Gizmos.matrix = Matrix4x4.TRS(attachmentStation.position, attachmentStation.rotation, attachmentStation.lossyScale);

                Vector3 pos = new Vector3(-0.25f, 0.435f, 0);
                Vector3 size = new Vector3(0.5f, 0.8f, 1.6f);
                Gizmos.DrawCube(pos, size);

                pos = new Vector3(-0.025f, 1.25f, 0);
                size = new Vector3(0.05f, 0.8f, 1.6f);
                Gizmos.DrawCube(pos, size);
            }
            
            #endregion
        }
    }

	[System.Serializable]
	public class PatrolPath
	{
		public List<Transform> patrolPoints = new List<Transform>();

		public int GetRandomPoint()
		{
			if (patrolPoints.Count <= 0)
				return -1;

			return Random.Range(0, patrolPoints.Count);
		}

		public List<Vector3> GetPathPositionList()
		{
            List<Vector3> newList =  new List<Vector3>();
			foreach (Transform item in patrolPoints)
			{
				newList.Add(item.position);

            }
			return newList;
        }

        public List<Vector3> GetPathRotationList()
        {
            List<Vector3> newList = new List<Vector3>();
            foreach (Transform item in patrolPoints)
            {
                newList.Add(item.rotation.eulerAngles);

            }
            return newList;
        }
    }

    /*
    public enum PatrolTypeEnum
    {
        Patrol = 0, //patrols waypoints
        Guard = 1,  //Holds position
        Sniper = 2, //Holds Position - Prioritises sniper sosigs
        AlertGuard = 3,  //Same as Guard - Goes to Alert Position
    }
    */

    public enum SupplySizeEnum
    {
        Small = 0,  //Campfire / Outside Area
        Medium = 1, //Small depot / Single Building
        Large = 2,  //Military Base / Town
    }
}