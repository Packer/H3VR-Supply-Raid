//using BGM;
using TNHBGLoader.Soundtrack;
using UnityEngine;
using FistVR;
namespace BGM;

public class BGM
{
    public static BGM_Interface SetupBGM(GameObject mgr)
    {
        return mgr.AddComponent<BGM_Interface>();
    }

    public static void SetTakeMusic(int level)
    {
        //BGM_Interface.SetTakeMusic(level);
    }

    public static void SetHoldMusic(int level)
    {
        //BGM_Interface.SetHoldMusic(level);
    }

    public static void QueueTake(int situation)
    {
        //BGM_Interface.QueueTake(situation);
    }

    /*
    public static void QueueTake(TNHBGLoader.Soundtrack.TrackSet set)
    {
        //BGM_Interface.QueueTake(set);
    }
    */

    public static void SpawnPanel(Vector3 position, Quaternion rotation)
    {
        //BGM_Interface.SpawnPanel(position, rotation);
    }

    public static void InitializeSoundtrackInterface()
    {
        BGM_Interface.InitializeSoundtrackInterface();
    }
}
