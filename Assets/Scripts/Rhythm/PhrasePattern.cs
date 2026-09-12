using UnityEngine;

namespace OutOfWay
{
    /// <summary>
    /// One chant rhythm. WordTimes is the whole spec: the call reveals the words on those
    /// times, and the gaps between them are the gaps the player has to honk back.
    /// </summary>
    [CreateAssetMenu(menuName = "OutOfWay/Phrase Pattern", fileName = "Phrase")]
    public class PhrasePattern : ScriptableObject
    {
        [Tooltip("The recorded line. Leave empty to fall back to the placeholder metronome.")]
        public AudioClip Clip;

        [Tooltip("Individual audio clips for each word in Words. Falls back to MusicConductor.WordClips if empty.")]
        public AudioClip[] WordClips;

        [Tooltip("Name for this rhythm, inspector only.")]
        public string Label = "Even";

        [Tooltip("Seconds from the start of the clip, one per word, in spoken order. Retime these against the recording.")]
        public float[] WordTimes = { 0f, 0.60f, 1.20f, 1.80f, 2.40f };

        [Tooltip("Words drawn on screen, same order as WordTimes.")]
        public string[] Words = { "Get", "Out", "Of", "The", "Way" };

        [Tooltip("How far a honk may land from its word before the round fails, in seconds.")]
        public float Tolerance = 0.28f;

        [Tooltip("Land every honk inside this and the round counts as perfect.")]
        public float PerfectTolerance = 0.13f;

        [Tooltip("Grace after the last word before the player's turn opens. Keep it small — honks before this count as TOO EARLY.")]
        public float ResponseLeadIn = 0.15f;

        [Tooltip("How long the player has to start honking before it counts as a miss.")]
        public float ResponseTimeout = 2.2f;

        public int Count => Mathf.Min(WordTimes.Length, Words.Length);
        public float CallEnd => Count == 0 ? 0f : WordTimes[Count - 1];
        public float ResponseStart => CallEnd + ResponseLeadIn;
        public float ResponseSpan => Count < 2 ? 0f : WordTimes[Count - 1] - WordTimes[0];
        public float TotalDuration => ResponseStart + ResponseTimeout + ResponseSpan + Tolerance;

        /// <summary>Seconds after the first honk that honk <paramref name="index"/> is due.</summary>
        public float OffsetFromFirst(int index) => WordTimes[index] - WordTimes[0];
    }
}
