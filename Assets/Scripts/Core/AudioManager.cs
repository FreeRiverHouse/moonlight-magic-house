using System.Collections.Generic;
using UnityEngine;

namespace MoonlightMagicHouse
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [System.Serializable]
        public struct SoundEntry
        {
            public string key;
            public AudioClip clip;
            [Range(0.5f, 1.5f)] public float pitchVariance;
        }

        [SerializeField] SoundEntry[] sounds;
        [SerializeField] AudioSource sfxSource;
        [SerializeField] AudioSource musicSource;

        Dictionary<string, SoundEntry> _map;
        bool _deterministicTestMode;

        public event System.Action<string, string, float> CuePlayed;
        public string LastCueKey { get; private set; } = "";
        public int CuePlayCount { get; private set; }

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (sfxSource   == null) sfxSource   = gameObject.AddComponent<AudioSource>();
            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f;
            _map = new Dictionary<string, SoundEntry>();
            if (sounds != null) foreach (var s in sounds) _map[s.key] = s;
            RegisterActivityCues();
        }

        public void Play(string key)
        {
            if (!_map.TryGetValue(key, out var entry) || entry.clip == null) return;
            sfxSource.pitch = _deterministicTestMode
                ? 1f
                : 1f + Random.Range(-entry.pitchVariance, entry.pitchVariance) * 0.1f;
            sfxSource.PlayOneShot(entry.clip);
            LastCueKey = key;
            CuePlayCount++;
            CuePlayed?.Invoke(key, "activity", sfxSource.pitch);
            Debug.Log($"[MoonlightActivityQA] audio cue={key} bus=activity pitch={sfxSource.pitch:0.00}");
        }

        public void SetDeterministicTestMode(bool enabled) => _deterministicTestMode = enabled;

        public void PlayMusic(AudioClip clip, float volume = 0.4f)
        {
            if (musicSource.clip == clip) return;
            musicSource.clip = clip;
            musicSource.volume = volume;
            musicSource.loop = true;
            musicSource.Play();
        }

        void RegisterActivityCues()
        {
            RegisterTone("cook-add", 523.25f, 0.13f, 0.14f);
            RegisterTone("cook-stir", 659.25f, 0.20f, 0.22f);
            RegisterTone("cook-bake", 783.99f, 0.28f, 0.18f);
            RegisterTone("cook-decorate", 987.77f, 0.22f, 0.24f);
            RegisterTone("play-throw", 587.33f, 0.12f, 0.22f);
            RegisterTone("play-chase", 698.46f, 0.18f, 0.28f);
            RegisterTone("play-jump", 783.99f, 0.16f, 0.20f);
            RegisterTone("play-catch", 880.00f, 0.20f, 0.20f);
            RegisterTone("garden-plant", 440.00f, 0.14f, 0.10f);
            RegisterTone("garden-water", 554.37f, 0.22f, 0.24f);
            RegisterTone("garden-tend", 659.25f, 0.18f, 0.18f);
            RegisterTone("garden-bloom", 830.61f, 0.30f, 0.26f);
            RegisterTone("read-open", 466.16f, 0.13f, 0.12f);
            RegisterTone("read-turn", 622.25f, 0.19f, 0.16f);
            RegisterTone("read-trace", 739.99f, 0.24f, 0.20f);
            RegisterTone("read-finish", 932.33f, 0.28f, 0.22f);
            RegisterTone("care-prep", 493.88f, 0.14f, 0.12f);
            RegisterTone("care-wash", 587.33f, 0.22f, 0.24f);
            RegisterTone("care-brush", 698.46f, 0.18f, 0.18f);
            RegisterTone("care-glow", 880.00f, 0.30f, 0.26f);
            RegisterTone("activity-complete", 1046.50f, 0.36f, 0.26f);
            RegisterTone("activity-try-again", 293.66f, 0.16f, 0.08f);
            RegisterTone("sleep", 392.00f, 0.34f, 0.10f);
            RegisterTone("cuddle", 739.99f, 0.24f, 0.20f);
            RegisterTone("room-change", 493.88f, 0.16f, 0.12f);
        }

        void RegisterTone(string key, float frequency, float duration, float harmonic)
        {
            if (_map.ContainsKey(key)) return;
            const int sampleRate = 22050;
            int sampleCount = Mathf.CeilToInt(duration * sampleRate);
            var samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float attack = Mathf.Clamp01(t / 0.018f);
                float release = Mathf.Clamp01((duration - t) / 0.09f);
                float envelope = attack * release * Mathf.Exp(-t * 2.4f);
                samples[i] = (Mathf.Sin(t * frequency * Mathf.PI * 2f) +
                    Mathf.Sin(t * frequency * 2f * Mathf.PI * 2f) * harmonic) * envelope * 0.18f;
            }

            var clip = AudioClip.Create($"MMH-{key}", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            _map[key] = new SoundEntry { key = key, clip = clip, pitchVariance = 0.08f };
        }
    }
}
