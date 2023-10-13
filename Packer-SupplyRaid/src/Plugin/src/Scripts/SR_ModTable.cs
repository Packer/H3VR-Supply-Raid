using FistVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SupplyRaid
{

    public class SR_ModTable : MonoBehaviour
    {
        public static SR_ModTable instance;

        [Header("Scanning")]
        private Collider[] colbuffer;
        private float m_scanTick = 1f;
        public Transform ScanningVolume;
        public LayerMask ScanningLM;

        private FVRFireArm detectedFireArm;
        private FVRFireArm lastFireArm;

        [Header("Spawn Positions")]
        public Transform[] spawnPoints;


        [System.Serializable]
        public class TableButton
        {
            public SR_GenericButton button;
            [HideInInspector]public LootTable attachmentTable;
        }

        [Header("Buttons")]
        [Tooltip("ironSights = 1 \n magnification = 2 \n reflex = 3 \n suppression = 4 \n stock = 5 \n laser = 6 \n illumination = 7 \n" +
            " grip = 8 \n recoilMitigation = 10 \n barrelExtention = 11 \n adapter = 12 \n bayonet = 13 \n projectileWeapon = 14 \n bipod = 15")]
        public TableButton[] buttons = new TableButton[16];

        //Generation Table
        private FVRObject fvrObject;
        private List<FVRObject.OTagEra> eras = new List<FVRObject.OTagEra>();
        private List<FVRObject.OTagFirearmMount> mounts = new List<FVRObject.OTagFirearmMount>();
        //private List<FVRObject.OTagAttachmentFeature> features = new List<FVRObject.OTagAttachmentFeature>();

        private void Start()
        {
            instance = this;
            colbuffer = new Collider[50];
        }

        public void Setup()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].button.text.text = SR_Manager.instance.character.attachmentsCost[i].ToString();
            }
        }

        private void Update()
        {
            m_scanTick -= Time.deltaTime;
            if (m_scanTick <= 0f)
            {
                m_scanTick = Random.Range(0.5f, 1f);
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

            if (detectedFireArm != null)
            {
                lastFireArm = detectedFireArm;
                detectedFireArm = null;
            }

            for (int i = 0; i < num; i++)
            {
                if (colbuffer[i].attachedRigidbody != null)
                {
                    FVRFireArm component = colbuffer[i].attachedRigidbody.gameObject.GetComponent<FVRFireArm>();

                    if (component != null)
                    {
                        detectedFireArm = component;
                        break;
                    }
                }
            }

            if (detectedFireArm != null && IM.OD.ContainsKey(detectedFireArm.ObjectWrapper.ItemID))
            {
                fvrObject = IM.OD[detectedFireArm.ObjectWrapper.ItemID];

                //Add all of the weapon's compatible mounts
                eras.Add(fvrObject.TagEra);
                mounts.AddRange(fvrObject.TagFirearmMounts);
                //features.Add(fvrobject.TagAttachmentFeature);

                DisableAllButtons();
                UpdateButtons();
            }
            else
                DisableAllButtons();
        }

        public void BuyAttachment(int i)
        {
            //Price check TODO SET THIS UP
            if (!SR_Manager.EnoughPoints(SR_Manager.instance.character.attachmentsCost[i]))
            {
                SR_Manager.PlayFailSFX();
                return;
            }

            if (SR_Global.SpawnLoot(buttons[i].attachmentTable, null, spawnPoints))
            {
                SR_Manager.SpendPoints(SR_Manager.instance.character.attachmentsCost[i]);
                SR_Manager.PlayConfirmSFX();
                return;
            }
            //Didn't work, fail sound
            SR_Manager.PlayFailSFX();
        }

        void UpdateButtons()
        {
            //Table Generation
            List<FVRObject.OTagAttachmentFeature> desiredFeature = new List<FVRObject.OTagAttachmentFeature>();

            bool[] bespoke = new bool[16];
            for (int i = 0; i < fvrObject.BespokeAttachments.Count; i++)
            {
                bespoke[(int)fvrObject.BespokeAttachments[i].TagAttachmentFeature] = true;
            }

            for (int i = 0; i < buttons.Length; i++)
            {
                //Ignore None and Decorations
                if (i == 0 || SR_Manager.instance.character.attachmentsCost[i] < 0)
                    continue;

                desiredFeature.Clear();
                desiredFeature.Add((FVRObject.OTagAttachmentFeature)i);

                //Don't recalculate loot tables if its the same weapon
                if (lastFireArm != detectedFireArm)
                {
                    buttons[i].attachmentTable.InitializeAttachmentTable(eras, mounts, desiredFeature);

                    //Remove GLOBAL character subtractions
                    buttons[i].attachmentTable = SR_Global.RemoveGlobalSubtractionOnTable(buttons[i].attachmentTable);

                    //Only bespoke on these attachments
                    if (bespoke[i])
                    {
                        //Fix Bespoke to specific attachments
                        if (mounts.Contains(FVRObject.OTagFirearmMount.Bespoke))
                        {
                            List<FVRObject> removeAttachments = new List<FVRObject>();
                            for (int y = buttons[i].attachmentTable.Loot.Count - 1; y >= 0; y--)
                            {
                                //If Bespoke attachments for this weapon does not contain the loot table item, remove it
                                if (!fvrObject.BespokeAttachments.Contains(buttons[i].attachmentTable.Loot[y]))
                                {
                                    removeAttachments.Add(buttons[i].attachmentTable.Loot[y]);
                                }
                            }

                            //Clear Unused attachments
                            for (int x = removeAttachments.Count - 1; x >= 0; x--)
                            {
                                buttons[i].attachmentTable.Loot.Remove(removeAttachments[x]);
                            }
                        }
                    }
                    
                }

                //Debug.Log("Loottable size:  " + buttons[i].attachmentTable.Loot.Count);

                //Enable or disable button if it has compatible features
                if (buttons[i].attachmentTable.Loot.Count > 0)
                {
                    buttons[i].button.gameObject.SetActive(true);
                    buttons[i].button.index = i;
                    buttons[i].button.text.text = SR_Manager.instance.character.attachmentsCost[i].ToString();
                }
                else
                    buttons[i].button.gameObject.SetActive(false);
            }
        }

        void DisableAllButtons()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].button != null && buttons[i].button.gameObject.activeSelf != false)
                    buttons[i].button.gameObject.SetActive(false);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(ScanningVolume.position, ScanningVolume.localScale * 0.5f);
        }
    }
}
