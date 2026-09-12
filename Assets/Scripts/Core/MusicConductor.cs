using System;
using UnityEngine;

namespace OutOfWay
{
    [Serializable]
    public class TempoTier
    {
        public int Bpm;
        public AudioClip Music;
        public AudioClip Metronome;

        public TempoTier(int bpm, AudioClip music, AudioClip metronome)
        {
            Bpm = bpm;
            Music = music;
            Metronome = metronome;
        }
    }

    /// <summary>
    /// Beat clock, multi-tempo looping background bed, and synchronized metronome.
    /// Tracks are available at 130 BPM, 140 BPM, and 150 BPM.
    /// Metronome runs synchronously for accessibility (default OFF).
    /// </summary>
    public class MusicConductor : MonoBehaviour
    {
        public const string BedResource = "Audio/BackgroundBed";
        public const string BGM130Resource = "Audio/BGM_130";
        public const string Metro130Resource = "Audio/Metro_130";
        public const string BGM140Resource = "Audio/BGM_140";
        public const string Metro140Resource = "Audio/Metro_140";
        public const string BGM150Resource = "Audio/BGM_150";
        public const string Metro150Resource = "Audio/Metro_150";

        const float BedVolume = 0.58f;
        const float MetronomeVolume = 0.65f;

        [Header("Tempo Tiers")]
        public TempoTier[] Tiers;
        public int CurrentTierIndex { get; private set; } = 0;
        public int TierCount => Tiers != null ? Tiers.Length : 0;

        [Header("Audio Sources")]
        public AudioSource Music;
        public AudioSource MetronomeSource;
        public AudioSource PhraseSource;

        [Header("Current State")]
        public float Bpm = 130f;
        public bool MetronomeClicks = false;
        public AudioClip BackgroundBed;

        [Tooltip("Playback rate for the chant / rhythm clock. Drives difficulty ramp and tempo progression.")]
        [Range(0.5f, 2f)] public float TempoScale = 1f;

        public bool MetronomeEnabled { get; private set; } = false;

        public event Action<bool> MetronomeToggled;
        public event Action<int, float> TierChanged;

        public float BeatInterval => 60f / Mathf.Max(40f, Bpm);
        public float PhraseTime => PhraseSource != null ? PhraseSource.time : 0f;
        public bool PhrasePlaying => PhraseSource != null && PhraseSource.isPlaying;
        public static MusicConductor Instance { get; private set; }

        float _lastSyncCheck;
        float _lastMusicTime;

        void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            if (PhraseSource != null) PhraseSource.pitch = TempoScale;

            // Keep MetronomeSource in lockstep with Music loop
            if (Music != null && Music.isPlaying && MetronomeSource != null && MetronomeSource.clip != null)
            {
                // Detect loop wrap
                if (Music.time < _lastMusicTime)
                {
                    MetronomeSource.time = 0f;
                }
                _lastMusicTime = Music.time;

                // Periodic drift correction
                if (Time.unscaledTime - _lastSyncCheck > 0.25f)
                {
                    _lastSyncCheck = Time.unscaledTime;
                    float metroLen = MetronomeSource.clip.length;
                    if (metroLen > 0.05f)
                    {
                        float targetTime = Music.time % metroLen;
                        if (Mathf.Abs(MetronomeSource.time - targetTime) > 0.035f)
                        {
                            MetronomeSource.time = targetTime;
                        }
                    }
                }
            }
        }

        public void SetupBed()
        {
            if (Tiers == null || Tiers.Length == 0)
            {
                var m130 = Resources.Load<AudioClip>(BGM130Resource);
                var met130 = Resources.Load<AudioClip>(Metro130Resource);
                var m140 = Resources.Load<AudioClip>(BGM140Resource);
                var met140 = Resources.Load<AudioClip>(Metro140Resource);
                var m150 = Resources.Load<AudioClip>(BGM150Resource);
                var met150 = Resources.Load<AudioClip>(Metro150Resource);

                if (m130 != null)
                {
                    Tiers = new[]
                    {
                        new TempoTier(130, m130, met130),
                        new TempoTier(140, m140 ?? m130, met140 ?? met130),
                        new TempoTier(150, m150 ?? m130, met150 ?? met130)
                    };
                }
            }

            if (BackgroundBed == null)
            {
                if (Tiers != null && Tiers.Length > 0 && Tiers[0].Music != null)
                    BackgroundBed = Tiers[0].Music;
                else
                    BackgroundBed = Resources.Load<AudioClip>(BedResource);
            }

            if (Music == null)
            {
                Music = gameObject.AddComponent<AudioSource>();
            }
            Music.playOnAwake = false;
            Music.spatialBlend = 0f;
            Music.loop = true;
            Music.volume = BedVolume;

            if (MetronomeSource == null)
            {
                MetronomeSource = gameObject.AddComponent<AudioSource>();
            }
            MetronomeSource.playOnAwake = false;
            MetronomeSource.spatialBlend = 0f;
            MetronomeSource.loop = true;

            // Accessibility preference: default is false (OFF)
            MetronomeEnabled = PlayerPrefs.GetInt("OutOfWay.Metronome", 0) == 1;
            MetronomeSource.mute = !MetronomeEnabled;
            MetronomeSource.volume = MetronomeEnabled ? MetronomeVolume : 0f;

            SetTier(0);

            if (Music != null && Music.clip != null && !Music.isPlaying)
            {
                Music.Play();
                if (MetronomeSource != null && MetronomeSource.clip != null)
                    MetronomeSource.Play();
            }
        }

