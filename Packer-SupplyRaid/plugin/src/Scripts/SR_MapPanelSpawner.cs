using System.Collections;
using UnityEngine;

namespace SupplyRaid
{
    public class SR_MapPanelSpawner : MonoBehaviour
    {
        public Transform spawnPoint;
        public SpawnTimeEnum spawnTime = SpawnTimeEnum.OnStart;
        private GameObject mapSelector;

        public enum SpawnTimeEnum
        {
            None,
            OnAwake,
            OnStart,
        }

        public void Awake()
        {
            if (spawnTime == SpawnTimeEnum.OnAwake)
                SpawnMenu();
        }

        public void Start()
        {
            if (spawnTime == SpawnTimeEnum.OnStart)
                SpawnMenu();
        }

        public void SpawnMenu()
        {
            //Spawn our map selector
            StartCoroutine(AssignMenu());
        }

        public IEnumerator AssignMenu()
        {
            if (spawnPoint == null)
            {
                Debug.LogWarning(name + " is missing spawn point!");
                yield break;
            }

            yield return StartCoroutine(SupplyRaidPlugin.CreateMapMenu(spawnPoint.position, spawnPoint.rotation.eulerAngles));

            if (SupplyRaidPlugin.mapSelector != null)
            {
                mapSelector = SupplyRaidPlugin.mapSelector;
                mapSelector.transform.SetParent(spawnPoint);
                SupplyRaidPlugin.mapSelector = null;
            }
        }
    }
}