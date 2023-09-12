using FistVR;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SupplyRaid
{
    public class SR_BuyMenu : MonoBehaviour
    {
        [Header("Spawn Points")]
        [SerializeField] Transform mainSpawn;
        [SerializeField] Transform ammoSpawn;
        [SerializeField] Transform[] attachmentSpawn;   //3 spawn points

        [Header("Loot Tables")]
        private SR_PurchaseCategory[] purchaseCategories;
        private LootTable[] lootTables;
        private SR_CharacterPreset character;

        [Header("Loot Buttons")]
        [SerializeField] Transform[] tabContent;
        private SR_GenericButton[] buttons;
        [SerializeField] GameObject buttonPrefab;
        [SerializeField] AudioClip[] audioClips = new AudioClip[3];
        [SerializeField] AudioSource audioSource;

        public Text pointDisplay;

        // Use this for initialization
        void Start()
        {

            if (SR_Manager.instance != null)
            {
                SR_Manager.PointEvent += UpdatePoints;
                SR_Manager.LaunchedEvent += StartGame;
                Debug.Log("Assigned EVENTS <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
            }
            pointDisplay.text = SR_Manager.instance.Points.ToString();

        }

        void OnDestroy()
        {
            SR_Manager.PointEvent -= UpdatePoints;
            SR_Manager.LaunchedEvent -= StartGame;
        }

		// Update is called once per frame
		void Update()
		{

        }

        private void StartGame()
        {
            character = SR_Manager.instance.character;
            purchaseCategories = character.purchaseCategories;
            GenerateLootTables();
            GenerateButtons();
        }

        private void UpdatePoints(int i)
        {
            if(SR_Manager.instance != null)
                pointDisplay.text = SR_Manager.instance.Points.ToString();
        }

        private void GenerateButtons()
        {
            buttons = new SR_GenericButton[purchaseCategories.Length];

            for (int i = 0; i < purchaseCategories.Length; i++)
            {
                SR_GenericButton newButton = Instantiate(buttonPrefab, tabContent[(int)purchaseCategories[i].itemCategory.type]).GetComponent<SR_GenericButton>();
                newButton.gameObject.SetActive(true);

                buttons[i] = newButton;
                newButton.button = newButton.GetComponent<Button>();
                newButton.index = i;
                newButton.spawner = this;
                newButton.thumbnail.sprite = purchaseCategories[i].itemCategory.thumbnail;
                newButton.text.text = purchaseCategories[i].cost.ToString();

                //Debug.Log("Button ID is " + i);
            }
        }

        public void SpawnLootButton(int i)
        {
            //Debug.Log("Press button " + i + "  - " + lootCategories[i].name);

            if (SR_Manager.EnoughPoints(purchaseCategories[i].cost))
            {
                if (SpawnLoot(i))
                {
                    audioSource.PlayOneShot(audioClips[0]);
                    SR_Manager.SpendPoints(purchaseCategories[i].cost);
                }
                else
                    audioSource.PlayOneShot(audioClips[1]);
            }
            else
            {
                //Play Sad Sound
                audioSource.PlayOneShot(audioClips[1]);
                return;
            }
        }

        public void PlayClickSound()
        {
            audioSource.PlayOneShot(audioClips[2]);
        }

        public void CompleteLoot()
        {
            audioSource.PlayOneShot(audioClips[3]);
        }

        bool SpawnLoot(int i)
        {
            FVRObject mainObject = null;
            FVRObject ammoObject = null;

            //Use Manual weapon IDs?
            if (purchaseCategories[i].itemCategory.objectID.Length > 0)
            {
                string id = purchaseCategories[i].itemCategory.objectID[Random.Range(0, purchaseCategories[i].itemCategory.objectID.Length)];

                if (id == "" || id == null)
                    return false;

                //Try to find the weapon ID
                if (!IM.OD.TryGetValue(id, out mainObject))
                    Debug.Log("Cannot find object with id: " + id);

                if (mainObject != null)
                    ammoObject = GetLowestCapacityAmmoObject(mainObject, null, purchaseCategories[i].itemCategory.minCapacity);
                else
                    return false;
            }
            else
            {
                //Weapon + Ammo references
                mainObject = lootTables[i].GetRandomObject();
                if (mainObject != null)
                    ammoObject = GetLowestCapacityAmmoObject(mainObject, lootTables[i].Eras, purchaseCategories[i].itemCategory.minCapacity);
                else
                    return false;
            }

            //Error Check
            if (mainObject == null)
                return false;

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
                    attach0 = lootTables[i].GetRandomBespokeAttachment(mainObject);
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

            //TODO, Add to cleanup... cause we might need to do that...

            if (spawnedMain != null || spawnedAmmo != null || spawnedAttach0 != null || spawnedAttach1 != null || spawnedAttach2 != null)
                return true;
            else 
                return false;
        }

        private void GenerateLootTables()
        {

            //Generate All Loot Category tables
            lootTables = new LootTable[purchaseCategories.Length];

            for (int i = 0; i < purchaseCategories.Length; i++)
            {
                lootTables[i] = purchaseCategories[i].itemCategory.InitializeLootTable();
            }
        }

        public void SetTab(int exception)
        {
            for (int i = 0; i < tabContent.Length; i++)
            {
                tabContent[i].gameObject.SetActive(false);
                
                if(i == exception)
                    tabContent[i].gameObject.SetActive(true);
            }
        }

        public void SpawnObjectAtPlace(FVRObject obj, Vector3 pos, Quaternion rotation)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(obj.GetGameObject(), pos, rotation);
            FVRFireArmMagazine component = gameObject.GetComponent<FVRFireArmMagazine>();
            if (component != null && component.RoundType != FireArmRoundType.aFlameThrowerFuel)
            {
                component.ReloadMagWithTypeUpToPercentage(AM.GetRandomValidRoundClass(component.RoundType), Mathf.Clamp(UnityEngine.Random.Range(0.3f, 1f), 0.1f, 1f));
            }
        }

        public void SpawnAmmoAtPlaceForGun(FVRObject gun, Vector3 pos, Quaternion rotation)
        {
            if (gun.CompatibleMagazines.Count > 0)
            {
                this.SpawnObjectAtPlace(gun.CompatibleMagazines[UnityEngine.Random.Range(0, gun.CompatibleMagazines.Count - 1)], pos, rotation);
            }
            else if (gun.CompatibleClips.Count > 0)
            {
                this.SpawnObjectAtPlace(gun.CompatibleClips[UnityEngine.Random.Range(0, gun.CompatibleClips.Count - 1)], pos, rotation);
            }
            else if (gun.CompatibleSpeedLoaders.Count > 0)
            {
                this.SpawnObjectAtPlace(gun.CompatibleSpeedLoaders[UnityEngine.Random.Range(0, gun.CompatibleSpeedLoaders.Count - 1)], pos, rotation);
            }
            else if (gun.CompatibleSingleRounds.Count > 0)
            {
                int num = UnityEngine.Random.Range(2, 5);
                for (int i = 0; i < num; i++)
                {
                    Vector3 vector = pos + Vector3.up * (0.05f * (float)i);
                    this.SpawnObjectAtPlace(gun.CompatibleSingleRounds[UnityEngine.Random.Range(0, gun.CompatibleSingleRounds.Count - 1)], vector, rotation);
                }
            }
        }


        public static FVRObject GetLowestCapacityAmmoObject(FVRObject o, List<FVRObject.OTagEra> eras = null, int Min = -1, int Max = -1, List<FVRObject.OTagSet> sets = null)
        {
            o = IM.OD[o.ItemID];
            if (o.CompatibleMagazines.Count > 0)
            {
                List<FVRObject> ammoCollection = GetLowestCapacity(o.CompatibleMagazines, Min);
                return ammoCollection[Random.Range(0, ammoCollection.Count)];
            }

            if (o.Category == FVRObject.ObjectCategory.Firearm && o.MagazineType != 0)
            {
                List<FVRObject> ammoCollection = GetLowestCapacity(IM.CompatMags[o.MagazineType], Min);
                return ammoCollection[Random.Range(0, ammoCollection.Count)];
            }

            if (o.CompatibleClips.Count > 0)
            {
                List<FVRObject> ammoCollection = GetLowestCapacity(o.CompatibleClips, Min);
                return ammoCollection[Random.Range(0, ammoCollection.Count)];
            }

            if (o.CompatibleSpeedLoaders.Count > 0)
            {
                List<FVRObject> ammoCollection = GetLowestCapacity(o.CompatibleSpeedLoaders, Min);
                return ammoCollection[Random.Range(0, ammoCollection.Count)];
            }

            if (o.CompatibleSingleRounds.Count > 0)
            {
                if ((eras == null || eras.Count < 1) && (sets == null || sets.Count < 1))
                {
                    return o.CompatibleSingleRounds[Random.Range(0, o.CompatibleSingleRounds.Count)];
                }

                List<FVRObject> list4 = new List<FVRObject>();
                for (int k = 0; k < o.CompatibleSingleRounds.Count; k++)
                {
                    bool flag = true;
                    if (eras != null && eras.Count > 0 && !eras.Contains(o.CompatibleSingleRounds[k].TagEra))
                    {
                        flag = false;
                    }

                    if (sets != null && sets.Count > 0 && !sets.Contains(o.CompatibleSingleRounds[k].TagSet))
                    {
                        flag = false;
                    }

                    if (flag)
                    {
                        list4.Add(o.CompatibleSingleRounds[k]);
                    }
                }

                if (list4.Count > 0)
                {
                    FVRObject result = list4[Random.Range(0, list4.Count)];
                    list4.Clear();
                    return result;
                }

                return o.CompatibleSingleRounds[0];
            }
            return null;
        }

        public static List<FVRObject> GetLowestCapacity(List<FVRObject> ammo, int minCapacity)
        {
            List<FVRObject> collected = new List<FVRObject>();

            //Get Lowest Capacity possible
            int lowest = int.MaxValue;
            for (int i = 0; i < ammo.Count; i++)
            {
                if(ammo[i].MagazineCapacity < lowest && ammo[i].MagazineCapacity >= minCapacity)
                    lowest = ammo[i].MagazineCapacity;
            }

            //Collect all lowest capacity
            for (int i = 0; i < ammo.Count; i++)
            {
                if (ammo[i].MagazineCapacity == lowest)
                    collected.Add(ammo[i]);
            }

            return collected;
        }
    }
}