using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OutOfWay
{
    [CreateAssetMenu(menuName = "OutOfWay/Phrase Library", fileName = "PhraseLibrary")]
    public class PhraseLibrary : ScriptableObject
    {
        public PhrasePattern[] Patterns;

        static readonly string[] RandomDescriptors = {
            "Syncopate", "Offbeat", "Freestyle", "Wildcard",
            "Breakbeat", "Shuffle", "Random Beat", "Funky", "Jitter", "Quickstep"
        };

        public PhrasePattern Pick() => Pick(-1);

        /// <summary>
        /// Selects or generates the next phrase pattern based on the player's score.
        /// Rhythm starts easy with steady beats, then gets progressively harder with syncopations
        /// and randomized beats at higher scores.
        /// </summary>
        public PhrasePattern Pick(int score)
        {
            if (score < 0)
                score = GameManager.Instance != null ? GameManager.Instance.Score : 0;

            float b = 60f / 130f;

            // Tier 1: Easy at first (Score 0-2)
            if (score <= 0)
            {
                return GetOrMakePreset("Even", b);
            }
            if (score <= 2)
            {
                // Score 1-2: 60% Even, 40% Hold
                return Random.value < 0.6f ? GetOrMakePreset("Even", b) : GetOrMakePreset("Hold", b);
            }

            // Tier 2: Intermediate (Score 3-5)
            if (score <= 5)
            {
                string[] pool = { "Even", "Hold", "Drag", "Swing" };
                return PickFromNamed(pool, b);
            }

            // Tier 3: Harder (Score 6-9)
            if (score <= 9)
            {
                // 30% chance of random beat, otherwise pick from harder presets (Rush, Stutter, Drag, Swing)
                if (Random.value < 0.30f)
                    return GenerateRandomPattern(b, score);

                string[] pool = { "Rush", "Stutter", "Drag", "Swing", "Hold" };
                return PickFromNamed(pool, b);
            }

            // Tier 4: Master / Random Beat (Score 10+)
            // Score 10-13: 70% random beat. Score 14+: 85% random beat.
            float randomBeatChance = score >= 14 ? 0.85f : 0.70f;
            if (Random.value < randomBeatChance)
            {
                return GenerateRandomPattern(b, score);
            }

            // Fallback to intense preset
            string[] hardPool = { "Rush", "Stutter", "Syncopate", "Swing" };
            return PickFromNamed(hardPool, b);
        }

        PhrasePattern PickFromNamed(string[] names, float b)
        {
            if (names == null || names.Length == 0) return GetOrMakePreset("Even", b);
            string chosen = names[Random.Range(0, names.Length)];
            return GetOrMakePreset(chosen, b);
        }

        PhrasePattern GetOrMakePreset(string name, float b)
        {
            if (Patterns != null)
            {
                for (int i = 0; i < Patterns.Length; i++)
                {
                    if (Patterns[i] != null && string.Equals(Patterns[i].Label, name, StringComparison.OrdinalIgnoreCase))
                        return Patterns[i];
                }
            }
            return CreatePreset(name, b);
        }

        /// <summary>
        /// Procedurally generates a randomized, humanly playable syncopated beat.
        /// </summary>
        public static PhrasePattern GenerateRandomPattern(float b, int score)
        {
            var pattern = CreateInstance<PhrasePattern>();
            string label = RandomDescriptors[Random.Range(0, RandomDescriptors.Length)];
            pattern.name = label;
            pattern.Label = label;

            // Musical gap multipliers in terms of beat interval b:
            // 0.5b = 8th note, 0.75b = dotted 8th, 1.0b = quarter, 1.25b = 5/16th, 1.5b = dotted quarter, 1.75b = 7/16th
            float[] gapOptions = { 0.5f, 0.75f, 1.0f, 1.25f, 1.5f, 1.75f };

            float[] gaps = new float[4];
            for (int i = 0; i < 4; i++)
            {
                // Avoid consecutive fast gaps (< 0.6) so player isn't forced to spam keys
                bool prevFast = i > 0 && gaps[i - 1] < 0.6f;
                float g;
                do
                {
                    g = gapOptions[Random.Range(0, gapOptions.Length)];
                } while (prevFast && g < 0.6f);

                gaps[i] = g;
            }

            // Compute cumulative word times
            var times = new float[5];
            times[0] = 0f;
            for (int i = 0; i < 4; i++)
            {
                times[i + 1] = times[i] + gaps[i] * b;
            }

            // Ensure total span is between 2.2b and 5.2b
            float total = times[4];
            if (total < 2.2f * b)
            {
                times[4] += 0.5f * b;
            }
            else if (total > 5.2f * b)
            {
                times[4] = times[3] + 0.75f * b;
            }

            pattern.WordTimes = times;
            // Higher scores have slightly tighter tolerance for challenge
            pattern.Tolerance = Mathf.Max(0.20f, 0.28f - Mathf.Clamp01((score - 6) / 20f) * 0.08f);
            pattern.PerfectTolerance = Mathf.Max(0.10f, 0.13f - Mathf.Clamp01((score - 6) / 20f) * 0.03f);
            pattern.ResponseLeadIn = 0.14f;
            pattern.ResponseTimeout = 2.0f;

            return pattern;
        }

        /// <summary>
        /// Base rhythms calibrated to 130 BPM.
        /// Base quarter beat b = 60 / 130 = ~0.4615s.
        /// When tempo scales up to 140 and 150 BPM, MusicConductor.TempoScale scales the rhythm clock.
        /// </summary>
        public static PhraseLibrary CreateDefault()
        {
            var library = CreateInstance<PhraseLibrary>();
            float b = 60f / 130f;
            library.Patterns = new[]
            {
                CreatePreset("Even", b),
                CreatePreset("Hold", b),
                CreatePreset("Drag", b),
                CreatePreset("Swing", b),
                CreatePreset("Rush", b),
                CreatePreset("Stutter", b),
                CreatePreset("Syncopate", b)
            };
            return library;
        }

        static PhrasePattern CreatePreset(string label, float b)
        {
            var pattern = CreateInstance<PhrasePattern>();
            pattern.name = label;
            pattern.Label = label;

            switch (label.ToLowerInvariant())
            {
                case "even":
                    pattern.WordTimes = new[] { 0f, b, b * 2f, b * 3f, b * 4f };
                    break;
                case "hold":
                    pattern.WordTimes = new[] { 0f, b * 0.75f, b * 1.5f, b * 2.25f, b * 3.75f };
                    break;
                case "drag":
                    pattern.WordTimes = new[] { 0f, b, b * 2f, b * 2.75f, b * 3.75f };
                    break;
                case "swing":
                    pattern.WordTimes = new[] { 0f, b * 0.833f, b * 1.417f, b * 2.25f, b * 2.833f };
                    break;
                case "rush":
                    pattern.WordTimes = new[] { 0f, b * 0.75f, b * 1.5f, b * 2.25f, b * 3f };
                    break;
                case "stutter":
                    pattern.WordTimes = new[] { 0f, b * 0.5f, b, b * 2.25f, b * 3f };
                    break;
                case "syncopate":
                    pattern.WordTimes = new[] { 0f, b * 0.75f, b * 1.25f, b * 2.5f, b * 3.25f };
                    break;
                default:
                    pattern.WordTimes = new[] { 0f, b, b * 2f, b * 3f, b * 4f };
                    break;
            }

            return pattern;
        }
    }
}
