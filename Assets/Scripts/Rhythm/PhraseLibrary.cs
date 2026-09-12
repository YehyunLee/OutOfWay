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
                Pattern("Even", new[] { 0f, b, b * 2f, b * 3f, b * 4f }),
                Pattern("Rush", new[] { 0f, b * 0.75f, b * 1.5f, b * 2.25f, b * 3f }),
                Pattern("Drag", new[] { 0f, b, b * 2f, b * 2.75f, b * 3.75f }),
                Pattern("Stutter", new[] { 0f, b * 0.5f, b, b * 2.25f, b * 3f }),
                Pattern("Hold", new[] { 0f, b * 0.75f, b * 1.5f, b * 2.25f, b * 3.75f }),
                Pattern("Swing", new[] { 0f, b * 0.833f, b * 1.417f, b * 2.25f, b * 2.833f })
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
