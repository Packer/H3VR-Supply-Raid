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


		void Update()
		{
			
		}

		public enum SetStateEnum
		{
			False = 0,
			True = 1,
			Inverse = 2,
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