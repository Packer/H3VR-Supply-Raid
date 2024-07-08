using System.Collections.Generic;
using UnityEngine;
using FistVR;

namespace SupplyRaid
{
    public class SR_AmmoSpawner : MonoBehaviour
    {
        public static SR_AmmoSpawner instance;

        public Transform Spawnpoint_Round;
        public Transform ScanningVolume;
        public LayerMask ScanningLM;
        public Transform selectionIcon;

        [HideInInspector]
        public bool[] purchasedAmmoTypes = new bool[28];    //Ammo Enum Length
        public GameObject rearmButton;
        public GameObject speedloaderButton;
        public GameObject clipButton;
        public GameObject roundButton;
        public GameObject togglePagesButton;
        [HideInInspector, Tooltip("0 = Rearm \n1 = Speed Loader\n2 = Clip\n3 Round")] 
        private bool[] purchaseButtons = new bool[4];
        private SR_GenericButton[] ammoButtons = new SR_GenericButton[28];

        public GameObject[] ammoTypeButtons = new GameObject[28];    //Ammo Enum Length

        public AmmoEnum selectedAmmoType = AmmoEnum.Standard;
        public FireArmRoundClass roundClass = FireArmRoundClass.FMJ;

        private List<FVRFireArmMagazine> m_detectedMags = new List<FVRFireArmMagazine>();
        private List<FVRFireArmClip> m_detectedClips = new List<FVRFireArmClip>();
        private List<Speedloader> m_detectedSLs = new List<Speedloader>();
        private List<SosigWeaponPlayerInterface> m_detectedSweapons = new List<SosigWeaponPlayerInterface>();
        private List<FVRFireArm> m_detectedFirearms = new List<FVRFireArm>();
        private List<FireArmRoundType> m_roundTypes = new List<FireArmRoundType>();
        private Collider[] colbuffer;
        private Dictionary<FireArmRoundType, FireArmRoundClass> m_decidedTypes = new Dictionary<FireArmRoundType, FireArmRoundClass>();
        private float m_scanTick = 1f;

        //Ammo
        List<AmmoRound> ammoList = new List<AmmoRound>();
        List<FireArmRoundType> allRoundTypes = new List<FireArmRoundType>();

        [Header("Ammo Page")]
        public GameObject ammoPage;

        //Round page
        [Header("Round Page")]
        public GameObject roundPage;
        public GameObject roundButtonPrefab;
        public Transform roundContainer;
        private List<SR_GenericButton> roundButtons = new List<SR_GenericButton>();
        private List<FVRObject> rounds = new List<FVRObject>();

        public static List<FVRObject> GetAllRounds()
        {
            List<FVRObject> gearIDs = new List<FVRObject>();

            //Loop through every item in the game and compare Keyword
            foreach (string key in IM.OD.Keys)
            {
                if (IM.OD.TryGetValue(key, out FVRObject fvrObject))
                {
                    if (fvrObject && fvrObject.Category == FVRObject.ObjectCategory.Cartridge)
                    {
                        gearIDs.Add(fvrObject);
                    }
                }
            }

            return gearIDs;
        }

