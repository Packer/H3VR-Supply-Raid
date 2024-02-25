using UnityEngine;

namespace SupplyRaid
{
    [CreateAssetMenu(fileName = "SR Assets", menuName = "Supply Raid/SR Assets", order = 1)]
    public class SR_Assets : ScriptableObject
    {
        [Header("Gameplay")]
        public SR_Compass srCompass;
        public SR_CaptureZone srCaptureZone;

        [Header("Menus")]
        public SR_Menu srMenu;
        public SR_BuyMenu srBuyMenu;
        public SR_AmmoSpawner srAmmoSpawner;
        public SR_MagazineDuplicator srMagazineDuplicator;
        public SR_ModTable srModTable;
        public SR_Recycler srRecycler;
        public SR_ResultsMenu srResultsMenu;
        public SR_SpawnMenu srSpawnMenu;
        public SR_SpawnStation srSpawnStation;
    }
}