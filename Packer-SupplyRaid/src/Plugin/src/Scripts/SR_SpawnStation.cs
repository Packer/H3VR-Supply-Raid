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
        [SerializeField] Transform[] spawnPoints;

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
                timeRemain -= Time.deltaTime;
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
            SR_Manager.PlayConfirmSFX();

            countDown = true;
            spawnButton.SetActive(false);

            for (int i = 0; i < SR_Manager.instance.character.StartGearLength(); i++)
            {
                SR_Global.SpawnLoot(
					SR_Manager.instance.character.StartGear(i).InitializeLootTable(), 
					SR_Manager.instance.character.StartGear(i), 
					spawnPoints);

				spawnPoints[0].position += Vector3.up * 0.25f;
            }
        }
    }
}