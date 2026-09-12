using System;
using UnityEngine;

namespace OutOfWay
{
    public enum RhythmPhase
    {
        Idle,
        Cue,
        HonkWindow,
        Resolved
    }

    public class RhythmDirector : MonoBehaviour
    {
        public static readonly string[] CueWords = { "GET", "OUT", "OF", "THE WAY" };

        public RhythmPhase Phase { get; private set; } = RhythmPhase.Idle;
        public int CueIndex { get; private set; } = -1;
        public float WindowOpen { get; private set; }
        public float WindowClose { get; private set; }
        public bool LastHitWasPerfect { get; private set; }
        public bool FailOnEarlyHonk = true;

        public event Action<int, string> CueBeat;
        public event Action HonkWindowOpened;
        public event Action Success;
        public event Action<string> Failed;

        MusicConductor _music;
        float _nextBeatAt;
        float _windowCenter;
        float _honkTickAt;
        bool _honkedThisEvent;
        bool _pendingHonkTick;

        public bool Busy => Phase == RhythmPhase.Cue || Phase == RhythmPhase.HonkWindow;

        public void Bind(MusicConductor music) => _music = music;

        public void BeginPhrase()
        {
            Phase = RhythmPhase.Cue;
            CueIndex = -1;
            _honkedThisEvent = false;
            _pendingHonkTick = false;
            _nextBeatAt = Time.time;
            _music.PlayPhrase();
        }

        public void ResetState()
        {
            Phase = RhythmPhase.Idle;
            CueIndex = -1;
            _honkedThisEvent = false;
            _pendingHonkTick = false;
        }

        public void NotifyHonk()
        {
            if (Phase == RhythmPhase.Idle || Phase == RhythmPhase.Resolved) return;

            if (Phase == RhythmPhase.Cue)
            {
                if (FailOnEarlyHonk)
                {
                    Phase = RhythmPhase.Resolved;
                    Failed?.Invoke("TOO EARLY");
                }
                return;
            }

            if (Phase != RhythmPhase.HonkWindow || _honkedThisEvent) return;

            _honkedThisEvent = true;
            float now = Time.time;
            if (now >= WindowOpen && now <= WindowClose)
            {
                LastHitWasPerfect = Mathf.Abs(now - _windowCenter) <= _music.BeatInterval * 0.14f;
                Phase = RhythmPhase.Resolved;
                Success?.Invoke();
            }
            else
            {
                Phase = RhythmPhase.Resolved;
                Failed?.Invoke("OFF BEAT");
            }
        }

        void Update()
        {
            if (_pendingHonkTick && Time.time >= _honkTickAt)
            {
                _pendingHonkTick = false;
                _music.Tick(true);
            }

            if (Phase == RhythmPhase.Idle || Phase == RhythmPhase.Resolved) return;

            if (Phase == RhythmPhase.HonkWindow && Time.time > WindowClose && !_honkedThisEvent)
            {
                Phase = RhythmPhase.Resolved;
                Failed?.Invoke("MISSED THE HORN");
                return;
            }

            if (Time.time < _nextBeatAt) return;

            if (Phase == RhythmPhase.Cue)
            {
                CueIndex++;
                if (CueIndex < CueWords.Length)
                {
                    bool last = CueIndex == CueWords.Length - 1;
                    _music.Tick(last);
                    CueBeat?.Invoke(CueIndex, CueWords[CueIndex]);
                    float beat = _music.BeatInterval;
                    float hit = beat * 0.42f;
                    _nextBeatAt = last ? Time.time + beat - hit : Time.time + beat;
                    return;
                }

                OpenHonkWindow();
            }
        }

        void OpenHonkWindow()
        {
            Phase = RhythmPhase.HonkWindow;
            float beat = _music.BeatInterval;
            float hit = beat * 0.42f;
            _windowCenter = Time.time + hit;
            WindowOpen = Time.time;
            WindowClose = Time.time + hit * 2f;
            _honkTickAt = _windowCenter;
            _pendingHonkTick = true;
            HonkWindowOpened?.Invoke();
        }
    }
}
