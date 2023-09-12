using FistVR;
using System.Collections.Generic;
using UnityEngine;

namespace SupplyRaid
{
	public class SR_SupplyPoint : MonoBehaviour
    {
        [Tooltip("Force this supply point to be the first spawn in position")]
        public bool forceFirstSpawn = false;

        [Tooltip("The time it takes to capture this point")]
        public int captureTime = 20;

        [Header("Panels")]
        public Transform buyMenu;
        public Transform ammoStation;
        public Transform recycler;
        public Transform duplicator;
        //public Transform itemSpawner;

        [Header("Gameplay")]

        [Tooltip("NOT IMPLEMENTED: Used for determining what the next supply point might be.")]
        public SupplySizeEnum supplySize = SupplySizeEnum.Medium;

        public Transform respawn;
        public Transform captureZone;
        public Transform[] sosigSpawns;

        [Header("Sosig Waypoints")]
        public Transform squadPoint;
        public Transform[] guardPoints;
        public Transform[] sniperPoints;
        public PatrolPath[] patrolPaths;


        private RaycastHit rayHit = new RaycastHit();

        void Start()
        {
            SR_Manager.AddSupplyPoint(this);
        }

        void OnEnable()
        {
            if(SR_Manager.instance != null)
                SR_Manager.AddSupplyPoint(this);
        }

        void OnDisable()
        {
            if (SR_Manager.instance != null)
                SR_Manager.RemoveSupplyPoint(this);
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
            for (int i = 0; i < guardPoints.Length; i++)
            {
                if (Physics.Raycast(guardPoints[i].position, Vector3.down, out rayHit, 200f))
                {
                    guardPoints[i].position = rayHit.point + (Vector3.up * 0.05f);
                }
            }

            //Sniper
            for (int i = 0; i < sniperPoints.Length; i++)
            {
                if (Physics.Raycast(sniperPoints[i].position, Vector3.down, out rayHit, 200f))
                {
                    sniperPoints[i].position = rayHit.point + (Vector3.up * 0.05f);
                }
            }
        }

		void OnDrawGizmos()
        {
            Vector3 spawnSize;

            if (sosigSpawns != null)
            {
                for (int i = 0; i < sosigSpawns.Length; i++)
                {
                    Gizmos.color = new Color(1f, 0.5f, 0, 1f);
                    Gizmos.DrawLine(sosigSpawns[i].position, sosigSpawns[i].position + sosigSpawns[i].forward);
                    Gizmos.DrawSphere(sosigSpawns[i].position, 0.25f);
                }
            }

            //Squad Waypoint
            if (squadPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(squadPoint.position + Vector3.up, squadPoint.position + Vector3.up + squadPoint.forward);

                Gizmos.DrawSphere(squadPoint.position + (Vector3.up * 0.125f), 0.25f);
                Gizmos.DrawSphere(squadPoint.position + Vector3.up, 0.25f);
                Gizmos.DrawSphere(squadPoint.position + (Vector3.up * 1.75f), 0.25f);

                spawnSize = new Vector3(squadPoint.localScale.x, 0.1f, squadPoint.localScale.z);
                Gizmos.DrawWireCube(squadPoint.position, spawnSize);
            }

            //Guards
            if (guardPoints != null && guardPoints.Length > 0)
            {
                for (int i = 0; i < guardPoints.Length; i++)
                {
                    Gizmos.color = new Color(1f, 0.1f, 0.1f, 1f);
                    Gizmos.DrawLine(guardPoints[i].position + Vector3.up, guardPoints[i].position + Vector3.up + guardPoints[i].forward);

                    Gizmos.DrawSphere(guardPoints[i].position + (Vector3.up * 0.125f), 0.25f);
                    Gizmos.DrawSphere(guardPoints[i].position + Vector3.up, 0.25f);
                    Gizmos.DrawSphere(guardPoints[i].position + (Vector3.up * 1.75f), 0.25f);
                }
            }

            //Snipers
            if (sniperPoints != null && sniperPoints.Length > 0)
            {
                for (int i = 0; i < sniperPoints.Length; i++)
                {
                    Gizmos.color = new Color(0.1f, 0.1f, 1f, 1f);
                    Gizmos.DrawLine(sniperPoints[i].position + (Vector3.up * 1.75f), sniperPoints[i].position + (Vector3.up * 1.75f) + (sniperPoints[i].forward * 3));

                    Gizmos.DrawSphere(sniperPoints[i].position + (Vector3.up * 0.125f), 0.25f);
                    Gizmos.DrawSphere(sniperPoints[i].position + Vector3.up, 0.25f);
                    Gizmos.DrawSphere(sniperPoints[i].position + (Vector3.up * 1.75f), 0.25f);
                }
            }

            spawnSize = new Vector3(3, 0.1f, 3);

            for (int i = 0; i < patrolPaths.Length; i++)
            {
                Gizmos.color = new Color(1f, 0.5f, 0, 1f);

                for (int x = 0; x < patrolPaths[i].patrolPoints.Count; x++)
                {
                    if (patrolPaths[i].patrolPoints[x] == null)
                        continue;

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

            if (captureZone != null)
            {
                Gizmos.color = new Color(1f, 0, 1f, 0.25f);
                Gizmos.DrawSphere(captureZone.position, 0.25f);
                Gizmos.DrawCube(captureZone.position, captureZone.localScale);
                Gizmos.color = new Color(1f, 0, 1f, 1f);
                Gizmos.DrawWireCube(captureZone.position, captureZone.localScale);
            }

            //Matrix Manipulated Below

            /*
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
            */

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
                Vector3 size = new Vector3(0.5f, 0.8f, 0.8f);
                Gizmos.DrawCube(pos, size);

                pos = new Vector3(-0.025f, 1.25f, 0);
                size = new Vector3(0.05f, 0.8f, 0.8f);
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