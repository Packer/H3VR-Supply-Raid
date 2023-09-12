using System;

namespace Supply_Raid_Editor
{
    public enum SquadBehaviour
    {
        CaptureSupplyPoint = 0,
        RandomSupplyPoint = 1,
    }

    [System.Serializable]
    public enum TeamEnum
    {
        Ally0 = 0,
        Team1 = 1,
        Team2 = 2,
        Team3 = 3,
    }

    public enum TeamSquadEnum
    {
        Ally0 = 0,
        Team1 = 1,
        Team2 = 2,
        Team3 = 3,
        RandomTeam,         //0-4
        RandomEnemyTeam,    //Only enemy teams 1-2-3 etc

    }
}