using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemTableContent : MonoBehaviour
{
    public string[] tagList;    //Enum
    public Text title;      //Text of the Content
    public Transform content; //Where drop down goes
    public List<Dropdown> dropdowns = new List<Dropdown>();

    public void RemoveLastDropdown()
    {
        if (dropdowns.Count <= 0)
            return;

        Destroy(dropdowns[dropdowns.Count - 1].gameObject);
        dropdowns.RemoveAt(dropdowns.Count - 1);
    }

    public void ClearAllDropdowns()
    {
        for (int i = 0; i < dropdowns.Count; i++)
        {
            Destroy(dropdowns[i].gameObject);
        }
        dropdowns.Clear();
    }

    public void AddDropdown(GameObject prefab)
    { 
        Dropdown dd = Instantiate(prefab, content).GetComponent<Dropdown>();

        dd.ClearOptions();

        List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();

        for (int i = 0; i < tagList.Length; i++)
        {
            Dropdown.OptionData item = new Dropdown.OptionData { text = tagList[i] };
            list.Add(item);
        }

        dd.AddOptions(list);
        dropdowns.Add(dd);
    }
}
