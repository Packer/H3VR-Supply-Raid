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
        private Collider[] colbuffer;
        private float m_scanTick = 1f;

        private bool canUpgrade = false;
        private bool canDuplicate = false;
        private bool canBuyNewMag = false;

        private void Start()
        {
            colbuffer = new Collider[50];
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

            if (selectedBox == null)
                return;

            //Selection Box
            Transform target = null;

            if (m_detectedMag && m_detectedMag.GameObject)
                target = m_detectedMag.PoseOverride ? m_detectedMag.PoseOverride : m_detectedMag.transform;

            if (!target && m_detectedFirearms.Count > 0 && m_detectedFirearms[0] && m_detectedFirearms[0].GameObject)
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
            int num = Physics.OverlapBoxNonAlloc(
                ScanningVolume.position, 
                ScanningVolume.localScale * 0.5f, 
                colbuffer, 
                ScanningVolume.rotation, 
                ScanningLM, QueryTriggerInteraction.Collide);

            magazineUpgrade = null;
            m_detectedMag = null;
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
            SetupCosts();
        }

        private void SetupCosts()
        {
            //-----------------------------
            // New Magazine
            //-----------------------------
            if (m_detectedFirearms.Count > 0)
                SetNewMagazineStatus(true);
            else
                SetNewMagazineStatus(false);

            if (m_detectedMag != null)
            {
                //-----------------------------
                // Duplicate Magazine
                //-----------------------------
                int powerIndex = (int)m_detectedMag.ObjectWrapper.TagFirearmRoundPower;
                float multiplier = SR_Manager.instance.character.powerMultiplier[powerIndex];

                costDuplicate = SR_Manager.Character().GetRoundCost(
                        SR_Manager.instance.character.duplicateMagazineCost,
                        m_detectedMag.m_capacity,
                        multiplier);

                //Can duplicate Magazine
                SetDuplicateStatus(true);

                //-----------------------------
                // Magazine Upgrade
                //-----------------------------

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
                    costUpgrade = SR_Manager.Character().GetRoundCost(
                        SR_Manager.instance.character.upgradeMagazineCost,
                        magazineUpgrade.MagazineCapacity,
                        multiplier);

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
            else
            {
                SetUpgradeStatus(false);
                SetDuplicateStatus(false);
            }
        }

        //-------------------------------------------------------------------
        // Duplicate
        //-------------------------------------------------------------------

        public void Button_Duplicate()
        {
            if (!canDuplicate
                || m_detectedMag == null
                || m_detectedMag != null && m_detectedMag.IsEnBloc
                || m_detectedMag.gameObject == null)
            {
                SR_Manager.PlayFailSFX();
                return;
            }

            if (SR_Manager.EnoughPoints(costDuplicate))
            {
                SR_Manager.PlayConfirmSFX();
                SR_Manager.SpendPoints(costDuplicate);

                Instantiate(m_detectedMag.gameObject, Spawnpoint_Mag.position, Spawnpoint_Mag.rotation);
            }
            else
                SR_Manager.PlayFailSFX();
        }

        void SetDuplicateStatus(bool status)
        {
            if (costDuplicate < 0)
            {
                duplicateBtn.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                duplicateBtn.sprite = statusImages[status ? 0 : 1];
                duplicateCostText.text = costDuplicate.ToString();
                canDuplicate = status;
            }
        }

        //-------------------------------------------------------------------
        // Upgrade Magazine
        //-------------------------------------------------------------------

        public void Button_Upgrade()
        {
            if (!canUpgrade
                || !SR_Manager.EnoughPoints(costUpgrade)
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
                canUpgrade = status;
            }
        }

        //-------------------------------------------------------------------
        // New Magazine
        //-------------------------------------------------------------------

        private FVRObject newMagazine;

        public void Button_SpawnMagazine()
        {
            if (!canBuyNewMag
                || m_detectedFirearms == null
                || m_detectedFirearms.Count <= 0
                || !m_detectedFirearms[0])
            {
                SR_Manager.PlayFailSFX();
                return;
            }

            if (newMagazine != null)
            {
                GameObject magazinePrefab = newMagazine.GetGameObject();

                if (magazinePrefab != null && SR_Manager.SpendPoints(costMagazine))
                {
                    GameObject spawnedMagazine = Instantiate(magazinePrefab, Spawnpoint_Mag.position + Vector3.up * 0.15f, Spawnpoint_Mag.rotation);

                    if (spawnedMagazine != null)
                    {
                        FVRFireArmMagazine fvrfireArmMagazine = spawnedMagazine.GetComponent<FVRFireArmMagazine>();

                        if (fvrfireArmMagazine)
                        {
                            FireArmRoundType roundType2 = fvrfireArmMagazine.RoundType;
                            FireArmRoundClass classFromType2 = GetClassFromType(roundType2);
                            fvrfireArmMagazine.ReloadMagWithType(classFromType2);

                            SR_Manager.PlayConfirmSFX();
                        }
                    }
                }
                else
                    SR_Manager.PlayFailSFX();
            }
        }

        void SetNewMagazineStatus(bool status)
        {
            //Disabled Button
            if (SR_Manager.instance.character.newMagazineCost < 0)
            {
                magazineBtn.transform.parent.gameObject.SetActive(false);
                return;
            }

            //Status
            newMagazine = null;
            magazineBtn.sprite = statusImages[status ? 0 : 1];
            canBuyNewMag = status;

            if (status == false)
            {
                magazineBtn.sprite = statusImages[1];
                canBuyNewMag = false;
                magazineCostText.text = "";
                return;
            }

            //Get New Magazine
            if (m_detectedFirearms[0] && IM.OD.ContainsKey(m_detectedFirearms[0].ObjectWrapper.ItemID))
            {
                FVRObject firearmFVR = IM.OD[m_detectedFirearms[0].ObjectWrapper.ItemID];
                
                /*
                FVRObject lowestCapacity = firearmFVR.CompatibleMagazines[0];
                for (int i = 1; i < firearmFVR.CompatibleMagazines.Count; i++)
                {
                    if (firearmFVR.CompatibleMagazines[i].MagazineCapacity < lowestCapacity.MagazineCapacity)
                        lowestCapacity = firearmFVR.CompatibleMagazines[i];
                }
                */

                newMagazine = SR_Global.GetLowestCapacityAmmoObject(firearmFVR);
            }

            //Missing or Failed Magazine, probably a bad mod
            if (newMagazine == null)
            {
                magazineBtn.sprite = statusImages[1];
                canBuyNewMag = false;
                magazineCostText.text = "";
                return;
            }

            //New Magazine Cost Calculation
            int powerIndex = (int)newMagazine.TagFirearmRoundPower;
            float multiplier = SR_Manager.instance.character.powerMultiplier[powerIndex];

            costMagazine = SR_Manager.Character().GetRoundCost(
                SR_Manager.instance.character.newMagazineCost,
                newMagazine.MagazineCapacity,
                multiplier);

            magazineCostText.text = costMagazine.ToString();
        }

        //-------------------------------------------------------------------
        // Misc
        //-------------------------------------------------------------------

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

        //-------------------------------------------------------------------
        // Editor
        //-------------------------------------------------------------------

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(ScanningVolume.position, ScanningVolume.localScale * 0.5f);
        }
    }
}