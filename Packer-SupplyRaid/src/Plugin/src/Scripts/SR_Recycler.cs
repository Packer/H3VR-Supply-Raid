using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FistVR;

namespace SupplyRaid
{
    public class SR_Recycler : MonoBehaviour
    {
        private List<FVRFireArm> weapons = new List<FVRFireArm>();

        public Transform ScanningVolume;
        private float m_scanTick = 0.8f;
        private Collider[] colbuffer;
        public LayerMask ScanningLM;
        private List<GameObject> cashList = new List<GameObject>();
        public Transform selectedBox;

        private void Start()
        {
            colbuffer = new Collider[50];
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
                SR_Manager.PlayPointsGainSFX();
            }

            if (weapons.Count <= 0)
            {
                if(!ignoreFail)
                    SR_Manager.PlayFailSFX();
                return;
            }

            if (weapons[0])
            {
                Destroy(weapons[0].gameObject);
                ignoreFail = true;
            }

            weapons.Clear();
            if (ignoreFail)
                SR_Manager.PlayPointsGainSFX();
            SR_Manager.instance.Points += SR_Manager.instance.character.recyclerPoints;
        }

        private void Update()
        {
            m_scanTick -= Time.deltaTime;
            if (m_scanTick <= 0f)
            {
                m_scanTick = Random.Range(0.6f, 0.8f);
                float num = Vector3.Distance(transform.position, GM.CurrentPlayerBody.transform.position);
                if (num < 12f)
                {
                    Scan();
                }
            }

            //Selection Box
            if (weapons.Count > 0 && weapons[0] && weapons[0].GameObject)
            {
                Transform target = weapons[0].PoseOverride ? weapons[0].PoseOverride : weapons[0].transform;
                selectedBox.position = target.position;
                selectedBox.rotation = target.rotation;
                selectedBox.localScale = target.localScale;
            }
        }

        int GetCashValue(string itemName)
        {
            switch (itemName)
            {
                case "CharcoalBriquette(Clone)":
                    return SR_Manager.instance.character.recyclerTokens;
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
            int num = Physics.OverlapBoxNonAlloc(
                ScanningVolume.position, 
                ScanningVolume.localScale * 0.5f, 
                colbuffer, 
                ScanningVolume.rotation, 
                ScanningLM, QueryTriggerInteraction.Collide);
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

                if (colbuffer[i].attachedRigidbody != null)
                {

                    FVRFireArm component = colbuffer[i].attachedRigidbody.gameObject.GetComponent<FVRFireArm>();
                    if (component != null)
                    {
                        if (!component.SpawnLockable)
                        {
                            if (!component.IsHeld && component.QuickbeltSlot == null && !weapons.Contains(component))
                            {
                                weapons.Add(component);
                            }
                        }
                    }
                }
            }

            if (weapons.Count > 0 && weapons[0] != null)
            {
                selectedBox.gameObject.SetActive(true);
                selectedBox.position = weapons[0].transform.position;
            }
            else
            {
                selectedBox.gameObject.SetActive(false);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(ScanningVolume.position, ScanningVolume.localScale * 0.5f);
        }
    }
}
