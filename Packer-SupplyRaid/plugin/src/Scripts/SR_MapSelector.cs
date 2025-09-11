using Atlas;
using FistVR;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SupplyRaid
{
    public class SR_MapSelector : MonoBehaviour
    {
        
        public static SR_MapSelector instance;

        public Transform buttonContent;
        private List<SR_GenericButton> mapButtons = new List<SR_GenericButton>();
        public GameObject mapPrefab;

        private List<CustomSceneInfo> collectedScenes = new List<CustomSceneInfo>();
        private int pageIndex = 0;

        public int mapPageCount = 12;   //Public so modding???

        [Header("Selected Map")]
        public Text mapTitle;
        public Text mapDescription;
        public RawImage mapThumbnail;
        private CustomSceneInfo selectedScene;

        private void Awake()
        {
            instance = this;
        }

        
        private void Start()
        {
            foreach (CustomSceneInfo customSceneInfo in AtlasPlugin.CustomSceneInfos)
            {
                if (customSceneInfo.GameMode == "supplyraid" 
                    || customSceneInfo.DisplayMode == "supplyraid" 
                    || customSceneInfo.DisplayName.Contains("SR_"))
                {
                    collectedScenes.Add(customSceneInfo);
                }
            }

            //Default to nothing
            mapTitle.text = "";
            if(collectedScenes.Count != 0)
                mapDescription.text = "";

            //Generate initial list
            GenerateMapButtonsFrom(pageIndex);
        }

        public void ScrollMaps(int amount)
        {
            pageIndex += amount * mapPageCount;

            if (pageIndex < 0)
                pageIndex = 0;
            else if(pageIndex >= collectedScenes.Count)
                pageIndex = collectedScenes.Count - 1;
            GenerateMapButtonsFrom(pageIndex);

            SM.PlayGlobalUISound(SM.GlobalUISound.Boop, GM.CurrentPlayerBody.transform.position);
        }

        void GenerateMapButtonsFrom(int i)
        {
            //Clear Old Buttons
            for (int j = 0; j < mapButtons.Count; j++)
            {
                Destroy(mapButtons[j].gameObject);
            }
            mapButtons.Clear();

            //Generate Pages
            for (int c = 0; i < collectedScenes.Count; i++, c++)
            {
                //Only support for mapPageCount maps per page
                if (c >= mapPageCount)
                    break;

                SR_GenericButton btn = Instantiate(mapPrefab, buttonContent).GetComponent<SR_GenericButton>();
                btn.index = i;
                btn.go.GetComponent<RawImage>().texture = collectedScenes[i].ThumbnailTexture;
                btn.text.text = collectedScenes[i].DisplayName;
                btn.gameObject.SetActive(true);
                mapButtons.Add(btn);
            }
        }

        public void SelectMap(int index)
        {
            selectedScene = collectedScenes[index];
            mapTitle.text = selectedScene.DisplayName;
            mapDescription.text = selectedScene.Description;
            mapThumbnail.texture = selectedScene.ThumbnailTexture;
            SM.PlayGlobalUISound(SM.GlobalUISound.Boop, GM.CurrentPlayerBody.transform.position);
        }

        public void LaunchMap()
        {
            if (selectedScene == null)
            {
                SM.PlayGlobalUISound(SM.GlobalUISound.Beep, GM.CurrentPlayerBody.transform.position);
                return;
            }

            AtlasPlugin.LoadCustomScene(selectedScene);
            SM.PlayGlobalUISound(SM.GlobalUISound.Beep, GM.CurrentPlayerBody.transform.position);
        }
    }
}