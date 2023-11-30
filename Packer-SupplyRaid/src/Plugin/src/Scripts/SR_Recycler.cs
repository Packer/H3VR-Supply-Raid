using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FistVR;

namespace SupplyRaid
{
    public class SR_Recycler : MonoBehaviour
    {
        public List<FVRFireArm> weapons = new List<FVRFireArm>();
        //[SerializeField] AudioSource audioSource;
        //[SerializeField] AudioClip[] clips;

        public Transform ScanningVolume;
        private float m_scanTick = 0.8f;
        private Collider[] colbuffer;
        public LayerMask ScanningLM;

        private List<GameObject> cashList = new List<GameObject>();

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
            bool ignoreFail = false;
            for (int i = 0; i < cashList.Count; i++)
            {
                int cash = GetCashValue(cashList[i].name);
                SR_Manager.instance.Points += cash;
                Destroy(cashList[i]);
                ignoreFail = true;
            }

            if(ignoreFail)
                SR_Manager.PlayPointsGainSFX();

            if (this.weapons.Count <= 0)
            {
                if(!ignoreFail)
                    SR_Manager.PlayFailSFX();
                return;
            }
            if (this.weapons[0] != null)
            {
                Destroy(this.weapons[0].gameObject);
            }
            this.weapons.Clear();
            SR_Manager.PlayPointsGainSFX();
            SR_Manager.instance.Points += SR_Manager.instance.character.recyclerPoints;
        }

        private void Update()
        {
            this.m_scanTick -= Time.deltaTime;
            if (this.m_scanTick <= 0f)
            {
                this.m_scanTick = Random.Range(0.6f, 0.8f);
                float num = Vector3.Distance(base.transform.position, GM.CurrentPlayerBody.transform.position);
                if (num < 12f)
                {
                    this.Scan();
                }
            }
        }

        int GetCashValue(string itemName)
        {
            switch (itemName)
            {
                case "CharcoalBriquette(Clone)":
                case "Ammo_69_CashMoney_D1(Clone)":
                    return 1;
                case "Ammo_69_CashMoney_D5(Clone)":
                    return 5;
                case "Ammo_69_CashMoney_D10(Clone)":
                    return 10;
                case "Ammo_69_CashMoney_D25(Clone)":
                    return 25;
                case "Ammo_69_CashMoney_D100(Clone)":
                    return 100;
                case "Ammo_69_CashMoney_D1000(Clone)":
                    return 1000;
                default:
                    return 0;
            }
        }

        private void Scan()
        {
            int num = Physics.OverlapBoxNonAlloc(this.ScanningVolume.position, this.ScanningVolume.localScale * 0.5f, this.colbuffer, this.ScanningVolume.rotation, this.ScanningLM, QueryTriggerInteraction.Collide);
            weapons.Clear();
            cashList.Clear();


            for (int i = 0; i < num; i++)
            {
                switch (colbuffer[i].name)
                {
                    default:
                        break;
                    case "CharcoalBriquette(Clone)":
                    case "Ammo_69_CashMoney_D1(Clone)":
                    case "Ammo_69_CashMoney_D5(Clone)":
                    case "Ammo_69_CashMoney_D10(Clone)":
                    case "Ammo_69_CashMoney_D25(Clone)":
                    case "Ammo_69_CashMoney_D100(Clone)":
                    case "Ammo_69_CashMoney_D1000(Clone)":
                        cashList.Add(colbuffer[i].gameObject);
                        break;
                }

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
