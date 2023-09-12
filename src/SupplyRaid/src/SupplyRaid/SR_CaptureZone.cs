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
        public Text captureText;
        [HideInInspector]
        public float captureRemain = 15;
        public float captureTick = 1;
        public AudioSource audioSource;
        public AudioClip audioTick;
        public AudioClip audioTickAlmost;
        public AudioClip audioFail;
        private Bounds bounds;

        void Start()
        {
            gameObject.SetActive(false);
        }

        void Update()
        {
            if (SR_Manager.instance.running && SR_Manager.instance.optionCaptureZone == true)
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
                    captureTick = 1;

                    //Count Down Sounds
                    if(captureRemain > 5)
                        audioSource.PlayOneShot(audioTick);
                    else
                        audioSource.PlayOneShot(audioTickAlmost);

                    //Visuals
                    if (captureText.gameObject.activeSelf == false)
                        captureText.gameObject.SetActive(true);

                    captureText.text = Mathf.RoundToInt(captureRemain).ToString();
                    captureText.transform.parent.LookAt(GM.CurrentPlayerBody.Head);

                    //Captued the point
                    if (captureRemain <= 0)
                    {

                        if(Networking.IsClient())
                            SR_Networking.instance.CapturedZone_Send();
                        else
                            SR_Manager.instance.GameCompleteCheck(SR_Manager.instance.level + 1);

                        captureRemain = SR_Manager.CurrentSupplyPoint().captureTime;
                        captureTick = Random.Range(0, 3);
                        captureText.gameObject.SetActive(false);

                    }
                }
                else
                {
                    if (captureText.gameObject.activeSelf == true)
                    {
                        audioSource.PlayOneShot(audioFail);
                        captureText.gameObject.SetActive(false);
                    }
                    captureRemain = SR_Manager.CurrentSupplyPoint().captureTime;
                    captureTick = Random.Range(0, 2);
                }
            }
        }

        public void MoveCaptureZone(Vector3 position, Vector3 scale)
        {
            transform.position = position;
            transform.localScale = scale;

            bounds.center = transform.position;
            bounds.size = transform.localScale;

            //Update Capture Time
            captureRemain = SR_Manager.CurrentSupplyPoint().captureTime;
        }

        bool WithinCaptureZone()
        {
            if (bounds.Contains(GM.CurrentPlayerBody.Head.position))
                return true;

            return false;
        }
    }
}