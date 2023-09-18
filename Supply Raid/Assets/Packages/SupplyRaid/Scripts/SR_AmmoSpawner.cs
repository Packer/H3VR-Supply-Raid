using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FistVR;

namespace SupplyRaid
{
	public class SR_AmmoSpawner : MonoBehaviour
    {
        public int seed = 11;

        private void Start()
        {
            colbuffer = new Collider[50];
        }

        private void Update()
        {
            m_scanTick -= Time.deltaTime;
            if (m_scanTick <= 0f)
            {
                float num = Vector3.Distance(base.transform.position, GM.CurrentPlayerBody.transform.position);
                if (num < 12f)
                {
                    Scan();
                    m_scanTick = UnityEngine.Random.Range(0.8f, 1f);
                }
                else
                    m_scanTick = UnityEngine.Random.Range(2f, 3f);
            }
        }

        public void SetRandomSeed()
        {
            seed = Random.Range(0, 1000);
        }

        public void Button_SpawnRound()
        {
            Random.InitState(seed);

            if (this.m_roundTypes.Count < 1)
            {
                SR_Manager.PlayFailSFX();
                return;
            }
            SR_Manager.PlayFailSFX();
            for (int i = 0; i < this.m_roundTypes.Count; i++)
            {
                FireArmRoundType fireArmRoundType = this.m_roundTypes[i];
                //FireArmRoundClass classFromType = this.GetClassFromType(fireArmRoundType);
                FireArmRoundClass classFromType = AM.GetRandomValidRoundClass(fireArmRoundType);
                FVRObject roundSelfPrefab = AM.GetRoundSelfPrefab(fireArmRoundType, classFromType);
                UnityEngine.Object.Instantiate<GameObject>(roundSelfPrefab.GetGameObject(), this.Spawnpoint_Round.position + Vector3.up * (float)i * 0.1f, this.Spawnpoint_Round.rotation);
            }
        }

        public void Button_SpawnSpeedLoader()
        {
            Random.InitState(seed);

            bool flag = false;
            for (int i = 0; i < this.m_detectedFirearms.Count; i++)
            {
                if (IM.OD.ContainsKey(this.m_detectedFirearms[i].ObjectWrapper.ItemID))
                {
                    FVRObject fvrobject = IM.OD[this.m_detectedFirearms[i].ObjectWrapper.ItemID];
                    if (fvrobject.CompatibleSpeedLoaders.Count > 0)
                    {
                        FVRObject fvrobject2 = fvrobject.CompatibleSpeedLoaders[0];
                        GameObject gameObject = fvrobject2.GetGameObject();
                        Speedloader speedloader = gameObject.GetComponent<Speedloader>();
                        if (!speedloader.IsPretendingToBeAMagazine)
                        {
                            flag = true;
                            GameObject gameObject2 = Instantiate<GameObject>(gameObject, Spawnpoint_Round.position + Vector3.up * (float)i * 0.1f, this.Spawnpoint_Round.rotation);
                            speedloader = gameObject2.GetComponent<Speedloader>();
                            FireArmRoundType type = speedloader.Chambers[0].Type;
                            FireArmRoundClass classFromType = GetClassFromType(type);
                            speedloader.ReloadClipWithType(classFromType);
                        }
                    }
                }
            }
            if (flag)
            {
                SR_Manager.PlayConfirmSFX();
                return;
            }
            SR_Manager.PlayFailSFX();
        }

