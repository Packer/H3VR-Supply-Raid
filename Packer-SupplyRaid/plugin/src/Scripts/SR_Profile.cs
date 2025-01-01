using UnityEngine;
using FistVR;

namespace SupplyRaid
{
    public class SR_Profile
    {
        public string name = "Profile";
        public float difficulty = 1f;
        public int startLevel = 0;
        public float playerCount = 1;

        public int maxEnemies = 10;
        public int maxSquadEnemies = 8;

        public int captures = 5;
        public int captureOrder = 0;
        public bool captureZone = true;

        public bool freeBuyMenu = false;
        public bool itemSpawner = false;
        public bool spawnLocking = true;

        public bool respawn = true;
        [Tooltip("None = 0,  100 = 100% Dropped ")]
        public int itemsDrop = 0;
        public int playerHealth = 5000;
        public bool hand = false;

        public string character = "";
        public string faction = "";
        public bool sosigWeapons = true;
    }
}