        void PopulateRoundPage()
        {
            //Debug.Log("Populating Rounds Page");
            //Clear all old buttons
            for (int i = 0; i < roundButtons.Count; i++)
            {
                if (roundButtons[i])
                    Destroy(roundButtons[i].gameObject);
            }

            List<FireArmRoundType> roundTypes = new List<FireArmRoundType>();

            //Detected Rounds
            roundTypes.AddRange(m_roundTypes);

            //Mag Rounds
            for (int i = 0; i < m_detectedMags.Count; i++)
            {
                if (m_detectedMags[i] && !roundTypes.Contains(m_detectedMags[i].RoundType))
                    roundTypes.Add(m_detectedMags[i].RoundType);
            }

            //Clip Rounds
            for (int i = 0; i < m_detectedClips.Count; i++)
            {
                if (m_detectedClips[i] 
                    && !roundTypes.Contains(m_detectedClips[i].RoundType))
                        roundTypes.Add(m_detectedClips[i].RoundType);
            }

            //Clip Rounds
            for (int i = 0; i < m_detectedSLs.Count; i++)
            {
                if (m_detectedSLs[i] 
                    && m_detectedSLs[i].ObjectWrapper 
                    && !roundTypes.Contains(m_detectedSLs[i].ObjectWrapper.RoundType))
                        roundTypes.Add(m_detectedSLs[i].ObjectWrapper.RoundType);
            }

            //Special Weapon
            for (int i = 0; i < m_detectedSweapons.Count; i++)
            {
                if (m_detectedSweapons[i] 
                    && m_detectedSweapons[i].ObjectWrapper 
                    && !roundTypes.Contains(m_detectedSweapons[i].ObjectWrapper.RoundType))
                        roundTypes.Add(m_detectedSweapons[i].ObjectWrapper.RoundType);
            }
            //Special Weapon
            for (int i = 0; i < m_detectedFirearms.Count; i++)
            {
                if (m_detectedFirearms[i]
                    && !roundTypes.Contains(m_detectedFirearms[i].RoundType))
                        roundTypes.Add(m_detectedFirearms[i].RoundType);
            }

            //Populate Rounds
            rounds = GetAllRounds();

            for (int i = rounds.Count - 1; i >= 0; i--)
            {
                if (!roundTypes.Contains(rounds[i].RoundType))
                    rounds.RemoveAt(i);
            }
            //Debug.Log("Rounds Total: " + rounds.Count);

            //Create Buttons!
            for (int i = 0; i < rounds.Count; i++)
            {
                ItemSpawnerID id;
                IM.Instance.SpawnerIDDic.TryGetValue(rounds[i].SpawnedFromId, out id);

                if (id != null)
                {
                    //Debug.Log("Round: " + i);
                    SR_GenericButton btn = Instantiate(roundButtonPrefab, roundContainer).GetComponent<SR_GenericButton>();
                    btn.gameObject.SetActive(true);
                    roundButtons.Add(btn);

                    //Description
                    btn.fvrObject = rounds[i];
                    btn.textB.text = rounds[i].DisplayName;
                    FireArmRoundClass classType = GetFirearmRoundClassFromType(rounds[i].ItemID, rounds[i].RoundType);
                    AmmoEnum ammo = SR_Global.GetAmmoEnum(classType);
                    btn.index = (int)ammo;  //index (Cost)
                    btn.genericButton = ammoButtons[(int)ammo]; //Ammo Page button for hiding ammo count

                    //Cost Text
                    if (purchasedAmmoTypes[(int)ammo])
                    {
                        if (SR_Manager.Character().modeRounds == 4)
                        {
                            int roundCost = SR_Manager.Character().GetRoundPowerCost(rounds[i].TagFirearmRoundPower, 1);
                            btn.text.text = roundCost.ToString();
                        }
                        else
                            btn.text.text = "";
                    }
                    else
                    {
                        btn.text.text = SR_Manager.Character().ammoUpgradeCost[(int)ammo].ToString();
                    }

                    //Debug.Log("Ammo IDS: " + ammo);
                    //Use same Ammo Type sprite
                    btn.thumbnail.sprite = ammoTypeButtons[(int)ammo].GetComponent<UnityEngine.UI.Image>().sprite;
                }
                else 
                {
                    Debug.Log("Supply Raid: No ID found for rounds");
                }
            }
        }

        public void BuySpecificRound(FVRObject fvrObject)
        {
            //Debug.Log("Spawning round " + index);
            if (!CanSpawn(SR_Manager.Character().modeRounds, SR_Manager.Character().roundsCost, 3))
            {
                SR_Manager.PlayFailSFX();
                return;
            }

            if (fvrObject != null)
            {
                Instantiate(fvrObject.GetGameObject(), Spawnpoint_Round.position, Spawnpoint_Round.rotation);
            }
            SR_Manager.PlayRearmSFX();

            /*
            FVRObject round = rounds[index];
            FireArmRoundClass classType = GetFirearmRoundClassFromType(round.RoundType);
            AmmoEnum ammo = SR_Global.GetAmmoEnum(classType);
            */
        }
        
