using FistVR;
using UnityEngine;

namespace SupplyRaid
{

	public class SR_GameObjects : MonoBehaviour
	{
		public GameStateEnum activeOn = GameStateEnum.None;
		public SetStateEnum setState = SetStateEnum.True;

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
			SupplyPointCaptured,
			SupplyPointAttack,

			LevelChange,
			GameLaunch,
            EndlessStart,
            LastLevelStart,

            ObjectiveComplete,
			ObjectiveFail,
			ObjectiveEnd,

		}
    }
}