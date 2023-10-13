using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Supply_Raid_Editor
{
    public class ObjectGroupUI : MonoBehaviour
    {
        public InputField inputName;
        public ObjectGroup objectGroup;


        public void EditName()
        {
            objectGroup.name = inputName.text;
            ItemCategoryUI.instance.objectIDPanel.SetActive(false);
        }

        public void OpenObjectGroup()
        {
            ItemCategoryUI.instance.OpenObjectGroup(this);
        }

        public void RemoveObjectGroup()
        {
            ItemCategoryUI.instance.RemoveObjectGroup(objectGroup);
        }
    }
}