        public void TogglePages()
        {
            roundPage.SetActive(ammoPage.activeSelf);
            ammoPage.SetActive(!roundPage.activeSelf);
            selectionIcon.gameObject.SetActive(!roundPage.activeSelf);
        }

        private void Start()
        {
            instance = this;
            colbuffer = new Collider[50];
        }

        private void Update()
        {
            m_scanTick -= Time.deltaTime;
            if (m_scanTick <= 0f)
            {
                float num = Vector3.Distance(transform.position, GM.CurrentPlayerBody.transform.position);
                if (num < 12f)
                {
                    Scan();
                    m_scanTick = Random.Range(0.8f, 1f);
                }
                else
                    m_scanTick = Random.Range(2f, 3f);
            }

            if (selectionIcon.gameObject.activeSelf && selectedAmmoType != AmmoEnum.None)
                selectionIcon.position = ammoTypeButtons[(int)selectedAmmoType].transform.position;
        }

        public void Setup()
        {
            ammoButtons = new SR_GenericButton[ammoTypeButtons.Length];

            //Ammo Types
            for (int i = 0; i < ammoTypeButtons.Length; i++)
            {
                SR_GenericButton btn = ammoTypeButtons[i].GetComponent<SR_GenericButton>();

                ammoButtons[i] = btn;
                //btn.index = i;
                if (SR_Manager.instance.character.ammoUpgradeCost[i] == 0)
                {
                    purchasedAmmoTypes[i] = true;
                    btn.text.text = "";
                }
                else
                    btn.text.text = SR_Manager.instance.character.ammoUpgradeCost[i].ToString();

                //Disable Buttons here after setup
            }

            //REARM
            switch (SR_Manager.instance.character.modeRearming)
            {
                case 0: //false
                    rearmButton.SetActive(false);
                    break;
                case 1: //true
                default:
                    rearmButton.SetActive(true);
                    rearmButton.GetComponent<FVRPointableButton>().Text.text = "";
                    break;
                case 2: //Buy Once
                case 3: //Buy Repeat
                    if (SR_Manager.instance.character.rearmingCost > 0)
                        rearmButton.GetComponent<FVRPointableButton>().Text.text = SR_Manager.instance.character.rearmingCost.ToString();
                    else
                        rearmButton.GetComponent<FVRPointableButton>().Text.text = "";
                    break;
            }

            //SPEED LOADERS
            switch (SR_Manager.instance.character.modeSpeedLoaders)
            {
                case 0: //false
                    speedloaderButton.SetActive(false);
                    break;
                case 1: //true
                default:
                    speedloaderButton.SetActive(true);
                    speedloaderButton.GetComponent<FVRPointableButton>().Text.text = "";
                    break;
                case 2: //Buy Once
                case 3: //Buy Repeat
                    if (SR_Manager.instance.character.speedLoadersCost > 0)
                        speedloaderButton.GetComponent<FVRPointableButton>().Text.text = SR_Manager.instance.character.speedLoadersCost.ToString();
                    else
                        speedloaderButton.GetComponent<FVRPointableButton>().Text.text = "";
                    break;
            }

            //CLIPS
            switch (SR_Manager.instance.character.modeClips)
            {
                case 0: //false
                    clipButton.SetActive(false);
                    break;
                case 1: //true
                default:
                    clipButton.SetActive(true);
                    clipButton.GetComponent<FVRPointableButton>().Text.text = "";
                    break;
                case 2: //Buy Once
                case 3: //Buy Repeat
                    if (SR_Manager.instance.character.clipsCost > 0)
                        clipButton.GetComponent<FVRPointableButton>().Text.text = SR_Manager.instance.character.clipsCost.ToString();
                    else
                        clipButton.GetComponent<FVRPointableButton>().Text.text = "";
                    break;
            }

            //ROUNDS
            switch (SR_Manager.instance.character.modeRounds)
            {
                case 0: //false
                    roundButton.SetActive(false);
                    togglePagesButton.SetActive(false);
                    break;
                case 1: //true
                default:
                    roundButton.SetActive(true);
                    togglePagesButton.SetActive(true);
                    roundButton.GetComponent<FVRPointableButton>().Text.text = "";
                    break;
                case 2: //Buy Once
                case 3: //Buy Repeat
                    if (SR_Manager.instance.character.roundsCost > 0)
                        roundButton.GetComponent<FVRPointableButton>().Text.text = SR_Manager.instance.character.roundsCost.ToString();
                    else
                        roundButton.GetComponent<FVRPointableButton>().Text.text = "";
                    break;
            }
        }

