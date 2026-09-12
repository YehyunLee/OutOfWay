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

        [Header("Word Sound Effects")]
        [Tooltip("Audio clips for the chant words: [0] Get, [1] Out, [2] Of, [3] The, [4] Way")]
        public AudioClip[] WordClips;

        [Header("Current State")]
        public float Bpm = 130f;
        public bool MetronomeClicks = false;
        public AudioClip BackgroundBed;

        [Tooltip("Playback rate for the chant / rhythm clock. Drives difficulty ramp and tempo progression.")]
        [Range(0.5f, 2f)] public float TempoScale = 1f;

        [Header("Start Beat & Measure Sync")]
        [Tooltip("Offset in seconds to the first downbeat/start beat in the audio loops (calibrated to ~0.022s).")]
        public float StartBeatOffset = 0.022f;

        public bool MetronomeEnabled { get; private set; } = false;

        public event Action<bool> MetronomeToggled;
        public event Action<int, float> TierChanged;
        public event Action<int> StartBeatTriggered;

        public float BeatInterval => 60f / Mathf.Max(40f, Bpm);
        public float BarInterval => 4f * BeatInterval;

        /// <summary>Continuous elapsed music playback time, unbroken across loops.</summary>
        public float SongTime { get; private set; }

        /// <summary>Current measure/bar index since the music began playing.</summary>
        public int CurrentBar { get; private set; }

        /// <summary>True for the single frame in which the audio crossed into a new bar / start beat.</summary>
        public bool IsStartBeatThisFrame { get; private set; }

        /// <summary>Estimated seconds until the next start beat (downbeat).</summary>
        public float TimeUntilNextStartBeat
        {
            get
            {
                if (Music == null || !Music.isPlaying || BarInterval <= 0.01f) return 0f;
                float effective = SongTime - StartBeatOffset;
                if (effective < 0f) return -effective;
                float rem = BarInterval - (effective % BarInterval);
                return (rem >= BarInterval - 0.002f) ? 0f : rem;
            }
        }

        public float PhraseTime => PhraseSource != null ? PhraseSource.time : 0f;
        public bool PhrasePlaying => PhraseSource != null && PhraseSource.isPlaying;
        public static MusicConductor Instance { get; private set; }

        float _lastSyncCheck;
        float _lastMusicTime;
        float _lastPlayhead;
        int _lastReportedBar = -1;

        void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            if (PhraseSource != null) PhraseSource.pitch = TempoScale;

            IsStartBeatThisFrame = false;

            if (Music != null && Music.isPlaying)
            {
                float playhead = Music.time;
                float delta = playhead - _lastPlayhead;
                if (delta < 0f && Music.clip != null)
                {
                    delta += Music.clip.length;
                }
                _lastPlayhead = playhead;

                if (delta >= 0f && delta < 0.5f)
                {
                    SongTime += delta;
                }

                // Keep MetronomeSource in lockstep with Music loop and musical measures (BarInterval)
                if (MetronomeSource != null && MetronomeSource.clip != null && BarInterval > 0.01f)
                {
                    _lastMusicTime = Music.time;

                    // Compute current phase within the 4-beat bar (measure)
                    float effectiveMusicTime = Music.time - StartBeatOffset;
                    while (effectiveMusicTime < 0f) effectiveMusicTime += BarInterval;
                    float targetMetroTime = effectiveMusicTime % BarInterval;

                    // Periodic sync & loop boundary correction:
                    // Prevents metronome from playing trailing MP3 container padding (> BarInterval)
                    // and eliminates any phase drift against the background music loop
                    if (MetronomeSource.time >= BarInterval || Mathf.Abs(MetronomeSource.time - targetMetroTime) > 0.025f)
                    {
                        MetronomeSource.time = targetMetroTime;
                    }
                }

                // Check for new bar crossing (the Start Beat / downbeat of the bar)
                if (BarInterval > 0.01f)
                {
                    float effectiveTime = SongTime - StartBeatOffset;
                    int bar = effectiveTime >= 0f ? Mathf.FloorToInt(effectiveTime / BarInterval) : -1;
                    if (bar > _lastReportedBar)
                    {
                        _lastReportedBar = bar;
                        CurrentBar = bar;
                        IsStartBeatThisFrame = true;
                        StartBeatTriggered?.Invoke(bar);
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

            if (PhraseSource == null)
            {
                PhraseSource = gameObject.AddComponent<AudioSource>();
            }
            PhraseSource.playOnAwake = false;
            PhraseSource.spatialBlend = 0f;
            PhraseSource.volume = 1f;

            if (WordClips == null || WordClips.Length == 0)
            {
                WordClips = new[]
                {
                    Resources.Load<AudioClip>("Audio/Word_Get") ?? Resources.Load<AudioClip>("Audio/get"),
                    Resources.Load<AudioClip>("Audio/Word_Out") ?? Resources.Load<AudioClip>("Audio/out"),
                    Resources.Load<AudioClip>("Audio/Word_Of") ?? Resources.Load<AudioClip>("Audio/of"),
                    Resources.Load<AudioClip>("Audio/Word_The") ?? Resources.Load<AudioClip>("Audio/the"),
                    Resources.Load<AudioClip>("Audio/Word_Way") ?? Resources.Load<AudioClip>("Audio/way")
                };
            }

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

            SongTime = 0f;
            CurrentBar = 0;
            _lastReportedBar = -1;
            _lastPlayhead = 0f;
            _lastMusicTime = 0f;

            bool wasMusicPlaying = Music != null && Music.isPlaying;
            bool wasMetroPlaying = MetronomeSource != null && MetronomeSource.isPlaying;

            if (Music != null && tier.Music != null)
            {
                Music.clip = tier.Music;
                Music.time = 0f;
                _lastMusicTime = 0f;
                _lastPlayhead = 0f;
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

        public void ResetMusicClock()
        {
            SongTime = 0f;
            CurrentBar = 0;
            _lastReportedBar = -1;
            _lastPlayhead = Music != null ? Music.time : 0f;
            _lastMusicTime = _lastPlayhead;
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

                if (enabled && Music != null && Music.isPlaying && BarInterval > 0.01f)
                {
                    float effectiveMusicTime = Music.time - StartBeatOffset;
                    while (effectiveMusicTime < 0f) effectiveMusicTime += BarInterval;
                    MetronomeSource.time = effectiveMusicTime % BarInterval;

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

        public void PlayWord(int index, AudioClip customClip = null, float volume = 1f)
        {
            var clip = customClip;
            if (clip == null && WordClips != null && index >= 0 && index < WordClips.Length)
                clip = WordClips[index];

            if (clip != null && PhraseSource != null)
            {
                PhraseSource.pitch = TempoScale;
                PhraseSource.PlayOneShot(clip, volume);
            }
        }

        public void PlayWord(string word, float volume = 1f)
        {
            if (string.IsNullOrEmpty(word)) return;
            int idx = word.ToLowerInvariant() switch
            {
                "get" => 0,
                "out" => 1,
                "of" => 2,
                "the" => 3,
                "way" => 4,
                _ => -1
            };
            if (idx >= 0) PlayWord(idx, null, volume);
        }

        public void StopChant()
        {
            if (PhraseSource != null) PhraseSource.Stop();
        }

        public void Tick(bool accent)
        {
            if (!MetronomeClicks || ProceduralAudio.Instance == null) return;
            if (accent) ProceduralAudio.Instance.Accent();
            else ProceduralAudio.Instance.Click();
        }
    }
}
