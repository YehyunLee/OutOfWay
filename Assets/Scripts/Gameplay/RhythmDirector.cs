using System;
using UnityEngine;

namespace OutOfWay
{
    public enum RhythmPhase
    {
        Idle,
        Call,
        Response,
        Resolved
    }

    /// <summary>
    /// Call and response. The chant reveals "Get Out Of The Way" on the pattern's rhythm,
    /// then the player honks once per word echoing it back.
    /// Honks are judged on their offset from the FIRST honk, so the player reproduces the
    /// shape of the rhythm rather than having to start on an exact beat.
    /// </summary>
    public class RhythmDirector : MonoBehaviour
    {
        public bool FailOnEarlyHonk = true;

        [Tooltip("Multiplies every pattern's hit window. Raise it to make the whole game more forgiving.")]
        [Range(0.25f, 4f)] public float ToleranceScale = 1f;

        public RhythmPhase Phase { get; private set; } = RhythmPhase.Idle;
        public PhrasePattern Pattern { get; private set; }
        public int CueIndex { get; private set; } = -1;
        public int HonksAccepted { get; private set; }
        public bool LastHitWasPerfect { get; private set; }
        public bool Busy => Phase == RhythmPhase.Call || Phase == RhythmPhase.Response;

        public event Action<int, string> CueBeat;
        public event Action ResponseOpened;
        public event Action<int> HonkAccepted;
        public event Action Success;
        public event Action<string> Failed;

        MusicConductor _music;
        PhraseLibrary _library;
        PhrasePattern _next;

        // Seconds into the phrase, in pattern time. Advances at TempoScale, so slow-time and
        // fast-time shrink or stretch the real-world windows without touching the pattern data.
        float _clock;
        bool _clipDriven;
        float _firstHonkAt;
        float _worstError;

        public void Bind(MusicConductor music, PhraseLibrary library)
        {
            _music = music;
            _library = library;
        }

        /// <summary>Chooses the next rhythm without starting it, so the spawner can size the run-up.</summary>
        public PhrasePattern PeekNext()
        {
            if (_next == null) _next = _library.Pick();
            return _next;
        }

        public void BeginPhrase()
        {
            Pattern = PeekNext();
            _next = null;
            if (Pattern == null) return;

            Phase = RhythmPhase.Call;
            CueIndex = -1;
            HonksAccepted = 0;
            LastHitWasPerfect = false;
            _firstHonkAt = -1f;
            _worstError = 0f;
            _clock = 0f;
            _clipDriven = Pattern.Clip != null;
            if (_clipDriven) _music.PlayPhrase(Pattern.Clip);
        }

        public void ResetState()
        {
            Phase = RhythmPhase.Idle;
            CueIndex = -1;
            HonksAccepted = 0;
            _firstHonkAt = -1f;
        }

        public void NotifyHonk()
        {
            if (!Busy) return;

            if (Phase == RhythmPhase.Call)
            {
                if (FailOnEarlyHonk) Fail("TOO EARLY");
                return;
            }

            if (_firstHonkAt < 0f)
            {
                _firstHonkAt = _clock;
                Accept(0f);
                return;
            }

            float error = Mathf.Abs(_clock - DueAt(HonksAccepted));
            if (error > Tolerance) Fail("OFF BEAT");
            else Accept(error);
        }

        void Update()
        {
            if (!Busy) return;

            if (_clipDriven && _music.PhrasePlaying) _clock = _music.PhraseTime;
            else _clock += Time.deltaTime * _music.TempoScale;

            if (Phase == RhythmPhase.Call) TickCall();
            else TickResponse();
        }

        void TickCall()
        {
            while (CueIndex + 1 < Pattern.Count && _clock >= Pattern.WordTimes[CueIndex + 1])
            {
                CueIndex++;
                if (!_clipDriven) _music.Tick(CueIndex == Pattern.Count - 1);
                CueBeat?.Invoke(CueIndex, Pattern.Words[CueIndex]);
            }

            if (_clock < Pattern.ResponseStart) return;

            Phase = RhythmPhase.Response;
            ResponseOpened?.Invoke();
        }

        void TickResponse()
        {
            if (_firstHonkAt < 0f)
            {
                if (_clock > Pattern.ResponseStart + Pattern.ResponseTimeout) Fail("TOO SLOW");
                return;
            }

            if (_clock > DueAt(HonksAccepted) + Tolerance) Fail("TOO SLOW");
        }

        float Tolerance => Pattern.Tolerance * ToleranceScale;
        float PerfectTolerance => Pattern.PerfectTolerance * ToleranceScale;

        float DueAt(int index) => _firstHonkAt + Pattern.OffsetFromFirst(index);

        void Accept(float error)
        {
            _worstError = Mathf.Max(_worstError, error);
            int index = HonksAccepted;
            HonksAccepted++;
            HonkAccepted?.Invoke(index);

            if (HonksAccepted < Pattern.Count) return;

            LastHitWasPerfect = _worstError <= PerfectTolerance;
            Phase = RhythmPhase.Resolved;
            Success?.Invoke();
        }

        void Fail(string reason)
        {
            Phase = RhythmPhase.Resolved;
            Failed?.Invoke(reason);
        }
    }
}
