using Atlas;
using Sodalite.ModPanel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SR_Menu_Manager : MonoBehaviour 
{
    public Transform menuContent;
    public GameObject menuUpButton;
    public GameObject menuDownButton;
    public Vector3 menuMoveValue;
    public AudioSource menuUpAudio;
    public AudioSource menuDownAudio;

    public RectTransform menuContentScroll;

    public GameObject buttonPrefab;

    public Text mapDescription;
    public RawImage mapThumb;
    [HideInInspector]
    public string mapID;
    [HideInInspector]
    public CustomSceneInfo currentlySelectedScene;
    public AudioSource startSound;

    public static SR_Menu_Manager instance;

    [HideInInspector]
	public List<SR_Menu_Button> supplyRaidMaps;
    [HideInInspector]
    public List<SR_Menu_Button> favoritedMaps;
    
    void Awake()
    {
        instance = this;
    }

    void Start () 
	{
        for (var a = menuContent.childCount - 1; a >= 0; a--)
        {
            Object.Destroy(menuContent.GetChild(a).gameObject);
        }

        foreach (var item in AtlasPlugin.CustomSceneInfos)
        {
            if (item.GameMode == "supplyraid" || item.DisplayMode == "supplyraid")
            {
                SR_Menu_Button button = Instantiate(buttonPrefab, menuContent).GetComponent<SR_Menu_Button>();

                button.sceneInfo = item;

                RawImage rawImage = button.GetComponent<RawImage>();
                rawImage.texture = button.sceneInfo.ThumbnailTexture;

                supplyRaidMaps.Add(button);
            }
        }
    }

    public void SelectMap(SR_Menu_Button button)
    {
        mapDescription.text = button.sceneInfo.Description;
        mapThumb.texture = button.sceneInfo.ThumbnailTexture;
        mapID = button.sceneInfo.Identifier;
        currentlySelectedScene = button.sceneInfo;
    }

    public void LaunchMap()
    {
        AtlasPlugin.LoadCustomScene(currentlySelectedScene);

        startSound.Play();

        //SteamVR_LoadLevel.Begin(instance.mapID, false, 0.5f, 0f, 0f, 0f, 1f);
    }

    public void MenuUp()
    {
        menuContentScroll.position -= menuMoveValue;

        menuUpAudio.Play();
    }

    public void MenuDown()
    {
        menuContentScroll.position += menuMoveValue;

        menuDownAudio.Play();
    }
}
