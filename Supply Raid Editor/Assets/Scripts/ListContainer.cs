using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Supply_Raid_Editor
{
    public class ListContainer : MonoBehaviour
    {
        public InputField inputField;
        public InputField inputFieldB;
        public InputField inputFieldC;

        public void CreateNewPurchaseCategory()
        {
            MenuManager.instance.CreateNewPurchaseCategory();
        }

        public void RemoveFromPurchaseCategories()
        {
            MenuManager.instance.DeletePurchaseCategory(this);
        }


        public void CreateNewStartGear()
        {
            MenuManager.instance.CreateNewStartGear();
        }

        public void RemoveFromStartGear()
        {
            MenuManager.instance.DeleteStartGear(this);
        }
    }
}