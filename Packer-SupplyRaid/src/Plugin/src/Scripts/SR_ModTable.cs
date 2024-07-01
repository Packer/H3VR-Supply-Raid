using FistVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


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
        private int attachmentCount = -1;

        public Transform selectedBox;

        [Header("Spawn Positions")]
        public Transform[] spawnPoints;

        [Header("Menus")]
        public GameObject menuAttachments;
        public GameObject menuAdapters;

        public GameObject attachmentsButton;
        public GameObject adaptersButton;
        [Header("Modul Support")]
        public GameObject modulButton;

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

        //Rail AdapterButtons
        public Sprite defaultSprite;
        public GameObject buttonPrefab;
        protected List<FVRPointableButton> buttonList = new List<FVRPointableButton>();

        //Generation Table
        //private FVRObject fvrObject;
        //private List<FVRObject.OTagEra> eras = new List<FVRObject.OTagEra>();
        private List<FVRObject.OTagEra> modernEras = new List<FVRObject.OTagEra>();
        //private List<FVRObject.OTagFirearmMount> mounts = new List<FVRObject.OTagFirearmMount>();
        //private List<FVRObject.OTagAttachmentFeature> features = new List<FVRObject.OTagAttachmentFeature>();

        private List<FVRObject> railAdapters = new List<FVRObject>();

        public void Setup()
        {
            instance = this;
            colbuffer = new Collider[60];

            //Get All Muzzle Brakes
            modernEras.Add(FVRObject.OTagEra.PostWar);
            modernEras.Add(FVRObject.OTagEra.Modern);
            modernEras.Add(FVRObject.OTagEra.WW2);

            //Populate Rail Adapters
            railAdapters.AddRange(IM.Instance.ObjectDic.Values);
            for (int i = railAdapters.Count - 1; i >= 0; i--)
            {
                if (railAdapters[i].TagAttachmentFeature != FVRObject.OTagAttachmentFeature.Adapter)
                    railAdapters.RemoveAt(i);
            }
            GenerateButtons();

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].button.text.text = SR_Manager.instance.character.attachmentsCost[i].ToString();
            }

            //Hide Adapters if not enabled
            if (adaptersButton && SR_Manager.Character().attachmentsCost[(int)FVRObject.OTagAttachmentFeature.Adapter] <= -1)
                adaptersButton.SetActive(false);
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

            if (detectedFireArm && detectedFireArm.GameObject)
            {
                Transform target = detectedFireArm.PoseOverride ? detectedFireArm.PoseOverride : detectedFireArm.transform;
                selectedBox.position = target.position;
                selectedBox.rotation = target.rotation;
                selectedBox.localScale = target.localScale;
                //Debug.Log("Update Box - POS: " + selectedBox.position + " - S: " + selectedBox.localScale);
            }
        }

        void GenerateButtons()
        {
            if (railAdapters == null || railAdapters.Count == 0 || !buttonPrefab)
                return;

            for (int i = 0; i < railAdapters.Count; i++)
            {
                if (railAdapters[i] == null)
                    continue;

                FVRObject item = railAdapters[i];
                int index = i;
                int cost = SR_Manager.Character().attachmentsCost[(int)FVRObject.OTagAttachmentFeature.Adapter];

                FVRPointable point = GenerateButton(item, cost);
                point.gameObject.GetComponent<Button>().onClick.AddListener(
                    delegate { PurchaseItemID(item, cost); });
            }
        }

        protected virtual FVRPointable GenerateButton(FVRObject itemObject, int itemCost)
        {
            if (itemObject == null)
            {
                Debug.LogError("Could not generate button with missing FVRObject");
                return null;
            }

            FVRPointableButton btn = Instantiate(buttonPrefab, buttonPrefab.transform.parent).GetComponent<FVRPointableButton>();
            btn.gameObject.SetActive(true);
            if (btn.Text != null)
                btn.Text.text = itemObject.DisplayName;

            ItemSpawnerID id;
            ManagerSingleton<IM>.Instance.SpawnerIDDic.TryGetValue(itemObject.SpawnedFromId, out id);
            //ItemSpawnerID id = IM.GetSpawnerID(itemObject.SpawnedFromId);

            if (id != null && id.Sprite != null)
                btn.Image.sprite = id.Sprite;
            else
                btn.Image.sprite = defaultSprite;

            buttonList.Add(btn);

            //Cost Display
            for (int x = 0; x < btn.transform.childCount; x++)
            {
                if (btn.transform.GetChild(x).name == "Cost")
                {
                    btn.transform.GetChild(x).GetComponent<Text>().text = itemCost.ToString();
                    break;
                }
            }
            return btn;
        }

        public void SelectMenu(GameObject menu)
        {
            menuAttachments.SetActive(false);
            menuAdapters.SetActive(false);

            SR_Manager.PlayConfirmSFX();
            menu.SetActive(true);
        }

        public GameObject PurchaseItemID(FVRObject fvrObject, int cost)
        {
            if (fvrObject == null || !SR_Manager.EnoughPoints(cost))
            {
                //Failed Sound
                SR_Manager.PlayFailSFX();
                return null;
            }

            SR_Manager.SpendPoints(cost);
            SR_Manager.PlayConfirmSFX();

            GameObject prefab = fvrObject.GetGameObject();
            GameObject newItem = Instantiate(prefab, spawnPoints[0].position, spawnPoints[0].rotation);

            return newItem;
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
                DisableAllButtons();
                UpdateButtons(IM.OD[detectedFireArm.ObjectWrapper.ItemID]);
                selectedBox.gameObject.SetActive(true);
                selectedBox.position = detectedFireArm.transform.position;
            }
            else
            {
                DisableAllButtons();
                selectedBox.gameObject.SetActive(false);
            }
        }

        public void BuyAttachment(int i)
        {
            //Price check TODO SET THIS UP
            if (!SR_Manager.EnoughPoints(SR_Manager.instance.character.attachmentsCost[i]))
            {
                SR_Manager.PlayFailSFX();
                return;
            }

            bool forceSecondary = false;

            if (i == 1 || i == 2) //Iron Sights & scopes
                forceSecondary = true;

            if (SR_Global.SpawnLoot(buttons[i].attachmentTable, null, spawnPoints, forceSecondary))
            {
                SR_Manager.SpendPoints(SR_Manager.instance.character.attachmentsCost[i]);
                SR_Manager.PlayConfirmSFX();
                return;
            }
            //Didn't work, fail sound
            SR_Manager.PlayFailSFX();
        }

        void UpdateButtons(FVRObject fvrObject)
        {
            if (fvrObject == null || detectedFireArm == null)
                return;

            //For each Button / Attachment Feature
            for (int i = 0; i < buttons.Length; i++)
            {
                //Ignore None and disabled attachments
                if (i == 0 || buttons[i] == null || SR_Manager.instance.character.attachmentsCost[i] < 0)
                    continue;

                //Don't recalculate loot tables if its the same weapon
                if (lastFireArm != detectedFireArm || attachmentCount != detectedFireArm.Attachments.Count)
                {
                    if (buttons[i].attachmentTable.Loot == null)
                        buttons[i].attachmentTable.Loot = new List<FVRObject>();

                    buttons[i].attachmentTable.Loot.Clear();
                    List<FVRObject> bespokeAttachments = new List<FVRObject>();

                    //-------------------------
                    //BESPOKE
                    //-------------------------
                    bool isBespoke = fvrObject != null ? (fvrObject.TagAttachmentMount == FVRObject.OTagFirearmMount.Bespoke) : false;

                    if (detectedFireArm != null && fvrObject != null)
                    {
                        if (detectedFireArm.IDSpawnedFrom != null
                        && detectedFireArm.IDSpawnedFrom.Secondaries != null
                        && detectedFireArm.IDSpawnedFrom.Secondaries.Length > 0)
                        {
                            //Debug.Log("Populating Bespoke");
                            //Add Item Secondaries
                            for (int s = 0; s < detectedFireArm.IDSpawnedFrom.Secondaries.Length; s++)
                            {
                                if (detectedFireArm.IDSpawnedFrom.Secondaries[s] != null)
                                {
                                    if (detectedFireArm.IDSpawnedFrom.Secondaries[s].MainObject != null &&
                                        !bespokeAttachments.Contains(detectedFireArm.IDSpawnedFrom.Secondaries[s].MainObject))
                                        bespokeAttachments.Add(detectedFireArm.IDSpawnedFrom.Secondaries[s].MainObject);
                                }
                            }

                        }

                        if (fvrObject.BespokeAttachments != null && fvrObject.BespokeAttachments.Count > 0)
                        {
                            //Add Bespoke
                            for (int b = 0; b < fvrObject.BespokeAttachments.Count; b++)
                            {
                                if (fvrObject.BespokeAttachments[b])
                                {
                                    if (!bespokeAttachments.Contains(fvrObject.BespokeAttachments[b]))
                                        bespokeAttachments.Add(fvrObject.BespokeAttachments[b]);
                                }
                            }

                            if(bespokeAttachments.Count > 0)
                                isBespoke = true;
                        }
                    }
                    //-------------------------

                    //Mixed Picatinny Mounts
                    bool picatinny = fvrObject.TagFirearmMounts.Contains(FVRObject.OTagFirearmMount.Picatinny);

                    //Attachment Adapters
                    if (detectedFireArm != null && !picatinny)
                    {
                        for (int z = 0; z < detectedFireArm.Attachments.Count; z++)
                        {
                            for (int y = 0; y < detectedFireArm.Attachments[z].AttachmentMounts.Count; y++)
                            {
                                if (detectedFireArm.Attachments[z].AttachmentMounts[y].Type == FVRFireArmAttachementMountType.Picatinny)
                                {
                                    picatinny = true;
                                    break;
                                }
                            }
                            if (picatinny)
                                break;
                        }
                    }

                    bool alwaysAllowed = false;
                    if ((FVRObject.OTagAttachmentFeature)i == FVRObject.OTagAttachmentFeature.Suppression
                    || (FVRObject.OTagAttachmentFeature)i == FVRObject.OTagAttachmentFeature.RecoilMitigation
                    || (FVRObject.OTagAttachmentFeature)i == FVRObject.OTagAttachmentFeature.BarrelExtension)
                    {
                        alwaysAllowed = true;
                    }

                    //Remove GLOBAL character subtractions
                    if (!isBespoke || alwaysAllowed || (isBespoke && picatinny))
                    {
                        //Current Era + All Modern Era's (More useful attachments)
                        List<FVRObject.OTagEra> eras = new List<FVRObject.OTagEra>();
                        eras.AddRange(modernEras);
                        if(eras.Contains(fvrObject.TagEra))
                            eras.Add(fvrObject.TagEra);

                        //Mounts
                        List<FVRObject.OTagFirearmMount> mounts = new List<FVRObject.OTagFirearmMount>();
                        mounts.AddRange(fvrObject.TagFirearmMounts);

                        if(picatinny && !mounts.Contains(FVRObject.OTagFirearmMount.Picatinny))
                            mounts.Add(FVRObject.OTagFirearmMount.Picatinny);

                        InitializeAttachmentTable(
                            buttons[i].attachmentTable, 
                            eras,
                            mounts, 
                            (FVRObject.OTagAttachmentFeature)i);
                    }

                    //Bespoke attachments
                    for (int y = 0; y < bespokeAttachments.Count; y++)
                    {
                        if (bespokeAttachments[y].TagAttachmentFeature == (FVRObject.OTagAttachmentFeature)i)
                        {
                            buttons[i].attachmentTable.Loot.Add(bespokeAttachments[y]);
                        }
                    }

                    buttons[i].attachmentTable = SR_Global.RemoveGlobalSubtractionOnTable(buttons[i].attachmentTable);

                    //Remove Meme
                    for (int num = buttons[i].attachmentTable.Loot.Count - 1; num >= 0; num--)
                    {
                        FVRObject fVRObject = buttons[i].attachmentTable.Loot[num];
                        if (fVRObject.TagSet == FVRObject.OTagSet.Meme)
                        {
                            buttons[i].attachmentTable.Loot.RemoveAt(num);
                            continue;
                        }
                    }

                }

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

            if(attachmentCount != detectedFireArm.Attachments.Count)
                attachmentCount = detectedFireArm.Attachments.Count;
        }

        public void InitializeAttachmentTable(LootTable table, List<FVRObject.OTagEra> eras = null, 
            List<FVRObject.OTagFirearmMount> mounts = null, FVRObject.OTagAttachmentFeature feature = FVRObject.OTagAttachmentFeature.None)
        {
            table.Loot = new List<FVRObject>(ManagerSingleton<IM>.Instance.odicTagCategory[FVRObject.ObjectCategory.Attachment]);

            for (int i = table.Loot.Count - 1; i >= 0; i--)
            {
                FVRObject fvrobject = table.Loot[i];
                if (eras != null && !eras.Contains(fvrobject.TagEra))
                {
                    table.Loot.RemoveAt(i);
                }
                else if (mounts != null && !mounts.Contains(fvrobject.TagAttachmentMount))
                {
                    table.Loot.RemoveAt(i);
                }
                else if (fvrobject.TagAttachmentFeature != feature)
                {
                    table.Loot.RemoveAt(i);
                }
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
