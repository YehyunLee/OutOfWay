using UnityEngine;

namespace OutOfWay
{
    /// <summary>
    /// Beat clock for the honk phrase.
    /// Drop the finished track on <see cref="Music"/> and uncheck <see cref="MetronomeClicks"/>.
    /// The vocal "GET OUT OF THE WAY" should last exactly 4 beats; the honk lands on the next beat.
    /// </summary>
    public class MusicConductor : MonoBehaviour
    {
        [Header("When the track is ready")]
        public float Bpm = 100f;
        public bool MetronomeClicks = true;
        public AudioSource Music;
        public AudioClip GetOutOfTheWayPhrase;

        public float BeatInterval => 60f / Mathf.Max(40f, Bpm);

        public static MusicConductor Instance { get; private set; }

        void Awake() => Instance = this;

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
