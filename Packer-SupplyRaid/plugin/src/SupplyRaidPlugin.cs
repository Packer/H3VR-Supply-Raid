using BepInEx;
using BepInEx.Bootstrap;
using Atlas;
using Atlas.Loaders;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using FistVR;

namespace SupplyRaid
{
	[BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
	[BepInProcess("h3vr.exe")]
	[BepInDependency("VIP.TommySoucy.H3MP", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("dll.potatoes.ptnhbgml", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(AtlasConstants.Guid, AtlasConstants.Version)]
    public class SupplyRaidPlugin : BaseUnityPlugin
	{
		public static bool h3mpEnabled = false;
		public static bool bgmEnabled = false;
		public static bool loadTnH = false;
		public static Text tnhButtonText = null;


		private void Awake()
		{
            AtlasPlugin.Loaders["supplyraid"] = new SandboxLoader();
            h3mpEnabled = Chainloader.PluginInfos.ContainsKey("VIP.TommySoucy.H3MP");
			bgmEnabled = Chainloader.PluginInfos.ContainsKey("dll.potatoes.ptnhbgml");

            SceneManager.activeSceneChanged += ChangedActiveScene;
        }

        void Start()
        {
        }

        public void GenerateAllJsons()
		{
			//GenerateNewSosig().ExportJson();
        }

        private void ChangedActiveScene(Scene current, Scene next)
        {
			if (next != null && next.name.Contains("TakeAndHold_Lobby"))
			{
				Logger.LogInfo("Supply Raid: Found TnH Lobby, Adding Supply Raid button");
                loadTnH = false;
                CreateTnHButton();
            }

			if (!loadTnH)
				return;

            TNH_Manager TnHm = FindObjectOfType<TNH_Manager>();
			Atlas.MappingComponents.TakeAndHold.TNH_ManagerOverride TnHoverRide = null;
            if (TnHm == null)
            {
                TnHoverRide = FindObjectOfType<Atlas.MappingComponents.TakeAndHold.TNH_ManagerOverride>();
            }

            if (TnHm != null || TnHoverRide != null)
            {
                Logger.LogInfo("Supply Raid: TnH manger found, attempting to convert");
                GameObject goSR = Instantiate(new GameObject());
				goSR.AddComponent<SR_TNH>().tnhManager = TnHm;
				goSR.GetComponent<SR_TNH>().tnHOverideManager = TnHoverRide;
            }

			if (h3mpEnabled && loadTnH)
			{
				//Todo stop H3MP breaking stuff on
			}
        }

		private void CreateTnHButton()
		{
			GameObject canvasCenter = GameObject.Find("MainMenuCanvas_Right");
            GameObject menu = Instantiate(canvasCenter);
			menu.transform.localScale = Vector3.one * 0.001f;
			menu.transform.position += Vector3.up * 1.5f;
			Transform newCanvas = menu.transform.GetChild(0);
			Button toggleButton = null;
			for (int i = newCanvas.childCount - 1; i >= 2; i--)
			{
				if (newCanvas.GetChild(i).name != "LvlSelect_Next")
					newCanvas.GetChild(i).gameObject.SetActive(false);
				else
					toggleButton = newCanvas.GetChild(i).gameObject.GetComponent<Button>();
            }

			//Title and Text
			Text title = newCanvas.GetChild(1).gameObject.GetComponent<Text>();
			title.text = "\r\nSupply Raid";
			title.horizontalOverflow = HorizontalWrapMode.Overflow;
			title.verticalOverflow = VerticalWrapMode.Overflow;
			title.fontSize = 128;

			//Button
			toggleButton.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
			tnhButtonText = toggleButton.GetComponent<Text>();
            tnhButtonText.horizontalOverflow = HorizontalWrapMode.Overflow;
            tnhButtonText.verticalOverflow = VerticalWrapMode.Overflow;
            tnhButtonText.fontSize = 96;
            tnhButtonText.text = "Disabled";

			toggleButton.name = "SupplyRaidToggle";
            toggleButton.onClick.RemoveAllListeners();
            toggleButton.onClick.AddListener(ToggleSupplyRaidTnH);
			toggleButton.GetComponent<FistVR.FVRPointableButton>().MaxPointingRange = 8;
			toggleButton.GetComponent<BoxCollider>().size = new Vector3(256, 96, 2.5f);
        }

		public void ToggleSupplyRaidTnH()
        {
			loadTnH = !loadTnH;
			if(!loadTnH)
				tnhButtonText.text = "Disabled";
            else
                tnhButtonText.text = "Enabled (Experimental)";
        }

		private void OnDestroy()
		{
		}
	}
}