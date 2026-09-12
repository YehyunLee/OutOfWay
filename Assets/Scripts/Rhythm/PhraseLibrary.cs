using UnityEngine;

namespace OutOfWay
{
    [CreateAssetMenu(menuName = "OutOfWay/Phrase Library", fileName = "PhraseLibrary")]
    public class PhraseLibrary : ScriptableObject
    {
        public PhrasePattern[] Patterns;

        int _last = -1;

        public PhrasePattern Pick()
        {
            if (Patterns == null || Patterns.Length == 0) return null;
            if (Patterns.Length == 1) return Patterns[0];

            int index = Random.Range(0, Patterns.Length);
            if (index == _last) index = (index + 1) % Patterns.Length;
            _last = index;
            return Patterns[index];
        }

        /// <summary>
        /// Placeholder rhythms so the loop plays before the recordings land.
        /// Times sit on a 0.6s beat (100 BPM) — retime against the real clips.
        /// No gap is under 0.35s; tighter than that is very hard to honk back by hand.
        /// </summary>
        public static PhraseLibrary CreateDefault()
        {
            var library = CreateInstance<PhraseLibrary>();
            library.Patterns = new[]
            {
                Pattern("Even", new[] { 0f, 0.60f, 1.20f, 1.80f, 2.40f }),
                Pattern("Rush", new[] { 0f, 0.35f, 0.70f, 1.05f, 1.40f }),
                Pattern("Drag", new[] { 0f, 0.60f, 1.20f, 1.55f, 2.15f }),
                Pattern("Stutter", new[] { 0f, 0.35f, 0.70f, 1.30f, 1.65f }),
                Pattern("Hold", new[] { 0f, 0.45f, 0.90f, 1.35f, 2.25f }),
                Pattern("Swing", new[] { 0f, 0.50f, 0.85f, 1.35f, 1.70f })
            };
            return library;
        }

        static PhrasePattern Pattern(string label, float[] times)
        {
            var pattern = CreateInstance<PhrasePattern>();
            pattern.name = label;
            pattern.Label = label;
            pattern.WordTimes = times;
            return pattern;
        }
    }
}
