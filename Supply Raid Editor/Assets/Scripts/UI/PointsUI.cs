using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Supply_Raid_Editor
{
    public class PointsUI : MonoBehaviour
    {
        public int index = -1;
        public InputField inputField;
        public Text text;
        public int id;

        public void RemovePointsLevel()
        {
            CharacterUI.instance.RemovePointsTab(this);
        }

        public void RemoveBossID()
        {
            FactionUI.instance.RemovePoolItem(id, this);
        }
    }
}