﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using FistVR;
using H3MP.Networking;

namespace SupplyRaid
{
    public class SR_Compass : MonoBehaviour
    {
        public static SR_Compass instance;

        [Header("Directions")]
        public Transform supplyPointDirection;
        public Transform lastSupplyDirection;
        public Text captureText;

        [Header("Setup")]
        public Transform face;  //Faces players head
        public Text directionText;
        public Text pointsText;
        public float distance = .25f;
        public Slider healthBar;
        public Text healthText;
        //public Text levelText;
        public Text capturesText;

        [Header("Networking")]
        public GameObject playerArrowPrefab;
        public int playerCount = 0;
        public List<Transform> playerArrows = new List<Transform>();

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            //v1.0.6 Hackjob
            Transform childArrow = Instantiate(supplyPointDirection.GetChild(0).gameObject, supplyPointDirection.GetChild(0)).transform;
            supplyPointDirection.GetChild(0).GetComponent<Image>().enabled = false;
            supplyPointDirection.GetChild(0).localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
            childArrow.localRotation = Quaternion.Euler(new Vector3(270, 0, 0));
            childArrow.localPosition = Vector3.zero;
        }

        void Update()
        {
            //If There is a manager and we're in gameplay
            if (SR_Manager.instance == null || !SR_Manager.instance.gameRunning)
                return;

            //GM.CurrentPlayerBody.transform.position;
            if (!SR_Manager.profile.hand)
                transform.position = GM.CurrentPlayerBody.LeftHand.position - GM.CurrentPlayerBody.LeftHand.forward * distance;
            else
                transform.position = GM.CurrentPlayerBody.RightHand.position - GM.CurrentPlayerBody.RightHand.forward * distance;
            transform.rotation = Quaternion.identity;

            face.LookAt(GM.CurrentPlayerBody.Head.position);

            //Direction
            //Quaternion pointRotation = SR_Manager.profile.hand ? GM.CurrentPlayerBody.RightHand.rotation : GM.CurrentPlayerBody.LeftHand.rotation;
            directionText.text = Mathf.FloorToInt(face.eulerAngles.y).ToString();

            //Last SupplyPoint
            Vector3 pos = SR_Manager.LastSupplyPoint().buyMenu.position;
            pos.y = transform.position.y;
            lastSupplyDirection.LookAt(pos);

            healthBar.value = GM.GetPlayerHealth();
            
            //Not in v1.0.0, error check
            if(healthText != null)
                healthText.text = (Mathf.CeilToInt(GM.GetPlayerHealth() * GM.CurrentPlayerBody.GetMaxHealthPlayerRaw())).ToString();
            
            Transform marker = SR_Manager.instance.GetCompassMarker();
            if (!marker)
                return;
            pos = marker.position;
            supplyPointDirection.GetChild(0).LookAt(pos); //Arrow points straight at position

            pos.y = transform.position.y;
            supplyPointDirection.LookAt(pos);

            pointsText.text = SR_Manager.instance.Points.ToString();
            capturesText.text = (SR_Manager.instance.CurrentCaptures - SR_Manager.profile.startLevel).ToString();

            if(SupplyRaidPlugin.h3mpEnabled)
                NetworkUpdate();
        }

        void NetworkUpdate()
        {
            if (!Networking.ServerRunning())
                return;

            //Player count updated, update arrows
            if (playerCount != Networking.GetPlayerCount())
            {
                CreatePlayerArrows();
                playerCount = Networking.GetPlayerCount();
            }

            int[] playerIDs = Networking.GetPlayerIDs();

            //Update Arrows to look at players
            for (int i = 0; i < playerIDs.Length; i++)
            {
                Vector3 pos = Networking.GetPlayer(playerIDs[i]).head.position;
                pos.y = transform.position.y;
                playerArrows[i].transform.LookAt(pos);
            }
        }

        void CreatePlayerArrows()
        {
            //Clear Old Arrows
            for (int i = 0; i < playerArrows.Count; i++)
            {
                Destroy(playerArrows[i].gameObject);
            }

            playerArrows.Clear();
            
            for (int i = 0; i < Networking.GetPlayerCount(); i++)
            {
                GameObject arrow = Instantiate(playerArrowPrefab, playerArrowPrefab.transform.parent);
                arrow.SetActive(true);
                playerArrows.Add(arrow.transform);
            }
        }
    }
}