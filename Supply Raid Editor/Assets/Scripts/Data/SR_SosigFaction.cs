using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace Supply_Raid_Editor
{
    [System.Serializable]
    public class SR_SosigFaction
    {
        [Tooltip("(REQUIRED) Name of this Sosig Faction")]
        public string name = "Faction Name";
        [Tooltip("Short explanation of this faction"), Multiline(6)]
        public string description = "A short description of this sosig faction";
        [Tooltip("(REQUIRED) The menu category this faction will go in, Recommend mod creator names etc")]
        public string category = "Mod";

        [Tooltip("This factions defenders team ID, 0 = Player, 1,2,3... = Enemy")]
        public TeamEnum teamID = TeamEnum.Team1;

        //List / Table / Group / Faction
        public List<FactionLevel> levels;
        [Tooltip("Endless looping levels after all levels are complete")]
        public List<FactionLevel> endless;

        public void ExportJson()
        {
            Debug.Log("Exporting Item");
            using (StreamWriter streamWriter = new StreamWriter(Application.dataPath + "/SR_Faction_" + name + ".json"))
            {
                string json = JsonUtility.ToJson(this, true);
                streamWriter.Write(json);
            }
        }
    }

    [System.Serializable]
    public class FactionLevel
    {
        public string name = " ";
        public int enemiesTotal = 8;    //Per Player
        public bool infiniteEnemies = false;        //If true, enemies will spawn up to onscreen or Total (Whatever is lowest)
        public bool infiniteSquadEnemies = false;   //If true, enemies will spawn up to onscreen or Total (Whatever is lowest)
        public float enemySpawnTimer = 1;           //How often enemies spawn through the rabbit hole system
        public float squadDelayTimer = 1;           //How long before squads start spawning
        
        //Boss
        [Tooltip("The minimum size for Bosses Groups")]
        public int bossCount = 0;
        [Tooltip("The sosig ID pool for stationary bosses")]
        public SosigPool bossPool;
        
        //GUARDS
        public int guardCount = 4;
        public SosigPool guardPool;

        //SNIPERS
        public int sniperCount = 2;
        public SosigPool sniperPool;

        //PATROL
        public int minPatrolSize = 2;
        public int maxPatrolSize = -1;
        public SosigPool patrolPool;

        //A Squad is a group of sosigs that spawn at a random supply point then move towards the capture supply point.
        //They can be assigned a team to help or hinder the player
        public int squadCount = 0;
        public SosigPool squadPool;

        public SquadBehaviour squadBehaviour = SquadBehaviour.CaptureSupplyPoint;
        public TeamSquadEnum squadTeamRandomized = TeamSquadEnum.Team1;
        public int squadSizeMin = 0;
        public int squadSizeMax = 0;
    }

    [System.Serializable]
    public class SosigPool
    {
        public int[] sosigEnemyID;

        public int Count()
        {
            return sosigEnemyID.Length;
        }
    }
}