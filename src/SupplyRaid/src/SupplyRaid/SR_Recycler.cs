using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FistVR;

namespace SupplyRaid
{
    public class SR_Recycler : MonoBehaviour
    {
        public List<FVRFireArm> weapons = new List<FVRFireArm>();
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip[] clips;


        public Transform ScanningVolume;
        private float m_scanTick = 1f;
        private Collider[] colbuffer;
        public LayerMask ScanningLM;

        /*
        void OnTriggerEnter(Collider other)
        {
            FVRFireArm weapon = other.gameObject.GetComponent<FVRFireArm>();
            if (weapon != null)
            {
                weapons.Add(weapon);
            }
        }

        void OnTriggerExit(Collider other)
        {
            FVRFireArm weapon = other.gameObject.GetComponent<FVRFireArm>();
            if (weapon != null)
            {
                weapons.Remove(weapon);
            }
        }

        public void Recycle()
        {
            List<FVRFireArm> toDestroy = new List<FVRFireArm>();

            for (int i = 0; i < weapons.Count; i++)
            {
                if (weapons[i].IsHeld == false && weapons[i].QuickbeltSlot == null)
                    toDestroy.Add(weapons[i]);
            }

            if (toDestroy.Count <= 0)
            {
                audioSource.PlayOneShot(clips[1]);
            }
            else
            {
                SupplyRaidManager.instance.Points += toDestroy.Count;
                audioSource.PlayOneShot(clips[0]);

                for (int i = 0; i < toDestroy.Count; i++)
                {
                    weapons.Remove(toDestroy[i]);
                    Destroy(toDestroy[i].gameObject);
                }
                toDestroy.Clear();
            }
        }
        */
        //------------------------------------------------------------------
        //------------------------------------------------------------------
        //------------------------------------------------------------------

        private void Start()
        {
            this.colbuffer = new Collider[50];
        }

        public void Button_Recycler()
        {
            if (this.weapons.Count <= 0)
            {
                audioSource.PlayOneShot(clips[1]);
                return;
            }
            if (this.weapons[0] != null)
            {
                UnityEngine.Object.Destroy(this.weapons[0].gameObject);
            }
            this.weapons.Clear();
            audioSource.PlayOneShot(clips[0]);
            SR_Manager.instance.Points += 1;
        }
        private void Update()
        {
            this.m_scanTick -= Time.deltaTime;
            if (this.m_scanTick <= 0f)
            {
                this.m_scanTick = UnityEngine.Random.Range(0.8f, 1f);
                float num = Vector3.Distance(base.transform.position, GM.CurrentPlayerBody.transform.position);
                if (num < 12f)
                {
                    this.Scan();
                }
            }
        }

        private void Scan()
        {
            int num = Physics.OverlapBoxNonAlloc(this.ScanningVolume.position, this.ScanningVolume.localScale * 0.5f, this.colbuffer, this.ScanningVolume.rotation, this.ScanningLM, QueryTriggerInteraction.Collide);
            this.weapons.Clear();
            for (int i = 0; i < num; i++)
            {
                if (this.colbuffer[i].attachedRigidbody != null)
                {
                    FVRFireArm component = this.colbuffer[i].attachedRigidbody.gameObject.GetComponent<FVRFireArm>();
                    if (component != null)
                    {
                        if (!component.SpawnLockable)
                        {
                            if (!component.IsHeld && component.QuickbeltSlot == null && !this.weapons.Contains(component))
                            {
                                this.weapons.Add(component);
                            }
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
    }
}
