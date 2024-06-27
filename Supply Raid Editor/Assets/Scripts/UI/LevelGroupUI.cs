using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Supply_Raid_Editor
{
    public class LevelGroupUI : MonoBehaviour
    {
        public int id = -1;

        public void OpenLevel()
        {
            FactionUI.instance.OpenLevel(id);
        }

        public void DeleteLevel()
        {
            DataManager.Faction().levels.RemoveAt(id);
        }

        public void OpenEndlessLevel()
        {
            FactionUI.instance.OpenEndlessLevel(id);
        }

        public void DeleteEndlessLevel()
        {
            DataManager.Faction().endless.RemoveAt(id);
        }
    }
}