        public void SetTier(int tierIndex)
        {
            if (Tiers == null || Tiers.Length == 0)
            {
                if (BackgroundBed != null && Music != null)
                {
                    Music.clip = BackgroundBed;
                    if (BackgroundBed.length > 0.4f) Bpm = 32f * 60f / BackgroundBed.length;
                    if (!Music.isPlaying) Music.Play();
                }
                return;
            }

            tierIndex = Mathf.Clamp(tierIndex, 0, Tiers.Length - 1);
            CurrentTierIndex = tierIndex;

            var tier = Tiers[tierIndex];
            Bpm = tier.Bpm;
            TempoScale = Bpm / 130f;

            bool wasMusicPlaying = Music != null && Music.isPlaying;
            bool wasMetroPlaying = MetronomeSource != null && MetronomeSource.isPlaying;

            if (Music != null && tier.Music != null)
            {
                Music.clip = tier.Music;
                Music.time = 0f;
                _lastMusicTime = 0f;
                if (wasMusicPlaying || !Music.isPlaying) Music.Play();
            }

            if (MetronomeSource != null && tier.Metronome != null)
            {
                MetronomeSource.clip = tier.Metronome;
                MetronomeSource.time = 0f;
                MetronomeSource.mute = !MetronomeEnabled;
                MetronomeSource.volume = MetronomeEnabled ? MetronomeVolume : 0f;
                if (wasMetroPlaying || !MetronomeSource.isPlaying) MetronomeSource.Play();
            }

            MetronomeClicks = false;
            TierChanged?.Invoke(CurrentTierIndex, Bpm);
        }

        public bool UpgradeTier()
        {
            if (Tiers == null || CurrentTierIndex >= Tiers.Length - 1) return false;
            SetTier(CurrentTierIndex + 1);
            return true;
        }

        public void ResetToStartingTier()
        {
            SetTier(0);
        }

        public void ToggleMetronome()
        {
            SetMetronome(!MetronomeEnabled);
        }

        public void SetMetronome(bool enabled)
        {
            MetronomeEnabled = enabled;
            PlayerPrefs.SetInt("OutOfWay.Metronome", enabled ? 1 : 0);

            if (MetronomeSource != null)
            {
                MetronomeSource.mute = !enabled;
                MetronomeSource.volume = enabled ? MetronomeVolume : 0f;

                if (enabled && Music != null && Music.isPlaying)
                {
                    float metroLen = MetronomeSource.clip != null ? MetronomeSource.clip.length : 1f;
                    if (metroLen > 0.05f)
                        MetronomeSource.time = Music.time % metroLen;

                    if (!MetronomeSource.isPlaying)
                        MetronomeSource.Play();
                }
            }

            MetronomeToggled?.Invoke(enabled);
        }

        public void SetBedVolume(float volume)
        {
            if (Music != null) Music.volume = volume;
            if (MetronomeSource != null && MetronomeEnabled)
                MetronomeSource.volume = volume * (MetronomeVolume / BedVolume);
        }

        public void RestoreBedVolume() => SetBedVolume(BedVolume);

        public void PlayPhrase(AudioClip clip)
        {
            if (clip == null || PhraseSource == null) return;
            PhraseSource.clip = clip;
            PhraseSource.pitch = TempoScale;
            PhraseSource.time = 0f;
            PhraseSource.Play();
        }

        public void Tick(bool accent)
        {
            if (!MetronomeClicks || ProceduralAudio.Instance == null) return;
            if (accent) ProceduralAudio.Instance.Accent();
            else ProceduralAudio.Instance.Click();
        }
    }
}
