using UnityEngine;

namespace OutOfWay
{
    public enum GameState
    {
        Title,
        Playing,
        Failed
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Tooltip("Seconds between the crash and the run restarting on its own.")]
        public float RestartDelay = 3f;

        public GameState State { get; private set; } = GameState.Title;
        public bool IsPlaying => State == GameState.Playing;
        public int Score { get; private set; }
        public int Best { get; private set; }
        public int Streak { get; private set; }

        public BusController Bus;
        public CameraRig Rig;
        public RhythmDirector Rhythm;
        public ObstacleSpawner Spawner;
        public GameUI UI;
        public MusicConductor Music;

        [Header("Tempo Progression")]
        [Tooltip("Scores at which tempo upgrades to 140 BPM (index 1) and 150 BPM (index 2).")]
        public int[] TempoMilestones = { 4, 8 };

        string _failReason = "HIT";
        int _honkFrame = -1;

        void Awake()
        {
            Instance = this;
            Best = PlayerPrefs.GetInt("OutOfWay.Best", 0);
        }

        public void Wire()
        {
            Rhythm.Success += OnCleared;
            Rhythm.Failed += OnRhythmFail;
            Rhythm.CueBeat += (i, word) => UI.ShowCue(i, word);
            Rhythm.ResponseOpened += () => UI.ShowResponse();
            Rhythm.HonkAccepted += OnHonkAccepted;
        }

        public void StartRun()
        {
            if (State == GameState.Playing) return;
            // Clears any revoke/restart still pending from the last crash, which would otherwise kill this run.
            CancelInvoke();
            State = GameState.Playing;
            Score = 0;
            Streak = 0;
            _failReason = "HIT";
            Bus.StartDriving();
            Rhythm.ResetState();
            Spawner.ResetSpawner();
            if (Music != null)
            {
                Music.ResetToStartingTier();
                Music.RestoreBedVolume();
            }
            ProceduralAudio.Instance.PlayEngine(true);
            UI.ShowPlaying();
        }

        public void Honk()
        {
            if (Time.frameCount == _honkFrame) return;
            _honkFrame = Time.frameCount;

            if (State == GameState.Title)
            {
                StartRun();
                return;
            }

            if (State == GameState.Failed)
            {
                Retry();
                return;
            }

            if (State != GameState.Playing) return;

            ProceduralAudio.Instance.Honk();
            UI.FlashHonk();
            Rig.Punch(0.08f);
            Rhythm.NotifyHonk();
        }

        public void HitObstacle(ObstacleController obstacle)
        {
            if (State != GameState.Playing || obstacle.Cleared) return;
            Fail("YOU HIT THEM");
        }

        void OnHonkAccepted(int index)
        {
            UI.ShowHonkAccepted(index);
            Streak++;
            UI.ShowStreak(Streak);
        }

        void OnCleared()
        {
            if (State != GameState.Playing) return;
            var obstacle = Spawner.Active;
            if (obstacle != null) obstacle.Dodge();
            Score++;
            if (Score > Best)
            {
                Best = Score;
                PlayerPrefs.SetInt("OutOfWay.Best", Best);
            }

            CheckTempoUpgrade();

            ProceduralAudio.Instance.Success();
            UI.ShowClear(Score, Rhythm.LastHitWasPerfect);
            Spawner.OnResolved();
            Rhythm.ResetState();
        }

        void CheckTempoUpgrade()
        {
            if (Music == null || Music.TierCount <= 1) return;

            int targetTier = 0;
            for (int i = TempoMilestones.Length - 1; i >= 0; i--)
            {
                if (Score >= TempoMilestones[i])
                {
                    targetTier = i + 1;
                    break;
                }
            }

            if (targetTier > Music.CurrentTierIndex && targetTier < Music.TierCount)
            {
                Music.SetTier(targetTier);
                if (UI != null)
                    UI.ShowBanner($"SPEED UP! {Music.Bpm:0} BPM", 2.4f);
            }
        }

        void OnRhythmFail(string reason)
        {
            if (State != GameState.Playing) return;
            _failReason = reason;
            Streak = 0;
            // Stay doomed — the bus keeps rolling into the obstacle for the hit.
            // If they somehow never collide (bike already aside), revoke on a short timeout.
            UI.ShowMiss(reason);
            UI.ShowStreak(0);
            Invoke(nameof(RevokeIfStillPlaying), 2.4f);
        }

        void RevokeIfStillPlaying()
        {
            if (State == GameState.Playing)
                Fail(_failReason);
        }

        public void Fail(string reason)
        {
            if (State != GameState.Playing) return;
            State = GameState.Failed;
            _failReason = reason;
            Bus.Crash();
            Rig.Punch(0.45f);
            Rhythm.ResetState();
            ProceduralAudio.Instance.PlayEngine(false);
            ProceduralAudio.Instance.Crash();
            if (Music != null) Music.SetBedVolume(0.22f);
            Invoke(nameof(StampRevoked), 0.45f);
            Invoke(nameof(Retry), Mathf.Max(0.6f, RestartDelay));
        }

        void StampRevoked()
        {
            ProceduralAudio.Instance.Revoked();
            UI.ShowRevoked(_failReason, Score, Best);
        }

        /// <summary>
        /// Rebuilds the run in place. The world is generated procedurally and the road recycles
        /// around the bus, so there is nothing to reload — the bus just drives on from here.
        /// </summary>
        public void Retry()
        {
            CancelInvoke();
            State = GameState.Title;
            StartRun();
        }

        void Update()
        {
            if (Bus != null && ProceduralAudio.Instance != null)
                ProceduralAudio.Instance.SetEngineSpeed(Mathf.InverseLerp(Bus.StartSpeed, Bus.MaxSpeed, Bus.Speed));
        }
    }
}
