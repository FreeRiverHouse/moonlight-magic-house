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

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (sfxSource   == null) sfxSource   = gameObject.AddComponent<AudioSource>();
            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
            _map = new Dictionary<string, SoundEntry>();
            if (sounds != null) foreach (var s in sounds) _map[s.key] = s;
        }

        public void Play(string key)
        {
            if (!_map.TryGetValue(key, out var entry) || entry.clip == null) return;
            sfxSource.pitch = 1f + Random.Range(-entry.pitchVariance, entry.pitchVariance) * 0.1f;
            sfxSource.PlayOneShot(entry.clip);
        }

        public void PlayMusic(AudioClip clip, float volume = 0.4f)
        {
            if (musicSource.clip == clip) return;
            musicSource.clip = clip;
            musicSource.volume = volume;
            musicSource.loop = true;
            musicSource.Play();
        }
    }
}
