using UnityEngine;

namespace OutOfWay
{
    /// <summary>SFX audio engine. Plays real audio clips when available with procedural fallbacks.</summary>
    public class ProceduralAudio : MonoBehaviour
    {
        public static ProceduralAudio Instance { get; private set; }

        public AudioClip HonkClip;

        AudioSource _oneshot;
        AudioSource _engine;
        AudioClip _honk;
        AudioClip _click;
        AudioClip _accent;
        AudioClip _success;
        AudioClip _crash;
        AudioClip _revoked;
        AudioClip _engineLoop;

        public static ProceduralAudio Create(Transform parent, AudioClip honkClip = null)
        {
            var t = Build.Empty("Audio", parent);
            var audio = t.gameObject.AddComponent<ProceduralAudio>();
            audio.HonkClip = honkClip;
            audio.BuildSources();
            return audio;
        }

        public void SetHonkClip(AudioClip clip)
        {
            if (clip != null)
            {
                HonkClip = clip;
                _honk = clip;
            }
        }

        void BuildSources()
        {
            Instance = this;
            _oneshot = gameObject.AddComponent<AudioSource>();
            _oneshot.playOnAwake = false;
            _oneshot.spatialBlend = 0f;

            _engine = gameObject.AddComponent<AudioSource>();
            _engine.playOnAwake = false;
            _engine.loop = true;
            _engine.spatialBlend = 0f;
            _engine.volume = 0.12f;

            if (HonkClip != null)
            {
                _honk = HonkClip;
            }
            else
            {
                _honk = Resources.Load<AudioClip>("Audio/honk") ?? Resources.Load<AudioClip>("Audio/Honk");
#if UNITY_EDITOR
                if (_honk == null)
                    _honk = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/Audio/honk.mp3");
#endif
                if (_honk == null)
                    _honk = Horn("honk", 392f, 311f, 0.28f);
            }
            _click = Tone("click", 880f, 0.06f, 0.22f);
            _accent = Tone("accent", 1174f, 0.08f, 0.28f);
            _success = Chord("clear", new[] { 523f, 659f, 784f }, 0.32f, 0.2f);
            _crash = Noise("crash", 0.45f, 0.45f);
            _revoked = Tone("revoked", 110f, 0.7f, 0.3f);
            _engineLoop = Engine("engine", 2.2f);
            _engine.clip = _engineLoop;
        }

        public void PlayEngine(bool on)
        {
            if (on && !_engine.isPlaying) _engine.Play();
            if (!on && _engine.isPlaying) _engine.Stop();
        }

        public void SetEngineSpeed(float normalized) =>
            _engine.pitch = Mathf.Lerp(0.75f, 1.35f, normalized);

        public void Honk()
        {
            if (_honk != null) _oneshot.PlayOneShot(_honk, 1f);
        }
        public void Click() => _oneshot.PlayOneShot(_click, 0.55f);
        public void Accent() => _oneshot.PlayOneShot(_accent, 0.7f);
        public void Success() => _oneshot.PlayOneShot(_success, 0.8f);
        public void Crash() => _oneshot.PlayOneShot(_crash, 1f);
        public void Revoked() => _oneshot.PlayOneShot(_revoked, 0.8f);

        static AudioClip Tone(string name, float freq, float duration, float volume)
        {
            const int rate = 44100;
            int samples = Mathf.CeilToInt(duration * rate);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)rate;
                float env = Mathf.Clamp01(t * 50f) * Mathf.Clamp01((duration - t) * 18f);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * volume;
            }

            var clip = AudioClip.Create(name, samples, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip Horn(string name, float a, float b, float duration)
        {
            const int rate = 44100;
            int samples = Mathf.CeilToInt(duration * rate);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)rate;
                float env = Mathf.Clamp01(t * 40f) * Mathf.Clamp01((duration - t) * 8f);
                data[i] = (Mathf.Sin(2f * Mathf.PI * a * t) + Mathf.Sin(2f * Mathf.PI * b * t)) * 0.5f * env * 0.7f;
            }

            var clip = AudioClip.Create(name, samples, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip Chord(string name, float[] freqs, float duration, float volume)
        {
            const int rate = 44100;
            int samples = Mathf.CeilToInt(duration * rate);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)rate;
                float env = Mathf.Clamp01(t * 30f) * Mathf.Clamp01((duration - t) * 6f);
                float s = 0f;
                for (int f = 0; f < freqs.Length; f++)
                    s += Mathf.Sin(2f * Mathf.PI * freqs[f] * t);
                data[i] = s / freqs.Length * env * volume;
            }

            var clip = AudioClip.Create(name, samples, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip Noise(string name, float duration, float volume)
        {
            const int rate = 44100;
            int samples = Mathf.CeilToInt(duration * rate);
            var data = new float[samples];
            var rng = new System.Random(7);
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)rate;
                float env = Mathf.Clamp01((duration - t) / duration);
                data[i] = ((float)rng.NextDouble() * 2f - 1f) * env * env * volume;
            }

            var clip = AudioClip.Create(name, samples, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip Engine(string name, float duration)
        {
            const int rate = 44100;
            int samples = Mathf.CeilToInt(duration * rate);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)rate;
                float rumble = Mathf.Sin(2f * Mathf.PI * 42f * t) * 0.35f;
                rumble += Mathf.Sin(2f * Mathf.PI * 87f * t) * 0.18f;
                rumble += Mathf.PerlinNoise(t * 18f, 0.2f) * 0.22f;
                data[i] = rumble * 0.35f;
            }

            var clip = AudioClip.Create(name, samples, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
