using FistVR;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SupplyRaid
{
    public class SR_BuyMenu : MonoBehaviour
    {
        [Header("Spawn Points")]
        [SerializeField] Transform[] spawnPoints;

        [Header("Loot Tables")]
        private List<SR_PurchaseCategory> purchaseCategories = new List<SR_PurchaseCategory>();
        private LootTable[] lootTables;

        [Header("Loot Buttons")]
        [SerializeField] Transform[] tabContent;
        [HideInInspector] public SR_GenericButton[] buttons;
        [SerializeField] GameObject buttonPrefab;
        //[SerializeField] AudioClip[] audioClips = new AudioClip[3];
        //[SerializeField] AudioSource audioSource;

        public Text pointDisplay;

        // Use this for initialization
        void Start()
        {
            if (SR_Manager.instance != null)
            {
                SR_Manager.PointEvent += UpdatePoints;
                SR_Manager.LaunchedEvent += StartGame;
                //Debug.Log("Assigned EVENTS <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                pointDisplay.text = SR_Manager.instance.Points.ToString();
            }

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
            purchaseCategories = SR_Manager.instance.character.purchaseCategories;
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
            buttons = new SR_GenericButton[purchaseCategories.Count];

            for (int i = 0; i < purchaseCategories.Count; i++)
            {
                SR_GenericButton newButton = Instantiate(buttonPrefab, tabContent[(int)purchaseCategories[i].ItemCategory().type]).GetComponent<SR_GenericButton>();
                newButton.gameObject.SetActive(true);

                buttons[i] = newButton;
                newButton.button = newButton.GetComponent<Button>();
                newButton.index = i;
                newButton.spawner = this;
                newButton.thumbnail.sprite = purchaseCategories[i].ItemCategory().Thumbnail();
                newButton.text.text = purchaseCategories[i].cost.ToString();
                newButton.name = purchaseCategories[i].ItemCategory().name;

                //Debug.Log("Button ID is " + i);
            }
        }

        public void SpawnLootButton(int i)
        {
            //Debug.Log("Press button " + i + "  - " + lootCategories[i].name);

            if (SR_Manager.EnoughPoints(purchaseCategories[i].cost))
            {
                if (SR_Global.SpawnLoot(lootTables[i], purchaseCategories[i].ItemCategory(), spawnPoints))
                {
                    SR_Manager.PlayConfirmSFX();
                    SR_Manager.SpendPoints(purchaseCategories[i].cost);
                }
                else
                    SR_Manager.PlayFailSFX();
            }
            else
            {
                //Play Sad Sound
                SR_Manager.PlayFailSFX();
                return;
            }
        }


        private void GenerateLootTables()
        {
            Debug.Log("Supply Raid: Purchase Items: " + purchaseCategories.Count);
            //Generate All Loot Category tables
            lootTables = new LootTable[purchaseCategories.Count];

            for (int i = 0; i < purchaseCategories.Count; i++)
            {
                if(purchaseCategories[i] != null)
                    lootTables[i] = purchaseCategories[i].ItemCategory().InitializeLootTable();
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
                component.ReloadMagWithTypeUpToPercentage(AM.GetDefaultRoundClass(component.RoundType), Mathf.Clamp(Random.Range(0.3f, 1f), 0.1f, 1f));
            }
        }

        public void SpawnAmmoAtPlaceForGun(FVRObject gun, Vector3 pos, Quaternion rotation)
        {
            if (gun.CompatibleMagazines.Count > 0)
            {
                SpawnObjectAtPlace(gun.CompatibleMagazines[Random.Range(0, gun.CompatibleMagazines.Count - 1)], pos, rotation);
            }
            else if (gun.CompatibleClips.Count > 0)
            {
                SpawnObjectAtPlace(gun.CompatibleClips[Random.Range(0, gun.CompatibleClips.Count - 1)], pos, rotation);
            }
            else if (gun.CompatibleSpeedLoaders.Count > 0)
            {
                SpawnObjectAtPlace(gun.CompatibleSpeedLoaders[Random.Range(0, gun.CompatibleSpeedLoaders.Count - 1)], pos, rotation);
            }
            else if (gun.CompatibleSingleRounds.Count > 0)
            {
                int num = Random.Range(2, 5);
                for (int i = 0; i < num; i++)
                {
                    Vector3 vector = pos + Vector3.up * (0.05f * (float)i);
                    SpawnObjectAtPlace(gun.CompatibleSingleRounds[Random.Range(0, gun.CompatibleSingleRounds.Count - 1)], vector, rotation);
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

                List<FVRObject> rounds = new List<FVRObject>();
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
                        rounds.Add(o.CompatibleSingleRounds[k]);
                    }
                }

                if (rounds.Count > 0)
                {
                    FVRObject result = rounds[Random.Range(0, rounds.Count)];
                    rounds.Clear();
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