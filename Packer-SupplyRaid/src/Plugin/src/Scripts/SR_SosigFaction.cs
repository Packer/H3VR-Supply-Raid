using BepInEx;
using FistVR;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SupplyRaid
{
    [System.Serializable]
    public class SR_SosigFaction
    {
        [Tooltip("(REQUIRED) Name of this Sosig Faction")]
        public string name = "";
        [Tooltip("Short explanation of this faction"), Multiline(6)]
        public string description = "A short description of this sosig faction";
        [Tooltip("(REQUIRED) The menu category this faction will go in, Recommend mod creator names etc")]
        public string category = "Mod";

        [Tooltip("Preview image of the faction when selected")]
        private Sprite thumbnail;

        [Tooltip("This factions defenders team ID, 0 = Player, 1,2,3... = Enemy")]
        public TeamEnum teamID = TeamEnum.Team1;

        //List / Table / Group / Faction
        public FactionLevel[] levels;
        [Tooltip("Endless looping levels after all levels are complete")]
        public FactionLevel[] endless;
        private string thumbnailPath = "";

        public void SetupThumbnailPath(string thumbPath)
        {
            thumbnailPath = thumbPath;
        }

        public Sprite Thumbnail()
        {
            if (thumbnailPath == "")
            {
                Debug.LogError("Thumbnail not defined for character : " + category + "/" + name);
                return null;
            }

            if (thumbnail == null)
                thumbnail = SR_Global.LoadSprite(thumbnailPath);

            if (thumbnail == null)
                return SR_Manager.instance.fallbackThumbnail;

            return thumbnail;
        }

        /*
        public void ExportJson()
        {
            using (StreamWriter streamWriter = new StreamWriter(Application.dataPath + "/SR_Faction_" + name + ".json"))
            {
                string json = JsonUtility.ToJson(this, true);
                streamWriter.Write(json);
            }
        }
        */

    }


    [System.Serializable]
    public class SosigPool
    {
        public SosigEnemyID[] sosigEnemyID;
        public string[] customPool; //Arctic_Patrol etc

        public int Count()
        {
            return sosigEnemyID.Length + customPool.Length;
        }
    }

    [System.Serializable]
    public class FactionLevel
    {
        public string name;

        [Header("Defenders")]

        [Tooltip("The total sosig count for this entire level")]
        public int enemiesTotal = 5;                //max overall enemies
        public bool infiniteEnemies = false;        //If true, enemies will spawn up to onscreen or Total (Whatever is lowest)
        public bool infiniteSquadEnemies = false;   //If true, enemies will spawn up to onscreen or Total (Whatever is lowest)
        public float enemySpawnTimer = 1;           //How often enemies spawn through the rabbit hole system

        [Space]
        [Header("Guards")]
        //GUARDS
        [Tooltip("The minimum size for Gaurds Groups")]
        public int guardCount = 0;
        [Tooltip("The sosig ID pool for stationary gaurds")]
        public SosigPool guardPool;

        [Space]
        [Header("Sniper")]
        //SNIPERS
        [Tooltip("The minimum size for Gaurds Groups")]
        public int sniperCount = 0;
        [Tooltip("The sosig ID pool for stationary snipers")]
        public SosigPool sniperPool;

        [Space]
        [Header("Patrol")]
        //PATROL
        [Tooltip("The minimum size for Patrol Groups")]
        public int minPatrolSize = 2;
        [Tooltip("The sosig ID pool for patrols")]
        public SosigPool patrolPool;

        [Space, Space]
        [Header("Squad")]
        //A Squad is a group of sosigs that spawn at a random supply point then move towards the capture supply point.
        //They can be assigned a team to help or hinder the player

        [Tooltip("The team the world patrol sosigs are on, 0 = Player, 1,2,3 = Enemies")]
        public TeamEnum squadTeamRandomized = TeamEnum.Team1;
        [Tooltip("The amount of squads that wil be spawned")]
        public int squadCount = 0;
        [Tooltip("The minimum sosigs in the squad")]
        public int squadSizeMin = 0;
        [Tooltip("The maximum sosigs in the squad")]
        public int squadSizeMax = 0;
        [Tooltip("Main Supply Point - Squads will move towards the next player capture supply point \n Random Supply Point - Squads will move to random supply points across the map")]
        public SquadBehaviour squadBehaviour = SquadBehaviour.RandomSupplyPoint;
        [Tooltip("The sosig ID pool for world patrols")]
        public SosigPool squadPool;
        
    }

    public enum SquadBehaviour
    {
        CaptureSupplyPoint = 0,
        RandomSupplyPoint = 1,
    }

    public enum TeamEnum
    {
        Ally0 = 0,
        Team1 = 1,
        Team2 = 2,
        Team3 = 3,
        Team4 = 4,
        Team5 = 5,
        Team6 = 6,
        Team7 = 7,
        Team8 = 8,
        RandomTeam = -1,         //0-4
        RandomEnemyTeam = -2,    //Only enemy teams 1-2-3 etc
    }
}