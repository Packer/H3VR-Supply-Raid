using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using H3MP.Networking;
using FistVR;

namespace SupplyRaid
{

    public class SR_Compass : MonoBehaviour
    {
        [Header("Directions")]
        public Transform supplyPointDirection;
        public Transform lastSupplyDirection;

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

        void Update()
        {
            //If There is a manager and we're in gameplay
            if (SR_Manager.instance == null || !SR_Manager.instance.gameRunning)
                return;

            //GM.CurrentPlayerBody.transform.position;
            if (!SR_Manager.instance.optionHand)
                transform.position = GM.CurrentPlayerBody.LeftHand.position - GM.CurrentPlayerBody.LeftHand.forward * distance;
            else
                transform.position = GM.CurrentPlayerBody.RightHand.position - GM.CurrentPlayerBody.RightHand.forward * distance;
            transform.rotation = Quaternion.identity;

            face.transform.LookAt(GM.CurrentPlayerBody.Head.position);

            //Direction
            directionText.text = Mathf.FloorToInt(GM.CurrentPlayerBody.Head.rotation.eulerAngles.y).ToString();

            //Last SupplyPoint
            Vector3 pos = SR_Manager.instance.GetLastSupplyPoint().position;
            pos.y = transform.position.y;
            lastSupplyDirection.LookAt(pos);

            //H3MP Setup NETWORKING player points

            healthBar.value = GM.GetPlayerHealth();
            
            //Not in v1.0.0, error check
            if(healthText != null)
                healthText.text = (Mathf.CeilToInt(GM.GetPlayerHealth() * GM.CurrentPlayerBody.GetPlayerHealthRaw())).ToString();

            Transform marker = SR_Manager.instance.GetCompassMarker();
            if (!marker)
                return;
            pos = marker.position;
            pos.y = transform.position.y;
            supplyPointDirection.LookAt(pos);

            pointsText.text = SR_Manager.instance.Points.ToString();
            capturesText.text = (SR_Manager.instance.CurrentCaptures - SR_Manager.instance.optionStartLevel).ToString();

            /*
            if(SR_Manager.instance.inEndless)
                levelText.text = (SR_Manager.instance.CurrentFactionLevel + SR_Manager.instance.faction.levels.Length).ToString();
            else
                levelText.text = SR_Manager.GetFactionLevel().ToString();
            */

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

            //DO nothing if single player
            //if (GameManager.players.Count <= 1)
            //    return;
            
            for (int i = 0; i < Networking.GetPlayerCount(); i++)
            {
                GameObject arrow = Instantiate(playerArrowPrefab, playerArrowPrefab.transform.parent);
                arrow.SetActive(true);
                playerArrows.Add(arrow.transform);
            }
        }
    }
}