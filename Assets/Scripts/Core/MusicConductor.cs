using UnityEngine;

namespace OutOfWay
{
    /// <summary>
    /// Beat clock plus looping bed.
    /// The placeholder bed lives at Resources/Audio/BackgroundBed.
    /// Swap clips later for other tempos; BPM is derived from a 32-beat (8 bar) loop.
    /// </summary>
    public class MusicConductor : MonoBehaviour
    {
        public const string BedResource = "Audio/BackgroundBed";

        [Header("When the track is ready")]
        public float Bpm = 100f;
        public bool MetronomeClicks = true;
        public AudioSource Music;
        public AudioClip GetOutOfTheWayPhrase;
        public AudioClip BackgroundBed;

        public float BeatInterval => 60f / Mathf.Max(40f, Bpm);
        public static MusicConductor Instance { get; private set; }

        void Awake() => Instance = this;

        public void SetupBed()
        {
            if (BackgroundBed == null)
                BackgroundBed = Resources.Load<AudioClip>(BedResource);

            if (Music == null) return;
            Music.playOnAwake = false;
            Music.spatialBlend = 0f;
            Music.loop = true;
            Music.volume = 0.58f;

            if (BackgroundBed == null) return;

            Music.clip = BackgroundBed;
            // Loop is 8 bars of 4/4 (32 beats) — keeps the honk phrase on the grid.
            if (BackgroundBed.length > 0.4f)
                Bpm = 32f * 60f / BackgroundBed.length;

            MetronomeClicks = false;
            if (!Music.isPlaying) Music.Play();
        }

        public void SetBedVolume(float volume)
        {
            if (Music != null) Music.volume = volume;
        }

        public void PlayPhrase()
        {
            if (GetOutOfTheWayPhrase == null || Music == null) return;
            Music.PlayOneShot(GetOutOfTheWayPhrase);
        }

        public void Tick(bool accent)
        {
            if (!MetronomeClicks || ProceduralAudio.Instance == null) return;
            if (accent) ProceduralAudio.Instance.Accent();
            else ProceduralAudio.Instance.Click();
        }
    }
}
