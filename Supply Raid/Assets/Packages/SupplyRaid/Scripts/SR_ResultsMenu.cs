using FistVR;
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
        public Text score;

        void Awake()
        {
            instance = this;
        }

        public void UpdateResults()
        {
            if (SR_Manager.instance == null || gameObject.activeSelf == false)
                return;

            if (!SR_Manager.instance.optionRespawn && SR_Manager.instance.optionCaptures <= 0)
            {
                objectiveComplete.text = "ENDED";
                objectiveComplete.color = Color.grey;
            }
            else
            {
                objectiveComplete.text = SR_Manager.instance.stats.ObjectiveComplete ? "COMPLETE" : "FAILED";
                objectiveComplete.color = SR_Manager.instance.stats.ObjectiveComplete ? Color.green : Color.red;
            }

            gameTime.text = FloatToTime(SR_Manager.instance.stats.GameTime);
            deaths.text = SR_Manager.instance.stats.Deaths.ToString();

            if (SR_Manager.instance.inEndless)
                level.text = (SR_Manager.instance.CurrentLevel + SR_Manager.instance.faction.levels.Length).ToString();
            else
                level.text = SR_Manager.instance.CurrentLevel.ToString();

            captures.text = SR_Manager.instance.CapturesTotal.ToString();
            kills.text = SR_Manager.instance.stats.Kills.ToString();

            if(score)
                score.text = SR_Manager.instance.stats.GetScore().ToString();
        }

        public static string FloatToTime(float toConvert)
        { 
            return string.Format("{0:#00}:{1:00}", Mathf.Floor(toConvert / 60f), Mathf.Floor(toConvert) % 60f);
        }

        public void ResetGame()
        {
            SR_Manager.PlayCompleteSFX();
            SteamVR_LoadLevel.Begin(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, false, 0.5f, 0f, 0f, 0f, 1f);

            //SR_Manager.instance.ResetGame();
        }

        public void MainMenu()
        {
            SR_Manager.PlayCompleteSFX();
            SteamVR_LoadLevel.Begin("MainMenu3", false, 0.5f, 0f, 0f, 0f, 1f);
        }
    }
}