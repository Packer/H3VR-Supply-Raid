using System.IO;
using UnityEngine;

namespace Supply_Raid_Editor
{
    public class SR_SosigFaction
    {
        public string name = "Faction Name";
        public string description = "A short description of this sosig faction";
        public string category = "Mod";

        public TeamEnum teamID = TeamEnum.Ally0;

        //List / Table / Group / Faction
        public FactionLevel[] levels;
        public FactionLevel[] endless;


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

    public class FactionLevel
    {
        public string name = "Level #";
        public int enemiesTotal = 5;    //Per Player
        //GUARDS
        public int guardCount = 0;
        public SosigEnemyID[] guardPool;

        //SNIPERS
        public int sniperCount = 0;
        public SosigEnemyID[] sniperPool;

        //PATROL
        public int minPatrolSize = 2;
        public SosigEnemyID[] patrolPool;

        //A Squad is a group of sosigs that spawn at a random supply point then move towards the capture supply point.
        //They can be assigned a team to help or hinder the player

        public TeamSquadEnum squadTeamRandomized = TeamSquadEnum.Team1;
        public int squadCount = 0;
        public int squadSizeMin = 0;
        public int squadSizeMax = 0;
        public SquadBehaviour squadBehaviour = SquadBehaviour.CaptureSupplyPoint;
        public SosigEnemyID[] squadPool;
    }
}