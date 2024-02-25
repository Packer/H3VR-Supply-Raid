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

        void Start()
        {
            gameObject.SetActive(false);
        }

        void Update()
        {
            if (SR_Manager.instance.gameRunning && SR_Manager.instance.optionCaptureZone == true)
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

                    //Count Down Sounds
                    if (captureRemain > 5)
                        SR_Manager.PlayTickSFX();
                    else
                        SR_Manager.PlayTickAlmostSFX();

                    
                    //Visuals
                    if (SR_Compass.instance.captureText.gameObject.activeSelf == false)
                        SR_Compass.instance.captureText.gameObject.SetActive(true);

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
                        SR_Compass.instance.captureText.gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (SR_Compass.instance.captureText.gameObject.activeSelf == true)
                    {
                        if(SR_Manager.instance.captureProtection <= 0)
                            SR_Manager.PlayFailSFX();
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