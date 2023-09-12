using FistVR;
using UnityEngine;

namespace SupplyRaid
{

	public class SR_SpawnStation : MonoBehaviour
	{
		private bool countDown = false;
		private float timeRemain = 60;

        [SerializeField] GameObject spawnButton;
        [Header("Spawn Points")]
        [SerializeField] Transform mainSpawn;
        [SerializeField] Transform ammoSpawn;
        [SerializeField] Transform[] attachmentSpawn;   //3 spawn points

        private Rigidbody rb;

		void Start()
		{
            rb = GetComponent<Rigidbody>();
        }

		void OnEnable()
		{
			countDown = false;
			timeRemain = 60;
            spawnButton.SetActive(true);
        }

		// Update is called once per frame
		void Update()
		{
			if (countDown)
			{
                timeRemain-= Time.deltaTime;
				if (timeRemain <= 0)
					gameObject.SetActive(false);

            }
		}

		void FixedUpdate()
		{
			
			if (rb.rotation.eulerAngles.x != 0 || rb.rotation.eulerAngles.z != 0)
			{
				Vector3 newRotation = rb.rotation.eulerAngles;
				newRotation.x = 0;
				newRotation.z = 0;

                rb.MoveRotation(Quaternion.Euler(newRotation));
			}
		}

        public void SpawnInitialGear()
        {
            countDown = true;
            spawnButton.SetActive(false);

            for (int i = 0; i < SR_Manager.instance.character.startGear.Length; i++)
            {
                SpawnLoot(
                    SR_Manager.instance.character.startGear[i], 
                    SR_Manager.instance.character.startGear[i].InitializeLootTable());
            }
        }

        void SpawnLoot(SR_ItemCategory buyCategory, LootTable lootTable)
        {
            if (buyCategory == null || lootTable == null)
                return;

            FVRObject mainObject;
            FVRObject ammoObject;


            //Use Manual weapon IDs?
            if (buyCategory.objectID.Length > 0)
            {
                string id = buyCategory.objectID[Random.Range(0, buyCategory.objectID.Length)];

                //Try to find the weapon ID
                if (!IM.OD.TryGetValue(id, out mainObject))
                    Debug.Log("Cannot find object with id: " + id);

                if (mainObject != null)
                    ammoObject = SR_BuyMenu.GetLowestCapacityAmmoObject(mainObject, null, buyCategory.minCapacity);
                else
                    return;
            }
            else
            {
                //Weapon + Ammo references
                mainObject = lootTable.GetRandomObject();
                if (mainObject != null)
                    ammoObject = SR_BuyMenu.GetLowestCapacityAmmoObject(mainObject, lootTable.Eras, buyCategory.minCapacity);
                else
                    return;
            }

            //Error Check
            if (mainObject == null)
                return;

            FVRObject attach0 = null;
            FVRObject attach1 = null;
            FVRObject attach2 = null;

            if (mainObject.RequiredSecondaryPieces.Count > 0)
            {
                attach0 = mainObject.RequiredSecondaryPieces[0];
                if (mainObject.RequiredSecondaryPieces.Count > 1)
                {
                    attach1 = mainObject.RequiredSecondaryPieces[1];
                }
                if (mainObject.RequiredSecondaryPieces.Count > 2)
                {
                    attach2 = mainObject.RequiredSecondaryPieces[2];
                }
            }
            else if (mainObject.RequiresPicatinnySight)
            {
                attach0 = SR_Manager.instance.lt_RequiredAttachments.GetRandomObject();
                if (attach0.RequiredSecondaryPieces.Count > 0)
                {
                    attach1 = attach0.RequiredSecondaryPieces[0];
                }
            }
            else if (mainObject.BespokeAttachments.Count > 0)
            {
                float num4 = UnityEngine.Random.Range(0f, 1f);
                if (num4 > 0.75f)
                {
                    attach0 = lootTable.GetRandomBespokeAttachment(mainObject);
                    if (attach0.RequiredSecondaryPieces.Count > 0)
                    {
                        attach1 = attach0.RequiredSecondaryPieces[0];
                    }
                }
            }

            //Spawned Refferences
            GameObject spawnedMain = null;
            GameObject spawnedAmmo = null;
            GameObject spawnedAttach0 = null;
            GameObject spawnedAttach1 = null;
            GameObject spawnedAttach2 = null;

            //Spawn the item objects in the game
            if (mainObject != null && mainObject.GetGameObject() != null)
                spawnedMain = UnityEngine.Object.Instantiate<GameObject>(mainObject.GetGameObject(), mainSpawn.position, mainSpawn.rotation);

            //Ammo
            if (ammoObject != null && ammoObject.GetGameObject() != null)
            {
                for (int x = 0; x < 3; x++)
                {
                    spawnedAmmo = UnityEngine.Object.Instantiate<GameObject>(ammoObject.GetGameObject(), ammoSpawn.position + ((Vector3.up * 0.25f) * x), ammoSpawn.rotation);
                }
            }

            //Attachments
            if (attach0 != null && attach0.GetGameObject() != null)
                spawnedAttach0 = UnityEngine.Object.Instantiate<GameObject>(attach0.GetGameObject(), attachmentSpawn[0].position, attachmentSpawn[0].rotation);

            if (attach1 != null && attach1.GetGameObject() != null)
                spawnedAttach1 = UnityEngine.Object.Instantiate<GameObject>(attach1.GetGameObject(), attachmentSpawn[1].position, attachmentSpawn[1].rotation);

            if (attach2 != null && attach2.GetGameObject() != null)
                spawnedAttach2 = UnityEngine.Object.Instantiate<GameObject>(attach2.GetGameObject(), attachmentSpawn[2].position, attachmentSpawn[2].rotation);

        }
    }
}