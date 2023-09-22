﻿using System.Collections.Generic;
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

        public bool[] purchased = new bool[28];    //Ammo Enum Length
        public GameObject rearmButton;
        public GameObject speedloaderButton;
        public GameObject clipButton;
        public GameObject roundButton;

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
        private List<FVRObject.OTagEra> m_validEras = new List<FVRObject.OTagEra>();
        private List<FVRObject.OTagSet> m_validSets = new List<FVRObject.OTagSet>();

        private float m_scanTick = 1f;

        //Ammo
        List<AmmoRound> ammoList = new List<AmmoRound>();
        List<FireArmRoundType> allRoundTypes = new List<FireArmRoundType>();

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
        }

        public void Setup()
        {
            for (int i = 0; i < ammoTypeButtons.Length; i++)
            {
                SR_GenericButton btn = ammoTypeButtons[i].GetComponent<SR_GenericButton>();
                //btn.index = i;
                btn.text.text = SR_Manager.instance.character.ammoUpgradeCost[i].ToString();

                //Disable Buttons here after setup
            }

            if(SR_Manager.instance.character.modeRearming == 0)
                rearmButton.SetActive(false);
            else
                rearmButton.SetActive(true);

            if (SR_Manager.instance.character.modeSpeedLoaders == 0)
                speedloaderButton.SetActive(false);
            else
                speedloaderButton.SetActive(true);

            if (SR_Manager.instance.character.modeClips == 0)
                clipButton.SetActive(false);
            else
                clipButton.SetActive(true);

            if (SR_Manager.instance.character.modeRounds == 0)
                roundButton.SetActive(false);
            else
                roundButton.SetActive(true);
        }

        public void SetAmmoType(AmmoEnum ammo)
        {
            //TODO check if we can set the ammo for this weapon
            selectedAmmoType = ammo;
            selectionIcon.position = ammoTypeButtons[(int)selectedAmmoType].transform.position;
        }

        public void Button_SpawnRound()
        {
            if (m_roundTypes == null || m_roundTypes.Count < 1 || ammoList == null)
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
            bool flag = false;
            for (int i = 0; i < m_detectedFirearms.Count; i++)
            {
                if (IM.OD.ContainsKey(m_detectedFirearms[i].ObjectWrapper.ItemID))
                {
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

                            FVRObject fvrobject = IM.OD[m_detectedFirearms[i].ObjectWrapper.ItemID];

                            if (fvrobject.CompatibleSpeedLoaders.Count > 0
                                && ammoList[x].roundType == fvrobject.CompatibleSpeedLoaders[0].RoundType)
                            {

                                GameObject gameObject = fvrobject.CompatibleSpeedLoaders[0].GetGameObject();
                                Speedloader speedloader = gameObject.GetComponent<Speedloader>();
                                if (!speedloader.IsPretendingToBeAMagazine)
                                {
                                    flag = true;
                                    GameObject newSpeedLoader = Instantiate(gameObject, Spawnpoint_Round.position + Vector3.up * i * 0.1f, Spawnpoint_Round.rotation);
                                    speedloader = newSpeedLoader.GetComponent<Speedloader>();
                                    speedloader.ReloadClipWithType(ammoList[x].roundClasses[y].roundClass);
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

        public void Button_SpawnClip()
        {
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
                    for (int i = 0; i < m_detectedMags.Count; i++)
                    {
                        if (ammoList[x].roundType == m_detectedMags[i].RoundType)
                        {
                            m_detectedMags[i].ReloadMagWithType(ammoList[x].roundClasses[y].roundClass);
                        }
                    }

                    //Clip Reload
                    for (int j = 0; j < m_detectedClips.Count; j++)
                    {
                        if (ammoList[x].roundType == m_detectedClips[j].RoundType)
                        {
                            m_detectedClips[j].ReloadClipWithType(ammoList[x].roundClasses[y].roundClass);
                        }
                    }

                    //SL
                    for (int k = 0; k < m_detectedSLs.Count; k++)
                    {
                        if (ammoList[x].roundType == m_detectedSLs[k].Chambers[0].Type)
                        {
                            m_detectedSLs[k].ReloadClipWithType(ammoList[x].roundClasses[y].roundClass);
                        }
                    }

                    //SWeapons
                    for (int l = 0; l < m_detectedSweapons.Count; l++)
                    {
                        m_detectedSweapons[l].W.InstaReload();
                    }
                }
            }
            SR_Manager.PlayRearmSFX();
        }

        private FireArmRoundClass GetClassFromType(FireArmRoundType t)
        {
            if (!m_decidedTypes.ContainsKey(t))
            {
                List<FireArmRoundClass> list = new List<FireArmRoundClass>();
                for (int i = 0; i < AM.SRoundDisplayDataDic[t].Classes.Length; i++)
                {
                    FVRObject objectID = AM.SRoundDisplayDataDic[t].Classes[i].ObjectID;
                    if (m_validEras.Contains(objectID.TagEra) && m_validSets.Contains(objectID.TagSet))
                    {
                        list.Add(AM.SRoundDisplayDataDic[t].Classes[i].Class);
                    }
                }
                if (list.Count > 0)
                {
                    m_decidedTypes.Add(t, list[Random.Range(0, list.Count)]);
                }
                else
                {
                    m_decidedTypes.Add(t, AM.GetRandomValidRoundClass(t));
                }
            }
            return m_decidedTypes[t];
        }

        private void Scan()
        {
            int num = Physics.OverlapBoxNonAlloc(ScanningVolume.position, ScanningVolume.localScale * 0.5f, colbuffer, ScanningVolume.rotation, ScanningLM, QueryTriggerInteraction.Collide);
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
                    if (component != null)
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
                    if (component3 != null && component3.FireArm == null && !m_detectedMags.Contains(component3))
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

            //Nothing in the scan zone
            if (ammoList.Count == 0)
            {
                selectionIcon.gameObject.SetActive(false);
                SetAmmoType(AmmoEnum.Standard);
                UpdateDisplayButtons();
                return;
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

        void UpdateDisplayButtons()
        {
            selectionIcon.gameObject.SetActive(false);
            for (int i = 0; i < ammoTypeButtons.Length; i++)
            {
                if (ammoTypeButtons[i] != null)
                    ammoTypeButtons[i].SetActive(false);
            }


            //Selection
            if (ammoList.Count > 0)
            {
                selectionIcon.gameObject.SetActive(true);
                selectionIcon.position = ammoTypeButtons[(int)selectedAmmoType].transform.position;
            }

            //Loop through entire list
            for (int i = 0; i < ammoList.Count; i++)
            {
                //Error Checking
                if (ammoList[i].roundClasses != null)
                {
                    for (int x = 0; x < ammoList[i].roundClasses.Count; x++)
                    {
                        //Debug.Log("Class Btn: " + ammoList[i].roundClasses[x].ammo);
                        if (ammoTypeButtons[(int)ammoList[i].roundClasses[x].ammo] != null
                            && SR_Manager.instance.character.ammoUpgradeCost[(int)ammoList[i].roundClasses[x].ammo] >= 0)
                        {
                            //Debug.Log("BUTTON ACTIVE!");
                            ammoTypeButtons[(int)ammoList[i].roundClasses[x].ammo].SetActive(true);
                        }
                    }
                }
            }
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