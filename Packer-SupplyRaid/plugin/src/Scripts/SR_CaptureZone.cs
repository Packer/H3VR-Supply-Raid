using FistVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using H3MP.Networking;

namespace SupplyRaid
{
    public class SR_CaptureZone : MonoBehaviour
    {
        public bool replaced = false;
        public Transform zone;
        [HideInInspector]
        public float captureRemain = 15;
        private float captureTick = 1;

        [Header("Audio")]
        //public AudioSource audioSource;
        //public AudioClip audioTick;
        //public AudioClip audioTickAlmost;
        //public AudioClip audioFail;
        private Bounds bounds;

        private float guardCaptureTime = 0;

        void Start()
        {
            gameObject.SetActive(false);
            if (SR_Manager.instance.captureZone != this)
                gameObject.SetActive(false);
        }

        void Update()
        {
            if (!SR_Compass.instance)
                return;

            if (SR_Manager.instance.gameRunning && SR_Manager.profile.captureZone == true)
                CaptureZoneScan();
            else
                gameObject.SetActive(false);

        }

        void CaptureZoneScan()
        {
            captureTick -= Time.deltaTime;
            if (captureTick <= 0f)
            {
                if (WithinCaptureZone())
                {
                    captureRemain -= 1;
                    if (captureRemain < 0)
                        captureRemain = 0;

                    captureTick = 1;

                    //Guard Capture Zone if alerted to player
                    if (guardCaptureTime < Time.time)
                    {
                        guardCaptureTime = Time.time + Random.Range(2, 10);

                        //Sosig Alertness (If somewhat aware, make them more aware)
                        for (int i = 0; i < SR_Manager.instance.defenderSosigs.Count; i++)
                        {
                            Sosig sosig = SR_Manager.instance.defenderSosigs[i];

                            /*
                            //Broke in 114
                            if (sosig.m_alertnessLevel < 0.25f)
                                continue;
                            */

                            Transform capturePoint = SR_Manager.AttackSupplyPoint().captureZone;
                            float x = capturePoint.localScale.x / 2;
                            float z = capturePoint.localScale.z / 2;
                            Vector3 guardPoint = capturePoint.position + new Vector3(Random.Range(-x, x), 0 , Random.Range(-z, z));

                            sosig.CommandGuardPoint(guardPoint, false);
                            /*
                            sosig.m_pathToPoint = transform.position;
                            sosig.SetGuardInvestigateDistanceThreshold(10);
                            sosig.CoverSearchRange = 10;

                            sosig.SetCurrentOrder(Sosig.SosigOrder.GuardPoint);
                            sosig.m_guardPoint = transform.position;
                            sosig.m_guardDominantDirection = transform.rotation.eulerAngles;
                            */
                            /*
                            //SR_Manager.instance.sosigAlertness = 0;

                            if (SR_Manager.instance.defenderSosigs[i].m_alertnessLevel >= 0)
                            {
                                SR_Manager.instance.defenderSosigs[i].m_alertnessLevel += 1.05f;
                            }

                            if (SR_Manager.instance.defenderSosigs[i].m_alertnessLevel >= 1
                                && !SR_Global.IsSosigInCombat(SR_Manager.instance.defenderSosigs[i]))
                            {
                            }
                            */
                        }
                    }

                    //Count Down Sounds
                    if (captureRemain > 5)
                    {
                        SR_Manager.PlayTickSFX();
                    }
                    else
                        SR_Manager.PlayTickAlmostSFX();

                    //Visuals
                    if (SR_Compass.instance && SR_Compass.instance.captureText.gameObject.activeSelf == false)
                    {
                        SR_Compass.instance.captureText.gameObject.SetActive(true);
                        /*
                        //BGM
                        if (SupplyRaidPlugin.bgmEnabled)
                        {
                            BGM.SetHoldMusic(SR_Manager.instance.CurrentCaptures);
                        }
                        */
                    }

                    if(SR_Compass.instance)
                        SR_Compass.instance.captureText.text = Mathf.RoundToInt(captureRemain).ToString();
                    //SR_Compass.instance.captureText.transform.parent.LookAt(GM.CurrentPlayerBody.Head);

                    //Captued the point
                    if (captureRemain <= 0)
                    {
                        if (Networking.IsClient())
                        {
                            //Move Zone out of the way
                            zone.position -= Vector3.up * 100;
                            SR_Networking.instance.CapturedZone_Send();
                        }
                        else
                        {
                            if (SR_Manager.instance.captureProtection <= 0)
                                SR_Manager.instance.CapturedPoint();
                        }

                        captureRemain = SR_Manager.AttackSupplyPoint().captureTime;
                        captureTick = Random.Range(0, 3);
                        if (SR_Compass.instance)
                            SR_Compass.instance.captureText.gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (SR_Compass.instance && SR_Compass.instance.captureText.gameObject.activeSelf == true)
                    {
                        if (SR_Manager.instance.captureProtection <= 0)
                        {
                            SR_Manager.PlayFailSFX();
                            /*
                            //BGM
                            if (SupplyRaidPlugin.bgmEnabled)
                            {
                                BGM.SetHoldMusic(SR_Manager.instance.CurrentCaptures);
                            }
                            */
                        }
                        if (SR_Compass.instance)
                            SR_Compass.instance.captureText.gameObject.SetActive(false);
                    }
                    captureRemain = SR_Manager.AttackSupplyPoint().captureTime;
                    captureTick = Random.Range(0, 2);
                }
            }
        }

        public void MoveCaptureZone(Transform point)
        {
            zone.position = point.position;
            zone.localScale = point.localScale;
            zone.rotation = point.rotation;

            bounds.center = zone.position;
            bounds.size = zone.localScale;

            //Update Capture Time
            captureRemain = SR_Manager.AttackSupplyPoint().captureTime;
        }

        bool WithinCaptureZone()
        {
            Vector3 localPoint = Quaternion.Inverse(zone.rotation) * (GM.CurrentPlayerBody.Head.position - zone.position);

            // Check if the local point is within the box's bounds
            return Mathf.Abs(localPoint.x) <= zone.localScale.x / 2f &&
                   Mathf.Abs(localPoint.y) <= zone.localScale.y / 2f &&
                   Mathf.Abs(localPoint.z) <= zone.localScale.z / 2f;
        }


    }
}