
using TNHBGLoader;
using TNHBGLoader.Soundtrack;
using TNH_BGLoader;
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

        public static GameObject bgmPanelGO;

        public void Awake()
        {
            //Move to its own method if it breaks on awake
            SoundtrackAPI.SelectedSoundtrackIndex = 0;
            Debug.Log("IS THIS NULL "  + SoundtrackAPI.GetCurrentSoundtrack);

            SoundtrackAPI.Soundtracks[SoundtrackAPI.SelectedSoundtrackIndex].AssembleMusicData();
            //SoundtrackAPI.GetCurrentSoundtrack.AssembleMusicData();
            Initialize("tnh", SoundtrackAPI.GetCurrentSoundtrack, 1.5f, PluginMain.AnnouncerMusicVolume.Value / 4f);
            //CurrentSoundtrack.AssembleMusicData();
            ClearQueue();
            Level = SR_Manager.instance.CurrentCaptures;

            Debug.Log("POST :(");
            // Initialize holdmusic
            Tracks = SoundtrackAPI.GetSet("take", Level);

            Debug.Log("POST A");
            // If the hold music has its own take theme, play it
            if (Tracks.Tracks.Any(x => x.Type == "take"))
                QueueTake(Tracks);
            else //Otherwise, get a take theme.
                QueueTake(SoundtrackAPI.GetSet("take", Level));
            Debug.Log("POST B");
            //Instance.PlayNextSongInQueue();
            Instance.PlayNextTrackInQueueOfType(new[] { "intro", "lo", "phase0" });
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
            var wait = rawimage.AddComponent<IconDisplayWaitForInit>();
            wait.panel = panel;
            wait.bgmpanel = bgmpanel;

            //get the bank last loaded and set banknum to it; if it doesnt exist it just defaults to 0
            if (!PluginMain.IsSoundtrack.Value)
                for (int i = 0; i < BankAPI.LoadedBankLocations.Count; i++)
                    if (System.IO.Path.GetFileNameWithoutExtension(BankAPI.LoadedBankLocations[i]) == PluginMain.LastLoadedBank.Value)
                    {
                        BankAPI.SwapBank(i);
                        break;
                    }

            if (PluginMain.IsSoundtrack.Value)
            {
                SoundtrackAPI.EnableSoundtrackFromGUID(PluginMain.LastLoadedSoundtrack.Value);
            }
            bgmpanel.SetIcon();

            bgmPanelGO = panel;
            //set last loaded announcer
            AnnouncerAPI.CurrentAnnouncerIndex = AnnouncerAPI.GetAnnouncerIndexFromGUID(PluginMain.LastLoadedAnnouncer.Value);
            Debug.Log("SpawnPanel End");

        }

        public static void InitializeSoundtrackInterface()
        {
            //Game Started, destroy the panel
            BankAPI.NukeSongSnippets();
            Destroy(bgmPanelGO);

            Debug.Log("InitializeSoundtrackInterface Start");
            Instance = Instantiate(new GameObject()).AddComponent<BGM_Interface>();
            Debug.Log("InitializeSoundtrackInterface End");

        }

    }
}
