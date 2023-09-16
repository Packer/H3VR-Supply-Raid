using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Atlas;
using FistVR;
using System.IO;
using System;
using System.Linq;
using H3MP;
using H3MP.Networking;

namespace H3VRMod
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    [BepInProcess("h3vr.exe")]
    public class HeadlessServerPlugin : BaseUnityPlugin
    {
        private readonly Hooks _hooks;

        private static bool cameraSet = true;

        private float speed = 10;

        private GameObject? canvasGO;

        public static HeadlessConfig config = new HeadlessConfig();

        public static Camera? playerCamera;

        private float cleanUpTime = 300;
        private float sceneReloadTime = 14400;
        private bool serverActive = false;
        private bool autoCleanup = true;
        private bool autoReload = true;
        private bool launched = false;
        private int lookedAtID = 0;

        public class HeadlessConfig
        {
            public bool autoStart = true;               //Automatically launches into the scene
            public bool logEnabled = false;             //Disable Log
            public string sceneName = "";               //If blank, won't launch into scene
            public float autoCleanupTime = 600;         //Auto cleans up everything, -0 does nothing
            public float autoSceneReloadTime = 14400;   // 4 Hours -0 does nothing
        }

        public HeadlessServerPlugin()
        {
            _hooks = new Hooks();
            _hooks.Hook();
        }

        private void Awake()
        {

        }

        void Start()
        {
            Invoke(nameof(StartBase), 1);
        }

        void StartBase()
        {
            LoadConfig();

            //Auto startup after 5 seconds
            if (config.autoStart)
                StartCoroutine(Launch());

            HelpCommands();
            launched = true;
        }

        IEnumerator Launch()
        {
            //Start H3MP
            StartServer();
            yield return new WaitForSeconds(3);

            //Load Custom Scene
            if (config.sceneName != "")
            {
                LoadScene();
                yield return new WaitForSeconds(5);
            }
            else
            {
                Debug.Log("HEADLESS SERVER: No Scene found in config, idling in main menu");
            }

            //Move Host out of the way
            GM.CurrentPlayerRoot.position = GM.CurrentPlayerRoot.position + (GM.CurrentPlayerRoot.right * 2);

            //Disable Cameras
            SetCameras(false);

            if (!config.logEnabled)
            {
                Debug.Log("HEADLESS SERVER: Logging disabled!");
                Debug.logger.logEnabled = config.logEnabled;
            }
        }
        IEnumerator HardReset()
        {
            if (config.logEnabled == false)
                Debug.logger.logEnabled = true;

            GotoMainMenu();
            yield return new WaitForSeconds(5);
            CloseServer();
            yield return new WaitForSeconds(3);
            StartServer();
            yield return new WaitForSeconds(5);
            StartCoroutine(Launch());
        }

        private void Update()
        {
            //Hotkeys
            ServerInput();

            if (!launched)
                return;

            if (GM.CurrentPlayerBody.GetPlayerIFF() != -3)
            {
                //Spectator Mode
                GM.CurrentPlayerBody.SetPlayerIFF(-3);
                GM.CurrentPlayerBody.DisableHitBoxes();
                GM.CurrentPlayerBody.m_isGhosted = true;
                GM.CurrentPlayerBody.SetHealthThreshold(500000f);
            }

            //Only compute when camera is active
            if (playerCamera != null && playerCamera.enabled == true)
            {
                MovePlayer();
                MousePointer();
            }

            //Auto Cleanup
            if (autoCleanup && Time.time >= cleanUpTime)
                AutoCleanup();

            //Scene Reload
            if (autoReload && Time.time >= sceneReloadTime)
                ReloadScene();
        }


        void ServerInput()
        {
            if (Input.GetKey(KeyCode.RightAlt))
            {
                if (config.logEnabled == false)
                    Debug.logger.logEnabled = true;

                //HELP
                if (Input.GetKeyDown(KeyCode.H))
                    HelpCommands();

                //Load Scene
                if (Input.GetKeyDown(KeyCode.Y))
                    LoadScene();

                //Load Config
                if (Input.GetKeyDown(KeyCode.O))
                    LoadConfig();

                //Toggle Cameras
                if (Input.GetKeyDown(KeyCode.P))
                    ToggleCameras();

                //Main Menu
                if (Input.GetKeyDown(KeyCode.T))
                    GotoMainMenu();

                //Hard Reset
                if (Input.GetKeyDown(KeyCode.R))
                    StartCoroutine(HardReset());

                //Server Toggle
                if (Input.GetKeyDown(KeyCode.X))
                    ToggleServer();

                //Clean up All
                if (Input.GetKeyDown(KeyCode.A))
                    CleanUpScene_All();

                //Clean up Mags
                if (Input.GetKeyDown(KeyCode.S))
                    CleanUpScene_AllMags();

                //Console Log Toggle
                if (Input.GetKeyDown(KeyCode.L))
                    ToggleConsole();

                //View Next Player
                if (Input.GetKeyDown(KeyCode.G))
                    LookAtNextPlayer();

                if (config.logEnabled == false)
                Debug.logger.logEnabled = false;
            }

        }

        void HelpCommands()
        {
            if (config.logEnabled == false)
                Debug.logger.logEnabled = true;

            Debug.Log("---(Unofficial) H3MP Headless Server Hotkeys---");
            Debug.Log("Right Alt + H - Show Commands in Console");
            Debug.Log("Right Alt + L - Toggle Logging in Console");
            Debug.Log("------------------------------");
            Debug.Log("Right Alt + O - Reload Config");
            Debug.Log("Right Alt + X - Toggle Server On/Off");
            Debug.Log("Right Alt + R - Automatic Hard Reset");
            Debug.Log("Right Alt + T - Load Main Menu");
            Debug.Log("Right Alt + Y - Load/Reload Scene");
            Debug.Log("Right Alt + P - Toggle Cameras");           
            Debug.Log("------------------------------");
            Debug.Log("Right Alt + A - Clean up All");
            Debug.Log("Right Alt + S - Clean up Mags");
            Debug.Log("------------------------------");
            Debug.Log("Right Alt + C - Toggle Auto Cleanup");
            Debug.Log("Right Alt + V - Toggle Scene Reload");
            Debug.Log("------------------------------");
            Debug.Log("Right Alt + G        - View next player");
            Debug.Log("------------------------------");
            Debug.Log("WASD                 - Move Camera");
            Debug.Log("Mouse Left / Right   - Turn Camera");
            Debug.Log("Mouse Left Click     - Click Button/Object");
            Debug.Log("Mouse Scroll         - Speed camera up/down");

            if (config.logEnabled == false)
                Debug.logger.logEnabled = false;

        }

        /*
        void SetIFF(int set)
        {
            int iff = Mathf.Clamp(GM.CurrentPlayerBody.GetPlayerIFF() + set, -3, 5);
            GM.CurrentPlayerBody.SetPlayerIFF(iff);
        }
        */

        void LookAtNextPlayer()
        {
            int[] playerArray = new int[GameManager.players.Count];

            int i = 0;
            foreach (KeyValuePair<int, H3MP.Scripts.PlayerManager> entry in GameManager.players)
            {
                playerArray[i] = entry.Key;
                i++;
            }

            if (lookedAtID + 1 >= playerArray.Length)
                lookedAtID = 0;
            else
                lookedAtID++;

            if (playerArray.Length >= 0)
            {

                Transform playerHead = GameManager.players[playerArray[lookedAtID]].head;

                if (playerHead != null)
                {
                    //TODO do physics checks to find best position for player
                    Vector3 newPos = playerHead.position - playerHead.forward;
                    newPos.y = playerHead.position.y - 0.4f;

                    GM.CurrentMovementManager.TeleportToPoint(newPos, true, playerHead.rotation.eulerAngles);
                }
            }

        }

        void ToggleConsole()
        {
            config.logEnabled = !config.logEnabled;
            Debug.Log("HEADLESS SERVER: Console set to " + config.logEnabled);
        }

        void ToggleServer()
        {
            if (serverActive)
                CloseServer();
            else
                StartServer();
        }

        void GotoMainMenu()
        {
            //Reenable log
            Debug.logger.logEnabled = true;

            SM.PlayGlobalUISound((SM.GlobalUISound)1, GM.CurrentPlayerRoot.position);
            SteamVR_LoadLevel.Begin("MainMenu3", false, 0.5f, 0f, 0f, 0f, 1f);
        }

        void LoadScene()
        {
            if (config.sceneName != "")
            {
                Debug.Log("HEADLESS SERVER: Loading Scene");
                SM.PlayGlobalUISound((SM.GlobalUISound)3, GM.CurrentPlayerRoot.position);

                CustomSceneInfo? info = AtlasPlugin.GetCustomScene(config.sceneName);
                if (info != null)
                    AtlasPlugin.LoadCustomScene(config.sceneName);
                else
                    SteamVR_LoadLevel.Begin(config.sceneName, false, 0.5f, 0f, 0f, 0f, 1f);
            }
            else
            {
                Debug.Log("HEADLESS SERVER: - No scene defined in config");
                SM.PlayGlobalUISound((SM.GlobalUISound)3, GM.CurrentPlayerRoot.position);
            }
        }

        void ReloadScene()
        {
            if (config.autoSceneReloadTime <= 0 || config.sceneName == "")
                return;

            SM.PlayGlobalUISound((SM.GlobalUISound)1, GM.CurrentPlayerRoot.position);

            sceneReloadTime = Time.time + config.autoSceneReloadTime;

            Debug.Log("HEADLESS SERVER: Reloading scene " + config.sceneName);

            LoadScene();
        }

        void StartServer()
        {
            //Start Server
            if (Mod.managerObject != null)
            {
                SM.PlayGlobalUISound((SM.GlobalUISound)2, GM.CurrentPlayerRoot.position);
                return;
            }
            //Spectator Mode
            GM.CurrentPlayerBody.SetPlayerIFF(-3);
            GM.CurrentPlayerBody.DisableHitBoxes();
            GM.CurrentPlayerBody.m_isGhosted = true;

            SM.PlayGlobalUISound((SM.GlobalUISound)0, GM.CurrentPlayerRoot.position);
            Mod.OnHostClicked();

            serverActive = true;
            Debug.Log("HEADLESS SERVER: Server Started");
        }

        void CloseServer()
        {
            //Start Server
            if (Mod.managerObject == null)
            {
                SM.PlayGlobalUISound((SM.GlobalUISound)2, GM.CurrentPlayerRoot.position);
                return;
            }
            SM.PlayGlobalUISound((SM.GlobalUISound)1, GM.CurrentPlayerRoot.position);
            Server.Close();

            serverActive = false;
            Debug.Log("HEADLESS SERVER: Server Closed");
        }

        void AutoCleanup()
        {
            if (config.autoCleanupTime <= 0)
                return;

            cleanUpTime = Time.time + config.autoCleanupTime;

            CleanUpScene_AllMags();
            CleanUpScene_All();
            Debug.LogError("HEADLESS SERVER: Automatic Server Cleanup, next clean up in (Secs)" + config.autoCleanupTime);
        }


        void LoadConfig()
        {
            List<string> directories = Directory.GetFiles(Paths.PluginPath + "/Packer-HeadlessServer/", "config.json", SearchOption.AllDirectories).ToList();

            if (directories.Count == 0)
            {
                SM.PlayGlobalUISound((SM.GlobalUISound)3, GM.CurrentPlayerRoot.position);
                Debug.LogError("HEADLESS SERVER: No Config File Found!");
                return;
            }
            else
            {
                using (StreamReader streamReader = new StreamReader(directories[0]))
                {
                    string json = streamReader.ReadToEnd();

                    config = new HeadlessConfig();

                    try
                    {
                        config = JsonUtility.FromJson<HeadlessConfig>(json);
                        Debug.Log("HEADLESS SERVER: Loaded Config");
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex.Message);
                    }
                }
            }


            //Timers
            cleanUpTime = config.autoCleanupTime;
            sceneReloadTime = config.autoSceneReloadTime;

            //Auto
            autoCleanup = config.autoCleanupTime > 0 ? true : false;
            autoReload = config.autoSceneReloadTime > 0 ? true : false;


            Debug.Log("HEADLESS SERVER: Cleanup Timer:" + cleanUpTime + " | Scene Reload Time: " + sceneReloadTime);
        }

        void MousePointer()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Create a ray from the camera to the mouse position
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                // Create a RaycastHit variable to store information about the hit
                RaycastHit hit;

                // Perform the raycast
                if (Physics.Raycast(ray, out hit))
                {
                    // Check if the raycast hit a GameObject with a collider
                    if (hit.collider != null)
                    {
                        // You can now access information about the hit object, e.g., hit.transform
                        // Do something with the hit object here
                        Debug.Log("Hit object: " + hit.transform.name);

                        FVRPointableButton btn = hit.collider.gameObject.GetComponent<FVRPointableButton>();
                        Button btnUI = btn.GetComponent<Button>();

                        if (btnUI == null)
                        {
                            if (hit.collider.transform.parent != null)
                                btnUI = hit.collider.transform.parent.gameObject.GetComponent<Button>();

                        }

                        if (btn == null)
                        {
                            if(hit.collider.transform.parent != null)
                                btn = hit.collider.transform.parent.gameObject.GetComponent<FVRPointableButton>();
                        }

                        if (btn == null)
                        {
                            if (hit.collider != null && hit.collider.transform != null)
                            {
                                for (int i = 0; i < hit.collider.transform.childCount; i++)
                                {
                                    btn = hit.collider.transform.GetChild(i).GetComponent<FVRPointableButton>();
                                    if (btn != null)
                                        break;
                                }
                            }
                        }

                        if (btn != null)
                        {
                            btn.Button.onClick.Invoke();
                        }

                        if(btnUI != null)
                            btnUI.onClick.Invoke();
                    }
                }
            }
        }

        void MovePlayer()
        {
            speed = Mathf.Clamp(speed + Input.mouseScrollDelta.y, 0.01f, 100f);

            GM.CurrentMovementManager.Mode = FVRMovementManager.MovementMode.Teleport;


            Vector3 movement = GM.CurrentPlayerBody.Head.position;
            movement.y = GM.CurrentPlayerBody.transform.position.y;

            Vector3 defaultMove = movement;

            Vector3 rotation = GM.CurrentPlayerBody.transform.forward;

            //Rotation
            if (Input.GetAxis("Horizontal") != 0)
            {
                float angle = Input.GetAxis("Horizontal");
                rotation = Quaternion.AngleAxis(angle, Vector3.up) * rotation;
                //GM.CurrentPlayerBody.Head.rotation = Quaternion.Euler(rotation);
            }

            
            
            //Position

            if (Input.GetKey(KeyCode.W))
                movement += GM.CurrentPlayerRoot.forward * speed * Time.deltaTime;

            if (Input.GetKey(KeyCode.S))
                movement += -GM.CurrentPlayerRoot.forward * speed * Time.deltaTime;

            if (Input.GetKey(KeyCode.A))
                movement -= GM.CurrentPlayerRoot.right * speed * Time.deltaTime;

            if (Input.GetKey(KeyCode.D))
                movement += GM.CurrentPlayerRoot.right * speed * Time.deltaTime;


            if (Input.GetKey(KeyCode.Q))
                movement -= GM.CurrentPlayerRoot.up * speed * Time.deltaTime;

            if (Input.GetKey(KeyCode.E))
                movement += GM.CurrentPlayerRoot.up * speed * Time.deltaTime;


            GM.CurrentMovementManager.TeleportToPoint(movement, true, rotation);

            /*
            if (Input.GetAxis("Vertical") != 0)
            {
                rotation = GM.CurrentPlayerBody.Head.parent.forward;
                float angle = Input.GetAxis("Vertical");
                rotation = Quaternion.AngleAxis(angle, Vector3.right) * rotation;

                GM.CurrentPlayerBody.Head.parent.rotation = Quaternion.Euler(rotation);
            }
            */


            if (movement != defaultMove)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
                Cursor.lockState = CursorLockMode.None;



            if (Input.GetKeyDown(KeyCode.LeftArrow))
                GM.CurrentMovementManager.TurnCounterClockWise();

            if (Input.GetKeyDown(KeyCode.RightArrow))
                GM.CurrentMovementManager.TurnClockWise();

            /*
            if (Input.GetKey(KeyCode.UpArrow))
                GM.CurrentMovementManager.TeleportToPoint(GM.CurrentPlayerRoot.position + (GM.CurrentPlayerRoot.forward * speed * Time.deltaTime), true);

            if (Input.GetKey(KeyCode.DownArrow))
                GM.CurrentMovementManager.TeleportToPoint(GM.CurrentPlayerRoot.position + (-GM.CurrentPlayerRoot.forward * speed * Time.deltaTime), true);

            if (Input.GetKeyDown(KeyCode.LeftArrow))
                GM.CurrentMovementManager.TurnCounterClockWise();

            if (Input.GetKeyDown(KeyCode.RightArrow))
                GM.CurrentMovementManager.TurnClockWise();


            if (Input.GetKeyDown(KeyCode.PageUp))
                GM.CurrentMovementManager.TeleportToPoint(GM.CurrentPlayerRoot.position + GM.CurrentPlayerRoot.up, true);

            if (Input.GetKeyDown(KeyCode.PageDown))
                GM.CurrentMovementManager.TeleportToPoint(GM.CurrentPlayerRoot.position + -GM.CurrentPlayerRoot.up, true);
            */

            //---------------------------------------------------------
            // No VR Mode?
            //---------------------------------------------------------
            if (playerCamera == null)
                return;
            
            //Forward/Back
            if (Input.GetKey(KeyCode.Keypad8))
                playerCamera.transform.position += playerCamera.transform.forward * speed * Time.deltaTime;

            if (Input.GetKey(KeyCode.Keypad5))
                playerCamera.transform.position -= playerCamera.transform.forward * speed * Time.deltaTime;

            //Rotate
            if (Input.GetKey(KeyCode.Keypad4))
                playerCamera.transform.Rotate(Vector3.up * -45 * Time.deltaTime);

            if (Input.GetKey(KeyCode.Keypad6))
                playerCamera.transform.Rotate(Vector3.up * 45 * Time.deltaTime);

            //Strafe
            if (Input.GetKey(KeyCode.Keypad3))
                playerCamera.transform.position += playerCamera.transform.right * speed * Time.deltaTime;

            if (Input.GetKey(KeyCode.Keypad1))
                playerCamera.transform.position -= playerCamera.transform.right * speed * Time.deltaTime;

            //Up/Down
            if (Input.GetKey(KeyCode.Keypad7))
                playerCamera.transform.position += playerCamera.transform.up * speed * Time.deltaTime;

            if (Input.GetKey(KeyCode.Keypad9))
                playerCamera.transform.position -= playerCamera.transform.up * speed * Time.deltaTime;

            //Tele up
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
                playerCamera.transform.position += playerCamera.transform.up * speed;

            //Tele up
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
                playerCamera.transform.position -= playerCamera.transform.up * speed;
            
        }

        void ToggleCameras()
        {
            SetCameras(!cameraSet);
        }

		void SetCameras(bool set)
        {
            SM.PlayGlobalUISound((SM.GlobalUISound)1, GM.CurrentPlayerRoot.position);
            Camera[] cameras = FindObjectsOfType<Camera>();

            cameraSet = set;
            for (int i = 0; i < cameras.Length; i++)
            {
                if(i == 0)
                    playerCamera = cameras[i];

                //Debug.Log("Setting camera " + i);
                cameras[i].enabled = cameraSet;
            }

            Debug.Log("HEADLESS SERVER: Cameras set to " + set);
        }

        public void CleanUpScene_AllMags()
        {
            Debug.Log("HEADLESS SERVER: Cleaning all Magazines");
            SM.PlayGlobalUISound(SM.GlobalUISound.Beep, transform.position);

            FVRFireArmMagazine[] magazines = FindObjectsOfType<FVRFireArmMagazine>();
            for (int i = magazines.Length - 1; i >= 0; i--)
            {
                if (!magazines[i].IsHeld && magazines[i].QuickbeltSlot == null && magazines[i].FireArm == null && !magazines[i].IsIntegrated)
                {
                    Destroy(magazines[i].gameObject);
                }
            }
            FVRFireArmRound[] rounds = FindObjectsOfType<FVRFireArmRound>();
            for (int j = rounds.Length - 1; j >= 0; j--)
            {
                if (!rounds[j].IsHeld && rounds[j].QuickbeltSlot == null && rounds[j].RootRigidbody != null)
                {
                    Destroy(rounds[j].gameObject);
                }
            }
            FVRFireArmClip[] clips = FindObjectsOfType<FVRFireArmClip>();
            for (int k = clips.Length - 1; k >= 0; k--)
            {
                if (!clips[k].IsHeld && clips[k].QuickbeltSlot == null && clips[k].FireArm == null)
                {
                    Destroy(clips[k].gameObject);
                }
            }
            Speedloader[] speedLoaders = FindObjectsOfType<Speedloader>();
            for (int l = speedLoaders.Length - 1; l >= 0; l--)
            {
                if (!speedLoaders[l].IsHeld && speedLoaders[l].QuickbeltSlot == null)
                {
                    Destroy(speedLoaders[l].gameObject);
                }
            }
        }

        public void CleanUpScene_All()
        {
            Debug.Log("HEADLESS SERVER: Cleaning all Spawnables");

            SM.PlayGlobalUISound(SM.GlobalUISound.Beep, transform.position);
            VaultSystem.ClearExistingSaveableObjects(true);
        }

        private void OnDestroy()
		{
			_hooks.Unhook();
		}
	}
}