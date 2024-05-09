using BepInEx;
using BepInEx.Bootstrap;
using Atlas;
using Atlas.Loaders;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
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
		private readonly Hooks _hooks;
		public static bool h3mpEnabled = false;
		public static bool bgmEnabled = false;
		public static bool loadTnH = false;
		public static Text tnhButtonText = null;
		public static Dictionary<int, SR_SosigEnemyTemplate> customSosigs = new Dictionary<int, SR_SosigEnemyTemplate>();
        public static Texture2D customSosigTexture;

        public SupplyRaidPlugin()
		{
			_hooks = new Hooks();
			_hooks.Hook();
		}

		private void Awake()
		{
            AtlasPlugin.Loaders["supplyraid"] = new SandboxLoader();
            h3mpEnabled = Chainloader.PluginInfos.ContainsKey("VIP.TommySoucy.H3MP");
			bgmEnabled = Chainloader.PluginInfos.ContainsKey("dll.potatoes.ptnhbgml");

            SceneManager.activeSceneChanged += ChangedActiveScene;
            LoadCustomSosigTexture();
            GenerateAllJsons();
        }

        void Start()
        {
            SR_Global.LoadCustomSosigs();
            StartCoroutine(SetupSosigTemplates());
        }

        public System.Collections.IEnumerator SetupSosigTemplates()
        {
            Debug.Log("Custom Sosigs count: " + SupplyRaidPlugin.customSosigs.Count);
            Debug.Log("Sosigs count: " + IM.Instance.odicSosigObjsByID.Count);

            foreach (var customTemplate in customSosigs)
            {
                Debug.Log("AAcf");
                //SR_SosigEnemyTemplate customTemplate = SupplyRaidPlugin.customSosigs.Ele;
                SosigEnemyTemplate template = customTemplate.Value.Initialize();

                template.SosigEnemyID = (SosigEnemyID)customTemplate.Value.sosigEnemyID;

                template.SosigPrefabs = new List<FVRObject>();

                for (int i = 0; i < customTemplate.Value.customSosig.Length; i++)
                {
                    //Get our Base Sosig
                    SosigEnemyID id = customTemplate.Value.customSosig[i].baseSosigID;
                    template.SosigPrefabs = IM.Instance.odicSosigObjsByID[id].SosigPrefabs;
                }

                if (!IM.Instance.olistSosigCats.Contains(template.SosigEnemyCategory))
                {
                    //Adding Category
                    IM.Instance.olistSosigCats.Add(template.SosigEnemyCategory);
                }
                if (!IM.Instance.odicSosigIDsByCategory.ContainsKey(template.SosigEnemyCategory))
                {
                    List<SosigEnemyID> sosigIDs = new List<SosigEnemyID>();
                    IM.Instance.odicSosigIDsByCategory.Add(template.SosigEnemyCategory, sosigIDs);
                    List<SosigEnemyTemplate> list15 = new List<SosigEnemyTemplate>();
                    IM.Instance.odicSosigObjsByCategory.Add(template.SosigEnemyCategory, list15);
                }
                if (template.SosigEnemyID != SosigEnemyID.None)
                {
                    IM.Instance.odicSosigIDsByCategory[template.SosigEnemyCategory].Add(template.SosigEnemyID);
                    IM.Instance.odicSosigObjsByCategory[template.SosigEnemyCategory].Add(template);
                    if (!IM.Instance.odicSosigObjsByID.ContainsKey(template.SosigEnemyID))
                    {
                        IM.Instance.odicSosigObjsByID.Add(template.SosigEnemyID, template);
                    }
                }
                yield return null;
            }
            Debug.Log("Sosigs count: " + IM.Instance.odicSosigObjsByID.Count);
        }

        public void GenerateAllJsons()
		{
			//GenerateNewSosig().ExportJson();
        }

		public SR_SosigEnemyTemplate GenerateNewSosig()
		{
            SR_SosigEnemyTemplate enemy = new SR_SosigEnemyTemplate();
            enemy.displayName = "Cool Sosig";
            enemy.sosigEnemyID = 69;
            enemy.configTemplates = new SR_SosigConfigTemplate[1];
            enemy.configTemplates[0] = new SR_SosigConfigTemplate();

            enemy.weaponOptionsID = new string[2];
            enemy.weaponOptionsID[0] = "SosiggunP90";
            enemy.weaponOptionsID[1] = "Sosiggun_PKM";

            enemy.weaponOptions_SecondaryID = new string[1];
            enemy.weaponOptions_SecondaryID[0] = "Sosiggun_Revolver";
            enemy.secondaryChance = 1;

            enemy.weaponOptions_TertiaryID = new string[1];
            enemy.weaponOptions_TertiaryID[0] = "SosiggunSVDS";
            enemy.tertiaryChance = 1;

            enemy.customSosig = new SR_CustomSosig[1];
            enemy.customSosig[0] = new SR_CustomSosig();
            enemy.customSosig[0].scaleBody = (Vector3.one * 2);

            enemy.outfitConfig = new SR_OutfitConfig[1];
            enemy.outfitConfig[0] = new SR_OutfitConfig();
            enemy.outfitConfig[0].headwearID = new string[1];
            enemy.outfitConfig[0].headwearID[0] = "Sosigaccesory_ww2_helmet_brown";

			customSosigs.Add(enemy.sosigEnemyID, enemy);

            return enemy;
        }


        void LoadCustomSosigTexture()
		{
            string path = Paths.PluginPath + "\\Packer-SupplyRaid\\CustomSosig_Base.png";
            Texture2D tex = null;

            byte[] fileData;

            if (File.Exists(path) && tex == null)
            {
                fileData = File.ReadAllBytes(path);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);
            }

            if (tex == null)
            {
                Debug.LogError("Supply Raid - Texture Not Found: " + path);
                customSosigTexture = null;
            }

            customSosigTexture = tex;
        }

        private void ChangedActiveScene(Scene current, Scene next)
        {
			if (next != null && next.name.Contains("TakeAndHold_Lobby"))
			{
				Logger.LogInfo("Supply Raid - Found TnH Lobby, Adding Supply Raid button");
				CreateTnHButton();

            }

            FistVR.TNH_Manager TnHm = FindObjectOfType<FistVR.TNH_Manager>();
			Atlas.MappingComponents.TakeAndHold.TNH_ManagerOverride TnHoverRide = null;
            if (TnHm == null)
            {
                TnHoverRide = FindObjectOfType<Atlas.MappingComponents.TakeAndHold.TNH_ManagerOverride>();
            }

            if (TnHm != null || TnHoverRide != null)
            {
				loadTnH = true;
                Logger.LogInfo("Supply Raid - TnH manger found, attempting to convert");
                GameObject goSR = Instantiate(new UnityEngine.GameObject());
				goSR.AddComponent<SR_TNH>().tnhManager = TnHm;
				goSR.GetComponent<SR_TNH>().tnHOverideManager = TnHoverRide;
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
			_hooks.Unhook();
		}
	}
}