using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterUI : MonoBehaviour
{

    public GameObject[] pages;


    public void OpenPage(int i)
    {
        CloseAllPages();
        pages[i].SetActive(true);
    }

    void CloseAllPages()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if(pages[i] != null)
                pages[i].SetActive(false);
        }
    }
}
