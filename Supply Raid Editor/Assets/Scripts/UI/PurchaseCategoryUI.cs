using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Supply_Raid_Editor
{
    public class PurchaseCategoryUI : MonoBehaviour
    {
        public InputField cost;
        public InputField itemCategory;

        public void RemovePurchaseCategory()
        {
            CharacterUI.instance.RemovePurchaseCategoryUI(this);
        }
    }
}
