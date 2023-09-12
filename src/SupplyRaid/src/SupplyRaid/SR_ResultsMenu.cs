using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SupplyRaid
{

    public class SR_ResultsMenu : MonoBehaviour
    {
        public static SR_ResultsMenu instance;

        public Text objectiveComplete;
        public Text gameTime;
        public Text deaths;
        public Text level;
        public Text captures;
        public Text kills;

        void Awake()
        {
            instance = this;
        }

        public void UpdateStats()
        {
            if (!SR_Manager.instance.optionRespawn && SR_Manager.instance.optionCaptures <= 0)
            {
                objectiveComplete.text = "ENDED";
                objectiveComplete.color = Color.grey;
            }
            else
            {
                objectiveComplete.text = SR_Manager.instance.statObjectiveComplete ? "COMPLETE" : "FAILED";
                objectiveComplete.color = SR_Manager.instance.statObjectiveComplete ? Color.green : Color.red;
            }

            gameTime.text = FloatToTime(SR_Manager.instance.gameTime);
            deaths.text = SR_Manager.instance.statDeaths.ToString();
            level.text = SR_Manager.instance.level.ToString();
            captures.text = SR_Manager.instance.statCaptures.ToString();
            kills.text = SR_Manager.instance.statKills.ToString();
        }

        string FloatToTime(float toConvert)
        { 
            return string.Format("{0:#00}:{1:00}", Mathf.Floor(toConvert / 60f), Mathf.Floor(toConvert) % 60f);
        }

        public void ResetGame()
        {
            SteamVR_LoadLevel.Begin(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, false, 0.5f, 0f, 0f, 0f, 1f);

            //SR_Manager.instance.ResetGame();
        }

        public void MainMenu()
        {
            SteamVR_LoadLevel.Begin("MainMenu3", false, 0.5f, 0f, 0f, 0f, 1f);
        }
    }
}