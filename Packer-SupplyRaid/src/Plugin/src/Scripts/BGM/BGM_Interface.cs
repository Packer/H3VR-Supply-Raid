/*
using TNHBGLoader;
using TNHBGLoader.Soundtrack;
using UnityEngine;
using FistVR;
using SupplyRaid;
using System.Linq;

namespace BGM
{

    public class BGM_Interface : SoundtrackPlayer
    {
        public static TrackSet Tracks;
        public static int Level = 0;

        public void Awake()
        {
            //Move to its own method if it breaks on awake

            Initialize("tnh", SoundtrackAPI.GetCurrentSoundtrack, 1.5f, PluginMain.AnnouncerMusicVolume.Value / 4f);

            ClearQueue();
            Level = SR_Manager.instance.CurrentCaptures;

            // Initialize holdmusic
            Tracks = SoundtrackAPI.GetSet("take", Level);

            // If the hold music has its own take theme, play it
            if (Tracks.Tracks.Any(x => x.Type == "take"))
                QueueTake(Tracks);
            else //Otherwise, get a take theme.
                QueueTake(SoundtrackAPI.GetSet("take", Level));
        }

        public static void SetTakeMusic(int level)
        {
            Tracks = SoundtrackAPI.GetSet("take", level);
            Instance.QueueRandomOfType(Tracks, "take");
            Instance.PlayNextSongInQueue();
        }

        public static void SetHoldMusic(int level)
        {
            Tracks = SoundtrackAPI.GetSet("hold", level);
            Instance.QueueRandomOfType(Tracks, "lo");
            Instance.PlayNextSongInQueue();
        }

        public override void PlayNextSongInQueue()
        {
            base.PlayNextSongInQueue();
        }

        public override void SwitchSong(Track newTrack, float timeOverride = -1)
        {
            base.SwitchSong(newTrack, timeOverride);

        }

        public static void QueueTake(int situation)
        {
            var set = SoundtrackAPI.GetSet("take", situation);
            QueueTake(set);
        }

        public static void QueueTake(TrackSet set)
        {
            Instance.QueueRandomOfType(set, "takeintro", false);
            Instance.QueueRandomOfType(set, "take");
        }

        public static void SpawnPanel(Vector3 position, Quaternion rotation)
        {
            Debug.Log("SpawnPanel Start");
            var bgmpanel = new TNHPanel();
            bgmpanel.Initialize("tnh", true, true, true);
            GameObject panel = bgmpanel.Panel.GetOrCreatePanel();
            panel.transform.position = position;
            panel.transform.localRotation = rotation * Quaternion.Euler(90, 0, 0);
            panel.GetComponent<FVRPhysicalObject>().SetIsKinematicLocked(true);
            //make rawimage ui thing
            var rawimage = new GameObject();
            var wait = rawimage.AddComponent<TNH_BGLoader.IconDisplayWaitForInit>();
            wait.panel = panel;
            wait.bgmpanel = bgmpanel;
            Debug.Log("SpawnPanel End");
        }
        public static void InitializeSoundtrackInterface()
        {
            Debug.Log("InitializeSoundtrackInterface Start");
            Instance = Instantiate(new GameObject()).AddComponent<BGM_Interface>();
            Debug.Log("InitializeSoundtrackInterface End");

        }

    }
}
*/