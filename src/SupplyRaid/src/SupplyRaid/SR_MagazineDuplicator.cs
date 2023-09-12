using FistVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SupplyRaid
{

	public class SR_MagazineDuplicator : MonoBehaviour
    {
        private void Start()
        {
            this.colbuffer = new Collider[50];
        }

        public void Button_Upgrade()
        {
            
            if (SR_Manager.instance.Points < 3)
            {
                audioSource.PlayOneShot(clips[1]);
                return;
            }
            if (this.m_detectedMag == null)
            {
                audioSource.PlayOneShot(clips[1]);
                return;
            }
            if (!IM.CompatMags.ContainsKey(this.m_detectedMag.MagazineType))
            {
                audioSource.PlayOneShot(clips[1]);
                return;
            }
            List<FVRObject> list = IM.CompatMags[this.m_detectedMag.MagazineType];
            FVRObject fvrobject = null;
            int num = 10000;
            for (int i = 0; i < list.Count; i++)
            {
                if (!(list[i].ItemID == this.m_detectedMag.ObjectWrapper.ItemID))
                {
                    if (list[i].MagazineCapacity > this.m_detectedMag.m_capacity && list[i].MagazineCapacity < num)
                    {
                        fvrobject = list[i];
                        num = list[i].MagazineCapacity;
                    }
                }
            }
            if (fvrobject != null)
            {
                SR_Manager.SpendPoints(3);
                UnityEngine.Object.Destroy(this.m_detectedMag.GameObject);
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(fvrobject.GetGameObject(), this.Spawnpoint_Mag.position, this.Spawnpoint_Mag.rotation);
                audioSource.PlayOneShot(clips[0]);
            }
        }

        public void Button_Duplicate()
        {
            if (this.m_detectedMag == null && this.m_detectedSL == null)
            {
                audioSource.PlayOneShot(clips[1]);
                return;
            }
            if (this.m_detectedMag != null && this.m_detectedMag.IsEnBloc)
            {
                audioSource.PlayOneShot(clips[1]);
                return;
            }
            if (this.m_storedDupeCost < 1)
            {
                audioSource.PlayOneShot(clips[1]);
                return;
            }
            if (SR_Manager.instance.Points >= this.m_storedDupeCost)
            {
                audioSource.PlayOneShot(clips[0]);
                SR_Manager.instance.Points -= this.m_storedDupeCost;

                if (this.m_detectedMag != null)
                {
                    FVRObject fvrobject = this.m_detectedMag.ObjectWrapper;
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(fvrobject.GetGameObject(), this.Spawnpoint_Mag.position, this.Spawnpoint_Mag.rotation);
                    FVRFireArmMagazine component = gameObject.GetComponent<FVRFireArmMagazine>();
                    for (int i = 0; i < Mathf.Min(this.m_detectedMag.LoadedRounds.Length, component.LoadedRounds.Length); i++)
                    {
                        if (this.m_detectedMag.LoadedRounds[i] != null && this.m_detectedMag.LoadedRounds[i].LR_Mesh != null)
                        {
                            component.LoadedRounds[i].LR_Class = this.m_detectedMag.LoadedRounds[i].LR_Class;
                            component.LoadedRounds[i].LR_Mesh = this.m_detectedMag.LoadedRounds[i].LR_Mesh;
                            component.LoadedRounds[i].LR_Material = this.m_detectedMag.LoadedRounds[i].LR_Material;
                            component.LoadedRounds[i].LR_ObjectWrapper = this.m_detectedMag.LoadedRounds[i].LR_ObjectWrapper;
                        }
                    }
                    component.m_numRounds = this.m_detectedMag.m_numRounds;
                    component.UpdateBulletDisplay();
                }
                else if (this.m_detectedSL != null)
                {
                    FVRObject fvrobject = this.m_detectedSL.ObjectWrapper;
                    GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(fvrobject.GetGameObject(), this.Spawnpoint_Mag.position, this.Spawnpoint_Mag.rotation);
                    Speedloader component2 = gameObject2.GetComponent<Speedloader>();
                    for (int j = 0; j < this.m_detectedSL.Chambers.Count; j++)
                    {
                        if (this.m_detectedSL.Chambers[j].IsLoaded)
                        {
                            component2.Chambers[j].Load(this.m_detectedSL.Chambers[j].LoadedClass, false);
                        }
                        else
                        {
                            component2.Chambers[j].Unload();
                        }
                    }
                }
                return;
            }
            audioSource.PlayOneShot(clips[1]);
        }

        private void Update()
        {
            this.m_scanTick -= Time.deltaTime;
            if (this.m_scanTick <= 0f)
            {
                this.m_scanTick = UnityEngine.Random.Range(0.2f, 0.3f);
                float num = Vector3.Distance(base.transform.position, GM.CurrentPlayerBody.transform.position);
                if (num < 6f)
                {
                    this.Scan();
                }
            }
        }

        private void Scan()
        {
            int num = Physics.OverlapBoxNonAlloc(this.ScanningVolume.position, this.ScanningVolume.localScale * 0.5f, this.colbuffer, this.ScanningVolume.rotation, this.ScanningLM, QueryTriggerInteraction.Collide);
            this.m_detectedMag = null;
            this.m_detectedSL = null;
            for (int i = 0; i < num; i++)
            {
                if (this.colbuffer[i].attachedRigidbody != null)
                {
                    FVRFireArmMagazine component = this.colbuffer[i].attachedRigidbody.gameObject.GetComponent<FVRFireArmMagazine>();
                    if (component != null && component.FireArm == null && !component.IsHeld && component.QuickbeltSlot == null && !component.IsIntegrated)
                    {
                        this.m_detectedMag = component;
                        break;
                    }
                    Speedloader component2 = this.colbuffer[i].attachedRigidbody.gameObject.GetComponent<Speedloader>();
                    if (component2 != null && !component2.IsHeld && component2.QuickbeltSlot == null && component2.IsPretendingToBeAMagazine)
                    {
                        this.m_detectedSL = component2;
                        break;
                    }
                }
            }
            this.SetCostBasedOnMag();
        }

        private void SetCostBasedOnMag()
        {
            if (this.m_detectedMag == null && this.m_detectedSL == null)
            {
               // this.OCIconDupe.SetOption(TNH_ObjectConstructorIcon.IconState.Cancel, this.OCIconDupe.Sprite_Cancel, 0);
                this.m_storedDupeCost = 0;
            }
            else
            {
                //this.OCIconDupe.SetOption(TNH_ObjectConstructorIcon.IconState.Accept, this.OCIconDupe.Sprite_Accept, 0);
                this.m_storedDupeCost = 2;
            }
            if (this.m_detectedMag != null)
            {
                if (!IM.CompatMags.ContainsKey(this.m_detectedMag.MagazineType))
                {
                    audioSource.PlayOneShot(clips[1]);
                    return;
                }
                List<FVRObject> list = IM.CompatMags[this.m_detectedMag.MagazineType];
                FVRObject fvrobject = null;
                int num = 10000;
                for (int i = 0; i < list.Count; i++)
                {
                    if (!(list[i].ItemID == this.m_detectedMag.ObjectWrapper.ItemID))
                    {
                        if (list[i].MagazineCapacity > this.m_detectedMag.m_capacity && list[i].MagazineCapacity < num)
                        {
                            fvrobject = list[i];
                            num = list[i].MagazineCapacity;
                        }
                    }
                }
                if (fvrobject != null)
                {
                    this.m_hasUpgradeableMags = true;
                   //this.OCIconUpgrade.SetOption(TNH_ObjectConstructorIcon.IconState.Accept, this.OCIconUpgrade.Sprite_Accept, 0);
                }
                else
                {
                    //this.OCIconUpgrade.SetOption(TNH_ObjectConstructorIcon.IconState.Cancel, this.OCIconUpgrade.Sprite_Cancel, 0);
                    this.m_hasUpgradeableMags = false;
                }
            }
            else
            {
                //this.OCIconUpgrade.SetOption(TNH_ObjectConstructorIcon.IconState.Cancel, this.OCIconUpgrade.Sprite_Cancel, 0);
                this.m_hasUpgradeableMags = false;
            }
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(this.ScanningVolume.position, this.ScanningVolume.localScale * 0.5f);
        }

        public Transform Spawnpoint_Mag;

        public Transform ScanningVolume;

        public LayerMask ScanningLM;

        private FVRFireArmMagazine m_detectedMag;

        private Speedloader m_detectedSL;

        private Collider[] colbuffer;

        private int m_storedDupeCost;

        private float m_scanTick = 1f;

        private bool m_hasUpgradeableMags;
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip[] clips;
    }
}