        public void SetAmmoType(AmmoEnum ammo)
        {
            //TODO check if we can set the ammo for this weapon
            selectedAmmoType = ammo;

            //If selected Ammo Type is enabled, then enable other wise hide selection
            if (ammoTypeButtons[(int)selectedAmmoType].activeSelf == true)
                selectionIcon.position = ammoTypeButtons[(int)selectedAmmoType].transform.position;
            else
            {
                selectionIcon.gameObject.SetActive(false);
            }
        }

        bool CanSpawn(int mode, int cost, int id)
        {
            //Already Purchased
            if (purchaseButtons[id] == true || mode <= 1)
                return true;

            //Pay for this
            if (mode > 1
                && cost > 0)
            {
                if (SR_Manager.EnoughPoints(cost))
                {
                    if (SR_Manager.SpendPoints(cost))
                    {
                        if (mode == 2) //Buy Once
                        {
                            purchaseButtons[id] = true;
                            //Update UI
                            switch (id)
                            {
                                case 0: //Rearm
                                    rearmButton.GetComponent<FVRPointableButton>().Text.text = "";
                                    break;
                                case 1: //Loader
                                    speedloaderButton.GetComponent<FVRPointableButton>().Text.text = "";
                                    break;
                                case 2: //Clip
                                    clipButton.GetComponent<FVRPointableButton>().Text.text = "";
                                    break;
                                case 3: //Rounds
                                    roundButton.GetComponent<FVRPointableButton>().Text.text = "";
                                    break;
                                default:
                                    break;
                            }
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        public void Button_SpawnRound()
        {
            if (m_roundTypes == null || m_roundTypes.Count < 1 || ammoList == null || 
                !CanSpawn(SR_Manager.instance.character.modeRounds, SR_Manager.instance.character.roundsCost, 3))
            {
                SR_Manager.PlayFailSFX();
                return;
            }

            //Loop through each Round Type
            for (int x = 0; x < ammoList.Count; x++)
            {
                if (ammoList[x].roundClasses == null)
                    continue;

                //Loop through each Round Class
                for (int y = 0; y < ammoList[x].roundClasses.Count; y++)
                {
                    if (ammoList[x].roundClasses[y].ammo == selectedAmmoType)
                    {
                        FVRObject roundSelfPrefab = AM.GetRoundSelfPrefab(ammoList[x].roundType, ammoList[x].roundClasses[y].roundClass);
                        if (roundSelfPrefab != null)
                        {
                            Instantiate(roundSelfPrefab.GetGameObject(), Spawnpoint_Round.position + Vector3.up * x * 0.1f, Spawnpoint_Round.rotation);
                            break;
                        }
                    }
                }
            }
            SR_Manager.PlayRearmSFX();
        }

        public void Button_SpawnSpeedLoader()
        {
            if (!CanSpawn(SR_Manager.instance.character.modeSpeedLoaders, SR_Manager.instance.character.speedLoadersCost, 1))
            {
                SR_Manager.PlayFailSFX();
                return;
            }

            bool flag = false;
            List<FVRObject> usableSpeedloaders = new List<FVRObject>();

            //Collect all compatible speedloaders
            for (int i = 0; i < m_detectedFirearms.Count; i++)
            {
                if (IM.OD.ContainsKey(m_detectedFirearms[i].ObjectWrapper.ItemID))
                {
                    List<FVRObject> speedloaders = new List<FVRObject>(ManagerSingleton<IM>.Instance.odicTagCategory[FVRObject.ObjectCategory.SpeedLoader]);
                    for (int x = speedloaders.Count - 1; x >= 0; x--)
                    {
                        if (speedloaders[x].Category == FVRObject.ObjectCategory.SpeedLoader
                            && speedloaders[x].RoundType == m_detectedFirearms[i].RoundType)
                        {
                            usableSpeedloaders.Add(speedloaders[x]);
                        }
                    }
                }
            }

            for (int i = 0; i < usableSpeedloaders.Count; i++)
            {
                GameObject gameObject = usableSpeedloaders[i].GetGameObject();
                Speedloader speedloader = gameObject.GetComponent<Speedloader>();
                if (!speedloader.IsPretendingToBeAMagazine)
                {
                    flag = true;
                    GameObject newSpeedLoader = Instantiate(gameObject, Spawnpoint_Round.position + Vector3.up * i * 0.1f, Spawnpoint_Round.rotation);
                    //speedloader = newSpeedLoader.GetComponent<Speedloader>();
                }
            }

            if (flag)
                SR_Manager.PlayRearmSFX();
            else
                SR_Manager.PlayFailSFX();
        }

        public void Button_SpawnClip()
        {
            if (!CanSpawn(SR_Manager.instance.character.modeClips, SR_Manager.instance.character.clipsCost, 2))
            {
                SR_Manager.PlayFailSFX();
                return;
            }

            bool flag = false;
            for (int i = 0; i < m_detectedFirearms.Count; i++)
            {
                if (IM.OD.ContainsKey(m_detectedFirearms[i].ObjectWrapper.ItemID))
                {
                    FVRObject fvrobject = IM.OD[m_detectedFirearms[i].ObjectWrapper.ItemID];

                    for (int x = 0; x < ammoList.Count; x++)
                    {
                        //Does not have round classes
                        if (ammoList[x].roundClasses == null)
                            continue;

                        //Loop through each Round Class
                        for (int y = 0; y < ammoList[x].roundClasses.Count; y++)
                        {
                            //If not the type we want, continue to next
                            if (ammoList[x].roundClasses[y].ammo != selectedAmmoType)
                                continue;

                            if (fvrobject.CompatibleClips.Count > 0 
                                && ammoList[x].roundType == fvrobject.CompatibleClips[0].RoundType)
                            {
                                flag = true;
                                FVRObject fvrobject2 = fvrobject.CompatibleClips[0];
                                GameObject newClip = Instantiate(fvrobject2.GetGameObject(), Spawnpoint_Round.position + Vector3.up * i * 0.1f, Spawnpoint_Round.rotation);
                                FVRFireArmClip component = newClip.GetComponent<FVRFireArmClip>();
                                component.ReloadClipWithType(ammoList[x].roundClasses[y].roundClass);
                            }
                            else if (fvrobject.CompatibleMagazines.Count > 0
                                && ammoList[x].roundType == fvrobject.CompatibleMagazines[0].RoundType)
                            {
                                GameObject gameObject2 = fvrobject.CompatibleMagazines[0].GetGameObject();
                                FVRFireArmMagazine fvrfireArmMagazine = gameObject2.GetComponent<FVRFireArmMagazine>();
                                if (fvrfireArmMagazine.IsEnBloc)
                                {
                                    flag = true;
                                    GameObject newMagazine = Instantiate(gameObject2, Spawnpoint_Round.position + Vector3.up * i * 0.1f, Spawnpoint_Round.rotation);
                                    fvrfireArmMagazine = newMagazine.GetComponent<FVRFireArmMagazine>();
                                    fvrfireArmMagazine.ReloadMagWithType(ammoList[x].roundClasses[y].roundClass);
                                }
                            }
                        }
                    }
                }
            }
            if (flag)
                SR_Manager.PlayRearmSFX();
            else
                SR_Manager.PlayFailSFX();
        }

        public void Button_ReloadGuns()
        {
            if (!CanSpawn(SR_Manager.instance.character.modeRearming, SR_Manager.instance.character.rearmingCost, 0))
            {
                SR_Manager.PlayFailSFX();
                return;
            }

            if (m_detectedMags.Count < 1 && m_detectedClips.Count < 1 && m_detectedSLs.Count < 1 && m_detectedSweapons.Count < 1)
            {
                SR_Manager.PlayFailSFX();
                return;
            }

            for (int x = 0; x < ammoList.Count; x++)
            {
                //Does not have round classes
                if (ammoList[x].roundClasses == null)
                    continue;
                
                //Loop through each Round Class
                for (int y = 0; y < ammoList[x].roundClasses.Count; y++)
                {
                    //If not the type we want, continue to next
                    if (ammoList[x].roundClasses[y].ammo != selectedAmmoType)
                        continue;

                    //Magazine Rearm
                    for (int z = 0; z < m_detectedMags.Count; z++)
                    {
                        if (ammoList[x].roundType == m_detectedMags[z].RoundType)
                        {
                            m_detectedMags[z].ReloadMagWithType(ammoList[x].roundClasses[y].roundClass);
                        }
                    }

                    //Clip Reload
                    for (int z = 0; z < m_detectedClips.Count; z++)
                    {
                        if (ammoList[x].roundType == m_detectedClips[z].RoundType)
                        {
                            m_detectedClips[z].ReloadClipWithType(ammoList[x].roundClasses[y].roundClass);
                        }
                    }

                    //Speed Loader
                    for (int z = 0; z < m_detectedSLs.Count; z++)
                    {
                        if (ammoList[x].roundType == m_detectedSLs[z].Chambers[0].Type)
                        {
                            m_detectedSLs[z].ReloadClipWithType(ammoList[x].roundClasses[y].roundClass);
                        }
                    }

                    //SWeapons
                    for (int z = 0; z < m_detectedSweapons.Count; z++)
                    {
                        m_detectedSweapons[z].W.InstaReload();
                    }
                }
            }
            SR_Manager.PlayRearmSFX();
        }

        private FireArmRoundClass GetFirearmRoundClassFromType(string itemID, FireArmRoundType t)
        {
            for (int i = 0; i < AM.SRoundDisplayDataDic[t].Classes.Length; i++)
            {
                if (AM.SRoundDisplayDataDic[t].Classes[i].ObjectID.ItemID == itemID)
                    return AM.SRoundDisplayDataDic[t].Classes[i].Class;
            }

            return AM.SRoundDisplayDataDic[t].Classes[0].Class;
        }

        private void Scan()
        {
            int num = Physics.OverlapBoxNonAlloc(
                ScanningVolume.position, 
                ScanningVolume.localScale * 0.5f, 
                colbuffer, 
                ScanningVolume.rotation, 
                ScanningLM, 
                QueryTriggerInteraction.Collide);
            m_roundTypes.Clear();
            m_detectedMags.Clear();
            m_detectedClips.Clear();
            m_detectedSLs.Clear();
            m_detectedSweapons.Clear();
            m_detectedFirearms.Clear();
            for (int i = 0; i < num; i++)
            {
                if (colbuffer[i].attachedRigidbody != null)
                {
                    FVRFireArm component = colbuffer[i].attachedRigidbody.gameObject.GetComponent<FVRFireArm>();
                    if (component != null && component.RoundType != FireArmRoundType.a69CashMoney)
                    {
                        if (!m_detectedFirearms.Contains(component))
                        {
                            m_detectedFirearms.Add(component);
                        }
                        if (!m_roundTypes.Contains(component.RoundType))
                        {
                            m_roundTypes.Add(component.RoundType);
                        }
                        if (component.Magazine != null && !m_detectedMags.Contains(component.Magazine))
                        {
                            m_detectedMags.Add(component.Magazine);
                        }
                        if (component.Attachments.Count > 0)
                        {
                            for (int j = 0; j < component.Attachments.Count; j++)
                            {
                                if (component.Attachments[j] is AttachableFirearmPhysicalObject)
                                {
                                    if (!m_roundTypes.Contains((component.Attachments[j] as AttachableFirearmPhysicalObject).FA.RoundType))
                                    {
                                        m_roundTypes.Add((component.Attachments[j] as AttachableFirearmPhysicalObject).FA.RoundType);
                                    }
                                    if ((component.Attachments[j] as AttachableFirearmPhysicalObject).FA.Magazine != null)
                                    {
                                        m_detectedMags.Add((component.Attachments[j] as AttachableFirearmPhysicalObject).FA.Magazine);
                                    }
                                }
                            }
                        }
                        if (component.GetIntegratedAttachableFirearm() != null && !m_roundTypes.Contains(component.GetIntegratedAttachableFirearm().RoundType))
                        {
                            m_roundTypes.Add(component.GetIntegratedAttachableFirearm().RoundType);
                        }
                    }

                    AttachableFirearmPhysicalObject component2 = colbuffer[i].attachedRigidbody.gameObject.GetComponent<AttachableFirearmPhysicalObject>();
                    if (component2 != null && !m_roundTypes.Contains(component2.FA.RoundType))
                    {
                        m_roundTypes.Add(component2.FA.RoundType);
                    }

                    FVRFireArmMagazine component3 = colbuffer[i].attachedRigidbody.gameObject.GetComponent<FVRFireArmMagazine>();
                    if (component3 != null 
                        && component3.FireArm == null 
                        && !m_detectedMags.Contains(component3) 
                        && component3.RoundType != FireArmRoundType.a69CashMoney)
                    {
                        m_detectedMags.Add(component3);
                    }

                    FVRFireArmClip component4 = colbuffer[i].attachedRigidbody.gameObject.GetComponent<FVRFireArmClip>();
                    if (component4 != null && component4.FireArm == null && !m_detectedClips.Contains(component4))
                    {
                        m_detectedClips.Add(component4);
                    }
                    Speedloader component5 = colbuffer[i].attachedRigidbody.gameObject.GetComponent<Speedloader>();
                    if (component5 != null && !m_detectedSLs.Contains(component5))
                    {
                        m_detectedSLs.Add(component5);
                    }
                    if (SR_Manager.instance.shakeReloading == TNH_SosiggunShakeReloading.On)
                    {
                        SosigWeaponPlayerInterface component6 = colbuffer[i].attachedRigidbody.gameObject.GetComponent<SosigWeaponPlayerInterface>();
                        if (component6 != null && !m_detectedSweapons.Contains(component6))
                        {
                            m_detectedSweapons.Add(component6);
                        }
                    }
                }
            }
            UpdateAmmoTypeDisplay();
        }

        void UpdateAmmoTypeDisplay()
        {
            //Collect all Roundtypes
            allRoundTypes.Clear();
            ammoList.Clear();

            //Rounds
            for (int i = 0; i < m_roundTypes.Count; i++)
            {
                if (!allRoundTypes.Contains(m_roundTypes[i]))
                    allRoundTypes.Add(m_roundTypes[i]);
            }

            //Magazines
            for (int i = 0; i < m_detectedMags.Count; i++)
            {
                if (!allRoundTypes.Contains(m_detectedMags[i].RoundType))
                    allRoundTypes.Add(m_detectedMags[i].RoundType);
            }

            //Clips
            for (int j = 0; j < m_detectedClips.Count; j++)
            {
                if (!allRoundTypes.Contains(m_detectedClips[j].RoundType))
                    allRoundTypes.Add(m_detectedClips[j].RoundType);
            }

            //SpeedLoaders
            for (int k = 0; k < m_detectedSLs.Count; k++)
            {
                if (!allRoundTypes.Contains(m_detectedSLs[k].Chambers[0].Type))
                    allRoundTypes.Add(m_detectedSLs[k].Chambers[0].Type);
            }

            //Put all Round Types into a single list
            for (int i = 0; i < allRoundTypes.Count; i++)
            {
                //For each round Type, get its Ammo Type counter part and assign it

                AmmoRound ammoConvert = new AmmoRound();
                ammoConvert.roundType = allRoundTypes[i];
                ammoConvert.roundClasses = new List<AmmoClass>();
                ammoList.Add(ammoConvert);
            }

            ClearAmmoButtons();
            //Nothing in the scan zone
            if (ammoList.Count == 0)
            {
                selectionIcon.gameObject.SetActive(false);
                SetAmmoType(AmmoEnum.Standard);
                UpdateDisplayButtons();
                return;
            }
            else
            {
                //Populate Round Page
                if(roundPage.activeSelf)
                    PopulateRoundPage();
                SetAmmoType(selectedAmmoType);
            }

            UpdateAmmoList();
        }

        /// <summary>
        /// Repopulates the Ammo List with valid data for spawning depending on the selected Ammo Type
        /// </summary>
        void UpdateAmmoList()
        {
            //Loop through entire list
            for (int i = 0; i < ammoList.Count; i++)
            {
                //Loop Through Each of this RoundTypes Classes and see if we can find the equivalent
                int ammoCount = AM.STypeClassLists[ammoList[i].roundType].Count;

                ammoList[i].roundClasses.Clear();

                //Loop through all Round Classes (FMJ / AP / HE etc)
                for (int x = 0; x < ammoCount; x++)
                {
                    FireArmRoundClass classType = AM.STypeClassLists[ammoList[i].roundType][x];

                    AmmoClass newAmmo = new AmmoClass
                    {
                        roundClass = classType,
                        ammo = SR_Global.GetAmmoEnum(classType),
                        isOnlyType = true,
                    };
                    ammoList[i].roundClasses.Add(newAmmo);
                }
            }

            UpdateDisplayButtons();
        }

        void ClearAmmoButtons()
        {
            for (int i = 0; i < roundButtons.Count; i++)
            {
                if (roundButtons[i])
                    Destroy(roundButtons[i].gameObject);
            }
        }

        void UpdateDisplayButtons()
        {
            selectionIcon.gameObject.SetActive(false);
            for (int i = 0; i < ammoTypeButtons.Length; i++)
            {
                if (ammoTypeButtons[i] != null)
                    ammoTypeButtons[i].SetActive(false);
            }

            //Loop through entire list
            for (int i = 0; i < ammoList.Count; i++)
            {
                //Error Checking
                if (ammoList[i].roundClasses != null)
                {
                    for (int x = 0; x < ammoList[i].roundClasses.Count; x++)
                    {
                        if (ammoTypeButtons[(int)ammoList[i].roundClasses[x].ammo] == null
                            || SR_Manager.instance.character.ammoUpgradeCost[(int)ammoList[i].roundClasses[x].ammo] <= -1)
                            continue;

                        if (SR_Manager.instance.character.ammoUpgradeCost[(int)ammoList[i].roundClasses[x].ammo] >= 0
                            && ammoPage.activeSelf == true)
                        {
                            //Debug.Log("BUTTON ACTIVE!");
                            ammoTypeButtons[(int)ammoList[i].roundClasses[x].ammo].SetActive(true);

                            //If purchased, select it
                            if (purchasedAmmoTypes[(int)ammoList[i].roundClasses[x].ammo])
                            {
                                SetSelectionIcon((int)selectedAmmoType);
                            }
                        }
                    }
                }
            }

            //Selection
            if (ammoList.Count > 0 && ammoPage.activeSelf == true)
            {
                SetSelectionIcon((int)selectedAmmoType);
            }
        }

        void SetSelectionIcon(int i)
        {
            selectionIcon.gameObject.SetActive(true);
            selectionIcon.position = ammoTypeButtons[i].transform.position;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(ScanningVolume.position, ScanningVolume.localScale * 0.5f);
        }


        public class AmmoRound
        {
            public FireArmRoundType roundType;  //Actual definition bullet type (556 etc)
            public List<AmmoClass> roundClasses = new List<AmmoClass>();
        }

        public class AmmoClass
        {
            //public int index = -1;
            //public FireArmRoundType roundType;  //Actual definition bullet type (556 etc)
            public FireArmRoundClass roundClass;    //Type of Bullet, FMJ/API etc
            public AmmoEnum ammo;           //Ammo Counter Part
            public bool isOnlyType = false; //If True, only 1 type of ammo can be spawned, so it should always be spawned
            //public bool hasAmmo = false;
        }
    }
}