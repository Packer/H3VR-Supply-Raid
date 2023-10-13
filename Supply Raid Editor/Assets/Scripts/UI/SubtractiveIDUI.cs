using UnityEngine;
using UnityEngine.UI;

namespace Supply_Raid_Editor
{
    public class SubtractiveIDUI : MonoBehaviour
    {
        public int index = -1;
        public InputField inputField;
        public SR_ItemCategory category;

        public void UpdateName()
        {
            category.subtractionID[index] = inputField.text;
        }

        public void RemoveSubtractiveID()
        {
            ItemCategoryUI.instance.RemoveSubtractionID(inputField.text);
        }

        public void UpdateCharacterName()
        {
            DataManager.Character().subtractionObjectIDs[index] = inputField.text;
            CharacterUI.instance.UpdateCharacterUI();
        }

        public void RemoveSubtractiveCharacterID()
        {
            CharacterUI.instance.RemoveSubtraction(this);
        }
    }
}
