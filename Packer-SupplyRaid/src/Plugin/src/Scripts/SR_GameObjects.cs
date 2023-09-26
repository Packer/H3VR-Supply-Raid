using FistVR;
using UnityEngine;

namespace SupplyRaid
{
	public class SR_GameObjects : MonoBehaviour
	{
		public GameStateEnum activeOn = GameStateEnum.None;
		public SetStateEnum setState = SetStateEnum.True;

		[Tooltip("Optional Supply Point Picker")]
		public SR_SupplyPoint supplyPoint;
		public GameObject[] gameObjects;

		void Start()
		{
            //Setup
            switch (activeOn)
			{
				case GameStateEnum.SupplyPointCaptured:
				case GameStateEnum.SupplyPointAttack:
				case GameStateEnum.SupplyPointIdle:
                    if (supplyPoint == null)
                        return;
					SR_Manager.SupplyPointChangeEvent += OnSupplyPointChange;
                    break;
				case GameStateEnum.BeforeLaunch:
					break;
				case GameStateEnum.GameLaunch:
                    SR_Manager.LaunchedEvent += SetGameObjects;
                    break;
                case GameStateEnum.Capture:
                    SR_Manager.SupplyPointChangeEvent += SetGameObjects;
                    break;
                case GameStateEnum.GameComplete:
                    SR_Manager.GameCompleteEvent += SetGameObjects;
                    break;
				case GameStateEnum.FinalSupplyPoint:
                    SR_Manager.SupplyPointChangeEvent += OnFinalSupplyPoint;
                    break;
				case GameStateEnum.EndlessStart:
                    SR_Manager.EndlessEvent += SetGameObjects;
                    break;
				case GameStateEnum.ObjectiveComplete:
				case GameStateEnum.ObjectiveFail:
				case GameStateEnum.ObjectiveEnd:
                    SR_Manager.ObjectiveEvent += OnObjectiveChange;
                    break;

				case GameStateEnum.None:
				default:
					break;
			}
		}

		void OnDestroy()
        {
            switch (activeOn)
            {
                case GameStateEnum.SupplyPointCaptured:
                case GameStateEnum.SupplyPointAttack:
                case GameStateEnum.SupplyPointIdle:
                    SR_Manager.SupplyPointChangeEvent -= OnSupplyPointChange;
                    break;
                case GameStateEnum.GameLaunch:
                    SR_Manager.LaunchedEvent -= SetGameObjects;
                    break;
                case GameStateEnum.GameComplete:
                    SR_Manager.GameCompleteEvent -= SetGameObjects;
                    break;
                case GameStateEnum.Capture:
                    SR_Manager.SupplyPointChangeEvent -= SetGameObjects;
                    break;
                case GameStateEnum.FinalSupplyPoint:
                    SR_Manager.SupplyPointChangeEvent -= OnFinalSupplyPoint;
                    break;
                case GameStateEnum.EndlessStart:
                    SR_Manager.EndlessEvent -= SetGameObjects;
                    break;
                case GameStateEnum.ObjectiveComplete:
                case GameStateEnum.ObjectiveFail:
                case GameStateEnum.ObjectiveEnd:
                    SR_Manager.ObjectiveEvent -= OnObjectiveChange;
                    break;

                case GameStateEnum.None:
                default:
                    break;
            }
        }

		void Update()
        {
            switch (activeOn)
            {
                case GameStateEnum.BeforeLaunch:
                    if (!SR_Manager.instance.gameRunning)
                        SetGameObjects();
                    break;
                case GameStateEnum.Capture:
                    break;
                case GameStateEnum.FinalSupplyPoint:
                    break;
                case GameStateEnum.EndlessStart:
                    break;

                case GameStateEnum.None:
                default:
                    break;
         
            
            }
        }

        void OnFinalSupplyPoint()
        {
            if(SR_Manager.instance.optionCaptures > 0 &&
                SR_Manager.instance.CurrentCaptures >= SR_Manager.instance.optionCaptures - 1)
                SetGameObjects();
        }

        void OnObjectiveChange()
        {
            switch (activeOn)
            {
                case GameStateEnum.ObjectiveComplete:
                    if (SR_Manager.instance.stats.ObjectiveComplete)
                        SetGameObjects();
                    break;
                case GameStateEnum.ObjectiveFail:
                    if (SR_Manager.instance.stats.ObjectiveComplete == false)
                    {
                        if(SR_Manager.instance.optionCaptures > 0)
                            SetGameObjects();
                    }
                    break;
                case GameStateEnum.ObjectiveEnd:
                    if (SR_Manager.instance.stats.ObjectiveComplete == false)
                    {
                        if (SR_Manager.instance.optionCaptures <= 0)
                            SetGameObjects();
                    }
                    break;
            }
        }

		void OnSupplyPointChange()
		{
			switch (activeOn)
            {
                case GameStateEnum.SupplyPointCaptured:
					if (SR_Manager.LastSupplyPoint() == supplyPoint)
                    {
                        SetGameObjects();
                    }
                    break;
                case GameStateEnum.SupplyPointAttack:
                    if (SR_Manager.AttackSupplyPoint() == supplyPoint)
                    {
                        SetGameObjects();
                    }
                    break;
                case GameStateEnum.SupplyPointIdle:
				default:
                    if (SR_Manager.LastSupplyPoint() != supplyPoint && SR_Manager.AttackSupplyPoint() != supplyPoint)
                    {
						SetGameObjects();
                    }
                    break;
            }
		}

		public void SetGameObjects()
		{
			for (int i = 0; i < gameObjects.Length; i++)
			{
				if (gameObjects[i] == null)
					continue;

                if (setState == SetStateEnum.True)
                    gameObjects[i].SetActive(true);
                else if (setState == SetStateEnum.False)
                    gameObjects[i].SetActive(false);
                else if (setState == SetStateEnum.Inverse)
                    gameObjects[i].SetActive(!gameObjects[i].activeSelf);
                else if (setState == SetStateEnum.Random)
                {
                    int random = Random.Range(0, 10);
                    gameObjects[i].SetActive(random < 6 ? true : false);
                }
            }
		}

		public enum SetStateEnum
		{
			False = 0,
			True = 1,
			Inverse = 2,
            Random = 3,
		}

		public enum GameStateEnum
		{
			None = 0,
            SupplyPointCaptured,    //When this point is captured
            SupplyPointAttack,      //When this point is the attack point
            SupplyPointIdle,		//When this point is not in use

            BeforeLaunch,			//Before the game has been launched
            GameLaunch,				//When game starts
			GameComplete,			//After the last Supply Point is Captured
			Capture,				//When a point is captured
			FinalSupplyPoint,		//Start of the last Capture Point defined by captures count
			EndlessStart,			//When Endless becomes True

			ObjectiveComplete,		//When the objective complete is true
			ObjectiveFail,			//When objective is set to fail (Death on no respawn)
			ObjectiveEnd,			//When objective has ended (Death on no respawn on marathon)
		}
    }
}