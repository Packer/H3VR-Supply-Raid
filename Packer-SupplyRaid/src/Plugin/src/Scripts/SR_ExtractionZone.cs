using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using FistVR;

namespace SupplyRaid
{
    public class SR_ExtractionZone : MonoBehaviour
    {
        public Transform extractionPoint;
        public float timeToExtract = 10;
        private float extractTimer = 0;
        private bool playerInZone = false;
        private float nextScan = 0;
        private float boundsDistance = 0;
        private float extractionTick = 0;

        void Start()
        {
            if (transform.parent != null)
                transform.SetParent(null);

            boundsDistance = transform.localScale.x;

            if (boundsDistance < transform.localScale.y)
                boundsDistance = transform.localScale.y;

            if (boundsDistance < transform.localScale.z)
                boundsDistance = transform.localScale.z;
        }

        void Update()
        {
            if (SR_Menu.instance.countDownCanvas == null)
                return;

            //Excute once at second
            if ((extractionTick -= Time.deltaTime) <= 0)
            {
                extractionTick = 1;
                UpdateScan();
                UpdateTimerText();
                if(playerInZone)
                    SR_Manager.PlayExtractionTickSFX();
            }
        }

        void UpdateTimerText()
        {
            if (playerInZone)
            {
                SR_Menu.instance.countDownCanvas.SetActive(true);
                SR_Menu.instance.countDownText.text = Mathf.RoundToInt(10 - extractTimer).ToString();
                SR_Menu.instance.countDownCanvas.transform.position = GM.CurrentPlayerBody.Head.transform.position;
                SR_Menu.instance.countDownCanvas.transform.rotation = GM.CurrentPlayerBody.Head.transform.rotation;
            }
            else
                SR_Menu.instance.countDownCanvas.SetActive(false);
        }

        void UpdateScan()
        {
            //Waiting for Next Scan


            //Inside Zone - Every update if inside
            if (playerInZone)
            {
                Debug.Log("InZone");
                if (!WithinZone(GM.CurrentPlayerBody.Head.position))
                {
                    Debug.Log("Player Left Zone");
                    playerInZone = false;
                    extractTimer = 0;
                    nextScan = 0;
                    return;
                }
                else
                {
                    //Timer
                    extractTimer += 1;

                    //Extract the player
                    if (extractTimer >= timeToExtract)
                    {
                        Debug.Log("EXTRACTED");
                        nextScan = 0;
                        extractTimer = 0;
                        playerInZone = false;
                        TeleportPlayer();
                        return;
                    }
                }
            }

            //Outside Zone
            if (nextScan < Time.time && !playerInZone)
            {
                if (WithinZone(GM.CurrentPlayerBody.Head.position))
                {
                    Debug.Log("Player Entered Zone");
                    playerInZone = true;
                    nextScan = 0;
                    return;
                }

                //How close are we to the zone to do our next check (Outside quadruple distance)
                if (Vector3.Distance(GM.CurrentPlayerBody.Head.position, transform.position) > boundsDistance * 2)
                {
                    nextScan = Time.time + Random.Range(0, 5);
                }
                else //Near it, start scanning
                    nextScan = 0;
            }
        }

        void TeleportPlayer()
        {
            GM.CurrentMovementManager.TeleportToPoint(
                extractionPoint.position,
                true,
                extractionPoint.rotation.eulerAngles);
        }

        bool WithinZone(Vector3 point)
        {
            Vector3 localPoint = Quaternion.Inverse(transform.rotation) * (point - transform.position);

            // Check if the local point is within the box's bounds
            return Mathf.Abs(localPoint.x) <= transform.localScale.x / 2f &&
                   Mathf.Abs(localPoint.y) <= transform.localScale.y / 2f &&
                   Mathf.Abs(localPoint.z) <= transform.localScale.z / 2f;
        }

        void OnDrawGizmos()
        {
            if (extractionPoint != null)
            {
                Gizmos.color = Color.green;
                Vector3 pointCenter = extractionPoint.position + Vector3.up * 0.9f;
                Gizmos.DrawWireCube(pointCenter, new Vector3(0.5f, 1.8f, 0.5f));
                Gizmos.DrawLine(pointCenter, pointCenter + extractionPoint.forward);

                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }
        }
    }
}
