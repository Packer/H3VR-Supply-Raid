using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FistVR;

namespace SupplyRaid
{
	public class SR_AmmoSpawner : MonoBehaviour
    {
        public Transform Spawnpoint_Round;
        public Transform ScanningVolume;
        public LayerMask ScanningLM;

        public bool[] purchased =  new bool[28];    //Ammo Enum Length
        public GameObject[] ammoTypeButtons =  new GameObject[28];    //Ammo Enum Length

        public AmmoEnum selectedAmmoType;
        public FireArmRoundClass roundClass = FireArmRoundClass.FMJ;

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


        List<AmmoConversion> ammoList = new List<AmmoConversion>();


        List<FireArmRoundType> roundTypes = new List<FireArmRoundType>();
        List<AmmoEnum> ammoTypes = new List<AmmoEnum>();

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
                    m_scanTick = Random.Range(0.8f, 1f);
                }
                else
                    m_scanTick = Random.Range(2f, 3f);
            }
        }

        public bool SetAmmoType(AmmoEnum ammo)
        {
            //TODO check if we can set the ammo for this weapon
            selectedAmmoType = ammo;

            return true;
        }

        public void Button_SpawnRound()
        {

            if (m_roundTypes.Count < 1)
            {
                SR_Manager.PlayFailSFX();
                return;
            }
            SR_Manager.PlayFailSFX();
            for (int i = 0; i < m_roundTypes.Count; i++)
            {
                FireArmRoundType fireArmRoundType = m_roundTypes[i];
                //FireArmRoundClass classFromType = this.GetClassFromType(fireArmRoundType);
                FireArmRoundClass classFromType = AM.GetRandomValidRoundClass(fireArmRoundType);
                FVRObject roundSelfPrefab = AM.GetRoundSelfPrefab(fireArmRoundType, classFromType);
                Instantiate(roundSelfPrefab.GetGameObject(), Spawnpoint_Round.position + Vector3.up * (float)i * 0.1f, Spawnpoint_Round.rotation);
            }
        }

        public void Button_SpawnSpeedLoader()
        {
            bool flag = false;
            for (int i = 0; i < m_detectedFirearms.Count; i++)
            {
                if (IM.OD.ContainsKey(m_detectedFirearms[i].ObjectWrapper.ItemID))
                {
                    FVRObject fvrobject = IM.OD[m_detectedFirearms[i].ObjectWrapper.ItemID];
                    if (fvrobject.CompatibleSpeedLoaders.Count > 0)
                    {
                        FVRObject fvrobject2 = fvrobject.CompatibleSpeedLoaders[0];
                        GameObject gameObject = fvrobject2.GetGameObject();
                        Speedloader speedloader = gameObject.GetComponent<Speedloader>();
                        if (!speedloader.IsPretendingToBeAMagazine)
                        {
                            flag = true;
                            GameObject gameObject2 = Instantiate<GameObject>(gameObject, Spawnpoint_Round.position + Vector3.up * (float)i * 0.1f, Spawnpoint_Round.rotation);
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
            bool flag = false;
            for (int i = 0; i < m_detectedFirearms.Count; i++)
            {
                if (IM.OD.ContainsKey(m_detectedFirearms[i].ObjectWrapper.ItemID))
                {
                    FVRObject fvrobject = IM.OD[m_detectedFirearms[i].ObjectWrapper.ItemID];
                    if (fvrobject.CompatibleClips.Count > 0)
                    {
                        FVRObject fvrobject2 = fvrobject.CompatibleClips[0];
                        GameObject gameObject = Instantiate<GameObject>(fvrobject2.GetGameObject(), Spawnpoint_Round.position + Vector3.up * (float)i * 0.1f, Spawnpoint_Round.rotation);
                        FireArmRoundType roundType = m_detectedFirearms[i].RoundType;
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
                            GameObject gameObject3 = Instantiate<GameObject>(gameObject2, Spawnpoint_Round.position + Vector3.up * (float)i * 0.1f, Spawnpoint_Round.rotation);
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
            if (m_detectedMags.Count < 1 && m_detectedClips.Count < 1 && m_detectedSLs.Count < 1 && m_detectedSweapons.Count < 1)
            {
                SR_Manager.PlayFailSFX();
                return;
            }

            SR_Manager.PlayRearmSFX();
            for (int i = 0; i < m_detectedMags.Count; i++)
            {
                FireArmRoundType roundType = m_detectedMags[i].RoundType;
                //FireArmRoundClass classFromType = this.GetClassFromType(roundType);
                FireArmRoundClass classFromType = AM.GetRandomValidRoundClass(roundType);
                m_detectedMags[i].ReloadMagWithType(classFromType);
            }
            for (int j = 0; j < m_detectedClips.Count; j++)
            {
                FireArmRoundType roundType2 = m_detectedClips[j].RoundType;
                //FireArmRoundClass classFromType2 = this.GetClassFromType(roundType2);
                FireArmRoundClass classFromType2 = AM.GetRandomValidRoundClass(roundType2);
                m_detectedClips[j].ReloadClipWithType(classFromType2);
            }
            for (int k = 0; k < m_detectedSLs.Count; k++)
            {
                FireArmRoundType type = m_detectedSLs[k].Chambers[0].Type;
                //FireArmRoundClass classFromType3 = this.GetClassFromType(type);
                FireArmRoundClass classFromType3 = AM.GetRandomValidRoundClass(type);
                m_detectedSLs[k].ReloadClipWithType(classFromType3);
            }
            for (int l = 0; l < m_detectedSweapons.Count; l++)
            {
                m_detectedSweapons[l].W.InstaReload();
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

        private void Scan()
        {
            int num = Physics.OverlapBoxNonAlloc(ScanningVolume.position, ScanningVolume.localScale * 0.5f, colbuffer, ScanningVolume.rotation, ScanningLM, QueryTriggerInteraction.Collide);
            m_roundTypes.Clear();
            m_detectedMags.Clear();
            m_detectedClips.Clear();
            m_detectedSLs.Clear();
            m_detectedSweapons.Clear();
            m_detectedFirearms.Clear();
            for (int i = 0; i < num; i++)
            {
                if (colbuffer[i].attachedRigidbody != null)
                {
                    FVRFireArm component = colbuffer[i].attachedRigidbody.gameObject.GetComponent<FVRFireArm>();
                    if (component != null)
                    {
                        if (!m_detectedFirearms.Contains(component))
                        {
                            m_detectedFirearms.Add(component);
                        }
                        if (!m_roundTypes.Contains(component.RoundType))
                        {
                            m_roundTypes.Add(component.RoundType);
                        }
                        if (component.Magazine != null && !m_detectedMags.Contains(component.Magazine))
                        {
                            m_detectedMags.Add(component.Magazine);
                        }
                        if (component.Attachments.Count > 0)
                        {
                            for (int j = 0; j < component.Attachments.Count; j++)
                            {
                                if (component.Attachments[j] is AttachableFirearmPhysicalObject)
                                {
                                    if (!m_roundTypes.Contains((component.Attachments[j] as AttachableFirearmPhysicalObject).FA.RoundType))
                                    {
                                        m_roundTypes.Add((component.Attachments[j] as AttachableFirearmPhysicalObject).FA.RoundType);
                                    }
                                    if ((component.Attachments[j] as AttachableFirearmPhysicalObject).FA.Magazine != null)
                                    {
                                        m_detectedMags.Add((component.Attachments[j] as AttachableFirearmPhysicalObject).FA.Magazine);
                                    }
                                }
                            }
                        }
                        if (component.GetIntegratedAttachableFirearm() != null && !m_roundTypes.Contains(component.GetIntegratedAttachableFirearm().RoundType))
                        {
                            m_roundTypes.Add(component.GetIntegratedAttachableFirearm().RoundType);
                        }
                    }
                    AttachableFirearmPhysicalObject component2 = colbuffer[i].attachedRigidbody.gameObject.GetComponent<AttachableFirearmPhysicalObject>();
                    if (component2 != null && !m_roundTypes.Contains(component2.FA.RoundType))
                    {
                        m_roundTypes.Add(component2.FA.RoundType);
                    }
                    FVRFireArmMagazine component3 = colbuffer[i].attachedRigidbody.gameObject.GetComponent<FVRFireArmMagazine>();
                    if (component3 != null && component3.FireArm == null && !m_detectedMags.Contains(component3))
                    {
                        m_detectedMags.Add(component3);
                    }
                    FVRFireArmClip component4 = colbuffer[i].attachedRigidbody.gameObject.GetComponent<FVRFireArmClip>();
                    if (component4 != null && component4.FireArm == null && !m_detectedClips.Contains(component4))
                    {
                        m_detectedClips.Add(component4);
                    }
                    Speedloader component5 = colbuffer[i].attachedRigidbody.gameObject.GetComponent<Speedloader>();
                    if (component5 != null && !m_detectedSLs.Contains(component5))
                    {
                        m_detectedSLs.Add(component5);
                    }
                    if (SR_Manager.instance.shakeReloading == TNH_SosiggunShakeReloading.On)
                    {
                        SosigWeaponPlayerInterface component6 = colbuffer[i].attachedRigidbody.gameObject.GetComponent<SosigWeaponPlayerInterface>();
                        if (component6 != null && !m_detectedSweapons.Contains(component6))
                        {
                            m_detectedSweapons.Add(component6);
                        }
                    }
                }
            }
        }

        void UpdateAmmoTypeDisplay()
        {
            //GET THE FIREARM ROUND TYPE, COLLECT ALL TYPES FROM ALL ATTACHMENTS AND WEAPONS
            //Loop through each FireArmRoundClass from that type and apply those types TO our index


            //List<FireArmRoundType> roundTypes = new List<FireArmRoundType>();

            //Collect all Roundtypes
            roundTypes.Clear();
            ammoTypes.Clear();
            ammoList.Clear();

            //Rounds
            for (int i = 0; i < m_roundTypes.Count; i++)
            {
                if(!roundTypes.Contains(m_roundTypes[i]))
                    roundTypes.Add(m_roundTypes[i]);
            }

            //Magazines
            for (int i = 0; i < m_detectedMags.Count; i++)
            {
                if (!roundTypes.Contains(m_detectedMags[i].RoundType))
                    roundTypes.Add(m_detectedMags[i].RoundType);
            }

            //Clips
            for (int j = 0; j < m_detectedClips.Count; j++)
            {
                if (!roundTypes.Contains(m_detectedClips[j].RoundType))
                    roundTypes.Add(m_detectedClips[j].RoundType);
            }
            
            //SpeedLoaders
            for (int k = 0; k < m_detectedSLs.Count; k++)
            {
                if (!roundTypes.Contains(m_detectedSLs[k].Chambers[0].Type))
                    roundTypes.Add(m_detectedSLs[k].Chambers[0].Type);
            }


            //Put all Round Types into a single list
            for (int i = 0; i < roundTypes.Count; i++)
            {
                //For each round Type, get its Ammo Type counter part and assign it

                AmmoConversion ammoConvert = new AmmoConversion();
                ammoConvert.roundType = roundTypes[i];
                ammoConvert.roundClass = FireArmRoundClass.Ball;

                ammoList.Add(ammoConvert);
            }

            UpdateAmmoList();

        }

        /// <summary>
        /// Repopulates the Ammo List with valid data for spawning depending on the selected Ammo Type
        /// </summary>
        void UpdateAmmoList()
        {
            //Loop through entire list
            for (int i = 0; i < ammoList.Count; i++)
            {

                //Loop Through Each of this RoundTypes Classes and see if we can find the equivalent
                int count = AM.STypeClassLists[ammoList[i].roundType].Count;
                for (int x = 0; x < count; x++)
                {
                    FireArmRoundClass type = AM.STypeClassLists[ammoList[i].roundType][x];
                    AmmoEnum ammoFound = AmmoConversion.GetAmmoEnum(type);

                    if (selectedAmmoType == ammoFound)
                    {
                        //Found our correct ammo type
                        ammoList[i].roundClass = type;
                        ammoList[i].ammo = ammoFound;
                        ammoList[i].hasAmmo = true;
                    }
                    else
                    {
                        //FOUND NOTHING
                        ammoList[i].hasAmmo = false;
                    }

                    //Gives each Available ammo Type
                    //ammoConvert.roundClass = AM.STypeClassLists[roundTypes[i]][x];
                }

                //ammoList[i].


            }


            FireArmRoundClass thing = AM.STypeClassLists[FireArmRoundType.a10gauge][0];


        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(ScanningVolume.position, ScanningVolume.localScale * 0.5f);
        }

        public class AmmoConversion
        {
            public int index = -1;
            public FireArmRoundType roundType;  //Actual definition bullet type (556 etc)
            public FireArmRoundClass roundClass;    //Type of Bullet, FMJ/API etc
            public AmmoEnum ammo;           //Ammo Counter Part
            public bool isOnlyType = false; //If True, only 1 type of ammo can be spawned, so it should always be spawned
            public bool hasAmmo = false;

            public static AmmoEnum GetAmmoEnum(FireArmRoundClass round)
            {
                switch (round)
                {

                    //----ROUNDS--------------------------------

                    case FireArmRoundClass.FMJ:
                    case FireArmRoundClass.Ball:
                    case FireArmRoundClass.Spitzer:
                    case FireArmRoundClass.DSM_Swarm:
                    case FireArmRoundClass.Cannonball:
                    case FireArmRoundClass.NERMAL:
                    case FireArmRoundClass.Mortar:
                    case FireArmRoundClass.MIRV:
                    case FireArmRoundClass.a20FMJ:
                        return AmmoEnum.Standard;

                    case FireArmRoundClass.JHP:
                    case FireArmRoundClass.SP:
                    case FireArmRoundClass.HighVelHP:
                    case FireArmRoundClass.HyperVelHP:
                        return AmmoEnum.HollowPoint;

                    case FireArmRoundClass.Tracer:
                    case FireArmRoundClass.DSM_Tracer:
                    case FireArmRoundClass.FLASHY:
                        return AmmoEnum.Tracer;

                    case FireArmRoundClass.AP:
                    case FireArmRoundClass.POINTYOWW:
                    case FireArmRoundClass.DSM_TurboPenetrator:
                    case FireArmRoundClass.a20AP:
                        return AmmoEnum.AP;

                    case FireArmRoundClass.Incendiary:
                    case FireArmRoundClass.X666_Baphomet:
                        return AmmoEnum.Incendiary;

                    case FireArmRoundClass.APIncendiary:
                    case FireArmRoundClass.a20APDS:
                        return AmmoEnum.API;

                    case FireArmRoundClass.PlusP_FMJ:
                    case FireArmRoundClass.SPESHUL:
                        return AmmoEnum.PlusP_FMJ;

                    case FireArmRoundClass.PlusP_JHP:
                        return AmmoEnum.PlusP_JHP;

                    case FireArmRoundClass.PlusP_API:
                        return AmmoEnum.PlusP_API;

                    case FireArmRoundClass.Subsonic_FMJ:
                        return AmmoEnum.Subsonic_FMJ;

                    case FireArmRoundClass.Subsonic_AP:
                        return AmmoEnum.Subsonic_AP;

                    case FireArmRoundClass.Subsonic_JHP:
                        return AmmoEnum.Subsonic_JHP;

                    //----SHELLS---------------------------

                    case FireArmRoundClass.Slug:
                    case FireArmRoundClass.DSM_Slugger:
                    case FireArmRoundClass.KS23_Barricade:
                        return AmmoEnum.Slug;

                    case FireArmRoundClass.BuckShot00:
                    case FireArmRoundClass.BuckShotNo2:
                    case FireArmRoundClass.BuckShotNo4:
                    case FireArmRoundClass.BuckShot000:
                    case FireArmRoundClass.BuckShotNo1:
                    case FireArmRoundClass.Double:
                    case FireArmRoundClass.MEGA:
                    case FireArmRoundClass.MegaBuckShot:
                    case FireArmRoundClass.KS23_Buckshot:
                        return AmmoEnum.Buckshot;

                    case FireArmRoundClass.Flechette:
                        return AmmoEnum.Flechette;

                    case FireArmRoundClass.DragonsBreath:
                        return AmmoEnum.Flechette;

                    case FireArmRoundClass.TripleHit:
                        return AmmoEnum.TripleHit;


                    //----GRENADE LAUNCHER---------------------------

                    case FireArmRoundClass.M397_AirBurst:
                    case FireArmRoundClass.X214_SteelBreaker:
                    case FireArmRoundClass.X477_CornerFrag:
                    case FireArmRoundClass.M720A1prop0:
                    case FireArmRoundClass.M720A1prop1:
                    case FireArmRoundClass.M430A1:
                    case FireArmRoundClass.RLV_HEF:
                    case FireArmRoundClass.RLV_HEFJ:
                    case FireArmRoundClass.a35x32_HE:
                    case FireArmRoundClass.a35x32_HEDP:
                    case FireArmRoundClass.a35x32_INCEN:
                    case FireArmRoundClass.a84mm_HE441B:
                    case FireArmRoundClass.a84mm_HEAT751:
                    case FireArmRoundClass.a84mm_HEDP502:
                        return AmmoEnum.GrenadeHE;

                    case FireArmRoundClass.M576_MPAPERS:
                        return AmmoEnum.GrenadeBuckshot;

                    case FireArmRoundClass.M651_CSGAS:
                    case FireArmRoundClass.KS23_CSGas:
                    case FireArmRoundClass.Kol_Smokescreen:
                    case FireArmRoundClass.RLV_SMK:
                    case FireArmRoundClass.RLV_SF1:
                    case FireArmRoundClass.RLV_TPM:
                    case FireArmRoundClass.a35x32_SMOKE:
                    case FireArmRoundClass.a84mm_SMOKE469C:
                        return AmmoEnum.GrenadeSmoke;



                    //----Generic---------------------------

                    case FireArmRoundClass.DSM_Volt:
                    case FireArmRoundClass.DSM_Mag:
                    case FireArmRoundClass.a65_APDS:
                    case FireArmRoundClass.a65_Frangible:
                    case FireArmRoundClass.a65_HET:
                    case FireArmRoundClass.a65_HP:
                    case FireArmRoundClass.MF13g_Buck:
                    case FireArmRoundClass.MF13g_Slugger:
                    case FireArmRoundClass.MF13g_Blooper:
                    case FireArmRoundClass.MF13g_Bleeder:
                    case FireArmRoundClass.MF13g_Moonshot:
                    case FireArmRoundClass.MF1850_Barbie:
                    case FireArmRoundClass.MF1850_Drongo:
                    case FireArmRoundClass.MF1850_Gobsmacka:
                    case FireArmRoundClass.MF1232_Bushfire:
                    case FireArmRoundClass.MF1232_FunnelSpider:
                    case FireArmRoundClass.MF366_Retort:
                    case FireArmRoundClass.MF366_Debuff:
                    case FireArmRoundClass.MF366_Salute:
                    case FireArmRoundClass.MFRPG_Classic:
                    case FireArmRoundClass.MFRPG_RocketPop:
                    case FireArmRoundClass.MFRPG_ToTheMoon:
                    case FireArmRoundClass.MFRPG_RockIt:
                    case FireArmRoundClass.MFRPG_CannedMeat:
                    case FireArmRoundClass.MFRPG_WRONGAMMO:
                    case FireArmRoundClass.BTSP:
                    case FireArmRoundClass.MFStickyFrag:
                    case FireArmRoundClass.MFStickyRobbieBurns:
                    case FireArmRoundClass.MFStickyRustyNail:
                    case FireArmRoundClass.MFStickyHighlandFling:
                    case FireArmRoundClass.MFSyringeBloodfire:
                    case FireArmRoundClass.MFSyringeKnockout:
                    case FireArmRoundClass.MFSyringeRage:
                    case FireArmRoundClass.CM_1:
                    case FireArmRoundClass.CM_5:
                    case FireArmRoundClass.CM_10:
                    case FireArmRoundClass.CM_20:
                    case FireArmRoundClass.CM_100:
                    case FireArmRoundClass.CM_1000:
                        return AmmoEnum.Special;

                    case FireArmRoundClass.Freedomfetti:
                    case FireArmRoundClass.X1776_FreedomParty:
                        return AmmoEnum.Firework;

                    case FireArmRoundClass.FragExplosive:
                    case FireArmRoundClass.Frag12FA:
                    case FireArmRoundClass.Frag12HE:
                    case FireArmRoundClass.DSM_Frag:
                    case FireArmRoundClass.DSM_Mine:
                    case FireArmRoundClass.Mk211:
                    case FireArmRoundClass.BOOOMY:
                    case FireArmRoundClass.M381_HighExplosive:
                    case FireArmRoundClass.Kol_Frag:
                    case FireArmRoundClass.Kol_HEAT:
                    case FireArmRoundClass.Kol_Megabuck:
                    case FireArmRoundClass.Kol_Inferno:
                    case FireArmRoundClass.a20HE:
                    case FireArmRoundClass.a20HEI:
                    case FireArmRoundClass.a20SAPHEI:
                        return AmmoEnum.Explosive;

                    //----MISC---------------------------

                    case FireArmRoundClass.Flare:
                    case FireArmRoundClass.MFFlareClassic:
                    case FireArmRoundClass.MFFlareDangerClose:
                    case FireArmRoundClass.MFFlareSunburn:
                    case FireArmRoundClass.MFFlareConflagration:
                    case FireArmRoundClass.a84mm_ILLUM545C:
                        return AmmoEnum.Flare;

                    case FireArmRoundClass.KS23_Flash:
                    case FireArmRoundClass.Kol_TriFlash:
                        return AmmoEnum.Flash;


                    case FireArmRoundClass.M781_Practice:
                    case FireArmRoundClass.X828_Aurora:
                        return AmmoEnum.Practice;

                    //----Unsorted---------------------------

                    default:
                        return AmmoEnum.None;
                }
            }

            public static FireArmRoundClass AmmoToFireArmRound(AmmoEnum ammo)
            {
                FireArmRoundClass type = FireArmRoundClass.FMJ;

                switch (ammo)
                {
                    case AmmoEnum.Standard:
                        break;
                    case AmmoEnum.HollowPoint:
                        break;
                    case AmmoEnum.AP:
                        break;
                    case AmmoEnum.API:
                        break;
                    case AmmoEnum.Incendiary:
                        break;
                    case AmmoEnum.Tracer:
                        break;
                    case AmmoEnum.Subsonic_FMJ:
                        break;
                    case AmmoEnum.Subsonic_AP:
                        break;
                    case AmmoEnum.Subsonic_JHP:
                        break;
                    case AmmoEnum.PlusP_FMJ:
                        break;
                    case AmmoEnum.PlusP_JHP:
                        break;
                    case AmmoEnum.PlusP_API:
                        break;
                    case AmmoEnum.Buckshot:
                        break;
                    case AmmoEnum.Slug:
                        break;
                    case AmmoEnum.TripleHit:
                        break;
                    case AmmoEnum.Flechette:
                        break;
                    case AmmoEnum.ShellHE:
                        break;
                    case AmmoEnum.GrenadeHE:
                        break;
                    case AmmoEnum.GrenadeSmoke:
                        break;
                    case AmmoEnum.GrenadeBuckshot:
                        break;
                    case AmmoEnum.Practice:
                        break;
                    case AmmoEnum.Flare:
                        break;
                    case AmmoEnum.Flash:
                        break;
                    case AmmoEnum.Explosive:
                        break;
                    case AmmoEnum.Firework:
                        break;
                    case AmmoEnum.DragonsBreathe:
                        break;
                    case AmmoEnum.Random:
                        break;

                    case AmmoEnum.None:
                    case AmmoEnum.Special:
                    default:
                        //AM.GetDefaultRoundClass();
                        break;
                }


                return type;
            }
        }
    }

    /// <summary>
    /// The Ammo Table Ammo Type Array Position
    /// </summary>
    public enum AmmoEnum
    {
        None = -1,
        //----Rounds
        Standard = 0,   //FMJ / Default on Single Ammo types
        HollowPoint = 1,
        AP = 2,
        API = 3,
        Incendiary = 4,
        Tracer = 5,
        Subsonic_FMJ = 6,
        Subsonic_AP = 7,
        Subsonic_JHP = 8,
        PlusP_FMJ = 9,
        PlusP_JHP = 10,
        PlusP_API = 11,
        //----Shells
        Buckshot = 12,
        Slug = 13,
        TripleHit = 14,
        Flechette = 15,
        ShellHE = 16,
        //----Grenade Launchers
        GrenadeHE = 17,
        GrenadeSmoke = 18,
        GrenadeBuckshot = 19,
        //----Misc
        Practice = 20,
        Flare = 21,
        Flash = 22,
        Explosive = 23,
        Firework = 24,
        DragonsBreathe = 25,
        Random = 26,
        Special = 27,
    }

    public enum AmmoTableEnum
    {
        None = -1,      //Nothing Defined
        Standard = 1,   //All default Rounds and Ammo

        Ball = 0,
        FMJ = 1,
        JHP = 2,
        SP = 3,
        Tracer = 4,
        AP = 5,
        Incendiary = 6,
        APIncendiary = 7,
        Spitzer = 8,
        BTSP = 9,
        HighVelHP = 10,
        HyperVelHP = 11,
        Slug = 12,
        BuckShot00 = 13,
        BuckShot000 = 14,
        FragExplosive = 0xF,
        Flechette = 0x10,
        DragonsBreath = 17,
        BuckShotNo2 = 18,
        BuckShotNo4 = 19,
        TripleHit = 20,
        Freedomfetti = 21,
        Frag12HE = 22,
        Frag12FA = 23,
        Flare = 24,
        Cannonball = 25,
        Mk211 = 26,
        BuckShotNo1 = 27,
        Double = 28,
        DSM_Frag = 30,
        DSM_Mag = 0x1F,
        DSM_Mine = 0x20,
        DSM_Slugger = 33,
        DSM_Swarm = 34,
        DSM_Tracer = 35,
        DSM_TurboPenetrator = 36,
        DSM_Volt = 37,
        PlusP_FMJ = 40,
        PlusP_JHP = 41,
        PlusP_API = 42,
        NERMAL = 50,
        SPESHUL = 51,
        FLASHY = 52,
        MEGA = 53,
        BOOOMY = 54,
        POINTYOWW = 55,
        Mortar = 60,
        MIRV = 61,
        MegaBuckShot = 62,
        M381_HighExplosive = 70,
        M397_AirBurst = 71,
        M576_MPAPERS = 72,
        M651_CSGAS = 73,
        M781_Practice = 74,
        X214_SteelBreaker = 75,
        X477_CornerFrag = 76,
        X666_Baphomet = 77,
        X828_Aurora = 78,
        X1776_FreedomParty = 79,
        M720A1prop0 = 80,
        M720A1prop1 = 81,
        M430A1 = 82,
        KS23_Buckshot = 90,
        KS23_Barricade = 91,
        KS23_CSGas = 92,
        KS23_Flash = 93,
        Subsonic_FMJ = 100,
        Subsonic_AP = 101,
        Subsonic_JHP = 102,
        Kol_Frag = 110,
        Kol_HEAT = 111,
        Kol_Inferno = 112,
        Kol_Megabuck = 113,
        Kol_Smokescreen = 114,
        Kol_TriFlash = 115,
        RLV_HEF = 120,
        RLV_HEFJ = 121,
        RLV_SMK = 122,
        RLV_SF1 = 123,
        RLV_TPM = 124,
        MF366_Retort = 130,
        MF366_Debuff = 131,
        MF366_Salute = 132,
        MF1850_Barbie = 140,
        MF1850_Drongo = 141,
        MF1850_Gobsmacka = 142,
        MF1232_Bushfire = 150,
        MF1232_FunnelSpider = 151,
        MFRPG_Classic = 160,
        MFRPG_RocketPop = 161,
        MFRPG_ToTheMoon = 162,
        MFRPG_RockIt = 163,
        MFRPG_CannedMeat = 164,
        MFRPG_WRONGAMMO = 165,
        MF13g_Buck = 170,
        MF13g_Slugger = 171,
        MF13g_Blooper = 172,
        MF13g_Bleeder = 173,
        MF13g_Moonshot = 174,
        a20FMJ = 180,
        a20HE = 181,
        a20HEI = 182,
        a20SAPHEI = 183,
        a20APDS = 184,
        a20AP = 185,
        MFStickyFrag = 190,
        MFStickyRobbieBurns = 191,
        MFStickyRustyNail = 192,
        MFStickyHighlandFling = 193,
        MFSyringeBloodfire = 200,
        MFSyringeKnockout = 201,
        MFSyringeRage = 202,
        MFFlareClassic = 210,
        MFFlareDangerClose = 211,
        MFFlareSunburn = 212,
        MFFlareConflagration = 213,
        CM_1 = 220,
        CM_5 = 221,
        CM_10 = 222,
        CM_20 = 223,
        CM_100 = 224,
        CM_1000 = 225,
        a35x32_HE = 230,
        a35x32_HEDP = 231,
        a35x32_INCEN = 232,
        a35x32_SMOKE = 233,
        a84mm_HE441B = 235,
        a84mm_HEAT751 = 236,
        a84mm_HEDP502 = 237,
        a84mm_ILLUM545C = 238,
        a84mm_SMOKE469C = 239,
        a65_APDS = 240,
        a65_Frangible = 241,
        a65_HET = 242,
        a65_HP = 243
    }
}