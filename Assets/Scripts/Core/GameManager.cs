using UnityEngine;
using UnityEngine.SceneManagement;

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

        public GameState State { get; private set; } = GameState.Title;
        public bool IsPlaying => State == GameState.Playing;
        public int Score { get; private set; }
        public int Best { get; private set; }

        public BusController Bus;
        public CameraRig Rig;
        public RhythmDirector Rhythm;
        public ObstacleSpawner Spawner;
        public GameUI UI;
        public MusicConductor Music;

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
            Rhythm.HonkWindowOpened += () => UI.ShowHonkWindow();
        }

        public void StartRun()
        {
            if (State == GameState.Playing) return;
            State = GameState.Playing;
            Score = 0;
            _failReason = "HIT";
            Bus.StartDriving();
            Rhythm.ResetState();
            Spawner.ResetSpawner();
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

            ProceduralAudio.Instance.Success();
            UI.ShowClear(Score, Rhythm.LastHitWasPerfect);
            Spawner.OnResolved();
            Rhythm.ResetState();
        }

        void OnRhythmFail(string reason)
        {
            if (State != GameState.Playing) return;
            _failReason = reason;
            // Stay doomed — the bus keeps rolling into the obstacle for the hit.
            // If they somehow never collide (bike already aside), revoke on a short timeout.
            UI.ShowMiss(reason);
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
            Invoke(nameof(StampRevoked), 0.45f);
        }

        void StampRevoked()
        {
            ProceduralAudio.Instance.Revoked();
            UI.ShowRevoked(_failReason, Score, Best);
        }

        public void Retry()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        void Update()
        {
            if (Bus != null && ProceduralAudio.Instance != null)
                ProceduralAudio.Instance.SetEngineSpeed(Mathf.InverseLerp(Bus.StartSpeed, Bus.MaxSpeed, Bus.Speed));
        }
    }
}
