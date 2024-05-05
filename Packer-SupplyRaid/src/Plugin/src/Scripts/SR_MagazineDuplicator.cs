using FistVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SupplyRaid
{

    public class SR_MagazineDuplicator : MonoBehaviour
    {
        [SerializeField] Text upgradeCostText;
        [SerializeField] Text duplicateCostText;
        [SerializeField] Text magazineCostText;

        [SerializeField] Image duplicateBtn;
        [SerializeField] Image upgradeBtn;
        [SerializeField] Image magazineBtn;
        [SerializeField] Sprite[] statusImages;

        private List<FVRFireArm> m_detectedFirearms = new List<FVRFireArm>();
        private Dictionary<FireArmRoundType, FireArmRoundClass> m_decidedTypes = new Dictionary<FireArmRoundType, FireArmRoundClass>();
        private List<FVRObject.OTagEra> m_validEras = new List<FVRObject.OTagEra>();
        private List<FVRObject.OTagSet> m_validSets = new List<FVRObject.OTagSet>();

        public Transform selectedBox;

        private int costDuplicate = 0;
        private int costUpgrade = 0;
        private int costMagazine = 0;
        private FVRObject magazineUpgrade;

        public Transform Spawnpoint_Mag;
        public Transform ScanningVolume;
        public LayerMask ScanningLM;
        private FVRFireArmMagazine m_detectedMag;
        private FVRFireArmMagazine lastMag;
        private Speedloader m_detectedSL;
        private Collider[] colbuffer;
        private float m_scanTick = 1f;

        private void Start()
        {
            colbuffer = new Collider[50];
        }

        public void Button_Upgrade()
        {
            
            if (!SR_Manager.EnoughPoints(costUpgrade)
                || m_detectedMag == null)
            {
                SR_Manager.PlayFailSFX();
                return;
            }

            if (magazineUpgrade != null)
            {
                SR_Manager.SpendPoints(costUpgrade);
                Destroy(m_detectedMag.GameObject);
                GameObject gameObject = Instantiate(magazineUpgrade.GetGameObject(), Spawnpoint_Mag.position, Spawnpoint_Mag.rotation);
                SR_Manager.PlayConfirmSFX();
            }
            else
                SR_Manager.PlayFailSFX();
        }

        public void Button_Duplicate()
        {
            if (m_detectedMag == null && m_detectedSL == null
                || m_detectedMag != null && m_detectedMag.IsEnBloc
                || costDuplicate < 1)
            {
                SR_Manager.PlayFailSFX();
                return;
            }
            
            if (SR_Manager.EnoughPoints(costDuplicate))
            {
                SR_Manager.PlayConfirmSFX();
                SR_Manager.SpendPoints(costDuplicate);

                if (m_detectedMag != null)
                {
                    FVRObject fvrobject = m_detectedMag.ObjectWrapper;
                    GameObject gameObject = Instantiate(fvrobject.GetGameObject(), Spawnpoint_Mag.position, Spawnpoint_Mag.rotation);
                    FVRFireArmMagazine component = gameObject.GetComponent<FVRFireArmMagazine>();
                    for (int i = 0; i < Mathf.Min(m_detectedMag.LoadedRounds.Length, component.LoadedRounds.Length); i++)
                    {
                        if (m_detectedMag.LoadedRounds[i] != null && m_detectedMag.LoadedRounds[i].LR_Mesh != null)
                        {
                            component.LoadedRounds[i].LR_Class = m_detectedMag.LoadedRounds[i].LR_Class;
                            component.LoadedRounds[i].LR_Mesh = m_detectedMag.LoadedRounds[i].LR_Mesh;
                            component.LoadedRounds[i].LR_Material = m_detectedMag.LoadedRounds[i].LR_Material;
                            component.LoadedRounds[i].LR_ObjectWrapper = m_detectedMag.LoadedRounds[i].LR_ObjectWrapper;
                        }
                    }
                    component.m_numRounds = m_detectedMag.m_numRounds;
                    component.UpdateBulletDisplay();
                }
                else if (m_detectedSL != null)
                {
                    FVRObject fvrobject = m_detectedSL.ObjectWrapper;
                    GameObject gameObject2 = Instantiate(fvrobject.GetGameObject(), Spawnpoint_Mag.position, Spawnpoint_Mag.rotation);
                    Speedloader component2 = gameObject2.GetComponent<Speedloader>();
                    for (int j = 0; j < m_detectedSL.Chambers.Count; j++)
                    {
                        if (m_detectedSL.Chambers[j].IsLoaded)
                        {
                            component2.Chambers[j].Load(m_detectedSL.Chambers[j].LoadedClass, false);
                        }
                        else
                        {
                            component2.Chambers[j].Unload();
                        }
                    }
                }
                return;
            }
            SR_Manager.PlayFailSFX();
        }

        private void Update()
        {
            m_scanTick -= Time.deltaTime;
            if (m_scanTick <= 0f)
            {
                m_scanTick = Random.Range(0.2f, 0.3f);
                float num = Vector3.Distance(transform.position, GM.CurrentPlayerBody.transform.position);
                if (num < 6f)
                {
                    Scan();
                }
            }


            //Selection Box
            Transform target = null;

            if (m_detectedMag && m_detectedMag.GameObject)
                target = m_detectedMag.PoseOverride ? m_detectedMag.PoseOverride : m_detectedMag.transform;

            if(!target && m_detectedSL && m_detectedSL.GameObject)
                target = m_detectedSL.PoseOverride ? m_detectedSL.PoseOverride : m_detectedSL.transform;

            if(!target && m_detectedFirearms.Count > 0 && m_detectedFirearms[0] && m_detectedFirearms[0].GameObject)
                target = m_detectedFirearms[0].PoseOverride ? m_detectedFirearms[0].PoseOverride : m_detectedFirearms[0].transform;

            if (target)
            {
                if (selectedBox.gameObject.activeSelf == false)
                    selectedBox.gameObject.SetActive(true);
                selectedBox.position = target.position;
                selectedBox.rotation = target.rotation;
                selectedBox.localScale = target.localScale;
            }
            else if (selectedBox.gameObject.activeSelf == true)
                selectedBox.gameObject.SetActive(false);
        }

        private void Scan()
        {
            int num = Physics.OverlapBoxNonAlloc(ScanningVolume.position, ScanningVolume.localScale * 0.5f, colbuffer, ScanningVolume.rotation, ScanningLM, QueryTriggerInteraction.Collide);
            m_detectedMag = null;
            m_detectedSL = null;
            m_detectedFirearms.Clear();

            for (int i = 0; i < num; i++)
            {
                if (colbuffer[i].attachedRigidbody != null)
                {

                    FVRFireArm firearm = this.colbuffer[i].attachedRigidbody.gameObject.GetComponent<FVRFireArm>();
                    if (firearm != null)
                    {
                        if (!m_detectedFirearms.Contains(firearm))
                        {
                            m_detectedFirearms.Add(firearm);
                        }
                    }

                    FVRFireArmMagazine component = colbuffer[i].attachedRigidbody.gameObject.GetComponent<FVRFireArmMagazine>();
                    if (component != null && component.FireArm == null && !component.IsHeld && component.QuickbeltSlot == null && !component.IsIntegrated)
                    {
                        m_detectedMag = component;
                        break;
                    }
                    Speedloader component2 = colbuffer[i].attachedRigidbody.gameObject.GetComponent<Speedloader>();
                    if (component2 != null && !component2.IsHeld && component2.QuickbeltSlot == null && component2.IsPretendingToBeAMagazine)
                    {
                        m_detectedSL = component2;
                        break;
                    }

                    //TODO find attachment magazines.
                    /*
                    FVRObject attachment = colbuffer[i].attachedRigidbody.gameObject.GetComponent<FVRObject>();
                    if (attachment != null && !attachment.IsHeld && attachment.QuickbeltSlot == null)
                    {
                        m_detectedSL = attachment.attac;
                        break;
                    }
                    */
                }
            }
            SetCostBasedOnMag();
        }

        private void SetCostBasedOnMag()
        {
            if (m_detectedFirearms.Count > 0)
                SetMagButtonStatus(true);
            else
                SetMagButtonStatus(false);

            if (m_detectedMag != null && lastMag != m_detectedMag)
            {
                lastMag = m_detectedMag;

                //Duplicate Magazine Cost Calculation
                int powerIndex = (int)m_detectedMag.ObjectWrapper.TagFirearmRoundPower;
                float multiplier = SR_Manager.instance.character.powerMultiplier[powerIndex];

                if (SR_Manager.instance.character.perRound)
                    costDuplicate = Mathf.CeilToInt(SR_Manager.instance.character.duplicateMagazineCost * m_detectedMag.m_capacity * multiplier);
                else
                    costDuplicate = Mathf.CeilToInt(SR_Manager.instance.character.duplicateMagazineCost * multiplier);

                //Can duplicate Magazine
                SetDuplicateStatus(true);


                /*
                IM.OD.ContainsKey(m_detectedMag.ObjectWrapper.ItemID);
                //Upgradable Check
                if (!IM.CompatMags.ContainsKey(m_detectedMag.MagazineType))
                {
                    SetUpgradeStatus(false);
                    m_hasUpgradeableMags = false;
                    SetDuplicateStatus(false);
                    return;
                }
                */

                //Populate Magazines
                List<FVRObject> mags = new List<FVRObject>(ManagerSingleton<IM>.Instance.odicTagCategory[FVRObject.ObjectCategory.Magazine]);
                for (int i = mags.Count - 1; i >= 0; i--)
                {
                    if (mags[i].MagazineType != m_detectedMag.MagazineType)
                        mags.RemoveAt(i);
                }

                //List <FVRObject> mags = IM.CompatMags[m_detectedMag.MagazineType];
                int num = 10000;
                for (int i = 0; i < mags.Count; i++)
                {
                    if (!(mags[i].ItemID == m_detectedMag.ObjectWrapper.ItemID))
                    {
                        if (mags[i].MagazineCapacity > m_detectedMag.m_capacity && mags[i].MagazineCapacity < num)
                        {
                            magazineUpgrade = mags[i];
                            num = mags[i].MagazineCapacity;
                        }
                    }
                }

                if (magazineUpgrade != null)
                {
                    powerIndex = (int)magazineUpgrade.TagFirearmRoundPower;
                    multiplier = SR_Manager.instance.character.powerMultiplier[powerIndex];

                    //Update Magazine Cost Calculation
                    if (SR_Manager.instance.character.perRound)
                        costUpgrade = Mathf.CeilToInt(SR_Manager.instance.character.upgradeMagazineCost * m_detectedMag.m_capacity * multiplier);
                    else
                        costUpgrade = Mathf.CeilToInt(SR_Manager.instance.character.upgradeMagazineCost * multiplier);

                    if (magazineUpgrade.ItemID == m_detectedMag.ObjectWrapper.ItemID)
                    {
                        SetUpgradeStatus(false);
                    }
                    else
                    {
                        SetUpgradeStatus(true);
                    }
                }
                else
                {
                    SetUpgradeStatus(false);
                }
            }
            else if(m_detectedMag == null)
            {
                SetUpgradeStatus(false);
                SetDuplicateStatus(false);
            }
        }

        public void Button_SpawnMagazine()
        {
            if (!m_detectedFirearms[0])
            {
                SR_Manager.PlayFailSFX();
                return;
            }

            if (IM.OD.ContainsKey(m_detectedFirearms[0].ObjectWrapper.ItemID))
            {
                FVRObject fvrobject = IM.OD[m_detectedFirearms[0].ObjectWrapper.ItemID];
                if (fvrobject.CompatibleMagazines.Count > 0)
                {
                    FVRObject fvrobject3 = fvrobject.CompatibleMagazines[0];
                    GameObject gameObject2 = fvrobject3.GetGameObject();

                    if (SR_Manager.SpendPoints(costMagazine))
                    {
                        SR_Manager.PlayConfirmSFX();
                        GameObject gameObject3 = Instantiate(gameObject2, Spawnpoint_Mag.position + Vector3.up * 0.1f, Spawnpoint_Mag.rotation);
                        FVRFireArmMagazine fvrfireArmMagazine = gameObject3.GetComponent<FVRFireArmMagazine>();
                        FireArmRoundType roundType2 = fvrfireArmMagazine.RoundType;
                        FireArmRoundClass classFromType2 = GetClassFromType(roundType2);
                        fvrfireArmMagazine.ReloadMagWithType(classFromType2);
                    }
                    else
                        SR_Manager.PlayFailSFX();
                    //Only do it for one weapon
                    return;
                }
            }
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

        void SetMagButtonStatus(bool status)
        {
            if (SR_Manager.instance.character.newMagazineCost < 0)
            {
                magazineBtn.transform.parent.gameObject.SetActive(false);
                return;
            }

            if (!magazineUpgrade)
                return;

            int powerIndex = (int)magazineUpgrade.TagFirearmRoundPower;
            float multiplier = SR_Manager.instance.character.powerMultiplier[powerIndex];

            //New Magazine Cost Calculation
            if (SR_Manager.instance.character.perRound)
            {
                if (m_detectedFirearms.Count <= 0 || m_detectedFirearms[0] == null)
                    return;

                if (m_detectedFirearms[0] &&
                    IM.OD.ContainsKey(m_detectedFirearms[0].ObjectWrapper.ItemID))
                {
                    FVRObject fvrobject = IM.OD[m_detectedFirearms[0].ObjectWrapper.ItemID];

                    if (fvrobject == null
                        || fvrobject.CompatibleMagazines == null
                        || fvrobject.CompatibleMagazines.Count <= 0)
                        return;
                    costMagazine = Mathf.CeilToInt(SR_Manager.instance.character.newMagazineCost * fvrobject.CompatibleMagazines[0].MagazineCapacity * multiplier);
                }
            }
            else
            {
                costMagazine = Mathf.CeilToInt(SR_Manager.instance.character.newMagazineCost * multiplier);
            }

            magazineBtn.sprite = statusImages[status ? 0 : 1];
            magazineCostText.text = costMagazine.ToString();
        }

        void SetUpgradeStatus(bool status)
        {

            if (costUpgrade < 0)
            {
                upgradeBtn.transform.parent.gameObject.SetActive(false);
                return;
            }
            else
            {
                upgradeBtn.sprite = statusImages[status ? 0 : 1];
                upgradeCostText.text = costUpgrade.ToString();
            }
        }

        void SetDuplicateStatus(bool status)
        {
            if (costDuplicate < 0)
                duplicateBtn.transform.parent.gameObject.SetActive(false);
            else
            {
                duplicateBtn.sprite = statusImages[status ? 0 : 1];
                duplicateCostText.text = costDuplicate.ToString();
            }

        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(ScanningVolume.position, ScanningVolume.localScale * 0.5f);
        }
    }
}