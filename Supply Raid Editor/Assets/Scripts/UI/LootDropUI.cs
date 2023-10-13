using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Supply_Raid_Editor
{
    public class LootDropUI : MonoBehaviour
    {
        public InputField chance;
        public InputField itemCategory;


        public void RemoveLootDrop()
        {
            CharacterUI.instance.RemoveLootDropUI(this);
        }
    }
}