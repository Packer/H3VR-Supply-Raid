using UnityEngine;
using UnityEngine.UI;

namespace Supply_Raid_Editor
{
    public class ObjectIDUI : MonoBehaviour
    {
        public int index = -1;
        public InputField inputField;
        public ObjectGroup objectGroup;

        public void UpdateName()
        {
            objectGroup.objectID[index] = inputField.text;
        }

        public void RemoveObjectID()
        {
            ItemCategoryUI.instance.RemoveObjectID(inputField.text);
        }
    }
}