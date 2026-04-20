using System.Collections.Generic;
using UnityEngine;

namespace MoonlightMagicHouse
{
    public class MoonlightMoodParticles : MonoBehaviour
    {
        [System.Serializable]
        struct MoodFX { public MoonlightMood mood; public ParticleSystem particles; }

        [SerializeField] List<MoodFX> fxMap;
        [SerializeField] ParticleSystem evolutionBurst;

        ParticleSystem _active;

        public void OnMoodChange(MoonlightMood mood)
        {
            if (_active != null) _active.Stop();
            var entry = fxMap.Find(f => f.mood == mood);
            _active = entry.particles;
            if (_active != null) _active.Play();
        }

        public void PlayEvolutionBurst()
        {
            if (evolutionBurst) evolutionBurst.Play();
        }
    }
}