        public void Button_SpawnClip()
        {
            Random.InitState(seed);

            bool flag = false;
            for (int i = 0; i < this.m_detectedFirearms.Count; i++)
            {
                if (IM.OD.ContainsKey(this.m_detectedFirearms[i].ObjectWrapper.ItemID))
                {
                    FVRObject fvrobject = IM.OD[this.m_detectedFirearms[i].ObjectWrapper.ItemID];
                    if (fvrobject.CompatibleClips.Count > 0)
                    {
                        FVRObject fvrobject2 = fvrobject.CompatibleClips[0];
                        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(fvrobject2.GetGameObject(), this.Spawnpoint_Round.position + Vector3.up * (float)i * 0.1f, this.Spawnpoint_Round.rotation);
                        FireArmRoundType roundType = this.m_detectedFirearms[i].RoundType;
                        FireArmRoundClass classFromType = this.GetClassFromType(roundType);
                        FVRFireArmClip component = gameObject.GetComponent<FVRFireArmClip>();
                        component.ReloadClipWithType(classFromType);
                    }
                    else if (fvrobject.CompatibleMagazines.Count > 0)
                    {
                        FVRObject fvrobject3 = fvrobject.CompatibleMagazines[0];
                        GameObject gameObject2 = fvrobject3.GetGameObject();
                        FVRFireArmMagazine fvrfireArmMagazine = gameObject2.GetComponent<FVRFireArmMagazine>();
                        if (fvrfireArmMagazine.IsEnBloc)
                        {
                            flag = true;
                            GameObject gameObject3 = UnityEngine.Object.Instantiate<GameObject>(gameObject2, this.Spawnpoint_Round.position + Vector3.up * (float)i * 0.1f, this.Spawnpoint_Round.rotation);
                            fvrfireArmMagazine = gameObject3.GetComponent<FVRFireArmMagazine>();
                            FireArmRoundType roundType2 = fvrfireArmMagazine.RoundType;
                            FireArmRoundClass classFromType2 = this.GetClassFromType(roundType2);
                            fvrfireArmMagazine.ReloadMagWithType(classFromType2);
                        }
                    }
                }
            }
            if (flag)
            {
                SR_Manager.PlayConfirmSFX();
                return;
            }
            SR_Manager.PlayFailSFX();
        }

        public void Button_ReloadGuns()
        {
            Random.InitState(seed);

            if (this.m_detectedMags.Count < 1 && this.m_detectedClips.Count < 1 && this.m_detectedSLs.Count < 1 && this.m_detectedSweapons.Count < 1)
            {
                SR_Manager.PlayFailSFX();
                return;
            }
            SR_Manager.PlayRearmSFX();
            for (int i = 0; i < this.m_detectedMags.Count; i++)
            {
                FireArmRoundType roundType = this.m_detectedMags[i].RoundType;
                //FireArmRoundClass classFromType = this.GetClassFromType(roundType);
                FireArmRoundClass classFromType = AM.GetRandomValidRoundClass(roundType);
                this.m_detectedMags[i].ReloadMagWithType(classFromType);
            }
            for (int j = 0; j < this.m_detectedClips.Count; j++)
            {
                FireArmRoundType roundType2 = this.m_detectedClips[j].RoundType;
                //FireArmRoundClass classFromType2 = this.GetClassFromType(roundType2);
                FireArmRoundClass classFromType2 = AM.GetRandomValidRoundClass(roundType2);
                this.m_detectedClips[j].ReloadClipWithType(classFromType2);
            }
            for (int k = 0; k < this.m_detectedSLs.Count; k++)
            {
                FireArmRoundType type = this.m_detectedSLs[k].Chambers[0].Type;
                //FireArmRoundClass classFromType3 = this.GetClassFromType(type);
                FireArmRoundClass classFromType3 = AM.GetRandomValidRoundClass(type);
                this.m_detectedSLs[k].ReloadClipWithType(classFromType3);
            }
            for (int l = 0; l < this.m_detectedSweapons.Count; l++)
            {
                this.m_detectedSweapons[l].W.InstaReload();
            }
        }

        private FireArmRoundClass GetClassFromType(FireArmRoundType t)
        {
            if (!this.m_decidedTypes.ContainsKey(t))
            {
                List<FireArmRoundClass> list = new List<FireArmRoundClass>();
                for (int i = 0; i < AM.SRoundDisplayDataDic[t].Classes.Length; i++)
                {
                    FVRObject objectID = AM.SRoundDisplayDataDic[t].Classes[i].ObjectID;
                    if (this.m_validEras.Contains(objectID.TagEra) && this.m_validSets.Contains(objectID.TagSet))
                    {
                        list.Add(AM.SRoundDisplayDataDic[t].Classes[i].Class);
                    }
                }
                if (list.Count > 0)
                {
                    this.m_decidedTypes.Add(t, list[UnityEngine.Random.Range(0, list.Count)]);
                }
                else
                {
                    this.m_decidedTypes.Add(t, AM.GetRandomValidRoundClass(t));
                }
            }
            return this.m_decidedTypes[t];
        }

