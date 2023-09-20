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

        private void Start()
        {
            colbuffer = new Collider[50];
        }

        public void Button_Upgrade()
        {
            
            if (!SR_Manager.EnoughPoints(SR_Manager.instance.character.upgradeMagazineCost))
            {
                SR_Manager.PlayFailSFX();
                return;
            }
            if (m_detectedMag == null)
            {
                SR_Manager.PlayFailSFX();
                return;
            }
            if (!IM.CompatMags.ContainsKey(m_detectedMag.MagazineType))
            {
                SR_Manager.PlayFailSFX();
                return;
            }
            List<FVRObject> list = IM.CompatMags[m_detectedMag.MagazineType];
            FVRObject fvrobject = null;
            int num = 10000;
            for (int i = 0; i < list.Count; i++)
            {
                if (!(list[i].ItemID == m_detectedMag.ObjectWrapper.ItemID))
                {
                    if (list[i].MagazineCapacity > m_detectedMag.m_capacity && list[i].MagazineCapacity < num)
                    {
                        fvrobject = list[i];
                        num = list[i].MagazineCapacity;
                    }
                }
            }
            if (fvrobject != null)
            {
                SR_Manager.SpendPoints(SR_Manager.instance.character.upgradeMagazineCost);
                Destroy(m_detectedMag.GameObject);
                GameObject gameObject = Instantiate(fvrobject.GetGameObject(), Spawnpoint_Mag.position, Spawnpoint_Mag.rotation);
                SR_Manager.PlayConfirmSFX();
            }
            else
                SR_Manager.PlayFailSFX();
        }

        public void Button_Duplicate()
        {
            if (m_detectedMag == null && m_detectedSL == null)
            {
                SR_Manager.PlayFailSFX();
                return;
            }
            if (m_detectedMag != null && m_detectedMag.IsEnBloc)
            {
                SR_Manager.PlayFailSFX();
                return;
            }
            if (SR_Manager.instance.character.duplicateMagazineCost < 1)
            {
                SR_Manager.PlayFailSFX();
                return;
            }
            
            if (SR_Manager.EnoughPoints(SR_Manager.instance.character.duplicateMagazineCost))
            {
                SR_Manager.PlayConfirmSFX();
                SR_Manager.SpendPoints(SR_Manager.instance.character.duplicateMagazineCost);

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
            if(m_detectedFirearms.Count > 0)
                SetMagButtonStatus(true);
            else
                SetMagButtonStatus(false);

            /*
            if (m_detectedMag == null && m_detectedSL == null)
            {
               // OCIconDupe.SetOption(TNH_ObjectConstructorIcon.IconState.Cancel, OCIconDupe.Sprite_Cancel, 0);
                m_storedDupeCost = 0;
            }
            else
            {
                //OCIconDupe.SetOption(TNH_ObjectConstructorIcon.IconState.Accept, OCIconDupe.Sprite_Accept, 0);
                m_storedDupeCost = 1;
            }
            */

            if (m_detectedMag != null)
            {
                if (!IM.CompatMags.ContainsKey(m_detectedMag.MagazineType))
                {
                    SR_Manager.PlayFailSFX();
                    return;
                }
                List<FVRObject> list = IM.CompatMags[m_detectedMag.MagazineType];
                FVRObject fvrobject = null;
                int num = 10000;
                for (int i = 0; i < list.Count; i++)
                {
                    if (!(list[i].ItemID == m_detectedMag.ObjectWrapper.ItemID))
                    {
                        if (list[i].MagazineCapacity > m_detectedMag.m_capacity && list[i].MagazineCapacity < num)
                        {
                            fvrobject = list[i];
                            num = list[i].MagazineCapacity;
                        }
                    }
                }
                if (fvrobject != null)
                {
                    SetButtonStatus(true);
                    m_hasUpgradeableMags = true;
                    //OCIconUpgrade.SetOption(TNH_ObjectConstructorIcon.IconState.Accept, OCIconUpgrade.Sprite_Accept, 0);
                }
                else
                {
                    SetButtonStatus(false);
                    //OCIconUpgrade.SetOption(TNH_ObjectConstructorIcon.IconState.Cancel, OCIconUpgrade.Sprite_Cancel, 0);
                    m_hasUpgradeableMags = false;
                }
            }
            else
            {
                SetButtonStatus(false);
                //OCIconUpgrade.SetOption(TNH_ObjectConstructorIcon.IconState.Cancel, OCIconUpgrade.Sprite_Cancel, 0);
                m_hasUpgradeableMags = false;
            }
        }

        public void Button_SpawnMagazine()
        {
            for (int i = 0; i < m_detectedFirearms.Count; i++)
            {
                if (IM.OD.ContainsKey(m_detectedFirearms[i].ObjectWrapper.ItemID))
                {
                    FVRObject fvrobject = IM.OD[m_detectedFirearms[i].ObjectWrapper.ItemID];
                    if (fvrobject.CompatibleMagazines.Count > 0)
                    {
                        FVRObject fvrobject3 = fvrobject.CompatibleMagazines[0];
                        GameObject gameObject2 = fvrobject3.GetGameObject();

                        if (SR_Manager.SpendPoints(SR_Manager.instance.character.newMagazineCost))
                        {
                            SR_Manager.PlayConfirmSFX();
                            GameObject gameObject3 = Instantiate(gameObject2, Spawnpoint_Mag.position + Vector3.up * (float)i * 0.1f, Spawnpoint_Mag.rotation);
                            FVRFireArmMagazine fvrfireArmMagazine = gameObject3.GetComponent<FVRFireArmMagazine>();
                            FireArmRoundType roundType2 = fvrfireArmMagazine.RoundType;
                            FireArmRoundClass classFromType2 = GetClassFromType(roundType2);
                            fvrfireArmMagazine.ReloadMagWithType(classFromType2);
                        }
                        else
                            SR_Manager.PlayFailSFX();
                    }
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

            magazineBtn.sprite = statusImages[status ? 0 : 1];
            magazineCostText.text = SR_Manager.instance.character.newMagazineCost.ToString();
        }

        void SetButtonStatus(bool status)
        {
            if (SR_Manager.instance.character.duplicateMagazineCost < 0)
                duplicateBtn.transform.parent.gameObject.SetActive(false);
            else
            {
                duplicateBtn.sprite = statusImages[status ? 0 : 1];
                duplicateCostText.text = SR_Manager.instance.character.duplicateMagazineCost.ToString();
            }

            if (SR_Manager.instance.character.upgradeMagazineCost < 0)
                upgradeBtn.transform.parent.gameObject.SetActive(false);
            else
            {
                upgradeBtn.sprite = statusImages[status ? 0 : 1];
                upgradeCostText.text = SR_Manager.instance.character.upgradeMagazineCost.ToString();
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(ScanningVolume.position, ScanningVolume.localScale * 0.5f);
        }

        public Transform Spawnpoint_Mag;

        public Transform ScanningVolume;

        public LayerMask ScanningLM;

        private FVRFireArmMagazine m_detectedMag;

        private Speedloader m_detectedSL;

        private Collider[] colbuffer;

        private int m_storedDupeCost = 1;

        private float m_scanTick = 1f;

        private bool m_hasUpgradeableMags = false;
    }
}