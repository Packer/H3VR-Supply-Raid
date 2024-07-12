using System.Collections;
using Atlas;
using System.Collections.Generic;
using UnityEngine;

public class SR_Menu_Button : MonoBehaviour 
{
    public bool isFavorited;

    public GameObject favoriteButton;

    public CustomSceneInfo sceneInfo;

    public void SelectMap()
    {
        SR_Menu_Manager.instance.SelectMap(this);
    }

    public void ToggleFavorited()
    {
        if (isFavorited)
        {
            isFavorited = false;
        }
        else
        {
            isFavorited = true;
        }
    }
}