        private void Scan()
        {
            int num = Physics.OverlapBoxNonAlloc(this.ScanningVolume.position, this.ScanningVolume.localScale * 0.5f, this.colbuffer, this.ScanningVolume.rotation, this.ScanningLM, QueryTriggerInteraction.Collide);
            this.m_roundTypes.Clear();
            this.m_detectedMags.Clear();
            this.m_detectedClips.Clear();
            this.m_detectedSLs.Clear();
            this.m_detectedSweapons.Clear();
            this.m_detectedFirearms.Clear();
            for (int i = 0; i < num; i++)
            {
                if (this.colbuffer[i].attachedRigidbody != null)
                {
                    FVRFireArm component = this.colbuffer[i].attachedRigidbody.gameObject.GetComponent<FVRFireArm>();
                    if (component != null)
                    {
                        if (!this.m_detectedFirearms.Contains(component))
                        {
                            this.m_detectedFirearms.Add(component);
                        }
                        if (!this.m_roundTypes.Contains(component.RoundType))
                        {
                            this.m_roundTypes.Add(component.RoundType);
                        }
                        if (component.Magazine != null && !this.m_detectedMags.Contains(component.Magazine))
                        {
                            this.m_detectedMags.Add(component.Magazine);
                        }
                        if (component.Attachments.Count > 0)
                        {
                            for (int j = 0; j < component.Attachments.Count; j++)
                            {
                                if (component.Attachments[j] is AttachableFirearmPhysicalObject)
                                {
                                    if (!this.m_roundTypes.Contains((component.Attachments[j] as AttachableFirearmPhysicalObject).FA.RoundType))
                                    {
                                        this.m_roundTypes.Add((component.Attachments[j] as AttachableFirearmPhysicalObject).FA.RoundType);
                                    }
                                    if ((component.Attachments[j] as AttachableFirearmPhysicalObject).FA.Magazine != null)
                                    {
                                        this.m_detectedMags.Add((component.Attachments[j] as AttachableFirearmPhysicalObject).FA.Magazine);
                                    }
                                }
                            }
                        }
                        if (component.GetIntegratedAttachableFirearm() != null && !this.m_roundTypes.Contains(component.GetIntegratedAttachableFirearm().RoundType))
                        {
                            this.m_roundTypes.Add(component.GetIntegratedAttachableFirearm().RoundType);
                        }
                    }
                    AttachableFirearmPhysicalObject component2 = this.colbuffer[i].attachedRigidbody.gameObject.GetComponent<AttachableFirearmPhysicalObject>();
                    if (component2 != null && !this.m_roundTypes.Contains(component2.FA.RoundType))
                    {
                        this.m_roundTypes.Add(component2.FA.RoundType);
                    }
                    FVRFireArmMagazine component3 = this.colbuffer[i].attachedRigidbody.gameObject.GetComponent<FVRFireArmMagazine>();
                    if (component3 != null && component3.FireArm == null && !this.m_detectedMags.Contains(component3))
                    {
                        this.m_detectedMags.Add(component3);
                    }
                    FVRFireArmClip component4 = this.colbuffer[i].attachedRigidbody.gameObject.GetComponent<FVRFireArmClip>();
                    if (component4 != null && component4.FireArm == null && !this.m_detectedClips.Contains(component4))
                    {
                        this.m_detectedClips.Add(component4);
                    }
                    Speedloader component5 = this.colbuffer[i].attachedRigidbody.gameObject.GetComponent<Speedloader>();
                    if (component5 != null && !this.m_detectedSLs.Contains(component5))
                    {
                        this.m_detectedSLs.Add(component5);
                    }
                    if (SR_Manager.instance.shakeReloading == TNH_SosiggunShakeReloading.On)
                    {
                        SosigWeaponPlayerInterface component6 = this.colbuffer[i].attachedRigidbody.gameObject.GetComponent<SosigWeaponPlayerInterface>();
                        if (component6 != null && !this.m_detectedSweapons.Contains(component6))
                        {
                            this.m_detectedSweapons.Add(component6);
                        }
                    }
                }
            }
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(this.ScanningVolume.position, this.ScanningVolume.localScale * 0.5f);
        }


        public Transform Spawnpoint_Round;

        public Transform ScanningVolume;

        public LayerMask ScanningLM;

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

        //[SerializeField] AudioSource audioSource;
        //[SerializeField] AudioClip[] clips;
    }
}