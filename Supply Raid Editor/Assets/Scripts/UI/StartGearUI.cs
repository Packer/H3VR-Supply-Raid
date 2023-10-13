using Supply_Raid_Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Supply_Raid_Editor
{
    public class StartGearUI : MonoBehaviour
    {
        public InputField itemCategory;

        public void RemoveStartGear()
        {
            CharacterUI.instance.RemoveStartGear(this);
        }
    }
}