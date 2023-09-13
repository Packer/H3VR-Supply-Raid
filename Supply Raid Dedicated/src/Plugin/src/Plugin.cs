using BepInEx;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
namespace H3VRMod
{
	[BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
	[BepInProcess("h3vr.exe")]
	public class Plugin : BaseUnityPlugin
	{
		private readonly Hooks _hooks;

        private static bool cameraSet = true;

		public Plugin()
		{
			_hooks = new Hooks();
			_hooks.Hook();
		}

		private void Awake()
		{

		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.P))
			{
				Debug.Log("Get the button!!");
                DisableAllCameras();
                //SteamVR_LoadLevel.Begin("SnowFields", false, 0.5f, 0f, 0f, 0f, 1f);
            }
		}

		void DisableAllCameras()
        {
            Debug.Log("Attempting to find cameras to disable");
            //GameObject[]  dndObjects = Object.GetDontDestroyOnLoadObjects();
            Camera[]  cameras = GameObject.FindObjectsOfType<Camera>();

            cameraSet = !cameraSet;
            for (int i = 0; i < cameras.Length; i++)
            {
                Debug.Log("Setting camera " + i);
                cameras[i].enabled = cameraSet;
            }

            /*
            if (dndObjects != null)
            {
                for (int i = 0; i < dndObjects.Length; i++)
                {
                    Camera cam = dndObjects[i].GetComponent<Camera>();
                    if (cam != null)
                    {
                        Debug.Log("Camera " + i + " default Near/Far values: " + cam.nearClipPlane + " / " + cam.farClipPlane);
                        cam.nearClipPlane = minDistance;
                        cam.farClipPlane = maxDistance;
                    }
                }
            }
            */
        }

		private void OnDestroy()
		{
			_hooks.Unhook();
		}
	}
}