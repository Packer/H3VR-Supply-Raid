using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Supply_Raid_Editor;

public class GenericButton : MonoBehaviour
{
    public int index = -1;
    public GameObject go;
    public Text text;
    public string description;
    public Image image;
    public int id;
    public bool toggle = false;

    public void OpenSosigLibrary(InputField field)
    {
        LibraryManager.instance.OpenSosigLibrary(field);
    }

    public void SelectLibraryItem()
    {
        LibraryManager.selectedItem = this;
        LibraryManager.instance.CloseLibrary(id, false);
    }

    public void OpenLevelGroup()
    {
        Debug.Log("Opening " + (toggle ? "endless" : "Level"));
        if (toggle)  //Endless
            FactionUI.instance.OpenEndlessLevel(index);
        else
            FactionUI.instance.OpenLevel(index);
    }

    public void DestroyLevel()
    {
        if (toggle)
            DataManager.Faction().endless.RemoveAt(index);
        else
            DataManager.Faction().levels.RemoveAt(index);

        FactionUI.instance.OpenLevels();
    }
}
